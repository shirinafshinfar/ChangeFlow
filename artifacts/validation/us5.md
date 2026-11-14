# اعتبارسنجی US5
T034–T042: `test-us5.log` اجرای کامل را با **۶۲ موفق، صفر ناموفق، صفر skipped** ثبت می‌کند. آزمون‌های تکمیلی، نیازی به تغییر رفتار executor موجود نشان ندادند.

- دو attempt واقعی HTTP و اتصال‌های مستقل SQLite هنگام نگه‌داشتن رزرو writer هم‌پوشانی دارند. Edit/Approve و Approve/RequestChanges در هر دو ترتیب برنده اجرا شدند؛ وضعیت، محتوا، baseline و افزایش واحد نسخه مستقلاً خوانده شدند. رقابت هم‌کلید یک اثر و رسید یکسان دارد.
- I/O تزریقی پس از mutation و ledger، هر دو اثر را rollback می‌کند. writer واقعی هنگام begin و reader واقعی هنگام commit خطای busy می‌سازند؛ پاسخ503 کلید را مسموم نمی‌کند و پس از آزادکردن قفل، retry همان کلید موفق است.
- خطا پس از commit واقعی با اتصال تازه reconcile می‌شود؛ DML دوباره اجرا نمی‌شود. شکست reconciliation پاسخ503 می‌دهد و retry بعدی رسید commit‌شده را بازیابی می‌کند.
- fingerprint شامل محتوای canonical، هدف و نسخه است؛ محدودهٔ عامل و عملیات مستقل است. خطاهای قطعی400/403/404/409/412 پیش از بررسی وضعیت زنده replay می‌شوند؛ admission نامعتبر ledger نمی‌سازد.
- restart و ساعت تزریقی۲۵ساعت جلوتر، status/body/Location را حفظ می‌کنند. schema و کد، TTL یا cleanup ندارند؛ پیاده‌سازی زمان را برای اعتبار رسید مصرف نمی‌کند.

این اجرا شواهد رقابت و خطای Persistence مربوط به SC-005/006/007/008/012 را ثبت کرد؛
شاهد مستقیم US5.1 برای stale PUT با key تازه در آن موجود نبود و در بخش تکمیلی زیر افزوده شد.
رقابت و خطای Persistence، تعهد اضافه بر شش سناریوی حداقلی تکلیف‌اند. تزریق خطا، تضمین در برابر
تمام خرابی‌های فیزیکی دیسک یا برق نیست. پذیرش انسانی معلق است.

بازبینی Converge: gate اولیه فقط شروع attempt دوم را نشان می‌داد. T061 نقطهٔ BeforeWriterReservation
را پس از Open اتصال دوم اضافه کرد؛ gate تنها پس از مشاهدهٔ آن آزاد می‌شود. شواهد تقویت‌شدهٔ
رقابت و recovery در اجرای نهایی۱۱۱/۱۱۱ موفق‌اند؛ مرجع نهایی final.md است.

## تکمیل سناریوی ششم — 2026-11-14، T065/T066

مرجع: Constitution VI/سناریوی6، FR-009/011/018، SC-005، US5.1، EC-05 و Plan/Execution Design.
آزمون [StaleContentUpdateTests](../../tests/ChangeFlow.Tests/Api/StaleContentUpdateTests.cs)
با نام `StalePutWithFreshKeyPreservesNewerContentAndReplaysVersionConflict` روی کد موجود اضافه شد.

مالک یک Draft را از GET می‌خواند؛ PUT معتبر عنوان/شرح/دلیل را تغییر می‌دهد. GET بعدی، محتوا و
ETag جدید را تأیید می‌کند. PUT دوم با محتوای متفاوت، ETag قبلی و key تازه، دقیقاً412 و
VERSION_CONFLICT می‌گیرد. GET و اتصال مستقل SQLite، حفظ محتوای جدید، مالکیت، Draft و Version
را تأیید می‌کنند. خطای قطعی در ledger ثبت می‌شود؛ replay همان رد، بدنهٔ یکسان، دو نتیجهٔ
ledger و یک entity را حفظ می‌کند. علت رد، idempotency mismatch یا تغییرناپذیری Approved نیست.

- اجرای هدفمند:۱ موفق، صفر ناموفق، صفر skipped، exit0؛
  [خروجی](test-stale-put-targeted-verified-2026-11-14.log).
- regression:۱۱۲ موفق، صفر ناموفق، صفر skipped، exit0؛
  [خروجی](test-stale-put-regression-2026-11-14.log).
- Build: صفر warning/error، exit0؛ [خروجی](build-stale-put-2026-11-14.log).

گزارش۱۱۱ آزمون قبلی معتبرِ همان اجراست، اما نگاشت قدیمی آن به این سناریوی دقیق بیش از شواهد
موجود بود؛ نتیجهٔ فعلی این نقص پوشش را رفع می‌کند. کد محصول، نیازمندی و وابستگی تغییر نکردند.
