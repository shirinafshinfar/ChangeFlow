# Data model: ChangeFlow

مرجع رفتار: [Spec](spec.md)؛ پژوهش: [Research](research.md)؛
انتخاب‌های طراحی قابلیت: [TB-004/TB-005/TB-007](../../docs/architecture/technical-baseline.md)؛
الگوریتم: [Execution Design](plan.md#execution-design)؛ پیام‌ها: [قرارداد](contracts/http-api.md).
این سند مدل طراحی است؛ جدول‌ها و migrationها هنوز ساخته نشده‌اند.
بازآرایی ارجاع‌ها: 2026-11-13؛ فیلدها، نوع‌ها و انتخاب‌های قبلی مدل تغییر نکرده‌اند.
قواعد محتوای اختیاری از FR-003/FR-017، baseline از FR-004، مشاهده از FR-014/FR-016،
زمان اثر لغو از FR-008، مدت تکرار از FR-018 و حذف از FR-019 مشتق‌اند؛ این مدل مرجع تصمیم محصولی نیست.

## ChangeRequest

| فیلد | نوع منطقی / ذخیره | قاعده و دلیل |
| --- | --- | --- |
| Id | UUID / TEXT canonical lowercase | server-generated؛ هویت پایدار برای مسیر و receipt |
| Title | Unicode / TEXT NOT NULL | اجباری، غیرwhitespace، حداکثر۲۰۰ scalar |
| Description | Unicode nullable / TEXT NULL | غایب/null/خالی → null؛ حداکثر۵۰۰۰ scalar |
| Reason | Unicode nullable / TEXT NULL | غایب/null/خالی → null؛ حداکثر۲۰۰۰ scalar |
| RequesterId | UUID / TEXT FK User | فقط از ActorContext؛ مالک تغییرناپذیر |
| Status | enum / TEXT CHECK | فقط Draft، PendingReview، ChangesRequested، Approved |
| Version | int64 / INTEGER NOT NULL | شروع۱؛ هر ویرایش/انتقال موفق +۱؛ حذف با شرط نسخه |
| ReviewBaselineTitle | TEXT NULL | محتوای آخرین ورود به ChangesRequested برای FR-004 |
| ReviewBaselineDescription | TEXT NULL | snapshot canonical؛ حفظ تفاوت null و مقدار واقعی |
| ReviewBaselineReason | TEXT NULL | مانند بالا؛ فیلد فنی لازم برای شرط ارسال مجدد |

Snapshot در RequestChanges با محتوای فعلی و در همان تراکنش مقداردهی/جایگزین می‌شود؛
در ChangesRequested باید موجود و معتبر باشد. Submit از این وضعیت فقط وقتی یکی از سه مقدار canonical
فعلی با baseline فرق دارد مجاز است. فقط تغییر Version کافی نیست؛ تغییر و بازگرداندن همان محتوا کافی نیست.
پس از Submit می‌توان baseline را نگه داشت؛ RequestChanges بعدی همیشه آن را تازه می‌کند.
در Draft baseline خالی است. عنوان baseline در ChangesRequested نباید null باشد.

ویرایش، هر سه مقدار محتوایی را مطابق PUT محاسبه می‌کند: Title اجباری، Description/Reason غایب یا
خالی پاک می‌شوند؛ بدون trim/NFC پنهان. حتی ویرایش معتبر هم‌محتوا Version را +۱ می‌کند و در
PendingReview نیاز به بررسی نسخهٔ جدید ایجاد می‌کند؛ با این حال شرط «تغییر محتوا» با Version برآورده نمی‌شود.
ویرایش و baseline هیچ‌گاه در Endpoint تصمیم‌گیری نمی‌شوند.

حذف فیزیکی فقط Draft مالک با مجوز شروع Requester و نسخهٔ جاری انتخاب شده است؛
ledger FK حذفی به ChangeRequest ندارد و با حذف آن cascade نمی‌شود. Id حذف‌شده دوباره استفاده نشود.
Approved فیزیکی حذف یا ویرایش نمی‌شود. بازیابی درخواست حذف‌شده قابلیت محصول نیست.

## چرخهٔ وضعیت

| مبدأ | عمل | مقصد | شرط در Core/Application |
| --- | --- | --- | --- |
| ایجاد | Create | Draft | Requester شروع؛ مالک از هویت |
| Draft | Submit | PendingReview | مالک + Requester شروع + نسخه |
| PendingReview | Approve | Approved | Reviewer شروع + غیرمالک + نسخهٔ بررسی‌شدهٔ فعلی |
| PendingReview | RequestChanges | ChangesRequested | Reviewer شروع + غیرمالک + نسخه؛ snapshot محتوا |
| ChangesRequested | Submit | PendingReview | مالک + Requester شروع + نسخه + تغییر نسبت به baseline |
| Draft | Delete | عدم وجود | مالک + Requester شروع + نسخه؛ بدون انتقال enum جدید |

ویرایش فقط Draft/ChangesRequested/PendingReview مالک؛ وضعیت حفظ و Version افزایش یابد.
تمام ترکیب‌های دیگر رد شوند. UUID/توکن معرف مجوز نیستند.

## User و AccessToken — زیرساخت هویت

| موجودیت | فیلدها | قیود |
| --- | --- | --- |
| User | Id TEXT PK، Enabled INTEGER، RolesMask INTEGER، AuthorizationVersion INTEGER | نقش‌ها بیت‌های Requester و Reviewer؛ snapshot یک query از ردیف کاربر |
| AccessToken | TokenHash BLOB PK، UserId FK، Enabled INTEGER | hash32byte؛ توکن خام فقط در خروجی امن seed محلی |

RolesMask/Enabled با maintenance محلی در تراکنش تغییر و AuthorizationVersion افزایش یابد؛
درخواست عمومی برای تغییر نقش وجود ندارد. query شروع، user و token enabled و نقش‌های فعلی را با هم می‌خواند؛
ActorContext `(UserId, roles, AuthorizationVersion)` فقط برای همان عملیات immutable است.
افزودن AuthorizationVersion برای trace و اثبات snapshot است، نه ابطال دوبارهٔ عملیات در حال اجرا.
User در محصول حذف نمی‌شود تا مالکیت و کلیدهای ledger orphan نشوند؛ maintenance فقط نقش/فعال‌بودن را تغییر می‌دهد.

## OperationResult — ledger تکرار

| فیلد | نوع | قاعده |
| --- | --- | --- |
| ActorId | UUID / TEXT FK User | identity درخواست پذیرفته‌شده |
| OperationKind | TEXT enum | Create، ReplaceContent، Submit، Approve، RequestChanges، Delete |
| IdempotencyKey | TEXT binary comparison | ASCII بدون whitespace، طول۱..۱۲۸؛ ترکیب سه فیلد اول PK |
| Fingerprint | BLOB SHA-256 | canonical payload شامل target، expectedVersion و محتوای ورودی |
| TargetId | UUID / TEXT | در Create، Id تولیدشده؛ در fingerprint create هدف null است |
| HttpStatus | INTEGER | پاسخ terminal قبلی؛ replay همان status را حفظ می‌کند |
| ResultCode | TEXT | Created، Updated، Submitted، Approved، ChangesRequested، Deleted یا code خطای قطعی |
| ResponseJson | TEXT | فقط receipt یا Problem عمومی و بدون محتوای درخواست/توکن/stack trace |
| Location | TEXT NULL | فقط Create؛ replay همان مسیر، حتی اگر درخواست بعداً حذف شده باشد |

هیچ ExpiresAt یا پاکسازی TTL وجود ندارد؛ حذف درخواست نتیجه را پاک نمی‌کند.
timestamp کسب‌وکار و audit کامل خارج از نیاز فعلی‌اند؛ به مدل اضافه نشده‌اند.
اثر و result در همان transaction باشند. نتیجهٔ نامعلوم با همین key reconcile شود، نه اجرای عملیات جدید.
برای key موجود و fingerprint متفاوت، 409 عمومی داده شود و نتیجهٔ قبلی overwrite نشود.
نتیجهٔ معتبر موجود قبل از lookup منبع زنده بازگردد، پس retry حذف روی منبع ناموجود هم نتیجهٔ قبلی دارد.
دریافت نتیجه نیازمند هویت همان actor و مجوز شروع مرتبط است؛ نقش لغوشده اجازهٔ replay نمی‌دهد.
همهٔ پاسخ‌های موفق mutation receipt کمینه‌اند، بنابراین رسید Approve پس از خروج از PendingReview
نیز title/description/reason ندارد؛ دادهٔ محتوایی اصلاً در ledger ذخیره نمی‌شود.

## Index و قیود ذخیره

- PK درخواست روی Id؛ index `(RequesterId, Id)` و `(Status, Id)` برای query مشاهدهٔ مجاز.
- PK ledger روی `(ActorId, OperationKind, IdempotencyKey)`؛ FK به User با RESTRICT.
- تمام Versionها باید مثبت و در محدودهٔ int64 باشند؛ سرریز خطای داخلی امن و بدون تغییر داده است.
- validation شمارش Unicode در Core انجام شود؛ SQL CHECK وضعیت، Version، طول hash و role mask را کنترل کند.
- SQL رشته‌سازی از ورودی ممنوع؛ همهٔ پارامترها bind شوند. order/filter فقط مجموعهٔ مجاز ثابت.
- Read/Write connection از pool با PRAGMAهای تأییدشده؛ DataReader قبل از commit بسته شود.
- schema version داخلی برای راه‌اندازی قابل تکرار؛ ارتقای schema با نگهداری ledger و بدون reset داده.

## مرز هم‌زمانی

ترتیب snapshot، lookup، mutation، commit و recovery در [Plan](plan.md#execution-design) و
انتخاب‌های طراحی همین قابلیت در [TB-004](../../docs/architecture/technical-baseline.md#tb-004) شرح داده شده‌اند؛
نقش Version در این مدل، رد تغییر مبتنی بر مشاهدهٔ قدیمی علاوه بر serialization است.
برای Approve همان expectedVersion از GET مورد بررسی استفاده شود؛ سیگنال مستقل «من واقعاً خواندم»
از سمت انسان قابل اثبات فنی نیست؛ API نسخهٔ مبنای تصمیم را الزام و stale بودن آن را رد می‌کند.
