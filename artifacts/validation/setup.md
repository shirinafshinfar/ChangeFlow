# Setup — 2026-11-13

- SDK واقعی: 10.0.403؛ runtimeهای .NET/ASP.NET: 10.0.14؛ Windows x64.
- سه پروژه با `dotnet sln ChangeFlow.slnx list` شناسایی شدند؛ Core هیچ PackageReference ندارد.
- نسخه‌های ثبت‌شده بدون جایگزینی اعمال شدند: Microsoft.Data.Sqlite10.0.15، Mvc.Testing10.0.15، xunit.v3 4.0.2.
- API رسمی NuGet وجود هر سه نسخه را تأیید کرد؛ جدیدترین‌بودن آن‌ها ادعا نمی‌شود.
- `dotnet restore ChangeFlow.slnx --force-evaluate --verbosity minimal -p:NuGetAudit=true -p:NuGetAuditMode=all`: exit 0؛ فایل‌های lock هر سه پروژه ایجاد شدند.
- `dotnet restore ChangeFlow.slnx --locked-mode --verbosity minimal`: exit 0.
- `dotnet build ChangeFlow.slnx --no-restore --configuration Release`: exit 0؛ صفر warning و صفر error.
- `dotnet list ChangeFlow.slnx package --vulnerable --include-transitive --no-restore --format json`: exit 0؛ هیچ بستهٔ آسیب‌پذیری در خروجی اعلام نشد؛ snapshot زمان اجرا، نه تضمین دائمی.
- گزارش‌های خام کنار این فایل‌اند. تعداد آزمون محصول اجراشده در Setup: ۰؛ Build معادل آزمون نیست.
- Git در PATH موجود نیست؛ runtime دستگاه ارتقا داده نشد.
- منابع بررسی تنظیم runner: https://xunit.net/docs/getting-started/v3/microsoft-testing-platform ؛ منابع نسخه‌ها: API رسمی https://api.nuget.org/v3-flatcontainer/ .
