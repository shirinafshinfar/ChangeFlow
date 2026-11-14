# US1 — 2026-11-13

T015–T021: ایجاد Draft، GET تکی با predicate مجوز و ETag، PUT replacement اتمیک و اعتبارسنجی Unicode scalar پیاده شدند.

- اجرای red: 16 آزمون، 16 شکست رفتاری (endpoint ساخته نشده و stub اعتبارسنجی)، Build موفق؛ `test-us1-red.log`.
- اجرای هدفمند پس از پیاده‌سازی: 16 passed / 0 failed / 0 skipped؛ exit 0؛ `test-us1.log`.
- regression زیرساخت و US1: 24 passed / 0 failed / 0 skipped؛ exit 0؛ `test-us1-regression.log`.
- فرمان‌ها: `dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation`؛ اجرای هدفمند از `-- --filter-class ChangeFlow.Tests.Api.ContentContractTests ChangeFlow.Tests.Core.ContentValidationTests` استفاده کرد.
- US1.1–12 / SC-001 / SC-009: POST→GET→PUT از ETag واقعی، حفظ مالکیت، clearing مستقل، رد عنوان غایب/سفید، مرزهای طول و scalar، جعل مالک/status، duplicate/type/surrogate نامعتبر و نبود تغییر جزئی بررسی شدند.
- DB و ledger با اتصال مستقل بررسی شدند؛ این نتیجه تکمیل کل قابلیت یا پذیرش انسانی نیست.
