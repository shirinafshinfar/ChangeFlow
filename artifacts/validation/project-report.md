# گزارش پروژهٔ ChangeFlow — 2026-11-13

قابلیت: `001-manage-change-requests` در `E:/ai-oroumand/ChangeFlow`.
اجرای ثبت‌شدهٔ 2026-11-13 شامل۱۱۱/۱۱۱ آزمون موفق و Build بدون warning/error بود،
اما آزمون مستقیم stale PUT با key تازه را نداشت. تکمیل شواهد در 2026-11-14:
آزمون هدفمند۱/۱ و regression کامل۱۱۲/۱۱۲ موفق، صفر ناموفق/skipped و Build بدون warning/error.
پذیرش انسانی **معلق** است؛ هویت ارزیاب، تاریخ، دامنه و نتیجهٔ ارزیابی واقعی هنوز ارائه نشده‌اند.

## اقلام تحویل و منابع

به‌روزرسانی 2026-11-14: سابقهٔ واقعی Analyze پیش از Implement بازیابی و نقش Coding Agent
و خلاصهٔ صریح معماری در README اضافه شد. کاربر مخزن عمومی
[shirinafshinfar/ChangeFlow](https://github.com/shirinafshinfar/ChangeFlow) را تعیین کرد.
تلاش نخست مرورگر متوقف شده بود؛ در تلاش بعدی، مخزن از صفحهٔ GitHub در Edge ساخته و
visibility=public از API تأیید شد. origin محلی نیز تنظیم شد.
مخزن با README اولیه و commit ابتدایی ساخته شد. انتقال فایل‌های تحویل از طریق ابزار GitHub
در درخواست create-tree با403 و پیام `Resource not accessible by integration` رد شد؛
سورس و گزارش‌های پروژه هنوز به مخزن منتقل نشده‌اند. این محدودیت دسترسی، ساخت مخزن را
باطل نمی‌کند؛ انتشار کامل فایل‌ها تا رفع دسترسی یا استفاده از مسیر مجاز دیگر باز می‌ماند.

منبع صورت‌مسئلهٔ موجود، پیوست اول کاربر در پوشهٔ attachments با شناسه
`a12c2c64-a283-4959-9f1e-11ee2e42e065` است. دامنهٔ تحویل جاری با اصلاحات صریح بعدی کاربر
و اصل VIII Constitution تطبیق داده شد؛ قلم فرضی افزوده نشده است.

| قلم | مسیر |
| --- | --- |
| Artifactهای SDD | [پوشهٔ قابلیت](../../specs/001-manage-change-requests/spec.md)، [Constitution](../../.specify/memory/constitution.md)، [Baseline](../../docs/architecture/technical-baseline.md) |
| سورس قابل ساخت و آزمون | [Solution](../../ChangeFlow.slnx)، `src/` و `tests/` |
| راهنمای اجرا | [README](../../README.md) |
| Analyze واقعی | [analyze.md](analyze.md) و [گزارش واقعی پیش از Implement، بازیابی‌شده](analyze-pre-implement-recovered.md)؛ Findings، تصمیم‌های اصلاحی و تحلیل مجدد |
| Converge و پیگیری | [converge.md](converge.md)؛ چهار یافته به T061–T064 تبدیل و در Implement جداگانه اصلاح شدند |
| Build/Test/Validation | [final.md](final.md)، گزارش‌های US و logهای خام همین پوشه |

## ردیابی

نام‌های جدول، کلاس‌های واقعی در `tests/ChangeFlow.Tests/{Api,Core,Persistence}` هستند.
شناسه‌های T سه‌رقمی Task هستند؛ T01–T18 در Quickstart گروه سناریو هستند و تعداد test نیستند.

| نیازمندی / معیار | Taskهای مرتبط | شاهد آزمون / گزارش |
| --- | --- | --- |
| FR-001 | T017، T019، T020، T015، T021 | ContentContractTests |
| FR-002 | T005، T008، T019، T015 | ContentContractTests، AuthenticationTests |
| FR-003 | T018، T019، T015، T028، T044 | ContentContractTests، PendingReviewEditTests، ResubmitContractTests |
| FR-004 | T024، T025، T045، T046، T043، T044 | SubmitContractTests، ResubmissionRulesTests، ResubmitContractTests |
| FR-005 | T029، T030، T032، T027 | ReviewContractTests |
| FR-006 | T029، T030، T032، T027 | ReviewContractTests |
| FR-007 | T019، T024، T029، T028 | PendingReviewEditTests، SubmitContractTests |
| FR-008 | T008، T023، T054، T014 | AuthenticationTests، AuthorizationSnapshotTests، PrivacyTests |
| FR-009 | T012، T019، T035، T049 | AtomicRollbackTests، DeleteContractTests |
| FR-010 | T024، T029، T027، T022، T063 | SubmitContractTests، ReviewContractTests، TransitionMatrixTests |
| FR-011 | T012، T019، T028، T034، T049، T065، T066 | StaleContentUpdateTests، PendingReviewEditTests، ConcurrentMutationTests، DeleteContractTests |
| FR-012 | T012، T034، T036، T050 | ConcurrentMutationTests، DeleteConcurrencyTests |
| FR-013 | T012، T035، T037، T039، T040 | AtomicRollbackTests، CommitRecoveryTests |
| FR-014 | T010، T020، T031، T051، T048، T054 | VisibilityTests |
| FR-015 | T019، T030، T028، T034 | PendingReviewEditTests، ConcurrentMutationTests |
| FR-016 | T020، T031، T051، T048، T054، T036 | VisibilityTests، ReviewVisibilityTests، PrivacyTests |
| FR-017 | T017، T018، T016، T015، T043، T063 | ContentValidationTests، ContentContractTests، ContractBoundaryTests |
| FR-018 | T011، T012، T036، T039، T041، T050 | IdempotencyContractTests، DurableReplayTests، DeleteConcurrencyTests |
| FR-019 | T052، T053، T049، T050 | DeleteContractTests، DeleteConcurrencyTests |
| SC-001 | T015، T021 | ContentContractTests / us1.md |
| SC-002 | T022، T026، T044، T047 | SubmitContractTests، ResubmitContractTests / us2.md، us4.md |
| SC-003 | T027، T033 | ReviewContractTests / us3.md |
| SC-004 | T028، T033 | PendingReviewEditTests، ReviewContractTests / us3.md |
| SC-005 | T034، T042، T065، T066 | StaleContentUpdateTests، ConcurrentMutationTests / us5.md |
| SC-006 | T035، T037، T042 | AtomicRollbackTests، CommitRecoveryTests / us5.md |
| SC-007 | T010، T042، T057 | ContentContractTests، CommitRecoveryTests / final.md |
| SC-008 | T028، T034، T033، T042 | PendingReviewEditTests، ConcurrentMutationTests / us3.md، us5.md |
| SC-009 | T016، T015، T021 | ContentValidationTests، ContentContractTests / us1.md |
| SC-010 | T027، T031، T048، T033 | ReviewVisibilityTests، VisibilityTests / us3.md، us6.md |
| SC-011 | T043، T044، T047 | ResubmissionRulesTests، ResubmitContractTests / us4.md |
| SC-012 | T036، T041، T050، T042 | IdempotencyContractTests، DurableReplayTests، DeleteConcurrencyTests / us5.md، us6.md |
| SC-013 | T023، T026 | AuthorizationSnapshotTests، PrivacyTests / us2.md، us6.md |
| SC-014 | T048، T054، T055 | VisibilityTests، PrivacyTests / us6.md |
| SC-015 | T049، T050، T055 | DeleteContractTests، DeleteConcurrencyTests / us6.md |

## شش حداقل تکلیف و تعهدهای افزوده

| گروه Quickstart | شاهد واقعی |
| --- | --- |
| T01 | ContentContractTests: ایجاد Draft با مالک واقعی و ledger |
| T02 | SubmitContractTests: Submit و حفظ محتوا/مالکیت |
| T03 | ReviewContractTests: منع Self-Approval برای carol |
| T04 | ReviewContractTests: Approve مستقل bob |
| T05 | PendingReviewEditTests: رد ویرایش Approved |
| T06 | [StaleContentUpdateTests](../../tests/ChangeFlow.Tests/Api/StaleContentUpdateTests.cs): PUT موفق، سپس PUT با ETag قدیمی و key تازه؛412/VERSION_CONFLICT و حفظ محتوای جدید در GET و DB مستقل |
| T07 | ContentValidationTests، ResubmissionRulesTests، ResubmitContractTests و آزمون‌های انتقال |
| T08 | ConcurrentMutationTests: Edit/Approve در هر دو ترتیب |
| T09 | ConcurrentMutationTests: Approve/RequestChanges و رقابت هم‌کلید |
| T10 | AuthenticationTests و ContentContractTests: توکن، نقش و ورودی جعلی |
| T11 | ReviewContractTests: منع Self-RequestChanges |
| T12 | AuthorizationSnapshotTests: لغو پیش و پس از snapshot |
| T13 | VisibilityTests، ReviewVisibilityTests، PrivacyTests |
| T14 | DeleteContractTests و DeleteConcurrencyTests |
| T15 | AtomicRollbackTests و FoundationTests |
| T16 | CommitRecoveryTests: exception پس از commit واقعی |
| T17 | DurableReplayTests و DeleteConcurrencyTests: restart، ساعت۲۵ساعت جلوتر و حذف |
| T18 | IdempotencyContractTests، CommitRecoveryTests: mismatch، busy و نتیجهٔ نامعلوم |

T08/T09/T14–T16 و شکست‌های Persistence تعهد اضافهٔ Constitution بر T01–T06 هستند؛
آزمون نسخهٔ قدیمیِ ترتیبی جای آزمون رقابت واقعی گزارش نشده است.
آزمون‌های حساس، علاوه بر HTTP، state/Version/baseline/ledger را از اتصال مستقل می‌خوانند.

## سازگاری و وضعیت تصمیم‌ها

چهار Task اصلاحی T061–T064 نیز تکمیل شدند: T061 برای تقویت شاهد رقابت FR-012/SC-005،
T062 برای HTTPS طراحی موجود، T063 برای پوشش FR-009/010/011/017 و T064 برای تحویل و ارزیابی مجدد.
Converge دوم شکاف اجرایی باقی‌مانده پیدا نکرد؛ گزارش آن بالا پیوند شده است.
در پایان اجرای قبلی۶۴ Task علامت تکمیل داشتند؛ پس از کشف شکاف آزمون، Phase11 و T065/T066
اضافه شدند. اکنون هر۶۶ Task تکمیل شده‌اند؛ T065 آزمون مستقیم سناریوی ششم را افزود و T066
شواهد و گزارش‌ها را تصحیح کرد. Converge پس از اصلاح، F5/F6 را بسته و Task تازه‌ای اضافه نکرد؛
علامت Task جایگزین شاهد آزمون، پذیرش انسانی یا شواهد تاریخی مفقود نیست.

- نسخهٔ خوانده‌شدهٔ Constitution: `2.0.2-draft.1`؛ تصویب و TODO تاریخ تصویب همچنان معلق است.
- فقط .NET شاهد انتخاب سراسری دارد؛ TB-001 تا TB-009 انتخاب طراحی قابلیت فعلی باقی‌اند.
- A-03 و Q1–Q9 از پاسخ‌های ثبت‌شده خوانده شدند؛ تصمیم محصولی یا معماری تازه به کاربر نسبت داده نشد.
- هیچ فایل حاکمیتی یا فایل Spec/Plan/Research/data-model/contracts در این پیاده‌سازی بازتعریف نشده است.
- Git در PATH وجود نداشت؛ commit، branch واقعی یا diff مبتنی بر Git ساخته نشده است.

## شواهد فرایند و محدودیت

در زمان گزارش قبلی، Analyze پیش از Implement در فایل‌های پروژه یافت نشده بود.
در 2026-11-14 سه پیام واقعیِ گزارش، پیشنهاد اصلاح و نتیجهٔ Analyze مجدد از تاریخچهٔ همان پروژه
[بازیابی شدند](analyze-pre-implement-recovered.md)؛ منبع، timestamp و SHA-256 پیام‌ها ثبت است.
متن C1/I1/I2/I3/I4 از روی عنوان بازسازی نشده است. [Analyze جاری](analyze.md) نیز خواندنی بود؛
hash اسناد قبل و بعد یکسان و پوشش ردیابی۳۴/۳۴ بود. اصلاح کد در Analyze انجام نشد.

گزارش‌های اولیهٔ Plan/Quickstart/Setup، مشاهدهٔ زمان همان مرحله را توصیف می‌کنند؛
گزارش نهایی، نسخه و نتیجهٔ اجرای تازه را جدا ثبت می‌کند. خطاهای عمدی TDD در logهای red محفوظ‌اند.

پذیرش انسانی، آزمون قطع برق/خرابی فیزیکی دیسک، استقرار چندنمونه، تست پلتفرم دیگر و restore عملیاتی
backup انجام نشده‌اند. محیط هدف، اجرای بسته و تک‌نمونه روی دیسک محلی است؛ production readiness
ادعا نمی‌شود. گزارش نهایی محل ثبت نتیجهٔ واقعی، تعداد آزمون و وضعیت servicing است.
