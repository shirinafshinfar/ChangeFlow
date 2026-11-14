# Quickstart و اعتبارسنجی ChangeFlow

این راهنمای **اجرای آینده پس از Implementation** است؛ API، solution، maintenance CLI و آزمون‌ها
هنوز ساخته نشده‌اند. دستورهای این صفحه در مرحلهٔ Plan اجرا و موفق گزارش نشده‌اند.
قرارداد: [HTTP](contracts/http-api.md)؛ مدل: [data-model](data-model.md)؛
انتخاب‌های طراحی قابلیت: [مبنای فنی](../../docs/architecture/technical-baseline.md)؛
پژوهش پشتیبان: [Research](research.md)؛ الگوریتم: [Plan](plan.md#execution-design).
ارجاع‌ها در 2026-11-13 بازآرایی شدند؛ نسخه‌ها، فرمان‌ها و نتایج مورد انتظار قبلی تغییر نکرده‌اند.

## پیش‌نیاز

- SDK10 با baseline10.0.403 و runtime servicing جاری شاخه10؛ نسخهٔ مشاهده‌شده10.0.14 است،
  baseline بسته‌های طراحی10.0.15. پیش از اعتبارسنجی، runtime با servicing جاری هماهنگ شود.
- PowerShell7 برای نمونه‌ها؛ SQLite روی دیسک محلی، یک نمونهٔ API، دسترسی NuGet برای restore نخست.
- در Implementation، global.json باید SDK10 با latestPatch و `test.runner=Microsoft.Testing.Platform` داشته باشد؛
  test project با xunit.v3 4.0.2/MTP و Microsoft.AspNetCore.Mvc.Testing10.0.15 ساخته شود.
- lock file همراه سورس ثبت شود؛ توکن خام، DB و فایل secrets نباید commit شوند.

## Build و آزمون آینده

از ریشه، فقط پس از ساخت سورس و testها:

```powershell
dotnet --info
dotnet restore ChangeFlow.slnx --locked-mode
dotnet build ChangeFlow.slnx --no-restore --configuration Release
New-Item -ItemType Directory -Force artifacts/validation | Out-Null
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation
```

برای تولید اولیهٔ lock، Implementation یک restore بدون locked-mode لازم دارد؛ هنگام تحویل، locked restore
باید موفق باشد. دستور test مخصوص SDK10/MTP است. TRX فرضی بدون نصب extension گزارش درخواست نشده است؛
خروجی runner و exit code واقعی ثبت شود و تعداد passed/failed/skipped از آن استخراج شود؛ discovery صفر موفقیت suite نیست.

## راه‌اندازی آموزشی آینده

maintenance حالت غیرHTTP همین برنامه، فقط Development و نیازمند دسترسی فایل محلی است؛ public endpoint ندارد.
normal startup schema را بدون reset آماده می‌کند ولی role/token را seed یا دوباره فعال نمی‌کند.
CLIهای زیر قرارداد setup این طرح‌اند و باید در Implementation ساخته شوند:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ChangeFlow__DatabasePath = Join-Path (Get-Location) '.local/changeflow.db'
dotnet dev-certs https --trust
dotnet run --project src/ChangeFlow.Api -- --seed-dev-identities --token-output .local/dev-tokens.json
dotnet run --project src/ChangeFlow.Api -- --urls https://localhost:7043
```

trust گواهی فقط در صورت نیاز توسط اجراکننده انجام می‌شود؛ اکنون اجرا نشده است.
seed چهار هویت بسازد: alice=Requester، bob=Reviewer، carol=هر دو، dave=Requester دیگر.
توکن تصادفی فقط یک بار در فایل محلی امن با propertyهای همین نام ذخیره شود؛ در DB فقط hash باشد.
اگر identity یا فایل خروجی موجود است، بدون اقدام صریح overwrite/reset نشود؛ seed تکراری نباید نقش لغوشده را برگرداند.
برای fixture تازه، فایل DB و token output جدید و مجزا انتخاب شود؛ حذف خودکار فایل موجود انجام نشود.

## جریان دستی آینده

پس از seed و شروع API، در ترمینال جدا:

```powershell
$tokens = Get-Content -Raw .local/dev-tokens.json | ConvertFrom-Json
$baseUrl = 'https://localhost:7043/api/change-requests'
$aliceHeaders = @{ Authorization = "Bearer $($tokens.alice)" }
$bobHeaders = @{ Authorization = "Bearer $($tokens.bob)" }
$createHeaders = $aliceHeaders.Clone()
$createHeaders['Idempotency-Key'] = [guid]::NewGuid().ToString()
$created = Invoke-WebRequest -Method Post -Uri $baseUrl -Headers $createHeaders -ContentType 'application/json' -Body '{"title":"تغییر آزمایشی"}'
$requestId = ($created.Content | ConvertFrom-Json).requestId
$draft = Invoke-WebRequest -Uri "$baseUrl/$requestId" -Headers $aliceHeaders
$submitHeaders = $aliceHeaders.Clone()
$submitHeaders['Idempotency-Key'] = [guid]::NewGuid().ToString()
$submitHeaders['If-Match'] = [string]$draft.Headers.ETag
Invoke-WebRequest -Method Post -Uri "$baseUrl/$requestId/submit" -Headers $submitHeaders
$pending = Invoke-WebRequest -Uri "$baseUrl/$requestId" -Headers $bobHeaders
$approveHeaders = $bobHeaders.Clone()
$approveHeaders['Idempotency-Key'] = [guid]::NewGuid().ToString()
$approveHeaders['If-Match'] = [string]$pending.Headers.ETag
Invoke-WebRequest -Method Post -Uri "$baseUrl/$requestId/approve" -Headers $approveHeaders
Invoke-WebRequest -Method Post -Uri "$baseUrl/$requestId/approve" -Headers $approveHeaders
Invoke-WebRequest -Uri "$baseUrl/$requestId" -Headers $bobHeaders -SkipHttpErrorCheck
Invoke-WebRequest -Uri "$baseUrl/$requestId" -Headers $aliceHeaders
```

انتظار: Create201 و GET اولیه Draft/Version۱؛ Submit200؛ مشاهدهٔ PendingReview توسط Bob؛
Approve200 و retry همان receipt فقط شامل requestId/result؛ GET Bob پس از Approved برابر404 عمومی؛
GET Alice برابر200 و Approved. نتیجهٔ واقعی پس از Implementation گزارش شود، نه در این صفحه.

## سناریوهای آزمون خودکار

Tها گروه سناریو هستند، نه تعداد test اجراشده. هر تست حساس علاوه بر HTTP، دادهٔ پایدار، Version،
baseline و ledger را پس از پایان transaction با اتصال مستقل بخواند.

| شناسه | دسته | سناریو و شاهد |
| --- | --- | --- |
| T01 | حداقل تکلیف | Create معتبر فقط Draft/Version۱ و مالک واقعی و یک ledger |
| T02 | حداقل تکلیف | Submit مالک Draft→PendingReview و Version+۱ |
| T03 | حداقل تکلیف | رد Self-Approval برای carol دو نقشی بدون تغییر درخواست |
| T04 | حداقل تکلیف | تأیید bob مستقل بر نسخهٔ بررسی‌شده |
| T05 | حداقل تکلیف | رد ویرایش Approved و حفظ محتوا/Version |
| T06 | حداقل تکلیف | stale If-Match برابر412 و حفظ تغییر جدیدتر |
| T07 | تکمیلی | چهار انتقال، ماتریس انتقال نامجاز، baseline و ارسال بی‌تغییر، تغییر/بازگرداندن؛ مرز Unicode، title اجباری، clearing فیلد غایب |
| T08 | اضافهٔ Constitution | رقابت واقعی Edit/Approve در دو ترتیب؛ نه تأیید نسخهٔ قدیمی و نه edit Approved |
| T09 | اضافهٔ Constitution | رقابت واقعی Approve/RequestChanges و retry هم‌کلید؛ حداکثر یک اثر |
| T10 | امنیت | token نامعتبر، نقش غایب، جعل مالک/status؛ handler واقعی و دادهٔ پایدار |
| T11 | امنیت | رد Self-RequestChanges برای کاربر دو نقشی |
| T12 | امنیت | لغو قبل و بعد snapshot؛ عملیات قبلی کامل، درخواست و replay بعدی رد |
| T13 | حریم خصوصی | ماتریس GET/list،404 یکسان، receipt replay بدون محتوا؛ عدم افشا در log/ledger |
| T14 | اضافهٔ Constitution | حذف Draft مالک؛ منع حذف دیگران/غیرDraft؛ رقابت Delete/Submit و stale delete |
| T15 | اضافهٔ Constitution | fault پس از mutation و بعد از ledger قبل از commit؛ rollback هر دو و بدون موفقیت کاذب |
| T16 | اضافهٔ Constitution | commit واقعی سپس قطع پاسخ/exception؛ retry/reconcile با همان key بدون اثر دوم |
| T17 | تکرار پایدار | replay پس از restart، بیش از۲۴ساعت و بعد از حذف؛ بدون TTL و احیا |
| T18 | شکست و تکرار | key غایب، mismatch ورودی/target/نسخه؛ خطای قطعی پایدار، transient بدون poison، busy/timeout و commit مبهم |

## روش اثبات رقابت و شکست

- SQLite فایل واقعی fixture و اتصال‌های مستقل؛ InMemory و اجرای صرفاً ترتیبی شاهد رقابت واقعی نیستند.
- دو attempt HTTP با Barrier/TaskCompletionSource پیش از transaction آزاد شوند؛ seam داخلی می‌تواند
  برنده را پس از رزرو writer نگه دارد تا رقیب وارد مسیر نوشتن شود، سپس آزاد کند.
  reader غیرلازم باز نماند؛ timeout تست bounded و بدون Sleep/وابستگی به scheduler باشد.
- دو ترتیب برندهٔ عملیات ناسازگار جدا اجرا شوند؛ پاسخ و DB نهایی هر دو بررسی شوند.
- لغو: عملیات پس از ActorContext و پیش از نوشتن نگه داشته شود؛ نقش از connection مستقل commit و gate آزاد شود؛
  سپس درخواست جدید و replay ارسال و Q6=A اثبات شود. maintenance دستی آینده:
  `dotnet run --project src/ChangeFlow.Api -- --revoke-role bob Reviewer`.
- fault فقط در DI تست؛ endpoint عمومی یا تنظیم production برای شکست‌سازی نباشد.
- commit مبهم باید با قطع پاسخ پس از commit واقعی هم آزموده شود؛ صرف mock کردن exception شاهد کافی نیست.

## گزارش و پذیرش

برای هر اجرا فرمان دقیق، SDK/runtime/native SQLite، PRAGMA واقعی، exit code و تعداد واقعی
اجراشده/موفق/ناموفق/skipped در `artifacts/validation/` ثبت شود؛ موارد اجرا‌نشده دلیل داشته باشند.
این مسیر خروجی آینده است و اکنون گزارش موفقیت ساختگی در آن ایجاد نشده است.
Analyze پس از Tasks و قبل از Implement خواندنی؛ Converge پس از Implement، شکاف‌ها را به Task تبدیل کند؛
اصلاحات دوباره آزمون و ارزیابی شوند. پذیرش انسانی فقط با ارزیابی واقعی مسئول در گزارش پروژه ثبت شود.
README آینده باید اجرای واقعی، محدودیت محیط بسته و backup کامل DB+ledger بدون TTL را توضیح دهد.
