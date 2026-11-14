# اعتبارسنجی نسخهٔ انتشار — 2026-11-14

مخزن عمومی: [shirinafshinfar/ChangeFlow](https://github.com/shirinafshinfar/ChangeFlow).
نسخهٔ سورس آمادهٔ انتشار: `21a5828`؛ این commit بر commit اولیهٔ مخزن بنا شده است.
فایل‌های تحویل در checkout مستقل ثبت و سپس از همان commit، clone مستقل دیگری برای
اعتبارسنجی ساخته شد؛ فایل‌های Build و ابزارهای محلی وارد commit نشدند.

## فرمان‌ها و نتیجهٔ واقعی

فرمان‌ها در `.local/verify-publication` با SDK موجود `10.0.404` و
`DOTNET_ROOT` و `PATH` فرایندی به SDK محلی اجرا شدند:

```powershell
dotnet restore ChangeFlow.slnx --locked-mode
dotnet build ChangeFlow.slnx --no-restore --configuration Release
dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --no-restore --results-directory E:/ai-oroumand/ChangeFlow/artifacts/validation
```

| بررسی | نتیجه | شاهد |
| --- | --- | --- |
| Restore قفل‌شده | exit 0؛ هر سه پروژه بازیابی شدند | [log](restore-publication-2026-11-14.log) |
| Release Build | exit 0؛ صفر warning و error | [log](build-publication-2026-11-14.log) |
| کل آزمون‌ها | exit 0؛۱۱۲ موفق، صفر ناموفق و skipped؛۱۷٫۴۰۶ثانیه | [log](test-publication-2026-11-14.log) |

بررسی فایل‌های ثبت‌شده برای خروجی Build، فایل‌های محیطی و الگوهای شناخته‌شدهٔ کلید خصوصی
و توکن GitHub انجام شد و موردی پیدا نشد. این بررسی ادعای ممیزی جامع امنیتی نیست.

## وضعیت انتقال

ساخت مخزن عمومی تأیید شده است. نوشتن از ابزار GitHub با403 رد شد؛ انتقال مستقیم Git
آماده است و در انتظار تکمیل ورود کاربر از طریق Git Credential Manager قرار دارد.
تا تأیید SHA شاخهٔ remote، انتشار کامل اعلام نمی‌شود.

این بررسی نسخهٔ تحویل، پذیرش انسانی یا تصویب Constitution محسوب نمی‌شود.
