---
description: "کارهای پیاده‌سازی قابلیت مدیریت درخواست تغییر ChangeFlow"
---

# Tasks: ChangeFlow — مدیریت درخواست تغییر

**Input**: اسناد قابلیت در `specs/001-manage-change-requests/`؛ شناسایی از خروجی واقعی setup-tasks.
**تاریخ تدوین:** 2026-11-13.
**Prerequisites**: [Plan](plan.md)، [Spec](spec.md)، [Research](research.md)، [data-model](data-model.md)، [contracts](contracts/http-api.md) و [Quickstart](quickstart.md).
**حاکمیت:** [Constitution](../../.specify/memory/constitution.md) نسخهٔ مصوب `2.0.2`، تاریخ تصویب 2026-11-14؛
نسخهٔ خوانده‌شده در زمان تدوین Tasks، `2.0.2-draft.1` بود؛ اصول با تصویب تغییر نکردند.
**دامنهٔ فنی:** [Technical Baseline](../../docs/architecture/technical-baseline.md)؛ تنها TB-000 (.NET بدون نسخه) دارای شاهد تأیید سراسری است.
TB-001 تا TB-009 انتخاب‌های طراحی همین قابلیت‌اند، نه الزام سراسری یا تصمیم تازه در Tasks.
**وضعیت اجرا — 2026-11-14:** هر۶۶ Task تکمیل شده‌اند؛ در زمان تولید اولیه، هر۶۰ Task باز بودند.
شواهد اجرای واقعی و۱۱۲ آزمون موفق در [گزارش پروژه](../../artifacts/validation/project-report.md)
و [اعتبارسنجی انتشار](../../artifacts/validation/publication-2026-11-14.md) است؛
پذیرش نسخهٔ تحویل جداگانه در [ثبت تأیید](../../docs/reviews/acceptance-2026-11-14.md) ثبت شده است.
علامت تکمیل Task به‌تنهایی شاهد آزمون یا پذیرش نیست.

**Tests**: آزمون‌ها طبق Spec بخش Verification Obligations و Constitution اصل VI الزام دارند؛
شش حداقل تکلیف و تعهد اضافهٔ رقابت واقعی/Persistence در گزارش‌ها تفکیک شوند.
شناسهٔ `T001` به بعد در این سند Task است؛ `T01` تا `T18` در Quickstart فقط گروه سناریو هستند.

## Format: `[ID] [P?] [Story] Description`

- هر کار: checkbox باز، شناسهٔ ترتیبی، برچسب داستان فقط در فاز داستان و مسیر دقیق فایل.
- [P] فقط برای نوشتن آزمون‌های مستقل در فایل‌های متفاوت و پس از تکمیل پیش‌نیازهاست؛ اجرای موفق آزمون به پیاده‌سازی مرتبط وابسته است.
- در هر داستان، آزمون‌ها پیش از رفتار نوشته و شکست رفتاری مرتبط دیده شود؛ شکست build/fixture جای شاهد شکست رفتار نیست.
- بعد از هر بخش مرتبط، آزمون اجرا و اصلاحات بازآزمون شوند؛ آزمون حساس علاوه بر HTTP، DB را با اتصال مستقل بررسی کند.

## Path Conventions

مسیرهای کد از ریشهٔ Repository هستند: `src/ChangeFlow.Core/`، `src/ChangeFlow.Api/` و `tests/ChangeFlow.Tests/`.
مسیرهای فایل‌های جدیدِ داخل دایرکتوری‌های Plan، تفکیک اجرایی همان مسئولیت‌ها هستند؛ پروژه، سرویس یا لایهٔ محصول تازه اضافه نمی‌شود.
`artifacts/validation/` خروجی شواهد اجرای آینده است؛ گزارش‌ها باید فرمان، exit code، تعداد واقعی و علت اجرا‌نشدن را ثبت کنند.

## پیش‌نیازهای باز و تفاوت‌های تاریخی منابع

- A-03 با دو پاسخ گزینهٔ A در Clarifications مورخ 2026-11-13 تعیین تکلیف شده است؛ FR-017 و US1.11–12 مرجع‌اند.
  T001 فقط ثبت شواهد و بررسی اثر همان پاسخ‌هاست؛ پرسش تکراری یا gate انتظار پاسخ وجود ندارد.
- Q1–Q9 و پاسخ‌های ثبت‌شدهٔ Clarifications تعیین تکلیف شده‌اند؛ ارجاع‌های نسخه و دامنهٔ قابلیت در اسناد همگام‌اند.
- تولید این فهرست، تصویب Constitution، تعمیم انتخاب‌های قابلیت یا مجوز اجرای خودکار Implement/Converge نیست.
- پیش از شروع پیاده‌سازی، `$speckit-analyze` جداگانه اجرا شود؛ موانع مؤثر باید در گام اصلاح مجاز رفع شوند.

## Phase 1: Setup — آماده‌سازی

**Goal / Purpose**: آماده‌سازی بستر و ثبت پیش‌نیازها؛ هیچ‌یک در زمان تولید این سند اجرا نشده‌اند.

**Independent Test / Checkpoint**: ساختار و وابستگی‌ها قابل restore/build باشند؛ خروجی واقعی ثبت شود.

### کارهای آماده‌سازی

- [X] T001 شواهد دو پاسخ گزینهٔ A دربارهٔ A-03 را از Clarifications مورخ 2026-11-13 در `specs/001-manage-change-requests/spec.md` بخوان و تطبیق FR-017/US1.11–12 با مدل، قرارداد و آزمون‌های برنامه‌ریزی‌شده را در `artifacts/validation/decision-review.md` ثبت کن؛ شمارش Unicode scalar و رد عنوانِ فقط شامل فاصله، تب یا خط جدید پاسخ دارند و دوباره پرسیده نشوند. این Task گزارش آینده است و از قبل کامل محسوب نمی‌شود؛ draft Constitution و دامنهٔ قابلیت TB-001 تا TB-009 حفظ شود.
- [X] T002 ساختار موجود در Plan را در `ChangeFlow.slnx`، `src/ChangeFlow.Core/ChangeFlow.Core.csproj`، `src/ChangeFlow.Api/ChangeFlow.Api.csproj` و `tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj` بساز؛ Core فقط BCL، Api→Core و Tests→Api/Core؛ آزمون استقلال دامنه نباید وابستگی HTTP/SQL به Core اضافه کند.
- [X] T003 نسخه‌های طراحی TB-001/TB-008 را برای همین قابلیت در `global.json` و سه فایل `.csproj` مرحلهٔ قبل اعمال کن؛ C#/target، Microsoft.Data.Sqlite، xunit.v3، MTP و Mvc.Testing را از جدول تاریخ‌دار Baseline بخوان؛ SDK rollForward=latestPatch و test.runner=Microsoft.Testing.Platform. ابتدا وضعیت واقعی نسخه و سازگاری را بررسی و اختلاف نیازمند تغییر تصمیم را گزارش کن؛ نسخهٔ تاریخی را جدیدترین معرفی نکن.
- [X] T004 restore نخست، تولید lock فایل `src/ChangeFlow.Api/packages.lock.json`، `src/ChangeFlow.Core/packages.lock.json` و `tests/ChangeFlow.Tests/packages.lock.json`، بررسی transitive/native و آسیب‌پذیری و سپس locked restore/build را انجام بده؛ خروجی واقعی در `artifacts/validation/setup.md` و حفاظت `.local/`، DB و secrets در `.gitignore`؛ مشکل restore/scan را موفق گزارش نکن.

