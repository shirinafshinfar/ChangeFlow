# Implementation Plan: ChangeFlow

**Branch**: `001-manage-change-requests` — شناسهٔ منطقی خروجی setup؛ در زمان طراحی شاخهٔ دارای commit تأیید نشده بود.
نسخهٔ تحویل در شاخهٔ `main` مخزن عمومی منتشر شده است؛ این انتشار، ایجاد شاخهٔ قابلیت در ریشهٔ محلی را اثبات نمی‌کند.
**Date**: 2026-11-12 | **Spec**: [spec.md](spec.md)
**Input**: پاسخ‌های Q1 تا Q9 و پاسخ‌های ثبت‌شدهٔ Clarify در Spec جاری.
**Status**: Phase 0 و Phase 1، سابقهٔ مرحلهٔ طراحی این سند هستند؛ در آن زمان محصول پیاده‌سازی یا پذیرفته نشده بود.
**وضعیت فعلی — 2026-11-14:** پیاده‌سازی و شواهد اجرا در [گزارش پروژه](../../artifacts/validation/project-report.md)
و پذیرش نسخهٔ تحویل در [ثبت تأیید کاربر](../../docs/reviews/acceptance-2026-11-14.md) ثبت شده‌اند.
**بازآرایی اسناد:** 2026-11-13؛ تاریخ و نتایج ثبت‌شدهٔ Plan اصلی حفظ شده‌اند.

## Summary

کاربرد انتخاب‌های طراحی همین قابلیت [مبنای فنی](../../docs/architecture/technical-baseline.md) در این قابلیت:
Version و تراکنش واحد state+ledger برای حفظ قواعد گردش درخواست، fingerprint برای تکرار امن،
baseline محتوای آخرین RequestChanges و receipt برای اجرای محدودیت محتوای replay در FR-018.
پژوهش و مقایسهٔ گزینه‌ها در [research.md](research.md)، مدل در [data-model.md](data-model.md)،
قرارداد در [contracts/http-api.md](contracts/http-api.md) و راهنمای بررسی در [quickstart.md](quickstart.md) است.

## Technical Context

