# ChangeFlow

Repository عمومی: [shirinafshinfar/ChangeFlow](https://github.com/shirinafshinfar/ChangeFlow).

HTTP API آموزشی مدیریت درخواست تغییر با .NET؛ قابلیت فعال:
[Spec](specs/001-manage-change-requests/spec.md)،
[Plan](specs/001-manage-change-requests/plan.md) و
[Tasks](specs/001-manage-change-requests/tasks.md).
وضعیت واقعی اجرا و محدودیت‌ها در [گزارش اعتبارسنجی](artifacts/validation/final.md)
و [گزارش پروژه](artifacts/validation/project-report.md) ثبت می‌شوند.

[پاسخ پنج سؤال مفهومی تکلیف](docs/answers/session-01.md) نیز در اقلام تحویلی موجود است.
Constitution نسخهٔ `2.0.2` در 2026-11-14 تصویب شد و پذیرش نسخهٔ تحویل محصول با پاسخ صریح
کاربر در [ثبت تأیید](docs/reviews/acceptance-2026-11-14.md) مستند است؛ اجرای موفق آزمون
به‌تنهایی پذیرش انسانی محسوب نمی‌شود.

## Stack و تصمیم‌های اصلی معماری

انتخاب‌های این قابلیت در [Plan](specs/001-manage-change-requests/plan.md) و
[مبنای فنی](docs/architecture/technical-baseline.md) ثبت شده‌اند؛ دامنهٔ تأیید آن‌ها از همان اسناد
خوانده شود. Stack فعلی C#14، .NET10، ASP.NET Core Minimal APIs و Microsoft.Data.Sqlite است؛
آزمون‌ها با xunit.v3، Microsoft.Testing.Platform و WebApplicationFactory اجرا می‌شوند.

- Core قواعد کسب‌وکار را با وابستگی صرف به BCL نگه می‌دارد. API به Core وابسته است؛ Endpoint
  ورودی و HTTP را نگاشت می‌کند و Application اجرای عملیات را هماهنگ می‌کند.
- SQLite روی دیسک محلی است. تراکنش BEGIN IMMEDIATE، کنترل Version و ثبت اتمیک تغییر و نتیجه در
  ledger، از بازنویسی تغییر جدیدتر و اثر دوبارهٔ retry جلوگیری می‌کنند؛ جزئیات recovery در Plan است.
- هویت از opaque bearer سمت سرور استخراج می‌شود؛ مجوز در شروع هر عملیات خوانده و snapshot
  همان عملیات ثابت می‌ماند. مجوز مشاهده در query اعمال و پاسخ mutation به receipt محدود می‌شود.

## ساخت و آزمون

Windows x64 محیط بررسی‌شده است. SDK پایه در `global.json` برابر10.0.403 با
`latestPatch` است؛ runtime و ASP.NET Core شاخه10 را با servicing جاری به‌روز نگه دارید.
اگر SDK محلی این جلسه در `.local/dotnet` موجود است، فقط برای همین ترمینال:

```powershell
$env:PATH = (Join-Path $PWD '.local/dotnet') + ';' + $env:PATH
dotnet --info
dotnet restore ChangeFlow.slnx --locked-mode
dotnet build ChangeFlow.slnx --no-restore --configuration Release
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation
```

بسته‌های دقیق و گذرا در سه `packages.lock.json` هستند. Core فقط به BCL وابسته است؛
API به Core و SQLite و آزمون‌ها به هر دو پروژه وابسته‌اند. runner، Microsoft.Testing.Platform
و xunit.v3 است؛ هیچ TRX فرضی بدون extension ساخته نمی‌شود. گزارش خام هر مرحله در
`artifacts/validation` نگه داشته شده است.

## اجرای محلی

از ریشهٔ Repository در PowerShell، یک مسیر DB و فایل توکن تازه انتخاب کنید:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ChangeFlow__DatabasePath = Join-Path $PWD '.local/changeflow.db'
dotnet run --project src/ChangeFlow.Api --configuration Release --no-build -- --seed-dev-identities --token-output .local/dev-tokens.json
dotnet run --project src/ChangeFlow.Api --configuration Release --no-build -- --urls https://localhost:7043
```

برنامه به‌صورت پیش‌فرض روی HTTPS پورت7043 اجرا می‌شود؛ binding صریح HTTP پیش از شروع سرویس رد می‌شود.
HTTPS به گواهی development معتبر نیاز دارد. `dotnet dev-certs https --check --trust`
فقط وضعیت اعتماد را بررسی می‌کند؛ اگر مورد اعتماد نیست، کاربر می‌تواند جداگانه
`dotnet dev-certs https --trust` را اجرا کند. این پروژه اعتماد سراسری دستگاه را تغییر نمی‌دهد.
در اعتبارسنجی مستقل، گواهی localhost با اثرانگشت خودش pin شد؛ بررسی TLS برنامه غیرفعال نشده است.

seed فقط در Development و با دسترسی فایل محلی اجرا می‌شود. alice نقش Requester،
bob نقش Reviewer، carol هر دو و dave نقش Requester دارد. توکن تصادفی فقط یک بار در فایل
خروجی با دسترسی محدود به حساب جاری نوشته می‌شود؛ DB فقط SHA-256 را نگه می‌دارد.
فایل توکن را نمایش عمومی، ارسال یا commit نکنید. DB نیز شامل محتوای حساس و ledger است؛
دسترسی پوشهٔ محلی را محدود نگه دارید. seed روی DB دارای هویت یا فایل خروجی موجود رد می‌شود؛
normal startup نقش لغوشده را برنمی‌گرداند و داده را reset نمی‌کند.

لغو یک نقش با همان مسیر DB، بدون endpoint عمومی:

```powershell
dotnet run --project src/ChangeFlow.Api --configuration Release --no-build -- --revoke-role bob Reviewer
```

در عملیات بعدی، از جمله replay و GET، مجوز تازه خوانده می‌شود؛ عملیات پذیرفته‌شده پیش از لغو
می‌تواند با رعایت نسخه و سایر پیش‌شرط‌ها کامل شود. CLI همچنین شناسهٔ واقعی User را می‌پذیرد.

## نمونهٔ گردش

در ترمینال دیگری، پس از برقرارشدن اعتماد HTTPS و seed:

```powershell
$tokens = Get-Content -Raw .local/dev-tokens.json | ConvertFrom-Json
$baseUrl = 'https://localhost:7043/api/change-requests'
$alice = @{ Authorization = "Bearer $($tokens.alice)" }
$bob = @{ Authorization = "Bearer $($tokens.bob)" }
$create = $alice.Clone()
$create['Idempotency-Key'] = [guid]::NewGuid().ToString()
$receipt = Invoke-WebRequest -UseBasicParsing -Method Post -Uri $baseUrl -Headers $create -ContentType 'application/json' -Body '{"title":"Example change"}'
$requestId = ($receipt.Content | ConvertFrom-Json).requestId
$draft = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/$requestId" -Headers $alice
$submit = $alice.Clone()
$submit['Idempotency-Key'] = [guid]::NewGuid().ToString()
$submit['If-Match'] = [string]$draft.Headers.ETag
Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$baseUrl/$requestId/submit" -Headers $submit
$pending = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/$requestId" -Headers $bob
$approve = $bob.Clone()
$approve['Idempotency-Key'] = [guid]::NewGuid().ToString()
$approve['If-Match'] = [string]$pending.Headers.ETag
Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$baseUrl/$requestId/approve" -Headers $approve
Invoke-WebRequest -UseBasicParsing -Method Post -Uri "$baseUrl/$requestId/approve" -Headers $approve
```

رسید فقط `requestId/result` دارد. پس از Approved، GET برای bob برابر404 و برای مالک برابر200 است.
همهٔ mutationها کلید می‌خواهند؛ غیر Create، نسخهٔ مشاهده‌شده در GET با If-Match نیز لازم است.
برای retry همان قصد، key و ورودی و نسخهٔ قبلی را حفظ کنید؛ پس از503/قطع پاسخ نیز key تازه نسازید.
برای قصد یا ورودی تازه، key تازه بدهید. خطای قطعی ثبت‌شده با همان key همان خطا را بازمی‌گرداند.

قرارداد کامل فهرست، cursor، PUT جایگزین، حذف Draft و خطاها در
[HTTP contract](specs/001-manage-change-requests/contracts/http-api.md) است.
نبود/خالی‌بودن شرح یا دلیل در PUT، مقدار قبلی را پاک می‌کند؛ عنوان همواره اجباری است.
PendingReview توسط مالک قابل ویرایش است؛ Reviewer باید نسخهٔ جدید را دوباره بخواند.
Self-Approval و Self-RequestChanges ممنوع‌اند. ارسال مجدد ChangesRequested نیازمند تفاوت مؤثر
محتوا با baseline آخرین بازگشت است.

## محدودیت و نگهداری

این اجرا برای یک نمونهٔ API، دیسک محلی و محیط آموزشی بسته است؛ شبکهٔ عمومی، چند replica
یا DB روی share در طرح این قابلیت پوشش ندارند. public login، مدیریت نقش تحت HTTP،
frontend یا قابلیت ثبت پذیرش انسانی وجود ندارد. bearer آموزشی جایگزین سامانهٔ هویت production نیست.

برای backup، API و ابزارهای متصل را متوقف و بسته‌شدن تراکنش‌ها را بررسی کنید؛
از **کل فایل DB شامل ledger** نسخه بگیرید و خروجی توکن لازم را جدا و امن نگه دارید.
بازیابی فقط جدول ChangeRequest، تضمین تکرار را از بین می‌برد. ledger، TTL و cleanup خودکار ندارد
و با استفاده رشد می‌کند. حذف Draft آن را حذف نمی‌کند. از حذف فایل journal فعال یا DB در حال استفاده پرهیز کنید.
سناریوی قطع برق فیزیکی و restore یک backup عملیاتی در این جلسه آزموده نشده‌اند.

راهنمای Quickstart متن مرحلهٔ طراحی و مبتنی بر PowerShell7 است؛ محیط این جلسه PowerShell5.1 بود.
فرمان‌های restore/build/test/seed/revoke و قرارداد گردش تطبیق داده شده‌اند؛
`-SkipHttpErrorCheck` مخصوص نسخه‌های جدیدتر PowerShell است. گزارش نهایی، روش واقعی
اجرای smoke و موارد اجرا‌نشده را از مثال‌های طراحی تفکیک می‌کند.

## نقش Coding Agent

در این پروژه، کدکس (Codex) با استفاده از Spec Kit در تولید و اصلاح Artifactهای SDD، پیشنهاد طراحی فنی، پیاده‌سازی API، نوشتن و اجرای Testها و تهیهٔ گزارش‌های Analyze، Converge و Validation مشارکت کرده است. شواهد اجرا و اصلاحات در [گزارش پروژه](artifacts/validation/project-report.md) و [گزارش Validation](artifacts/validation/final.md) ثبت شده‌اند.

تصمیم‌های محصولی با پاسخ‌های ثبت‌شدهٔ کاربر در بخش Clarifications فایل Spec تعیین شده‌اند. بازبینی اسناد و خروجی‌های Agent با مشارکت کاربر انجام شده است؛ وضعیت تصویب Constitution و Acceptance نهایی محصول از اسناد و گزارش‌های مربوط خوانده می‌شود و از موفقیت Testها استنتاج نمی‌شود.