---

## Phase 2: Foundational — زیرساخت مسدودکننده

**Goal / Purpose**: پایهٔ هویت، ذخیره‌سازی، قرارداد و آزمون پیش از همهٔ داستان‌ها.

**Independent Test / Checkpoint**: handler واقعی و replay/rollback پایه مستقیماً در سطح handler و سرویس تراکنش با SQLite فایل واقعی و اتصال مستقل آزموده شوند؛ به endpointهای mutation که هنوز ساخته نشده‌اند وابسته نباشند. بررسی HTTP پس از ساخت endpointها در US1 و مسیرهای شکست US5 انجام می‌شود.

### زیرساخت مشترک همین قابلیت

- [X] T005 ActorContext را در `src/ChangeFlow.Core/ActorContext.cs` immutable برای هر عملیات بساز و رکوردهای هویت را در `src/ChangeFlow.Api/Persistence/IdentityRecords.cs` تعریف کن؛ قیود عین data-model: `User`: «Id TEXT PK، Enabled INTEGER، RolesMask INTEGER، AuthorizationVersion INTEGER | نقش‌ها بیت‌های Requester و Reviewer؛ snapshot یک query از ردیف کاربر»؛ `AccessToken`: «TokenHash BLOB PK، UserId FK، Enabled INTEGER | hash32byte؛ توکن خام فقط در خروجی امن seed محلی»؛ RolesMask/Enabled در maintenance با افزایش AuthorizationVersion تغییر کنند؛ User حذف نشود (FR-002، FR-008).
- [X] T006 schema نسخه‌دار و راه‌اندازی بدون reset را در `src/ChangeFlow.Api/Persistence/Schema.sql` و `SchemaInitializer.cs` پیاده کن؛ همهٔ ستون‌ها و قیود data-model، PK درخواست، indexهای `(RequesterId, Id)` و `(Status, Id)`، PK مرکب ledger، FK User با RESTRICT، نبود FK حذفی ledger به ChangeRequest، CHECK وضعیت/Version/hash/role و عدم TTL را حفظ کن؛ اعتبارسنجی طول Unicode در Core است (FR-009، FR-011، FR-018، FR-019).
- [X] T007 اتصال مستقل عملیات، SQL پارامتری، pool و reader کوتاه را در `src/ChangeFlow.Api/Persistence/SqliteConnectionFactory.cs` بساز؛ DELETE/EXTRA، foreign_keys=ON، انتظار قفل حداکثر۵ثانیه، readback PRAGMA و مانع readiness برای تنظیم نامعتبر مطابق TB-004؛ connection/command مشترک بین عملیات نباشد؛ native sqlite_version ثبت شود.
- [X] T008 در `src/ChangeFlow.Api/Authentication/OpaqueBearerHandler.cs` توکن را با hash SHA-256 به query واحد user/token/roles فعلی نگاشت و ActorContext همان عملیات را منتشر کن؛ snapshot برای GET/DELETE/replay هم تازه، بدون cache بین درخواست‌ها و بدون بازخوانی نقش وسط عملیات؛ DB failure→503 و invalid token→401؛ role/userId ورودی معتبر نیست (FR-002، FR-008).
- [X] T009 حالت‌های غیرHTTP seed و revoke-role در `src/ChangeFlow.Api/Maintenance/DevelopmentIdentityCommands.cs` و dispatch در `src/ChangeFlow.Api/Program.cs` را طبق Quickstart بساز؛ فقط Development و دسترسی فایل محلی، چهار هویت alice/bob/carol/dave، توکن تصادفی۳۲بایتی فقط یک‌بار در خروجی امن؛ عدم overwrite/reset ضمنی و عدم seed/re-enable در startup عادی؛ هیچ public admin/login endpoint نساز.
- [X] T010 در `src/ChangeFlow.Api/Endpoints/HttpContract.cs` و `src/ChangeFlow.Api/Program.cs` JSON camelCase، strict property/type/duplicate/surrogate، سقف۱۲۸KiB، Content-Type، headerهای اجباری، ETag قوی/If-Match مثبت int64، key ASCII۱..۱۲۸ case-sensitive و Cache-Control:no-store را پیاده کن؛ status/code/Problem و ترتیب خطا دقیقاً قرارداد، بدون افشای SQL/stack/token (FR-009، FR-011، FR-014، FR-018).
- [X] T011 رکورد و canonical fingerprint را در `src/ChangeFlow.Api/Persistence/OperationResult.cs` و `OperationFingerprint.cs` پیاده کن؛ قیود عین data-model: `ActorId`: «UUID / TEXT FK User | identity درخواست پذیرفته‌شده»؛ `OperationKind`: «TEXT enum | Create، ReplaceContent، Submit، Approve، RequestChanges، Delete»؛ `IdempotencyKey`: «TEXT binary comparison | ASCII بدون whitespace، طول۱..۱۲۸؛ ترکیب سه فیلد اول PK»؛ `Fingerprint`: «BLOB SHA-256 | canonical payload شامل target، expectedVersion و محتوای ورودی»؛ `TargetId`: «UUID / TEXT | در Create، Id تولیدشده؛ در fingerprint create هدف null است»؛ `HttpStatus`: «INTEGER | پاسخ terminal قبلی؛ replay همان status را حفظ می‌کند»؛ `ResultCode`: «TEXT | Created، Updated، Submitted، Approved، ChangesRequested، Deleted یا code خطای قطعی»؛ `ResponseJson`: «TEXT | فقط receipt یا Problem عمومی و بدون محتوای درخواست/توکن/stack trace»؛ `Location`: «TEXT NULL | فقط Create؛ replay همان مسیر، حتی اگر درخواست بعداً حذف شده باشد»؛ payload پیش از validation محتوایی و پس از parse نوع داده canonical شود؛ غایب/null/خالی اختیاری یکسان و expectedVersion/target جزء fingerprint باشند؛ PK عامل/عملیات/key و عدم overwrite نتیجه حفظ شود (FR-018).
- [X] T012 در `src/ChangeFlow.Api/Persistence/MutationExecutor.cs` و `OperationResultStore.cs` مرز BEGIN IMMEDIATE non-deferred serializable بساز؛ lookup ledger قبل از منبع زنده، mismatch بدون overwrite، replay بدون DML، بررسی نسخه و اثر+terminal result در یک transaction، rollback در شکست پیش از commit؛ syntax/header/auth قبل admission ledger نسازند، خطای قطعی کاربردی ثبت و transient poison نشود؛ فقط پس از قطعیت commit موفقیت بده؛ تا تکمیل recovery در داستان US5 نتیجهٔ نامعلوم موفق گزارش نشود (FR-009، FR-011 تا FR-013، FR-018).
- [X] T013 در `tests/ChangeFlow.Tests/Fixtures/ChangeFlowFactory.cs`، `SqliteFixture.cs` و `OperationGates.cs` test host با handler/توکن واقعی، DB فایل مجزای fixture، seed مستقیم چهار وضعیت و ledger، بازخوانی مستقل DB و gateهای TaskCompletionSource/Barrier بساز؛ نقاط fault و قطع پاسخ فقط seam تزریق‌شده از DI تست باشند، بدون endpoint یا تنظیم production؛ timeout محدود و بدون Sleep.
- [X] T014 آزمون‌های startup/PRAGMA، تداوم schema، FK/ledger، replay/rollback پایه با فراخوانی مستقیم MutationExecutor، invalid-token در برابر DB-unavailable با handler واقعی، منع seed دوباره و عدم حضور fault seam در host عادی را در `tests/ChangeFlow.Tests/Persistence/FoundationTests.cs` و `tests/ChangeFlow.Tests/Api/AuthenticationTests.cs` بنویس و اجرا کن؛ داده را از اتصال مستقل بررسی کن، بدون نیاز به endpointهای آیندهٔ محصول؛ خروجی و تعداد واقعی در `artifacts/validation/foundation.md`؛ مانع زیرساختی پیش از داستان‌ها رفع شود.