**Language/Version**: [TB-001 و جدول سوابق تاریخ‌دار محیط](../../docs/architecture/technical-baseline.md#tb-001).

**Primary Dependencies**: نسخه‌های طراحی و lock/scan در TB-001 و [TB-008](../../docs/architecture/technical-baseline.md#tb-008)؛ در این بازآرایی تغییر نکرده‌اند.

**Storage**: [TB-004](../../docs/architecture/technical-baseline.md#tb-004)؛ مدل اختصاصی قابلیت،
PK ledger و حذف فیزیکی Draft بدون cascade سابقهٔ نتیجه در `data-model.md`.

**Testing**: TB-008؛ گروه‌های T01–T18 و شواهد اختصاصی قابلیت در `quickstart.md`.

**Target Platform**: [TB-002](../../docs/architecture/technical-baseline.md#tb-002)؛ وضعیت اعتبارسنجی پلتفرم‌ها همان سابقهٔ اولیه است.

**Project Type**: HTTP web-service آموزشی؛ بدون frontend، public login و مدیریت حساب.

**Performance Goals**: Spec هدف latency/throughput/uptime تجاری تعیین نکرده است؛ هدف دلخواه اختراع نمی‌شود.
انتظار قفل در TB-004 و نگاشت retry در قرارداد ثبت شده‌اند؛ اندازه‌گیری عملکرد شاهد آینده می‌خواهد.

**Constraints**: TB-004 تا TB-007 سازوکار را تعیین می‌کنند؛ سیاست مشاهده از FR-014/FR-016،
زمان اثر لغو از FR-008، مدت تضمین تکرار از FR-018 و رفتار محتوای اختیاری از FR-003/FR-017 خوانده شود.

**Scale/Scope**: دامنهٔ استقرار TB-002؛ قابلیت فعلی دو نقش و ترکیب آن‌ها، چهار وضعیت و هشت عملیات HTTP دارد؛
page/سقف حمل در `contracts/http-api.md` مرجع اصلی دارند.

## Constitution Check

مرجع جاری: [Constitution](../../.specify/memory/constitution.md) نسخهٔ مصوب `2.0.2`، تاریخ تصویب 2026-11-14.
در بازبینی طراحی، نسخهٔ خوانده‌شده `2.0.2-draft.1` و تصویب‌نشده بود؛ تصویب بعدی اصول را تغییر نداد.
بررسی طراحی اولیه با نسخهٔ2.0.0-draft.1 انجام شده بود؛ نتایج اولیهٔ زیر شاهد اجرای محصول نیستند.
اجازهٔ Plan از درخواست کاربر است و به معنی تصویب Constitution یا پذیرش محصول نیست.
Gate پیش از Research: PASS برای قواعد جاری؛ منع قدیمی ویرایش PendingReview در سند جاری اصلاح شده است.
یادداشت‌های قدیمیِ هماهنگ‌سازی Constitution در Spec/checklist با وضعیت جاری همگام شده‌اند؛
تصمیم‌های محصولی ثبت‌شده حفظ می‌شوند؛ وضعیت تصویب‌نشده متعلق به زمان طراحی بود، نه وضعیت فعلی سند.

| اصل | قبل از Research | پس از طراحی و شاهد |
| --- | --- | --- |
| I — معماری | PASS | PASS؛ Core فقط BCL، بدون HTTP/SQL؛ Endpoint سبک، R-01 و ساختار زیر |
| II — گردش/مالکیت | PASS | PASS؛ baseline، Version، منع هر دو self-review، حذف شرطی؛ T01–T09 و T14 |
| III — هویت/مجوز | PASS | PASS؛ R-03، snapshot شروع، مشاهدهٔ محدود و رسید replay؛ T10–T13 |
| IV — یکپارچگی | PASS | PASS؛ R-02/R-04، BEGIN IMMEDIATE، state+ledger اتمیک؛ T06، T08–T09، T14–T18 |
| V — ردیابی | PASS | PASS؛ جدول FR زیر و ردیابی وضعیت A-03؛ تصمیم جدید محصولی پنهان نشده |
| VI — شواهد | PASS | PASS طراحی؛ T01–T06 حداقل تکلیف، رقابت و fault جدا؛ آزمون اجراشده ادعا نمی‌شود |
| VII — فرایند | PASS | PASS؛ توقف پس از طراحی؛ Analyze خواندنی، Converge پس از Implementation و بازآزمون اصلاحات |
| VIII — دامنه | PASS | PASS؛ فقط HTTP؛ maintenance محلی برای setup، نه endpoint محصول؛ بدون AI/UI |

Gate پس از Phase 1: PASS در سطح طراحی برای محیط آموزشی. استثنای حل‌نشدهٔ امنیتی/معماری وجود ندارد.
محدودیت‌های محیط بسته، تک‌نمونه و رشد ledger در TB-002/TB-005/TB-007 ثبت و در Research توجیه شده‌اند؛ production readiness ادعا نمی‌شود.
A-03 با دو پاسخ گزینهٔ A در [Clarifications مورخ 2026-11-13](spec.md#clarifications) تعیین تکلیف شده است؛
شمارش طول و عنوان نامعتبر تابع FR-017 و US1.11–12 هستند، نه فرض باز.

## Project Structure

### Documentation (this feature)

```text
specs/001-manage-change-requests/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/http-api.md
├── checklists/requirements.md
└── tasks.md
```

`tasks.md` اکنون فهرست کارهای تولیدشدهٔ همین قابلیت است؛ بازبودن کارها به معنی اجراشدن آن‌ها نیست.

### Source Code (repository root)

ساختار زیر طرح مرحلهٔ طراحی بود؛ اکنون پیاده‌سازی شده و شواهد اجرای آن در گزارش پروژه آمده است:

```text
ChangeFlow.slnx
README.md
global.json
src/
├── ChangeFlow.Core/
│   ├── ChangeFlow.Core.csproj
│   ├── ChangeRequest.cs
│   ├── ActorContext.cs
│   └── ChangeRequestRules.cs
└── ChangeFlow.Api/
    ├── ChangeFlow.Api.csproj
    ├── Program.cs
    ├── Endpoints/
    ├── Application/
    ├── Authentication/
    ├── Persistence/
    └── Maintenance/
tests/ChangeFlow.Tests/
├── ChangeFlow.Tests.csproj
├── Core/
├── Api/
├── Persistence/
└── Fixtures/
artifacts/validation/
```

**Structure Decision**: درخت بالا کاربرد [TB-003](../../docs/architecture/technical-baseline.md#tb-003)
در همین قابلیت است؛ جهت وابستگی، مسئولیت لایه‌ها و seam آزمون در مبنا مرجع اصلی دارند.

## Complexity Tracking

نقضی برای توجیه وجود ندارد. دو پروژه برای اثبات استقلال Domain؛ SQL صریح برای atomicity؛
baseline برای Q4 و ledger بدون TTL برای Q9 ضروری‌اند. maintenance صریح محلی برای seed/لغو آزمون
دامنهٔ حساب کاربری عمومی ایجاد نمی‌کند. انتزاع تازه فقط در صورت نیاز مستند افزوده شود.

## Execution Design

متن زیر انتقال و یکپارچه‌سازی الگوریتم قبلی R-04 و خلاصهٔ همین Plan است، نه انتخاب الگوریتم تازه.
انتخاب‌های طراحی قابلیت در TB-004/TB-005/TB-007؛ schema در `data-model.md`؛ شکل پیام و ترتیب خطا در قرارداد.
زمان اثر لغو از FR-008، سیاست مشاهده از FR-014/FR-016 و مدت تضمین replay از FR-018 خوانده شود.

### الگوریتم تراکنش قابلیت

این مسیر برای ایجاد، ویرایش، فرمان‌های وضعیت و حذف استفاده می‌شود:

1. snapshot معتبر هویت/نقش شروع گرفته شود؛ reader کوتاه تمام و اتصال آزاد شود.
   نقش لازم و شکل ورودی/headerها کنترل شوند؛ ActorContext در همان عملیات تغییر نکند.
   مجوز بعدی جای snapshot شروع را نمی‌گیرد، اما وضعیت/مالکیت/نسخه داخل transaction بررسی می‌شوند.
2. canonical input، operation kind، target و expectedVersion تولید و fingerprint محاسبه شود؛
   تفکیک parse/validation و نام فیلدها همان قرارداد HTTP است.
3. BEGIN IMMEDIATE و lookup کلید `(ActorId, OperationKind, IdempotencyKey)` پیش از وضعیت زنده.
4. اگر موجود است، mismatch ورودی/هدف/نسخه → پاسخ mismatch موجود در قرارداد؛
   match → نتیجهٔ قبلی بدون DML دامنه. replay انتقال تازه نیست؛ قالب رسید کمینهٔ قرارداد
   محدودیت محتوایی FR-018 برای Reviewer غیرمالک را حفظ می‌کند.
5. اگر موجود نیست، درخواست و Version فعلی داخل transaction خوانده شود؛ دسترسی منبع، مالکیت،
   وضعیت، baseline اصلاح و Version بررسی شوند. جزئیات اولویت خطا همان قرارداد است.
6. mutation با شرط `Id AND Version=expectedVersion` و افزایش Version؛ برای حذف نیز شرط نسخه.
   تغییر دامنه و result immutable در همان transaction commit شوند؛ فقط پس از commit موفق پاسخ موفق ارسال شود.
7. خطای قطعی کاربردیِ قابل canonicalize، مانند وضعیت نامعتبر، نسخهٔ قدیمی یا validation قابل نگاشت،
   باید terminal result بدون تغییر درخواست در همان transaction شود؛ تکرار همان key همان خطا را بدهد.
   خطای auth، JSON خراب، header ناقص یا key نامعتبر پیش از admission کلید رزرو نمی‌کند؛
   خطاهای موقت busy/I/O terminal result نیستند. فهرست پاسخ‌های قطعی و استثناهای key موجود در قرارداد است.

هیچ رکورد durable InProgress قبل از اثر دامنه لازم نیست: رقیب روی قفل DB منتظر می‌ماند و
پس از پایان رقیب نتیجهٔ committed را می‌خواند یا timeout موقت می‌گیرد.
GET/list predicateهای مجوز را پیش از materialize/page اعمال کنند؛ mutation receipt فقط id/result.
قواعد ساختاری/مقایسهٔ baseline و مقادیر canonical در مدل، و معناهای محصولی در FR-004/FR-017 هستند.

### بازیابی خطا و نتیجهٔ نامعلوم

- در busy هنگام commit، فقط commit همان transaction retry یا rollback شود؛ DML دوباره داخل همان transaction اجرا نشود.
- در commit مبهم، اتصال/transaction قبلی تعیین تکلیف و بسته شود؛ اتصال تازه با writer reservation
  وضعیت همان key را reconcile کند. result موجود، نتیجهٔ committed است و همان پاسخ بازگردد.
- اگر result نبود و rollback قطعی شد، retry با همان key و کنترل قواعد مجاز است؛
  اگر DB قابل خواندن یا قطعیت نتیجه قابل احراز نیست، پاسخ موقت قرارداد با حفظ همان key برای retry داده شود.
- موفقیت ساختگی، اجرای کورکورانه با key جدید و عملیات جبرانی حدسی ممنوع‌اند؛ restart باید همان ledger پایدار را استفاده کند.
- قالب خطا، status/code و Retry-After در قرارداد باقی‌اند؛ قواعد log/backup و ثبت PRAGMA در
  [TB-009](../../docs/architecture/technical-baseline.md#tb-009) برای همین قابلیت ثبت شده‌اند.

## Requirement Traceability

Tها شناسهٔ گروه سناریو هستند و شمار test اجراشده نیستند؛ تعریف آن‌ها در quickstart است.

| نیاز | طراحی | بررسی |
| --- | --- | --- |
| FR-001 | ChangeRequest/Create | T01 |
| FR-002 | هویت R-03 | T10 |
| FR-003 | Core، PUT replacement | T07 |
| FR-004 | baseline آخرین بازگشت | T02، T07 |
| FR-005 | استقلال و نسخهٔ بررسی‌شده | T03، T04 |
| FR-006 | استقلال RequestChanges | T07، T11 |
| FR-007 | Approved immutable | T05 |
| FR-008 | snapshot مجوز شروع | T10، T12 |
| FR-009 | رد بدون mutation | T05، T10، T15 |
| FR-010 | ماتریس انتقال | T07 |
| FR-011 | ETag/Version/412 | T06 |
| FR-012 | BEGIN IMMEDIATE وconditional write | T08، T09، T14 |
| FR-013 | rollback/recovery R-04 | T15، T16، T18 |
| FR-014 |404 عمومی یکسان | T13 |
| FR-015 | ویرایش/Approve روی نسخهٔ فعلی | T08 |
| FR-016 | predicate مشاهده/receipt | T13 |
| FR-017 | Unicode scalar وnull canonical | T07 |
| FR-018 | ledger پایدار بدون TTL | T09، T16، T17، T18 |
| FR-019 | حذف فیزیکی شرطی | T14، T17 |

## Phase Results and Validation Evidence

- Phase 0: research.md و R-01 تا R-06 کامل؛ هیچ unknown مسدودکننده باقی نمانده است.
- Phase 1: data-model.md، contracts/http-api.md و quickstart.md ایجاد شدند؛ Gate پس از طراحی PASS.
- فرمان واقعی setup: `powershell -NoProfile -ExecutionPolicy Bypass -File .specify/scripts/powershell/setup-plan.ps1 -Json`؛
  نتیجه موفق و مسیر Spec/Plan و شناسهٔ منطقی feature برگشت.
- فرمان واقعی محیط: `dotnet --info`؛ موفق، SDK10.0.403 و runtime10.0.14 مشاهده شدند.
- بررسی واقعی با PowerShell: وجود پنج artifact، توازن code fence، نبود جای‌نگهدار، لینک‌های محلی،
  نگاشت هر۱۹ نیازمندی FR، ارجاع گروه‌های آزمون و parse مثال‌های JSON قرارداد؛ نتیجه PASS، خطای شناسایی‌شده صفر.
- restore/Build/Test محصول اجرا نشده‌اند؛ تعداد آزمون کاربردی **۰**؛ Quickstart دستور آینده است.
- Constitution همچنان draft و تاریخ تصویب معلق؛ گزارش واقعی پذیرش انسانی وجود ندارد.
- فایل extensions.yml قبل و بعد از طراحی بررسی شد و موجود نبود؛ hook ثبت‌شده‌ای برای اجرا وجود ندارد.

<a id="refactor-2026-11-13"></a>
## سوابق بازآرایی 2026-11-13

- انتخاب‌های طراحی قابلیت با شناسه‌های TB-001 تا TB-009 به مبنای فنی منتقل شدند؛
  دامنهٔ سراسری آن‌ها تأیید نشده است؛
  R-01 تا R-06 برای پژوهش پشتیبان حفظ شدند. Gateهای PASS بالا نتیجهٔ طراحی اولیه‌اند، نه آزمون تازه.
- الگوریتم مرحله‌ای و recovery از Research به Execution Design همین سند منتقل شد؛
  مدل و قرارداد قبلی پیش از انتقال بررسی شدند و جزئیات موجود آن‌ها دوباره کپی نشد.
- Spec مرجع سیاست محصولی باقی است و هیچ نیازمندی یا پاسخ کاربر در این بازآرایی تغییر نکرده است.
- تاریخ‌ها و نسخه‌های قبلی حفظ‌اند؛ مشاهدهٔ تاریخی SDK/runtime و patch مرجع در جدول مبنا ثبت است؛
  این بازآرایی بررسی تازهٔ نسخهٔ نصب‌شده یا آخرین servicing نیست.
- یادداشت‌های قدیمیِ هماهنگ‌سازی Constitution و فهرست ابهام‌های AGENTS به وضعیت جاری ارجاع می‌دهند؛
  پاسخ‌های ثبت‌شده نباید دوباره باز معرفی شوند.
- A-03 پس از دو پاسخ واقعی Clarify تعیین تکلیف شد؛ restore، pin/scan نهایی، سازگاری اجرایی و آزمون‌ها
  همچنان شاهد آینده می‌خواهند؛ هیچ انتخاب فنی سراسری یا سیاست تازه‌ای در همگام‌سازی اتخاذ نشده است.

در اصلاح ترتیب Tasks، GET تکی موجود در قرارداد در US1 و پیش از checkpoint MVP قرار می‌گیرد؛
checkpoint زیرساخت، replay/rollback را مستقیم در سرویس تراکنش با SQLite واقعی بررسی می‌کند،
و بررسی HTTP پس از ساخت endpointها انجام می‌شود. این اصلاح ترتیب، قرارداد یا معماری تازه‌ای نیست.

گام بازبینی بعدی `$speckit-analyze` است؛ Plan مجوز اجرای خودکار Implementation نیست.

