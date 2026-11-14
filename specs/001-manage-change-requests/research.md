# Research: ChangeFlow

**تاریخ پژوهش اصلی:** 2026-11-12 | **تاریخ بازآرایی:** 2026-11-13
**وضعیت:** پژوهش پشتیبان طرح موجود، بدون ادعای تصویب انسانی یا موفقیت پیاده‌سازی.
منابع و گزینه‌های قبلی حفظ شده‌اند؛ در این بازآرایی منبع وب یا وضعیت فعلی دستگاه دوباره بررسی نشده است.
پژوهش اصلی با دو عامل پژوهشی طبق مهارت Plan انجام شده بود؛ این جمله سابقه است، نه پژوهش تازه.

متن انتخاب‌های طراحی همین قابلیت در [مبنای فنی پروژه](../../docs/architecture/technical-baseline.md) قرار دارد.
این سند سؤال، گزینه‌ها، دلیل انتخاب و بده‌بستان‌ها را نگه می‌دارد؛ الگوریتم در
[Plan](plan.md#execution-design)، ساختار داده در [data-model](data-model.md) و جزئیات پیام‌ها در
[قرارداد HTTP](contracts/http-api.md) مرجع اصلی دارند.
رفتارهای محصول از [Spec](spec.md) خوانده شوند؛ انتقال سند، تصمیم محصولی تازه نیست.

<a id="r-01"></a>
## R-01 — بستر و شکل برنامه

**سؤال پژوهشی:** برای HTTP API کوچک با قید .NET کاربر، کدام شاخه و شکل برنامه کمترین پیچیدگی لازم را دارد؟
**متن انتخاب طراحی قابلیت:** [TB-001](../../docs/architecture/technical-baseline.md#tb-001)،
[TB-002](../../docs/architecture/technical-baseline.md#tb-002) و
[TB-003](../../docs/architecture/technical-baseline.md#tb-003).

**گزینه‌ها:** شاخهٔ LTS انتخاب‌شده در برابر .NET8 و .NET11؛ Minimal APIs در برابر Controllers؛
Core مستقل در برابر قراردادن قواعد در HTTP.
**دلیل انتخاب موجود:** قید .NET کاربر و اندازهٔ API، شاخهٔ LTS و route handler سبک را توجیه می‌کنند؛
.NET8 baseline جدید انتخاب نشد و .NET11 ضرورتی برای تمرین نداشت. استقلال Core امکان بررسی قواعد بدون HTTP را می‌دهد.
**بده‌بستان:** Controllers همچنان گزینهٔ معتبرند، اما برای تعداد عملیات فعلی ارزش اضافهٔ ضروری نشان داده نشد؛
استقلال دامنه یک پروژه اضافه می‌خواهد ولی از وابستگی قواعد به وب جلوگیری می‌کند.

**شاهد محیطی:** اعداد SDK/runtime مشاهده‌شده و patch گزارش‌شدهٔ منبع رسمی هنگام پژوهش، بدون تغییر در
[جدول سوابق نسخه و محیط مبنا](../../docs/architecture/technical-baseline.md#سوابق-نسخه-و-محیط-برای-tb-001)
ثبت شده‌اند؛ ارتقای دستگاه انجام نشده بود و snapshot تاریخی، ادعای «جدیدترین نسخهٔ امروز» نیست.

**منابع حفظ‌شده:**
[سیاست رسمی پشتیبانی .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)،
[Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0).
**فرض/محدودیت:** یک نمونهٔ محلی و workload آموزشی؛ هدف throughput/uptime تجاری در Spec وجود ندارد.

<a id="r-02"></a>
## R-02 — Persistence و کنترل رقابت

**سؤال پژوهشی:** چگونه بررسی پیش‌شرط و ثبت تغییر در FR-009 و FR-011 تا FR-013 بدون سرویس دادهٔ جدا اتمیک بماند؟
**متن انتخاب طراحی قابلیت:** [TB-004](../../docs/architecture/technical-baseline.md#tb-004) و
[TB-009](../../docs/architecture/technical-baseline.md#tb-009)؛ طرح فیلدها و indexها در [مدل](data-model.md).

**گزینه‌ها:** SQL صریح SQLite، EF Core، PostgreSQL، InMemory/JSON file؛ journal حذف‌شونده در برابر WAL.
**دلیل انتخاب موجود:** قفل نویسندهٔ SQLite در خود DB برقرار است، نه حافظهٔ یک فرایند؛
SQL صریح در مدل کوچک، مرز state+result را خوانا می‌کند. DELETE/EXTRA برای نیاز فعلی راه‌اندازی و backup ساده‌تری دارد.
EXTRA همگام‌سازی دایرکتوری journal را هم درخواست می‌کند؛ انتخاب و تنظیم‌ها در مبنا ثبت‌اند.
**بده‌بستان:** نویسندگان سری می‌شوند و انتظار/timeout ممکن است؛ PostgreSQL برای بار بیشتر و چند نمونه مناسب‌تر
اما نیازمند سرویس جداست. EF Core ممکن است ولی در این طرح ضروری تشخیص داده نشد.
InMemory/JSON file برای اثبات تراکنش و رقابت کافی نیستند؛ WAL برای throughput بالاتر فعلاً لازم نیست.
**محدودیت:** تضمین مستقل از OS، دیسک و fsync برای برق‌رفتگی ادعا نشده است؛ network share هدف طرح نیست.

**منابع حفظ‌شده:**
[تراکنش‌های provider](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions)،
[SQLite synchronous](https://www.sqlite.org/pragma.html#pragma_synchronous)،
[بستهٔ provider](https://www.nuget.org/packages/Microsoft.Data.Sqlite/10.0.15).

<a id="r-03"></a>
## R-03 — هویت آموزشی و اجرای سیاست مجوز

**سؤال پژوهشی:** چگونه هویت معتبر و نقش فعلی در محیط بسته، بدون public account API، سیاست FR-002 و FR-008 را اجرا کنند؟
**متن انتخاب طراحی قابلیت:** [TB-005](../../docs/architecture/technical-baseline.md#tb-005).
جزئیات User/AccessToken/ActorContext در [مدل](data-model.md) و جای snapshot در
[الگوریتم Plan](plan.md#execution-design) است؛ زمان اثر لغو و دامنهٔ مشاهده در FR-008 و FR-016 باقی‌اند.

**گزینه‌ها:** opaque token با lookup مجوز فعلی؛ JWT فقط با claims نقش؛ OIDC/OAuth استاندارد همراه lookup؛
ASP.NET Core Identity account endpoints.
**دلیل انتخاب موجود:** هویت بستهٔ آموزشی با lookup سرور، به سرور هویت جدا وابسته نیست و به role ادعایی client اعتماد نمی‌کند؛
برای اجرای سیاست انتخاب‌شدهٔ Q6=A، snapshot همان عملیات نگه داشته می‌شود. این استدلال، انتخاب دوبارهٔ سیاست لغو نیست.
**بده‌بستان:** JWT فقط با claims، اثر لغو در عملیات بعدی را تأمین نمی‌کند؛ OIDC/OAuth با lookup برای گسترش واقعی
مناسب‌تر ولی setup بیشتری دارد. Identity امکانات حساب فراتر از مسئله اضافه می‌کند.
**محدودیت آموزشی:** راهکار منتخب OAuth/JWT یا راهکار production نیست؛ دامنهٔ پوشش و حفاظت token/DB،
نبود rotation/MFA/ضدسوءاستفاده و محدودیت maintenance در TB-005 ثبت‌اند و نباید با انتقال متن پنهان شوند.
DB failure در authentication با invalid token تفاوت دارد؛ نگاشت قبلی در قرارداد حفظ شده است.

**منابع حفظ‌شده:**
[Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-10.0)،
[محدودیت access token اختصاصی](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)،
[Resource authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based?view=aspnetcore-10.0).

<a id="r-04"></a>
## R-04 — اتمیک‌بودن نتیجه و بازیابی تکرار

**سؤال پژوهشی:** در قطع پاسخ، رقابت و commit نامعلوم چگونه FR-012، FR-013 و FR-018 بدون اثر دوباره برقرار بمانند؟
**متن انتخاب طراحی قابلیت:** [TB-007](../../docs/architecture/technical-baseline.md#tb-007) با زیرساخت TB-004.
الگوریتم مرحله‌ای و مسیرهای busy/rollback/reconcile از نسخهٔ قبلی این بخش به
[Execution Design در Plan](plan.md#execution-design) منتقل شده‌اند؛ رکوردها و fingerprint در
[data-model](data-model.md) و status/codeهای نتیجه در [contracts](contracts/http-api.md) هستند.

**گزینه‌ها:** ledger هم‌تراکنش و پایدار؛ cache حافظه؛ ledger در storage جدا؛ کلید بدون actor؛
پاکسازی TTL؛ ثبت durable InProgress پیش از اثر.
**دلیل انتخاب موجود:** state و result هم‌تراکنش، اثر بدون ledger یا ledger بدون اثر نمی‌سازند؛
قفل DB رقیب را تا نتیجهٔ committed یا timeout متوقف می‌کند، بنابراین durable InProgress جدا لازم تشخیص داده نشد.
**بده‌بستان:** serialization و رشد ledger هزینه دارند؛ cache حافظه/restart، storage جدا و کلید مشترک actorها
تضمین‌ها را حفظ نمی‌کنند. TTL با مدت تعیین‌شده در FR-018 ناسازگار است؛ مدت تضمین در Spec باقی می‌ماند.
خطای قطعی و transient از نظر ثبت نتیجه متفاوت‌اند؛ جزئیات admission و بازیابی در Plan/contract حفظ شده‌اند.
**محدودیت:** الگوریتم استنتاج طراحی از تضمین تراکنش و نیازمندی‌هاست، نه قابلیت آمادهٔ provider؛
backup باید state و result را با هم حفظ کند و commit نامعلوم نباید موفقیت فرضی تولید کند.

**منابع حفظ‌شده:**
[Atomic commit](https://www.sqlite.org/atomiccommit.html)،
[Database errors](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/database-errors).

<a id="r-05"></a>
## R-05 — قرارداد و اعتبارسنجی

**سؤال پژوهشی:** چگونه نسخهٔ مبنای تصمیم، خطاهای قابل تشخیص و replay بدون افشا در HTTP نمایش داده شوند؟
**متن انتخاب طراحی قابلیت:** [TB-006](../../docs/architecture/technical-baseline.md#tb-006).
مسیر، نام فیلد، payload، headers، statusها، ترتیب خطا و canonicalization در
[قرارداد](contracts/http-api.md) و ساختار ذخیره/baseline در [مدل](data-model.md) هستند؛
خط‌مشی تغییر محتوا در FR-003/FR-017، مشاهده در FR-014/FR-016 و replay در FR-018 مرجع محصولی دارند.

**گزینه‌ها:** ETag/If-Match برای Version؛ PUT replacement در برابر PATCH با حفظ فیلد غایب؛
receipt کمینه در برابر پاسخ mutation حاوی کل محتوا؛ شمارش Unicode scalar در برابر UTF-16 code unit.
**دلیل انتخاب موجود:** precondition نسخهٔ مشاهده‌شده را در قرارداد قابل سنجش می‌کند؛ receipt نیاز به snapshot حساس
در ledger را حذف می‌کند. PATCH با حفظ فیلد غایب با Clarify آخر ناسازگار است.
**بده‌بستان:** دریافت محتوا به GET مجاز نیاز دارد؛ شمارش UTF-16 ممکن است scalar خارج BMP را دو بار بشمارد.
تفسیر مقایسهٔ canonical فعلی با baseline، شرط FR-004 را بدون اتکا به صرف افزایش Version بررسی می‌کند؛
تفصیل آن در مدل موجود است، نه تصمیم عمومی برای تمام قابلیت‌ها.
**وضعیت A-03:** دو پاسخ صریح گزینهٔ A در [Clarifications مورخ 2026-11-13](spec.md#clarifications)
شمارش Unicode scalar و نامعتبر بودن عنوانِ فقط شامل فاصله، تب یا خط جدید را تعیین تکلیف کرده‌اند؛
مرجع رفتار FR-017 و US1.11–12 است. `EnumerateRunes`، نبود trim/NFC و null canonical انتخاب‌های
طراحی قبلی در مدل/قرارداد هستند؛ ثبت پاسخ‌ها تصمیم فنی سراسری جدیدی ایجاد نمی‌کند.

**منابع حفظ‌شده:**
[HTTP semantics و If-Match](https://www.rfc-editor.org/rfc/rfc9110.html#name-if-match)،
[428](https://www.rfc-editor.org/rfc/rfc6585.html#section-3)،
[Rune enumeration](https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.enumeraterunes?view=net-10.0).

<a id="r-06"></a>
## R-06 — آزمون و بازتولید

**سؤال پژوهشی:** چه ابزار و چه شاهدی رقابت واقعی، لغو مجوز و rollback پایدار را قابل بازتولید می‌کند؟
**متن انتخاب طراحی قابلیت:** [TB-008](../../docs/architecture/technical-baseline.md#tb-008).
نسخهٔ ابزارها در مبنا و سناریوها، barrier/fault و فرمان‌های آینده در [Quickstart](quickstart.md) ثبت‌اند.

**گزینه‌ها:** xUnit v3/MTP، v2 جدید، mock DB، EF InMemory، file-backed SQLite با test host واقعی،
آزمون صرف HTTP در برابر بازخوانی مستقل دادهٔ پایدار.
**دلیل انتخاب موجود:** test host با handler/credential واقعی، و SQLite مستقل fixture، مرزهای امنیت و تراکنش را
واقعاً در مسیر آزمون قرار می‌دهند؛ gate و fault injection امکان کنترل زمان‌بندی و محل شکست را می‌دهند.
**بده‌بستان:** fixture فایل و هماهنگی رقابت از unit test ساده پرهزینه‌تر است، اما mock/InMemory یا اجرای صرفاً
ترتیبی برای ادعای atomicity کافی نیست. v2 برای suite جدید انتخاب نشده بود.
**محدودیت:** تست‌ها هنوز ساخته/اجرا نشده‌اند؛ reporter بدون نصب و discovery صفر شاهد موفقیت نیستند.
pin/scan وابستگی‌های native و ثبت نتیجهٔ واقعی در Implementation/Validation باقی است.

**منابع حفظ‌شده:**
[Integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)،
[xUnit MTP](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)،
[بستهٔ xunit.v3](https://www.nuget.org/packages/xunit.v3/4.0.2)،
[MTP driver](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-mtp).

## نتیجهٔ پژوهش و موارد باز

انتخاب‌های فنی قبلی حفظ شده‌اند؛ در پژوهش اصلی unknown مسدودکنندهٔ طراحی ثبت نشده بود.
A-03 با پاسخ‌های ثبت‌شده تعیین تکلیف شده است؛ مشاهدهٔ محیط و منابع نسخه‌ها دوباره انجام نشده‌اند.
اعتبارسنجی سازوکار، restore، scan، native SQLite و اجرای تست شاهد آینده می‌خواهد؛ این بازآرایی موفقیت آن‌ها را اثبات نمی‌کند.
دامنهٔ آموزشی، نبود هدف کارایی تجاری و هزینهٔ رشد ledger در TB-002/TB-007 ثبت‌اند؛
تغییر مدت تضمین صرفاً با تغییر فنی مجاز نیست و باید از FR-018 عبور کند.

تمایز خطای قطعی، auth/header نامعتبر و mismatch کلید در Plan/contract حفظ شده است؛
وضعیت جاریِ هماهنگ‌سازی Constitution و دامنهٔ قابلیت در [سوابق بازآرایی Plan](plan.md#refactor-2026-11-13)
ثبت است؛ تصمیم محصولی پاسخ‌داده‌شده دوباره باز معرفی نمی‌شود.
