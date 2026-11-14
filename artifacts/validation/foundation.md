# Foundational — 2026-11-13

T005–T014: هویت و ActorContext، schema، اتصالات و PRAGMA، maintenance، قرارداد ورودی، ledger/fingerprint، executor و fixture پیاده شدند.

فرمان: `dotnet test --project tests/ChangeFlow.Tests/ChangeFlow.Tests.csproj --configuration Release --results-directory artifacts/validation`

نتیجهٔ آخر: exit 0؛ total=8، passed=8، failed=0، skipped=0؛ گزارش خام `test-foundation.log`.

شواهد: فایل SQLite واقعی مستقل، readback تنظیمات، حفظ داده در راه‌اندازی دوباره، replay بدون DML دوم، mismatch، rollback پس از mutation/ledger، handler واقعی و مجوز فعلی، خطای واقعی قفل DB، نبود observer در host عادی، seed یک‌باره، حفاظت خروجی توکن و حفظ لغو نقش پس از startup.

در توسعه، یک خطای missing using و یک شکست ACL ویندوز دیده و اصلاح شدند؛ اجرای ناموفق نخست maintenance: 7 passed / 1 failed؛ پس از اصلاح دسترسی فایل، هر 8 آزمون موفق شدند. شکست‌ها به موفقیت تبدیل نشده‌اند؛ نتیجهٔ آخر پس از اصلاح گزارش شده است.

این checkpoint آزمون سرویس تراکنش/handler است؛ endpointهای محصول و آزمون HTTP داستان‌ها هنوز در این مرحله تکمیل نشده‌اند. پذیرش انسانی ادعا نمی‌شود.
