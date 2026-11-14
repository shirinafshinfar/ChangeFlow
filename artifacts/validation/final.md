# اعتبارسنجی نهایی — 2026-11-13

**تکمیل مستندات تحویل در 2026-11-14:** گزارش واقعی Analyze پیش از Implement و تصمیم‌های اصلاحی
از تاریخچهٔ همین پروژه [بازیابی شدند](analyze-pre-implement-recovered.md).
یادداشت نبود آن گزارش در بخش‌های تاریخی زیر، وضعیت زمان همان بررسی را توصیف می‌کند.
بازیابی سند و اصلاح README آزمون محصول تازه‌ای نبود. سپس نسخهٔ commitشدهٔ تحویل در clone
مستقل دوباره restore/build/test شد:۱۱۲/۱۱۲ موفق، صفر ناموفق/skipped و Build بدون warning/error.
فرمان‌ها و خروجی این اجرای تازه در [اعتبارسنجی انتشار](publication-2026-11-14.md) ثبت شده‌اند.

محل اجرا: `E:/ai-oroumand/ChangeFlow`؛ Windows x64؛ PowerShell5.1.
نتیجهٔ ثبت‌شدهٔ اجرای 2026-11-13: **۱۱۱ آزمون اجراشده،۱۱۱ موفق، صفر ناموفق، صفر skipped**.
مدت اعلام‌شدهٔ runner:۱۵٫۹۵۰ثانیه. پذیرش انسانی انجام نشده است.

**به‌روزرسانی 2026-11-14:** آزمون مستقیم سناریوی ششم اضافه شد؛ اجرای هدفمند۱/۱ و
regression کامل۱۱۲/۱۱۲ موفق‌اند. جزئیات و خروجی واقعی در بخش تکمیلی انتهای این سند آمده‌اند.
شمار۱۱۱ بالا، سابقهٔ اجرای پیشین است و شاهد مستقیم stale PUT با key تازه نبود.

## محیط واقعی و نسخه‌ها

- SDK نصب‌شدهٔ اولیه10.0.403 و runtimeهای اولیه10.0.14 بودند؛ سوابق اولیه تغییر داده نشدند.
- برای اجرای نهایی، SDK10.0.404 و .NET/ASP.NET Core10.0.15 به‌صورت محلی در
  `.local/dotnet` استخراج شدند؛ نصب سیستمی و PATH دائمی تغییر نکردند.
- نسخهٔ جاری از [انتشار رسمی10.0.15](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.15/10.0.15.md)
  و [metadata رسمی](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json) بررسی شد.
  SHA-512 آرشیو SDK با metadata برابر بود؛ در صورت عدم تطابق استخراج اجرا نمی‌شد.
- اجرای مستقیم installer PowerShell به‌علت ExecutionPolicy انجام نشد؛ از آرشیو رسمی و بررسی hash
  استفاده شد. این خطا موفقیت نصب گزارش نشده است و سیاست سراسری تغییر نکرد.
- `global.json` پایه10.0.403 و latestPatch را حفظ کرد؛ SDK انتخاب‌شده در این اجرا10.0.404 است.
  بسته‌ها و lockها همان نسخه‌های ثبت‌شده‌اند؛ ارتقای وابستگی پنهانی انجام نشد.
- SQLite بومی واقعی: **3.53.3**؛ log اجرای CLI و سرور ثبت شده است.
  آزمون Foundation، `journal_mode=delete`، `synchronous=3` و `foreign_keys=1` را
  با اتصال مستقل تأیید می‌کند؛ startup نیز تنظیمات واقعی را می‌خواند.
- Git در PATH موجود نیست؛ هیچ commit یا branch واقعی ساخته نشد.

## فرمان‌ها و خروجی نهایی

فرمان‌ها از ریشه با PATH و DOTNET_ROOT همان فرایند به SDK محلی اجرا شدند:

```powershell
$env:PATH = (Join-Path $PWD '.local/dotnet') + ';' + $env:PATH
$env:DOTNET_ROOT = Join-Path $PWD '.local/dotnet'
dotnet --info
dotnet restore ChangeFlow.slnx --locked-mode
dotnet build ChangeFlow.slnx --no-restore --configuration Release
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation
dotnet list ChangeFlow.slnx package --vulnerable --include-transitive --no-restore --format json
```