---

## Phase 3: User Story 1 — ایجاد و آماده‌سازی درخواست (P1) — MVP

**Goal / Purpose**: ایجاد Draft و replacement محتوای درخواست توسط مالک.

**Independent Test / Checkpoint**: با هویت واقعی Requester، POST و PUT را اجرا و Draft، مالک، محتوا، Version و ledger را با اتصال مستقل بررسی کن؛ ایجاد/بررسی سایر داستان‌ها لازم نیست.

### آزمون‌های US1 — پیش از پیاده‌سازی رفتار

- [X] T015 [P] [US1] آزمون قرارداد POST→GET→PUT در `tests/ChangeFlow.Tests/Api/ContentContractTests.cs` برای US1.1–12، T01/T07/T10 از Quickstart بنویس؛ Create201/Location، GET200/View/ETag و PUT با همان ETag و پاسخ200 receipt؛ جعل مالک/status، غیرمالک، عنوان غایب یا فقط فاصله/تب/خط جدید، clear غایب/null/خالی و عدم تغییر جزئی، Unicode scalar و raw JSON/transport/header نامعتبر را پوشش بده؛ نسخهٔ client از GET خوانده شود، نه حدس یا دسترسی تست به DB (FR-001 تا FR-003، FR-009، FR-011، FR-014، FR-016، FR-017).
- [X] T016 [P] [US1] در `tests/ChangeFlow.Tests/Core/ContentValidationTests.cs` مرزهای دقیق طول۲۰۰/۵۰۰۰/۲۰۰۰ و یک بیشتر، optional/null، شمارش scalar خارج BMP به‌صورت یک واحد، حرف و علامت ترکیبیِ جدا هرکدام یک واحد و رد عنوان فقط فاصله/تب/خط جدید را طبق FR-017 و US1.11–12 آزمون کن؛ نبود trim/NFC طبق طراحی موجود مدل بررسی شود؛ پیش از منطق رفتار، شکست مربوط به انتظار دامنه را مشاهده کن.

### پیاده‌سازی و اعتبارسنجی US1

- [X] T017 [US1] در `src/ChangeFlow.Core/ChangeRequest.cs` مدل درخواست را بساز؛ قیود عین data-model: `Id`: «UUID / TEXT canonical lowercase | server-generated؛ هویت پایدار برای مسیر و receipt»؛ `Title`: «Unicode / TEXT NOT NULL | اجباری، غیرwhitespace، حداکثر۲۰۰ scalar»؛ `Description`: «Unicode nullable / TEXT NULL | غایب/null/خالی → null؛ حداکثر۵۰۰۰ scalar»؛ `Reason`: «Unicode nullable / TEXT NULL | غایب/null/خالی → null؛ حداکثر۲۰۰۰ scalar»؛ `RequesterId`: «UUID / TEXT FK User | فقط از ActorContext؛ مالک تغییرناپذیر»؛ `Status`: «enum / TEXT CHECK | فقط Draft، PendingReview، ChangesRequested، Approved»؛ `Version`: «int64 / INTEGER NOT NULL | شروع۱؛ هر ویرایش/انتقال موفق +۱؛ حذف با شرط نسخه»؛ `ReviewBaselineTitle`: «TEXT NULL | محتوای آخرین ورود به ChangesRequested برای FR-004»؛ `ReviewBaselineDescription`: «TEXT NULL | snapshot canonical؛ حفظ تفاوت null و مقدار واقعی»؛ `ReviewBaselineReason`: «TEXT NULL | مانند بالا؛ فیلد فنی لازم برای شرط ارسال مجدد»؛ در Draft baseline خالی و RequesterId تغییرناپذیر؛ سرریز Version مثبت int64 خطای امن بدون تغییر باشد (FR-001، FR-002، FR-011، FR-017).
- [X] T018 [US1] در `src/ChangeFlow.Core/ChangeRequestRules.cs` و `ContentValidation.cs` اعتبارسنجی اتمیک و replacement هر سه فیلد را طبق FR-003/FR-017 پیاده کن؛ نبود عنوان کل ویرایش را رد کند؛ شرح/دلیل غایب/null/خالی پاک شوند؛ هم‌محتوا هم Version را زیاد کند؛ برای A-03 پاسخ تازه اختراع نکن.
- [X] T019 [US1] Create/ReplaceContent را در `src/ChangeFlow.Api/Application/ContentCommands.cs` و `Persistence/ChangeRequestStore.cs` به Core، ActorContext و MutationExecutor وصل کن؛ مالک واقعی، Draft اولیه، شرط Id/Version، مجوز مالک در Draft/ChangesRequested/PendingReview، حفظ وضعیت و immutable Approved؛ نوشتن پارامتری و receipt/ledger بدون محتوا (FR-001 تا FR-003، FR-007، FR-009، FR-015، FR-018).
- [X] T020 [US1] POST و PUT را در `src/ChangeFlow.Api/Endpoints/ContentEndpoints.cs` و GET تکی `/api/change-requests/{id}` را در `src/ChangeFlow.Api/Endpoints/ReadEndpoints.cs` با `Application/ReadRequest.cs` و `Persistence/ChangeRequestQueries.cs` بساز و در `src/ChangeFlow.Api/Program.cs` ثبت کن؛ GET همان قرارداد موجود View+ETag و predicate کامل FR-016 در SQL را اجرا کند: مالک Requester همهٔ وضعیت‌ها، Reviewer فقط PendingReview دیگران و اجتماع دو نقش؛ ناموجود/فاقد مشاهده404 یکسان و بدون baseline/token/ledger. POST/PUT فقط parse/call/map، Location و receipt و ترتیب خطای قرارداد؛ replay همان status/body/Location. GET پیش از checkpoint US1 آماده باشد (FR-001، FR-003، FR-011، FR-014، FR-016، FR-018).
- [X] T021 [US1] آزمون‌های US1.1–12 از جمله جریان client واقعی POST→GET→PUT با ETag مشاهده‌شده را اجرا؛ state و ledger را برای تأیید نتیجه جداگانه از اتصال مستقل DB بخوان، نه برای تأمین Version ورودی client؛ شکست‌ها را در همان بخش اصلاح و بازآزمون کن؛ SC-001/SC-009 و شواهد و شمار واقعی در `artifacts/validation/us1.md`؛ MVP را تکمیل کل قابلیت یا پذیرش انسانی معرفی نکن.

