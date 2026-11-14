# HTTP contract: ChangeFlow v1

مرجع رفتاری: [Spec](../spec.md)؛ انتخاب‌های طراحی قابلیت:
[TB-005 تا TB-007](../../../docs/architecture/technical-baseline.md)؛
پژوهش: [Research](../research.md)؛ مدل: [data-model](../data-model.md)؛
الگوریتم تراکنش/recovery: [Plan](../plan.md#execution-design).
قرارداد طراحی است، نه API اجراشده. مسیر پایه `/api/change-requests` و پاسخ‌ها JSON هستند.
بازآرایی ارجاع‌ها: 2026-11-13؛ مسیرها، payloadها و status/codeهای قبلی تغییر نکرده‌اند.
سیاست مشاهده و پنهان‌کردن وجود منبع از FR-014/FR-016، زمان لغو از FR-008، مدت replay از FR-018
و رفتار فیلد اختیاری از FR-003/FR-017 می‌آیند؛ این قرارداد فقط نمایش فنی همان سیاست‌هاست.

## هویت، نسخه و تکرار

- همهٔ مسیرها `Authorization: Bearer <opaque-token>` می‌خواهند. هیچ role/userId ورودی منبع هویت نیست.
- همهٔ mutationها `Idempotency-Key` با ۱ تا ۱۲۸ نویسهٔ ASCII قابل چاپِ غیرwhitespace می‌خواهند؛
  کلید case-sensitive و در محدودهٔ actor و operation kind است. نمونهٔ پیشنهادی UUID تازه برای هر قصد تازه.
- برای PUT، DELETE، Submit، Approve و RequestChanges، `If-Match: "7"` به شکل یک Version مثبت
  int64 اجباری است؛ `*`، ETag ضعیف، چند مقدار یا عدد نامعتبر پذیرفته نشوند.
- GET تکی موفق `ETag: "7"` و Version را برمی‌گرداند؛ هر ویرایش یا انتقال موفق Version را +۱ می‌کند.
  همهٔ فرمان‌ها باید از نسخهٔ مشاهده‌شده استفاده کنند؛ Approve نمی‌تواند نسخهٔ جدید را حدس بزند.
- receipt اولیه و replay هر دو کمینه‌اند: `{"requestId":"uuid","result":"Approved"}`.
  هیچ title/description/reason در receipt نیست؛ محتوا فقط از GET دارای دسترسی خوانده می‌شود.
- replay همان actor، operation، key و fingerprint، status/body/Location ذخیره‌شده را برمی‌گرداند،
  حتی اگر منبع تغییر وضعیت داده یا حذف شده باشد. بررسی هویت و نقش شروع تازه همچنان لازم است.
  تغییر target، expectedVersion یا محتوای canonical با همان key → 409 `IDEMPOTENCY_MISMATCH`.
- همهٔ پاسخ‌ها `Cache-Control: no-store` داشته باشند؛ replay نباید ETag محتوا یا snapshot حساس ذخیره کند.

## محتوای ورودی و خروجی

### ContentInput برای Create و PUT

```json
{"title":"اصلاح نام فیلد","description":"شرح تغییر پیشنهادی","reason":"شفافیت قرارداد"}
```

`title` همیشه اجباری، string غیرwhitespace و حداکثر۲۰۰ Unicode scalar؛
`description` و `reason` اختیاری، null یا string، با سقف‌های۵۰۰۰ و۲۰۰۰ scalar.
غایب/null/رشتهٔ خالی برای هرکدام به null تبدیل شود؛ در PUT مقدار ذخیره‌شدهٔ همان فیلد پاک می‌شود.
با مقدار غیرخالی معتبر جایگزین شود. متن trim/NFC نشود؛ شمارش با Unicode scalar مطابق FR-017 و پاسخ ثبت‌شدهٔ A-03 در [Clarifications](../spec.md#clarifications).
رشتهٔ JSON حاوی surrogate نامعتبر رد شود. title غایب/null/خالی، کل عملیات را رد می‌کند.
پذیرش title فقط whitespace مجاز نیست؛ length فیلد اختیاری whitespaceدار بدون trim حساب می‌شود.

ویژگی‌های ناشناخته، duplicate property، `requesterId`، `status`، `version` و فیلدهای baseline در ورودی رد شوند؛
نه اینکه ورودی ادعایی مالکیت silently نادیده گرفته و بعداً به‌عنوان رفتار معتبر پذیرفته شود.
در endpointهای commands، بدنهٔ خالی یا `{}` هم‌معناست؛ سایر propertyها یا محتوا خطاست.
Content-Type برای body JSON برابر `application/json`؛ سقف حمل درخواست۱۲۸KiB برای جلوگیری از ورودی بی‌حد است،
نه محدودیت تازه بر طول فیلدهای منطقی؛ این سقف حتی escape دوازده‌بایتیِ هر scalar را برای مجموع
حداکثر۷۲۰۰ نویسهٔ مجاز پوشش می‌دهد. JSON بزرگ‌تر413 می‌گیرد؛ content type نامعتبر415.

### ChangeRequestView

```json
{
  "id":"00000000-0000-4000-8000-000000000001",
  "title":"اصلاح نام فیلد",
  "description":null,
  "reason":null,
  "requesterId":"00000000-0000-4000-8000-000000000010",
  "status":"PendingReview",
  "version":2
}
```

baseline، token hash، نقش‌ها و ledger هرگز در View عمومی نباشند.

### Problem

خطاها `application/problem+json` با `type: about:blank`، `title` عمومی، `status` و `code` پایدار؛
خطای اعتبارسنجی می‌تواند `errors` از نام فیلد به توضیح داشته باشد، بدون echo محتوای حساس.
جزئیات SQL، stack trace، raw token، fingerprint و اینکه درخواست متعلق به چه کسی است افشا نشوند.
دو حالت ناموجود و فاقد مشاهده باید دقیقاً body عمومی `{"type":"about:blank","title":"Not found","status":404,"code":"NOT_FOUND"}` داشته باشند.
عدم افشا در body/status/headers هدف آزمون است؛ یکسانی زمان پاسخ رمزنگارانه ادعا نمی‌شود.

## مسیرها

| Method / Path | operation kind | نقش شروع و مالکیت | موفقیت | اثر |
| --- | --- | --- | --- | --- |
| POST `/api/change-requests` | Create | Requester | 201 + Location | Draft، Version۱، مالک از هویت |
| GET `/api/change-requests/{id}` | read | Requester مالک در هر وضعیت؛ Reviewer دیگران فقط PendingReview | 200 + ETag + View | بدون تغییر |
| GET `/api/change-requests` | list | همان predicate مشاهده | 200 + Page | فقط ردیف‌های مجاز |
| PUT `/api/change-requests/{id}` | ReplaceContent | Requester مالک | 200 receipt Updated | فقط Draft/ChangesRequested/PendingReview؛ وضعیت حفظ و Version+۱ |
| POST `/api/change-requests/{id}/submit` | Submit | Requester مالک | 200 receipt Submitted | Draft یا ChangesRequested → PendingReview |
| POST `/api/change-requests/{id}/approve` | Approve | Reviewer غیرمالک | 200 receipt Approved | PendingReview → Approved |
| POST `/api/change-requests/{id}/request-changes` | RequestChanges | Reviewer غیرمالک | 200 receipt ChangesRequested | PendingReview → ChangesRequested و snapshot baseline |
| DELETE `/api/change-requests/{id}` | Delete | Requester مالک | 200 receipt Deleted | فقط Draft؛ حذف فیزیکی شرطی |

Create ContentInput می‌گیرد؛ PUT همان ContentInput را به‌صورت replacement محتوا می‌گیرد.
هیچ مسیر PATCH، public login، ثبت‌نام، مدیریت نقش، ثبت پذیرش انسانی یا رابط frontend وجود ندارد.
برای mutationهای مختلف روی یک request، operation kindهای متفاوت scope متفاوت key دارند؛
دو هدف متفاوت با همان key در همان actor/operation mismatch است.

### شرط‌های ویژه

- Submit از ChangesRequested: حداقل یکی از سه مقدار محتوایی canonical با آخرین baseline متفاوت باشد؛
  در غیر این صورت409 `CHANGES_REQUIRED`. تغییر و بازگرداندن همان مقدار، مجوز ارسال نیست.
- Approve و RequestChanges روی درخواست خود حتی برای کاربر دو نقشی403 `SELF_REVIEW_FORBIDDEN`.
- mutation غیرمجاز برای غیرمالکِ دارای نقش لازم، اگر منبع را هم نمی‌تواند ببیند404 عمومی است؛
  اگر مجوز مشاهده دارد ولی اختیار mutation ندارد403 `OPERATION_FORBIDDEN`، بدون تغییر داده.
- Approved همواره بدون mutation می‌ماند؛ ویرایش یا انتقال نامعتبر409 `INVALID_STATE` پس از کنترل دسترسی/نسخه.
- نسخهٔ stale برای منبع قابل دسترسی412 `VERSION_CONFLICT`؛ نبود منبع یا مشاهده همچنان404 است.
- حذف Draft و Submit در رقابت، یا ویرایش و Approve در رقابت، هر دو روی یک نسخه موفق نمی‌شوند.

### Page

queryها: `limit` پیش‌فرض۵۰ و بازه۱..۱۰۰، `cursor` اختیاری opaque base64url از آخرین Id؛
سرویس قبل از page شدن predicate دسترسی را در SQL اعمال کند. ترتیب Id صعودی ثابت است.
cursor نامعتبر400؛ pagination snapshot زمانی تضمین نمی‌کند و برای تعیین مجوز استفاده نمی‌شود.
ناموجودبودن ردیف مجاز، `{"items":[],"nextCursor":null}` است.
هر item یک View است. `nextCursor` فقط اگر ردیف مجاز بیشتری وجود دارد مقدار می‌گیرد؛ total count افشا نمی‌شود.
فیلتر وضعیت، جست‌وجو و sort دلخواه در نسخهٔ اولیه اضافه نمی‌شوند.

## ترتیب ارزیابی و خطا

1. محدودیت transport، JSON قابل parse و authentication؛ توکن نامعتبر401 و `WWW-Authenticate: Bearer`.
   خطای DB در احراز هویت503 است، نه401.
2. نقش شروع لازم (یا اجتماع مجاز برای GET)؛ نبود نقش403 `ROLE_REQUIRED`، پیش از lookup منبع.
3. headerهای اجباری و ساختار canonical؛ key غایب400 `IDEMPOTENCY_KEY_REQUIRED`؛
   If-Match غایب428 `PRECONDITION_REQUIRED`؛ شکل غلط400 `INVALID_PRECONDITION`.
4. در تراکنش، ledger matching بر کنترل وضعیت زنده اولویت دارد؛ key mismatch409.
5. منبع و مشاهده، سپس مالکیت/استقلال، سپس Version، سپس وضعیت/قواعد محتوایی و mutation.
   درخواست ناموجود یا پنهان404 عمومی؛ stale412، وضعیت نامعتبر409.
6. خطای اعتبارسنجی محتوایی400 `VALIDATION_FAILED` بدون تغییر entity؛
   خطای transient ذخیره‌سازی503 `STORAGE_UNAVAILABLE` با `Retry-After: 1` و دستور حفظ همان key.
7. خطای غیرمنتظره500 عمومی؛ اگر commit نامعلوم است نتیجه ابتدا reconcile شود و تا قطعیت موفقیت اعلام نشود.

در syntax/header/auth نامعتبر ledger رزرو نمی‌شود. پس از احراز نقش و ساخت canonical،
نتیجهٔ قطعی کاربردی (400 محتوایی،403 منبعی،404،409،412) بدون تغییر entity در ledger ثبت شود؛
retry همان key همان نتیجه را بازمی‌گرداند. اصلاح ورودی/نسخه نیازمند key جدید است.
busy/I/O یا قطع اتصال نتیجهٔ قطعی شکست کاربردی نیستند و key را poison نمی‌کنند.
fingerprint باید از payload پیش از validation محتوایی ولی پس از parse نوع داده ساخته شود؛
رشتهٔ بیش‌ازحدطول canonicalizable است، نوع غلط یا JSON خراب نیست.

## نمونهٔ جریان معتبر

1. POST توسط Alice با key تازه، دریافت201 و requestId.
2. GET توسط Alice، دریافت ETag `"1"`.
3. POST submit با If-Match `"1"` و key تازه، دریافت Submitted.
4. GET توسط Bob Reviewer، دریافت PendingReview با ETag `"2"`.
5. Approve با همان نسخه و key تازه → receipt Approved؛ retry همان key → همان receipt.
6. GET توسط Bob پس از Approved →404؛ GET توسط Alice →200 با Approved.

پاسخ‌های مثال هدف اعتبارسنجی آینده‌اند؛ در مرحلهٔ Plan اجرا نشده‌اند.