| بررسی | نتیجهٔ مشاهده‌شده | شاهد |
| --- | --- | --- |
| info | exit0؛ SDK10.0.404، runtime10.0.15 | [environment-final.log](environment-final.log) |
| locked restore | exit0؛ سه پروژه | [restore-final.log](restore-final.log) |
| Release build | exit0؛ صفر warning و صفر error | [build-final.log](build-final.log) |
| MTP/xUnit test | exit0؛۱۱۱/۱۱۱، صفر skipped | [test-final.log](test-final.log) |
| scan مستقیم و گذرا | exit0؛ بستهٔ آسیب‌پذیر گزارش نشد | [dependency-audit-final.json](dependency-audit-final.json) |
| حفاظت secrets | صفر تطابق توکن‌های واقعی seed با سورس/گزارش؛ ACL خروجی محافظت‌شده با یک entry حساب جاری | بررسی مستقل فایل‌ها؛ بدون چاپ توکن |

scan تصویر زمان اجراست و تضمین دائمی نبود آسیب‌پذیری نیست. هیچ TRX فرضی گزارش نشده است.

## اجرای مستقل و تطبیق Quickstart

فرمان seed واقعی با `ASPNETCORE_ENVIRONMENT=Development`،
DB مطلق `.local/smoke-fixed/changeflow.db` و خروجی نسبی
`--token-output .local/smoke-fixed/dev-tokens.json` اجرا شد؛ exit0.
سرور با `dotnet run --project src/ChangeFlow.Api --configuration Release --no-build`
و بدون تعیین URL روی `https://localhost:7043` شروع شد.

گواهی localhost معتبر ولی در trust store مورد اعتماد نبود. اعتماد دستگاه تغییر نکرد.
helper محلی فقط با تطابق اثرانگشت همان گواهی و hostname=localhost، زنجیرهٔ self-signed را پذیرفت؛
خطای hostname نادیده گرفته نشد و validation TLS برنامه تغییر نکرد.

| مرحلهٔ مستقل | نتیجه |
| --- | --- |
| Create |201 |
| GET مالک |200، ETag1 |
| Submit با ETag مشاهده‌شده |200 |
| GET Reviewer |200، ETag2 |
| Approve و replay همان key/ورودی |200، بدنهٔ یکسان |
| GET Reviewer پس از Approved |404 |
| GET مالک پس از Approved |200، Approved |
| CLI لغو نقش bob Reviewer | exit0 |
| GET/list بعدی bob |403 |
| اجرای صریح با URL مبتنی بر HTTP | رد پیش از شروع listening؛ native exit=-532462766، شکست مورد انتظار |

شاهدها: [seed](smoke-seed-verified.log)، [سرور](smoke-server-verified.log)،
[گردش HTTPS](smoke-https-verified.log)، [revoke](smoke-revoke.log)،
[اثر لغو](smoke-revocation-verified.log)، [رد HTTP](smoke-http-rejected.log).
سرورهای موقت این بررسی متوقف شدند؛ هیچ سرویس پس‌زمینهٔ متعلق به این اجرا باقی نماند.

Quickstart دستورهای طراحی PowerShell7 دارد؛ محیط جاری5.1 است.
فرمان‌های build/test و CLI و معنای جریان HTTP با README تطبیق داده شدند.
مثال تعاملی `Invoke-WebRequest -SkipHttpErrorCheck` عیناً در5.1 اجرا نشده؛
در عوض همان جریان روی Kestrel واقعی با HttpClient و pin گواهی آزموده شد.
فرمان اعتماد سراسری گواهی، restore عملیاتی backup و قطع برق فیزیکی اجرا نشدند.

## خطاهای کشف‌شده و بازآزمون

- logهای red مراحل US، شکست عمدی پیش از پیاده‌سازی‌اند؛ نتایج هر مرحله در us1.md تا us6.md ثبت است.
- اولین build آزمون HTTPS به‌علت نبود helper و یک build به‌علت قفل موقت فایل شکست خوردند؛
  هیچ test موفقی از آن‌ها گزارش نشده است. پس از ساخت ترتیبی مجدد،
  `test-converge-tls-red-behavior.log` پنج آزمون واقعی با چهار شکست و یک موفقیت داشت؛
  پس از اصلاح، suite کامل۱۱۱/۱۱۱ موفق شد.
- اجرای مستقل نخست، خروجی نسبی token در پوشهٔ پروژهٔ وب را آشکار کرد.
  `RunWorkingDirectory` با ریشه هماهنگ شد و seed/سرور/گردش با مسیر تازه دوباره اجرا شدند.
  شاهد شکست اولیه در `smoke-https.log` حفظ شده است.