---

## Phase 4: User Story 2 — ارسال درخواست برای بررسی (P1)

**Goal / Purpose**: Submit مالک از Draft با مجوز شروع معتبر.

**Independent Test / Checkpoint**: Draft آماده در fixture را Submit کن؛ وضعیت PendingReview، حفظ محتوا/مالکیت و Version+۱؛ مجوز پیش از snapshot و حین اجرا جدا بررسی شوند.

### آزمون‌های US2

- [X] T022 [P] [US2] در `tests/ChangeFlow.Tests/Api/SubmitContractTests.cs` Submit آمادهٔ Draft→PendingReview، نقش/مالک نامعتبر، Approved و انتقال نامجاز، header/stale و حفظ داده را طبق US2.1–3 و T02/T05/T06/T07 بنویس و شکست رفتار پیش از پیاده‌سازی را ثبت کن (FR-004، FR-007 تا FR-011).
- [X] T023 [P] [US2] در `tests/ChangeFlow.Tests/Api/AuthorizationSnapshotTests.cs` لغو پیش از شروع و پس از ActorContext را با gate و commit نقش در اتصال مستقل بیازما؛ عملیات جاری با سایر پیش‌شرط‌ها کامل و درخواست/replay بعدی رد شود، بدون Sleep؛ US2.4–5، Q6=A و T12 (FR-008).

### پیاده‌سازی و اعتبارسنجی US2

- [X] T024 [US2] قاعدهٔ Submit از Draft و منع ترکیب‌های نامجاز را در `src/ChangeFlow.Core/ChangeRequestRules.cs` با مالک+Requester شروع و Version پیاده کن؛ صحت مجوز نقش از snapshot و وضعیت/نسخه در مرز ثبت باشد؛ شاخهٔ ChangesRequested در US4 تکمیل می‌شود (FR-004، FR-008، FR-010).
- [X] T025 [US2] در `src/ChangeFlow.Api/Application/SubmitCommand.cs` و `Endpoints/SubmitEndpoints.cs` POST submit را به conditional store/MutationExecutor وصل کن؛ فقط تغییر وضعیت/Version، محتوا و مالک ثابت، terminal failure و replay طبق قرارداد (FR-004، FR-009، FR-011، FR-018).
- [X] T026 [US2] US2 و regression US1 را اجرا و پاسخ و دادهٔ پایدار را مقایسه کن؛ SC-002/SC-013 و گزارش واقعی در `artifacts/validation/us2.md`؛ شکست را پیش از عبور از checkpoint اصلاح و بازآزمون کن.

---

## Phase 5: User Story 3 — تصمیم مستقل Reviewer (P1)

**Goal / Purpose**: Approve/RequestChanges مستقل و ویرایش حین PendingReview با نسخهٔ معتبر.

**Independent Test / Checkpoint**: PendingReview آماده و دو هویت را از fixture بخوان؛ هر تصمیم مستقل، منع هر دو self-review، و رد تأیید نسخهٔ پیش از ویرایش را ثابت کن.

### آزمون‌های US3

- [X] T027 [P] [US3] در `tests/ChangeFlow.Tests/Api/ReviewContractTests.cs` Approve و RequestChanges، هر دو self-review برای carol، نقش/مالک و وضعیت نامجاز، receipt بدون محتوا و snapshot baseline را قبل از پیاده‌سازی بیازما؛ T03/T04/T07/T11 و US3.1–4/8 (FR-005، FR-006، FR-009، FR-010).
- [X] T028 [P] [US3] در `tests/ChangeFlow.Tests/Api/PendingReviewEditTests.cs` US3.5–7 را بیازما: ویرایش مالک PendingReview با حفظ وضعیت و Version+۱، Approve نسخهٔ قدیمی412، ویرایش Approved بدون اثر؛ T05/T06 (FR-003، FR-007، FR-011، FR-015).

### پیاده‌سازی و اعتبارسنجی US3

- [X] T029 [US3] در `src/ChangeFlow.Core/ChangeRequestRules.cs` Approve/RequestChanges فقط PendingReview و Reviewer غیرمالک را پیاده کن؛ self-review حتی دو نقشی ممنوع، Approved immutable؛ RequestChanges مقادیر baseline را از محتوای همان نسخه بگیرد؛ «عنوان baseline در ChangesRequested نباید null باشد.» (FR-005 تا FR-007، FR-010، FR-015).
- [X] T030 [US3] در `src/ChangeFlow.Api/Application/ReviewCommands.cs` و `Persistence/ChangeRequestStore.cs` conditional Approve/RequestChanges با Version بررسی‌شده و snapshot baseline هم‌تراکنش با ledger را متصل کن؛ هیچ snapshot حساس در receipt ذخیره نشود؛ 403/404/412/409 با اولویت قرارداد (FR-005، FR-006، FR-011، FR-015، FR-018).
- [X] T031 [US3] آزمون یکپارچهٔ مشاهدهٔ نسخهٔ قابل بررسی را در `tests/ChangeFlow.Tests/Api/ReviewVisibilityTests.cs` برای GET تکیِ ساخته‌شده در T020 تکمیل کن؛ PendingReview آماده، مالک/Reviewer/دو نقشی، View+ETag فعلی، عدم افشای baseline/token/ledger و404 عمومی پس از Approved برای Reviewer غیرمالک؛ دریافت ETag پیش از تصمیم بررسی از مسیر HTTP ثابت شود؛ endpoint تازه‌ای نساز (US3.9، FR-014، FR-016، SC-010).
- [X] T032 [US3] دو POST approve و request-changes را در `src/ChangeFlow.Api/Endpoints/ReviewEndpoints.cs` map و در `Program.cs` ثبت کن؛ If-Match اجباری، key و receipt کمینه؛ replay Approve پس از خروج از PendingReview با مجوز تازه فقط رسید قبلی، بدون محتوای درخواست (FR-005، FR-006، FR-018).
- [X] T033 [US3] US3 و regression US1/US2 را اجرا و استقلال Reviewer، baseline، نسخه و receipt را در DB مستقل بررسی کن؛ SC-003/SC-004/SC-008/SC-010، نتایج واقعی در `artifacts/validation/us3.md`؛ اصلاحات همان بخش را بازآزمون کن.

---

## Phase 6: User Story 5 — رقابت، شکست و تکرار پایدار (P1)

**Goal / Purpose**: اثبات نبود اثر دوباره، بازنویسی خاموش، ذخیرهٔ جزئی و موفقیت کاذب.

**Independent Test / Checkpoint**: با fixture آماده، عملیات هم‌پوشان و شکست پیش/پس از commit را ایجاد و پاسخ، Version، state و ledger را از اتصال مستقل بررسی کن؛ فقط آزمون ترتیبی کافی نیست.

