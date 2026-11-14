# فهرست اقلام تحویلی ChangeFlow

مخزن عمومی قابل بررسی: [shirinafshinfar/ChangeFlow](https://github.com/shirinafshinfar/ChangeFlow).
این فهرست به فایل‌های اصلی پروژه ارجاع می‌دهد تا نسخه‌های تکراری اسناد از هم جدا نشوند.
بستهٔ ZIP تحویل، فایل‌های پروژه را با همین ساختار پوشه‌ها در بر می‌گیرد؛ شناسهٔ commit
نسخهٔ بسته در فایل README.txt کنار ZIP ثبت می‌شود تا با نسخهٔ مخزن قابل تطبیق باشد.

| قلم تحویلی | فایل یا پوشهٔ اصلی |
| --- | --- |
| Artifactهای SDD | [Spec](../../specs/001-manage-change-requests/spec.md)، [Plan](../../specs/001-manage-change-requests/plan.md)، [Tasks](../../specs/001-manage-change-requests/tasks.md)، Research، data-model، contracts و quickstart در همان پوشه |
| اصول و معماری | [Constitution](../../.specify/memory/constitution.md)، [Baseline](../architecture/technical-baseline.md) |
| سورس و فایل‌های اجرا | [Solution](../../ChangeFlow.slnx)، [src](../../src/)، [tests](../../tests/)، global.json و فایل‌های قفل وابستگی‌ها |
| گزارش واقعی Build و Test | [اعتبارسنجی انتشار و لینک logها](../../artifacts/validation/publication-2026-11-14.md)، [گزارش نهایی](../../artifacts/validation/final.md) |
| Analyze و تصمیم‌های اصلاحی | [Analyze](../../artifacts/validation/analyze.md)، [متن تاریخی بازیابی‌شده](../../artifacts/validation/analyze-pre-implement-recovered.md) |
| Converge و کارهای اصلاحی | [Converge](../../artifacts/validation/converge.md)، [Tasks](../../specs/001-manage-change-requests/tasks.md) |
| راهنمای اجرا و نقش Coding Agent | [README پروژه](../../README.md) |
| پاسخ پنج سؤال مفهومی | [docs/answers/session-01.md](../answers/session-01.md) |
| وضعیت تحویل و تأیید کاربر | [گزارش پروژه](../../artifacts/validation/project-report.md)، [ثبت تأیید](../reviews/acceptance-2026-11-14.md) |

آخرین اجرای ثبت‌شده:۱۱۲ آزمون موفق، صفر ناموفق و skipped؛ Build بدون خطا و هشدار.
بسته‌بندی اسناد، اجرای Build/Test تازه نیست. گزارش‌های تاریخی وضعیت زمان اجرای خود را بیان
می‌کنند؛ وضعیت فعلی تصویب Constitution و پذیرش کاربر از سند و ثبت تأیید خوانده می‌شود.
