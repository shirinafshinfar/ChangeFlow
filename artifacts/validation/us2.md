# US2 — 2026-11-13

T022–T026: Submit از Draft و سیاست مجوز شروع پیاده و بررسی شدند.

- red هدفمند Submit: 1 failed / 0 passed؛ endpoint هنوز وجود نداشت؛ Build موفق، exit 2؛ `test-us2-red.log`.
- پس از پیاده‌سازی، suite شامل زیرساخت و US1/US2: total=26، passed=26، failed=0، skipped=0، exit 0؛ `test-us2.log`.
- فرمان: `dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation`.
- US2.1–5 / SC-002 / SC-013: Submit معتبر، مالک/نقش نامعتبر، stale و وضعیت نامجاز، gate پس از snapshot، لغو commit‌شده با اتصال مستقل، تکمیل عملیات جاری و منع درخواست/replay بعدی، Version و ledger پایدار.
- دو هشدار xUnit1051 در تست gate دیده شد و استفاده از CancellationToken آزمون به آن افزوده شد؛ regression بعدی این اصلاح را نیز بررسی می‌کند.