### آزمون‌های US5 — شواهد اضافه بر حداقل تکلیف

- [X] T034 [P] [US5] در `tests/ChangeFlow.Tests/Persistence/ConcurrentMutationTests.cs` T06/T08/T09 را بنویس: stale مستقل، Edit/Approve و Approve/RequestChanges در هر دو ترتیب برنده، دو اتصال و دو HTTP attempt واقعاً هم‌پوشان، حداکثر یک اثر؛ TaskCompletionSource/Barrier و timeout محدود (FR-011، FR-012، FR-015).
- [X] T035 [P] [US5] در `tests/ChangeFlow.Tests/Persistence/AtomicRollbackTests.cs` T15 را بنویس: fault پس از mutation و پس از ledger قبل از commit؛ هیچ تغییر درخواست/Version یا ledger ناقص و هیچ موفقیت کاذب؛ خطای I/O مجزا از رد کاربردی (FR-009، FR-013).
- [X] T036 [P] [US5] در `tests/ChangeFlow.Tests/Api/IdempotencyContractTests.cs` key غایب/نامعتبر، mismatch payload/target/expectedVersion، scope مستقل actor/operation، replay ترتیبی/هم‌زمان، terminal error پایدار و transient بدون poison را برای mutationهای ساخته‌شده پوشش بده؛ T09/T18 و US5.5–10؛ نقش فعلی و رسید بی‌محتوا پس از Approve (FR-008، FR-016، FR-018).
- [X] T037 [P] [US5] در `tests/ChangeFlow.Tests/Persistence/CommitRecoveryTests.cs` T16/T18 را بنویس: busy در begin و commit، rollback قطعی، commit نامعلوم، قطع پاسخ/exception بعد از commit واقعی و DB غیرقابل خواندن؛ نتیجه با همان key reconcile، DML دوم ممنوع و عدم موفقیت تا قطعیت (FR-012، FR-013، FR-018).

### تکمیل مسیرهای شکست و اعتبارسنجی US5

- [X] T038 [US5] در `tests/ChangeFlow.Tests/Fixtures/MutationFaults.cs` و `ChangeFlowFactory.cs` تزریق نقاط پس از writer reservation، mutation، ledger و commit واقعی و قطع response را تکمیل کن؛ در `src/ChangeFlow.Api/Persistence/MutationExecutor.cs` فقط seam داخلی برای DI تست، بدون route یا تنظیم عمومی اضافه شود؛ برای T08/T09/T15/T16/T18 از Sleep و mock-only commit پرهیز کن.
- [X] T039 [US5] در `src/ChangeFlow.Api/Persistence/MutationExecutor.cs` مسیر دقیق Plan را تکمیل کن: busy commit فقط retry همان commit یا rollback، بستن/تعیین تکلیف اتصال قبلی و writer reservation تازه برای reconcile، result موجود→replay، نبود result با rollback قطعی→retry همان key، DB نامعلوم→503/Retry-After؛ نه key جدید، DML تکراری یا جبران حدسی (FR-013، FR-018).
- [X] T040 [US5] نگاشت busy/I/O/خطای غیرمنتظره و جلوگیری از رزرو/poison کلید در transient را در `src/ChangeFlow.Api/Endpoints/HttpContract.cs` و `Persistence/MutationExecutor.cs` نهایی کن؛ ترتیب admission، terminal validation/resource errors و عدم overwrite mismatch عین قرارداد؛ auth DB error نباید401 شود (FR-008، FR-009، FR-013، FR-018).
- [X] T041 [US5] در `tests/ChangeFlow.Tests/Persistence/DurableReplayTests.cs` restart host روی همان فایل و replay بعد از بیش از۲۴ساعت را بدون انتظار واقعی۲۴ساعته ثابت کن؛ نبود TTL/ExpiresAt/cleanup را بررسی کن و نتیجهٔ قبلی همان status/body/Location باشد؛ T17، replay پس از حذف در US6 اضافه می‌شود (FR-018).
- [X] T042 [US5] suiteهای US5 و regression داستان‌های P1 را اجرا؛ HTTP و state/Version/baseline/ledger از اتصال مستقل را برای هر interleaving ثبت و اصلاحات را بازآزمون کن؛ SC-005/SC-006/SC-007/SC-008/SC-012، شمار واقعی و تفکیک آزمون رقابت/Persistence اضافه بر شش حداقل در `artifacts/validation/us5.md`.

---

## Phase 7: User Story 4 — اصلاح و ارسال مجدد (P2)

**Goal / Purpose**: Submit دوباره فقط پس از تغییر مؤثر محتوای آخرین بازگشت.

**Independent Test / Checkpoint**: ChangesRequested آماده با baseline معلوم را بدون Approve/Create همین تست اصلاح و Submit کن؛ بی‌تغییر و تغییر سپس بازگرداندن رد، تغییر معتبر یک فیلد پذیرفته شود.

### آزمون‌های US4

- [X] T043 [P] [US4] در `tests/ChangeFlow.Tests/Core/ResubmissionRulesTests.cs` US4.1–4 و T07 را بیازما: baseline آخرین بازگشت، مقایسهٔ canonical، تغییر تنها یکی از عنوان/شرح/دلیل، clear اختیاری، تغییر سپس برگشت، هم‌محتوا با Version جدید؛ Version به‌تنهایی اصلاح نیست (FR-003، FR-004، FR-017).
- [X] T044 [P] [US4] در `tests/ChangeFlow.Tests/Api/ResubmitContractTests.cs` PUT و submit روی ChangesRequested آماده، CHANGES_REQUIRED409 بدون تغییر entity، replay خطای قطعی و key تازه پس از اصلاح، حفظ مالکیت/وضعیت در PUT و baseline تازهٔ بازگشت بعدی را بیازما (FR-003، FR-004، FR-009، FR-018).

### پیاده‌سازی و اعتبارسنجی US4

- [X] T045 [US4] در `src/ChangeFlow.Core/ChangeRequestRules.cs` شرط تفاوت حداقل یکی از سه مقدار canonical با ReviewBaseline را برای Submit از ChangesRequested پیاده کن؛ baseline آخرین RequestChanges مرجع است، نه تعداد ویرایش یا Version؛ متن trim/NFC نشود مگر پاسخ واقعی بازبینی A-03 اسناد را تغییر داده باشد (FR-004).
- [X] T046 [US4] در `src/ChangeFlow.Api/Application/SubmitCommand.cs` و `Persistence/ChangeRequestStore.cs` شاخهٔ ChangesRequested را داخل مرز اتمیک به baseline-rule وصل کن؛ POST موجود submit بدون endpoint تازه، baseline در هر RequestChanges تازه و عنوان آن non-null؛ خطا یا stale هیچ تغییر جزئی ندهد (FR-004، FR-009، FR-011، FR-018).
- [X] T047 [US4] US4 و regression چهار انتقال را اجرا، مقادیر canonical و baseline را مستقل بازخوانی و اصلاحات را بازآزمون کن؛ SC-002/SC-011، گزارش واقعی در `artifacts/validation/us4.md`؛ چرخهٔ کامل ارسال/برگشت/اصلاح/ارسال روی همان درخواست برقرار باشد.