- gate اولیهٔ رقابت فقط آغاز attempt دوم را اثبات می‌کرد؛ T061 آن را به نقطهٔ پس از بازشدن
  اتصال دوم منتقل کرد. رقابت‌های دو ترتیب برنده و هم‌کلید در
  `test-converge-fixes.log` و سپس `test-final.log` دوباره موفق شدند.
- آزمون HTTP مرز طول، replacement اختیاری، If-Match نامعتبر،413/415، بدنهٔ command،
  ماتریس چهار وضعیت و سرریز نسخه نیز در اجرای نهایی حضور دارند.

ردیابی هر۱۹ FR،۱۵ SC و گروه‌های T01–T18، با تفکیک شش حداقل تکلیف از تعهدهای اضافهٔ رقابت
و خطای Persistence، در [گزارش پروژه](project-report.md) آمده است.
وضعیت Converge و کارهای اصلاحی در [گزارش Converge](converge.md) ثبت می‌شود.

## موارد باقی‌ماندهٔ غیرخودکار

پذیرش انسانی با هویت/تاریخ/دامنه/نتیجه هنوز ثبت نشده است. Constitution پیش‌نویس باقی است.
متن کامل Analyze قدیمی پیش از Implement در فایل‌ها در دسترس نبود؛
[Analyze جاری](analyze.md) با زمان واقعی و فقط‌خواندنی ثبت شد.
این محدودیت با نتیجهٔ آزمون محصول یا تصویب انسانی جایگزین نشده است.

## اعتبارسنجی تکمیلی — 2026-11-14، T065/T066

SDK انتخاب‌شده در این اجرا10.0.404 بود؛ از SDK محلی موجود استفاده شد و نسخه یا وابستگی تازه‌ای
نصب نشد. فرمان‌های واقعی از ریشه با PATH/DOTNET_ROOT فرایندی به `.local/dotnet` اجرا شدند:

```powershell
dotnet --version
dotnet build ChangeFlow.slnx --no-restore --configuration Release
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --no-restore -- --filter-class ChangeFlow.Tests.Api.StaleContentUpdateTests
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --no-restore --results-directory artifacts/validation
```

| بررسی | نتیجهٔ واقعی | خروجی |
| --- | --- | --- |
| Release build | exit0، صفر warning/error | [build-stale-put-2026-11-14.log](build-stale-put-2026-11-14.log) |
| آزمون هدفمند نهایی | exit0؛۱ موفق، صفر ناموفق/skipped؛۳٫۹۵۲ثانیه | [test-stale-put-targeted-verified-2026-11-14.log](test-stale-put-targeted-verified-2026-11-14.log) |
| کل مجموعه | exit0؛۱۱۲ موفق، صفر ناموفق/skipped؛۱۴٫۲۱۰ثانیه | [test-stale-put-regression-2026-11-14.log](test-stale-put-regression-2026-11-14.log) |

آزمون مستقیم [StaleContentUpdateTests](../../tests/ChangeFlow.Tests/Api/StaleContentUpdateTests.cs)
اکنون شاهد سناریوی ششم است: پس از ثبت ویرایش جدید، PUT با key تازه و ETag قدیمی412 می‌گیرد؛
محتوای جدید و Version در GET و DB مستقل حفظ می‌شوند. ledger و replay خطای قطعی نیز بررسی شدند.
آزمون تازه روی منطق موجود موفق شد؛ تغییری در کد برنامه لازم نشد.

نتیجهٔ قدیمی۱۱۱/۱۱۱ و اجرای قبلیِ صفر شکاف، این آزمون مستقیم را شامل نمی‌شدند؛ ادعای پوشش
کامل قبلی با این شاهد تکمیل و تصحیح شد. اجرای اولیهٔ آزمون تازه نیز در
`test-stale-put-targeted-2026-11-14.log` موجود است؛ نتیجهٔ نهایی جدول، مربوط به نسخهٔ تکمیل‌شدهٔ
همان آزمون با بررسی صریح تمام فیلدهای View است.

در این گام restore، scan وابستگی یا smoke مستقل Kestrel دوباره اجرا نشدند؛ بسته‌ها و کد محصول
تغییر نکردند و نتایج آن بررسی‌ها در بخش تاریخی این سند متعلق به اجرای پیشین‌اند.