---

## Phase 8: User Story 6 — مشاهده و حذف پیش‌نویس شخصی (P2)

**Goal / Purpose**: تکمیل GET/list مجاز و حذف فیزیکی Draft با سابقهٔ replay پایدار.

**Independent Test / Checkpoint**: چهار وضعیت آماده و نقش‌های مختلف را مستقل از جریان ایجاد/بررسی fixture کن؛ ماتریس GET/list، حذف مجاز/نامجاز، stale و رقابت حذف/Submit و replay پس از حذف را ثابت کن.

### آزمون‌های US6

- [X] T048 [P] [US6] در `tests/ChangeFlow.Tests/Api/VisibilityTests.cs` US6.1–3/T13 را برای GET تکی و list، مالک/Reviewer/دو نقشی/بدون نقش، عدم افشای existence در body/status/headers، حذف ردیف نامجاز پیش از pagination، cursor نامعتبر و صفحهٔ خالی بیازما (FR-008، FR-014، FR-016).
- [X] T049 [P] [US6] در `tests/ChangeFlow.Tests/Api/DeleteContractTests.cs` US6.4–5 و T14 را بنویس؛ DELETE فقط Draft مالک Requester، شرط Version/key، stale، غیرمالک و غیرDraft، receipt200 و عدم تغییر/حذف دیگران؛ snapshot و ledger مستقل بررسی شوند (FR-009، FR-011، FR-019).
- [X] T050 [P] [US6] در `tests/ChangeFlow.Tests/Persistence/DeleteConcurrencyTests.cs` US6.6–7 و T14/T17 را برای هر دو ترتیب واقعی Delete/Submit، دو اجرای هم‌کلید Delete، replay پس از restart/زمان/حذف و حفظ ledger بدون احیای entity بیازما (FR-012، FR-018، FR-019).

### پیاده‌سازی و اعتبارسنجی US6

- [X] T051 [US6] list را در `src/ChangeFlow.Api/Persistence/ChangeRequestQueries.cs`، `Application/ListRequests.cs` و `Endpoints/ReadEndpoints.cs` کامل کن؛ predicate همان GET پیش از page، Id صعودی، limit پیش‌فرض۵۰ و۱..۱۰۰، cursor base64url از آخرین Id، nextCursor فقط ردیف مجاز بیشتر، بدون total/filter/search/sort جدید؛ snapshot زمانی ادعا نشود (FR-014، FR-016).
- [X] T052 [US6] DeleteDraft را در `src/ChangeFlow.Core/ChangeRequestRules.cs`، `src/ChangeFlow.Api/Application/DeleteDraft.cs` و `Persistence/ChangeRequestStore.cs` پیاده کن؛ مالک/Requester شروع/Draft/Version، حذف فیزیکی شرطی و ثبت نتیجه در همان tx، بدون cascade ledger، بدون استفادهٔ دوباره از Id و بدون قابلیت احیا (FR-009، FR-011، FR-018، FR-019).
- [X] T053 [US6] DELETE `/api/change-requests/{id}` را در `src/ChangeFlow.Api/Endpoints/DeleteEndpoints.cs` و ثبت آن در `Program.cs` متصل کن؛ If-Match/key اجباری، receipt Deleted، lookup نتیجه قبل از منبع برای replay روی درخواست حذف‌شده؛ status/code دقیق قرارداد (FR-018، FR-019).
- [X] T054 [US6] در `tests/ChangeFlow.Tests/Api/PrivacyTests.cs` T13 را تکمیل کن؛ receipt و ledger و log نباید title/description/reason یا token/key خام افشا کنند؛ replay Approve غیرمالک پس از تغییر وضعیت فقط رسید، GET آن404؛ حذف/مشاهده با مجوز لغوشده رد شوند (FR-008، FR-014، FR-016، FR-018).
- [X] T055 [US6] US6 و regression همهٔ داستان‌ها را اجرا و HTTP، entity حذف‌شده و ledger باقی‌مانده را مستقل بررسی کن؛ SC-014/SC-015 و SC-012، اصلاحات و بازآزمون در `artifacts/validation/us6.md`؛ دو عملیات ناسازگار بر یک Version موفق نشوند.

---

## Phase 9: Polish & Cross-Cutting Concerns — تحویل و شواهد

**Goal / Purpose**: کنترل نهایی مرزها، قابلیت بازتولید و گزارش صادقانه؛ بدون افزودن محصول تازه.

**Independent Test / Checkpoint**: همهٔ سناریوهای انتخاب‌شده با خروجی واقعی و ردگیری نیازمندی‌ها بررسی شوند؛ پذیرش انسانی از سبزشدن تست استنتاج نشود.

### کارهای نهایی

- [X] T056 آزمون استقلال Core و عدم ثبت endpointهای maintenance/fault/login و عدم secret/content در log را در `tests/ChangeFlow.Tests/Core/ArchitectureTests.cs` و `Api/PrivacyTests.cs` کامل کن؛ structured logging با correlationId/operation/actor/outcome/elapsed در `src/ChangeFlow.Api/Program.cs` مطابق TB-009، بدون داشبورد/audit جدید.
- [X] T057 فرمان‌های واقعی Quickstart برای info، locked restore، Release build و MTP test را اجرا؛ native SQLite، PRAGMA، scan وابستگی‌ها، exit code و تعداد passed/failed/skipped و علت موارد اجرا‌نشده را در `artifacts/validation/final.md` ثبت کن؛ discovery صفر موفقیت نیست، نسخه‌ها بدون بررسی ارتقا نیابند؛ T01–T18 و شش حداقل در برابر تعهد رقابت/Persistence جدا گزارش شوند.
- [X] T058 در `README.md` اجرای واقعاً بررسی‌شده، seed/revoke محلی، حفاظت secrets، محدودیت تک‌نمونه/محیط بسته، backup آفلاین کل DB+ledger و نبود TTL را مستند کن؛ تطبیق مثال‌های `specs/001-manage-change-requests/quickstart.md` با فرمان‌های واقعی را گزارش کن و متن اجرا‌نشده را موفق معرفی نکن.
- [X] T059 پس از پیاده‌سازی و با اجرای جداگانهٔ مهارت Converge، گزارش شکاف نسبت به Spec/Plan/Tasks را در `artifacts/validation/converge.md` نگه دار؛ Converge فقط کارهای باقی‌مانده را به `specs/001-manage-change-requests/tasks.md` اضافه کند، کد را در آن مرحله تغییر نده؛ کار حاصل در گام Implement جدا و آزمون و ارزیابی مجدد شود.
- [X] T060 در `artifacts/validation/project-report.md` زنجیرهٔ FR/SC→Task→آزمون و گزارش واقعی Analyze پیش از Implement، Converge و Validation، موانع و شکاف‌های باز را جمع‌بندی کن؛ پذیرش انسانی فقط پس از ارزیابی واقعی مسئول با هویت/تاریخ/دامنه/نتیجه، و تا آن زمان صریحاً معلق؛ قابلیت پذیرش در API نساز؛ فهرست تحویل را با منبع تکلیف تطبیق بده و قلم فرضی نساز.

---

## Requirement Traceability

این نگاشت ارتباط نیازمندی و Task را نشان می‌دهد و با کارهای اصلاحی تکمیل شده است؛
خود جدول نتیجهٔ آزمون یا اجرای رسمی Analyze نیست. شواهد واقعی در گزارش پروژه ارجاع دارند.

| نیازمندی / معیار | Taskهای مرتبط |
| --- | --- |
| FR-001 | T017، T019، T020، T015، T021 |
| FR-002 | T005، T008، T019، T015 |
| FR-003 | T018، T019، T015، T028، T044 |
| FR-004 | T024، T025، T045، T046، T043، T044 |
| FR-005 | T029، T030، T032، T027 |
| FR-006 | T029، T030، T032، T027 |
| FR-007 | T019، T024، T029، T028 |
| FR-008 | T008، T023، T054، T014 |
| FR-009 | T012، T019، T035، T049 |
| FR-010 | T024، T029، T027، T022 |
| FR-011 | T012، T019، T028، T034، T049، T065، T066 |
| FR-012 | T012، T034، T036، T050 |
| FR-013 | T012، T035، T037، T039، T040 |
| FR-014 | T010، T020، T031، T051، T048، T054 |
| FR-015 | T019، T030، T028، T034 |
| FR-016 | T020، T031، T051، T048، T054، T036 |
| FR-017 | T017، T018، T016، T015، T043 |
| FR-018 | T011، T012، T036، T039، T041، T050 |
| FR-019 | T052، T053، T049، T050 |
| SC-001 | T015، T021 |
| SC-002 | T022، T026، T044، T047 |
| SC-003 | T027، T033 |
| SC-004 | T028، T033 |
| SC-005 | T034، T042، T065، T066 |
| SC-006 | T035، T037، T042 |
| SC-007 | T010، T042، T057 |
| SC-008 | T028، T034، T033، T042 |
| SC-009 | T016، T015، T021 |
| SC-010 | T027، T031، T048، T033 |
| SC-011 | T043، T044، T047 |
| SC-012 | T036، T041، T050، T042 |
| SC-013 | T023، T026 |
| SC-014 | T048، T054، T055 |
| SC-015 | T049، T050، T055 |

### نگاشت گروه‌های آزمون Quickstart

| گروه سناریو | Task آزمون |
| --- | --- |
| T01 | T015 |
| T02 | T022 |
| T03، T04 | T027 |
| T05 | T028 |
| T06 | T065؛ آزمون مستقیم StaleContentUpdateTests؛ T028 و T034 پوشش تکمیلی، T066 ثبت شواهد |
| T07 | T016، T027، T043، T044 |
| T08 | T034 |
| T09 | T034، T036 |
| T10 | T014، T015 |
| T11 | T027 |
| T12 | T023 |
| T13 | T048، T054 |
| T14 | T049، T050 |
| T15 | T035 |
| T16 | T037 |
| T17 | T041، T050 |
| T18 | T036، T037 |

## Dependencies & Execution Order

### Phase Dependencies

```text
Setup (T001–T004) → Foundational (T005–T014)
  └─ US1 (P1) → US2 (P1) → US3 (P1) → US5 (P1) → US4 (P2) → US6 (P2) → Polish
```

ترتیب اجرایی مبنا، شمارهٔ Taskها و فازهاست: Setup → Foundational → US1 → US2 → US3 → US5 → US4 → US6 → Polish.
A-03 پاسخ دارد؛ T001 فقط ثبت شواهد و بررسی اثر پاسخ است. T002→T003→T004 و سپس T005→T014 به‌ترتیب انجام شوند؛
پایه پیش از آزمون‌های هر داستان کامل و buildپذیر باشد.

### User Story Dependencies

| داستان | پیش‌نیاز پیاده‌سازی | استقلال آزمون |
| --- | --- | --- |
| US1 | Foundation و بررسی شواهد پاسخ ثبت‌شدهٔ A-03 | client از POST→GET→PUT و ETag واقعی استفاده کند؛ بدون Submit/Review |
| US2 | US1 برای Core/Store مشترک | Draft با seed مستقیم؛ بدون POST/Approve در همان تست |
| US3 | US1/US2 برای Rules/Store/Executor و GET تکی موجود | PendingReview آماده؛ مشاهدهٔ Reviewer و ETag در T031 آزموده می‌شود |
| US5 | US1/US2/US3 برای مسیر واقعی رقابت | state آماده؛ تست rollback/retry بدون چرخهٔ کسب‌وکار قبلی |
| US4 | US1/US2/US3/US5 برای replacement، Submit و baseline/recovery | ChangesRequested و baseline آماده؛ بدون اجرای Reviewer در همان تست |
| US6 | US1/US2/US3/US5؛ در ترتیب مبنا پس از US4 | چهار وضعیت آماده و ledger؛ Delete/Submit به مسیر Submit ساخته‌شده متکی است |

وابستگی پیاده‌سازی با استقلال تست یکسان نیست. fixtureهای مستقیم اجازه می‌دهند هر داستان جدا ارزیابی شود؛
وابستگی کد مشترک پنهان نشده است. فاز Polish پس از همهٔ داستان‌هاست؛ Converge تنها پس از پیاده‌سازی.

### Within Each User Story

آزمون‌های گروه اول → مدل/قواعد → Application/Persistence → endpoint/ثبت route → اجرای هدفمند و regression → checkpoint.
در US5 ابتدا تست‌ها تعریف و سپس seamهای کنترل/بازیابی تکمیل می‌شوند؛ شکست محیط/compile، شکست رفتار محسوب نشود.
کارهای روی `ChangeRequestRules.cs`، `ChangeRequestStore.cs`، `MutationExecutor.cs` و `Program.cs` ترتیبی‌اند.
پس از هر بخش مرتبط آزمون اجرا شود؛ Taskهای verify پایان داستان، جای آزمون حین کار را نمی‌گیرند.
`$speckit-analyze` پیش از Implement است؛ T059 پس از Implement اجرا می‌شود و هیچ نتیجهٔ موفق آینده از پیش ثبت نمی‌شود.

### Parallel Opportunities

پس از Foundation، فقط Taskهای [P] هر ردیف زیر می‌توانند برای نوشتن آزمون در فایل‌های جدا هم‌زمان شوند.
هر ردیف پس از checkpoint پیش‌نیاز داستان خود آغاز می‌شود؛ هیچ ردیف مجوز اجرای موازی مراحل وابسته نیست.
این مثال‌ها برنامهٔ کارند و در تولید Tasks عاملی موازی اجرا نشده است.

## Parallel Examples per User Story

| داستان | نمونهٔ کارهای موازی | شرط |
| --- | --- | --- |
| US1 | T015 + T016 | Foundation آماده؛ دو فایل تست مجزا |
| US2 | T022 + T023 | US1 آماده؛ SubmitContractTests و AuthorizationSnapshotTests مجزا |
| US3 | T027 + T028 | US2 آماده؛ ReviewContractTests و PendingReviewEditTests مجزا |
| US5 | T034 + T035 + T036 + T037 | US3 آماده؛ fixture یکسان ولی DB مستقل و فایل‌های تست مجزا |
| US4 | T043 + T044 | پیش‌نیازهای US4 کامل؛ Core و API tests مجزا |
| US6 | T048 + T049 + T050 | پیش‌نیازهای US6 کامل؛ فایل و DB مستقل |

## Implementation Strategy

### MVP First (User Story 1 Only)

1. یافته‌های Analyze را در گام اصلاحی رفع کن؛ پاسخ ثبت‌شدهٔ A-03 را بدون پرسش مجدد بخوان و Setup/Foundation را کامل کن.
2. T015 تا T021 را برای ایجاد و ویرایش Draft پیاده و مستقلاً بررسی کن.
3. در checkpoint US1 برای بازبینی توقف کن. این MVP آموزشی هنوز تحویل کل گردش یا اثبات کامل رقابت/recovery نیست.
4. نمایش یا انتشار خارج از درخواست فعلی مجوز خودکار ندارد؛ شواهد واقعی و محدودیت increment را گزارش کن.

### Incremental Delivery

پس از هر داستان، تست هدفمند، DB مستقل و regression داستان‌های قبلی بررسی شود.
پس از P1ها، اصلاح/ارسال مجدد و مشاهده/list/حذف P2 تکمیل شوند. ادغام در فایل‌های مشترک ترتیبی باشد.
برای تحویل کل قابلیت همهٔ ۱۹ FR و ۱۵ SC و گروه‌های T01–T18 شاهد لازم دارند؛ عدد Task، تعداد تست اجراشده نیست.

### Notes

- تعداد فعلی کارها:۶۶؛۶۰ کار اولیه (Setup=۴، Foundational=۱۰، US1=۷، US2=۵، US3=۷، US5=۹، US4=۵، US6=۸، Polish=۵)
  و۶ کار اصلاحی Converge در Phase10 و Phase11؛ همگی علامت تکمیل دارند.
- ۱۵ Task علامت [P] دارند؛ این نشانه مستقل‌بودن نوشتن تست‌ها در محدودهٔ مشخص است.
- در زمان تولید اولیه هیچ checkbox از قبل تکمیل نشده بود و آزمون محصول اجرا نشده بود؛
  وضعیت و شواهد اجرای فعلی از بخش وضعیت اجرا و گزارش‌های مرتبط خوانده شوند.
- خطای مؤثر یا نقض حل‌نشدهٔ امنیت/معماری مانع ادعای تکمیل است؛ گزارش پذیرش انسانی تا ارزیابی واقعی معلق بماند.

## Phase 10: Convergence

- [X] T061 برای F1، نقطهٔ آزمون پس از بازشدن اتصال دوم و پیش از writer reservation را در `MutationExecutor.cs` و `MutationFaults.cs` اضافه کن؛ gate فقط پس از مشاهدهٔ آن آزاد شود. رقابت‌های Edit/Approve، Approve/RequestChanges، هم‌کلید و Delete/Submit را با اتصال‌های هم‌پوشان بازآزمون و شواهد US5/US6 را تصحیح کن؛ FR-012، SC-005، T034/T050 (partial، HIGH).
- [X] T062 برای F2، پیکربندی اجرای API در `Program.cs` را با HTTPS لازم در TB-005 و Quickstart هماهنگ کن؛ پیش‌فرض امن و رد binding صریح HTTP پیش از شروع سرویس، بدون تغییر قرارداد محصول یا endpoint تازه. آزمون پیکربندی و اجرای HTTPS واقعی با گواهی localhost را ثبت کن؛ plan: TB-005، T058 (partial، HIGH).
- [X] T063 برای F3، آزمون‌های HTTP حد طول Unicode، replacement مقدار اختیاری، If-Match نامعتبر، سقف transport و Content-Type و ماتریس وضعیت/عملیات را تکمیل کن؛ رد باید داده و ledger را طبق قرارداد حفظ کند. سرریز Version نیز بدون تغییر بررسی شود؛ FR-009/010/011/017، SC-004/009، T015/T022/T027 (partial، MEDIUM).
- [X] T064 برای F4، پس از اصلاحات، T057/T058/T060 را با SDK/runtime servicing بررسی‌شده، locked restore/build/test و شمار واقعی، scan، seed/revoke و smoke مستقل تکمیل کن؛ گزارش‌های `final.md`، `converge.md` و `project-report.md` محدودیت شواهد Analyze قدیمی و پذیرش انسانی معلق را صریح نگه دارند. سپس Converge را دوباره اجرا کن؛ Constitution VI/VII/VIII (partial، MEDIUM).

## Phase 11: Convergence

- [X] T065 [US5] CRITICAL — برای یافتهٔ F5، آزمون یکپارچهٔ مستقیم جلوگیری از lost update را در `tests/ChangeFlow.Tests/Api/StaleContentUpdateTests.cs` اضافه و اجرا کن: مالکِ Requester یک Draft قابل ویرایش را از GET با ETag فعلی بخواند؛ PUT معتبر با key تازه محتوای عنوان/شرح/دلیل را تغییر دهد؛ GET بعدی افزایش Version و محتوای جدید را نشان دهد؛ سپس PUT با محتوای متفاوت، همان ETag قدیمی و یک key تازهٔ دیگر ارسال شود و دقیقاً412 با code=VERSION_CONFLICT بگیرد، نه409 مربوط به idempotency یا رد وضعیت/مجوز. با GET و اتصال مستقل DB، حفظ عنوان/شرح/دلیل جدید، مالکیت، وضعیت و Version را بررسی کن؛ ثبت terminal result در ledger و replay همان رد بدون اثر تازه نیز مطابق Plan باشد. آزمون هدفمند و regression را اجرا و فرمان، exit code و شمار واقعی را ثبت کن؛ اگر آزمون نقصی نشان داد، فقط همان نقص را در گام Implement اصلاح و بازآزمون کن. منبع: Constitution VI/سناریوی6 و اصلIV، FR-009/011/018، SC-005، US5.1، EC-05، Plan/Execution Design، T019/T034/T042 (partial).
- [X] T066 برای یافتهٔ F6، پس از اجرای واقعی T065، نگاشت سناریوی ششم/T06 و FR-011/SC-005 را در `artifacts/validation/project-report.md` و گزارش‌های `us5.md`، `final.md` و `converge.md` با ارجاع به آزمون مستقیم و خروجی اجرای تازه اصلاح کن؛ روشن کن که گزارش قبلی111 آزمون موفق، شاهد مستقیم stale PUT با key تازه نبود و نتیجهٔ قبلیِ صفر شکاف باید با این کشف تکمیل شود. سوابق واقعی اجراهای گذشته را حفظ کن؛ موفقیت آینده یا آزمون اجرا‌نشده نساز. نتیجه و شمار اجرای هدفمند/regression و شکاف‌های واقعاً باقی‌مانده را ثبت و سپس Converge را در گام جداگانه دوباره اجرا کن. منبع: Constitution VI/VII، FR-011، SC-005، T042/T057/T059/T060 (partial، MEDIUM).
