# PROGRESS

## M0 — Repo, Docker Compose, CI, schema + migration (branch `feature/m0-scaffold`) — ✅ รอรีวิว

เกณฑ์ SPEC ข้อ 9: `docker compose up` แล้วเปิดเว็บได้; `dotnet test` ผ่าน — **ผ่าน** (พร้อมเกณฑ์เพิ่มจาก PROMPT-M0 ทุกข้อ)

### สิ่งที่ทำ
- **Repo layout ตาม CLAUDE.md ข้อ 2**
  - `src/EasySchedule.Domain` — entity ตาม SPEC ข้อ 3: School (+license), Term, ScheduleMode, Period, ClassLevel, ClassSection, BreakSlot, Teacher, Room, Subject, StudyPlan (+StudyPlanSection), Lesson, LessonTeacher, LessonGroup, SubjectRule, Reservation, Snapshot, Placement, User, AuditLog; enum `LoadType` 1–7 (บริหาร…พัก), `ScheduleModeType`, `LicensePlan`, `RoomType`, `SubjectRuleType`
  - `src/EasySchedule.Solver` — สัญญา `ISolver` / `SolverInput` / `SolverResult` (ไม่อ้าง DB)
  - `src/EasySchedule.Infrastructure` — `EasyScheduleDbContext` (DB `EasyScheduleWeb`, `Thai_100_CI_AS`), global query filter `SchoolId`, กันเขียนข้ามโรงเรียน, cascade ตาม SPEC ข้อ 11, migration `InitialCreate`, seeder, Dapper (พร้อมใช้ M3)
  - `src/EasySchedule.Importer` — `ImportSourceGuard` (อ่านได้เฉพาะ `*_Import` + ReadOnly); ตัว import จริง M1
  - `src/EasySchedule.Api` — `GET /api/health`, `POST /api/auth/login`, `GET /api/auth/me`, `GET/POST /api/schools/{id}/classlevels` (เทอมที่ active หรือ `?termId=`), Swagger + JWT, role SuperAdmin/SchoolAdmin/Viewer, user-secrets
  - `tests/EasySchedule.Solver.Tests` (19), `tests/EasySchedule.Api.Tests` (35, Testcontainers), `tests/EasySchedule.Benchmark` (โครง CLI), `tests/e2e` (Playwright 3)
  - `web/` — React + TS + Vite + Tailwind + TanStack Query, `src/i18n/th.ts`, ESLint; หน้า login + รายชื่อระดับชั้น
- **Seed**: โรงเรียนตัวอย่าง DEMO01 (Trial, 4 ห้อง/ระดับ) — ป.1 (11), ม.1 (21), ม.4 (34) ระดับละ 2 ห้อง, ครู `ครู 001`–`ครู 010`, วิชา 8, โหมด 0 และ 1, BreakSlot ค่าเริ่มต้นตรงกันทั้งโรงเรียน (ทุกห้อง × ทุกวัน), แผนการเรียน 2 แผน
- **Docker**: `docker-compose.yml` (db + db-init + api + web → :8080; API ใช้ login `easyschedule_app` ไม่ใช่ sa), `docker-compose.onprem.yml` (SQL Express, ปิด seed/swagger, port 80, ข้อมูลใน `./data`, restart always, สร้าง superadmin คนแรกจาก `BOOTSTRAP_SUPERADMIN_PASSWORD`)
- **ตามคำตอบผู้ใช้ (2026-11-18)**: รายวิชา + แผนการเรียนเป็นข้อมูลระดับโรงเรียน, แผนมีรายวิชารายระดับชั้น-รายภาคเรียน, `Subject.Category` (พื้นฐาน/เพิ่มเติม/เพิ่มเติมเลือก/กิจกรรม), `HomeroomId` → ห้องเรียนประจำ
- **CI** `.github/workflows/easyschedule-ci.yml`: `dotnet build` + `dotnet format --verify-no-changes` + `dotnet test`; `npm ci` + `npm run lint` + `npm run build`; `docker compose up --build` + `npx playwright test`

### วิธีรัน
```bash
cp .env.example .env              # แล้วเปลี่ยนทุกค่า
docker compose up --build         # http://localhost:8080 (Swagger: /swagger)
```
login `admin` / `viewer` / `superadmin` — รหัสผ่าน = `SEED_USER_PASSWORD` ใน `.env`

ติดตั้งในโรงเรียน (ตั้ง `BOOTSTRAP_SUPERADMIN_PASSWORD` ใน `.env` สำหรับครั้งแรก):
```bash
docker compose -f docker-compose.yml -f docker-compose.onprem.yml up -d --build
```

### ผลตรวจ (2026-11-18, DB ว่าง)

| เกณฑ์ | คำสั่ง | ผล |
|---|---|---|
| เปิดเว็บเห็นหน้า login | `docker compose down -v && docker compose up --build -d` | ✅ health `ok`, `/login` แสดงผล |
| login แล้วเห็น ป.1 ม.1 ม.4 ไทยไม่เพี้ยน | Playwright | ✅ |
| `dotnet test` | `dotnet test` | ✅ 55/55 (Solver.Tests 19, Api.Tests 36) รวม tenant isolation 10 ข้อ, Thai round-trip ผ่าน API + ตรวจทุกคอลัมน์เป็น nvarchar, bootstrap |
| on-prem ครั้งแรก | `docker compose -f docker-compose.yml -f docker-compose.onprem.yml up -d` (DB ว่าง) | ✅ ไม่มีข้อมูลตัวอย่าง, สร้าง superadmin, login ได้, Swagger ปิด, ข้อมูลอยู่ใน `DATA_DIR` |
| API ไม่ใช้ sa | ตรวจ `IS_SRVROLEMEMBER('sysadmin','easyschedule_app')` | ✅ = 0, ฐานเป็น `Thai_100_CI_AS` |
| `npx playwright test` | `cd web && npx playwright test` | ✅ 3/3 |
| lint / format | `npm run lint`, `dotnet format --verify-no-changes` | ✅ |
| ไม่มีรหัสผ่านใน git | `git grep -iE "password[=]"` | ✅ เหลือ `.env.example` และ `src/docs/deployment/android.md` (เอกสาร Flutter เดิม, placeholder — D-027) |

Benchmark: ยังไม่มี (solver = M3)

หมายเหตุ: sandbox ที่ใช้ตรวจต้องเพิ่ม proxy/CA ตอน docker build ผ่าน override ชั่วคราว (ไม่ได้ commit) — CI บน GitHub เป็นตัวยืนยันอีกชั้น

### คำถามค้าง
1. **ย้าย repo** (ผู้ใช้ตัดสิน: ย้าย) — รอ repo ใหม่ (ผู้ใช้สร้าง repo ว่าง แล้วให้ Claude เข้าถึง) จากนั้น push ประวัติ branch นี้ไป
2. **PDF แผนการเรียนนครสวรรค์** — ขอไฟล์เพื่อปรับโครง `StudyPlan`/`StudyPlanSubject` ให้ตรงรูปแบบจริงก่อนทำ importer ใน M1
3. **ครู/ห้องผูกเทอม** — ตอนนี้ยังผูก `TermId` ตาม SPEC ข้อ 3 (M1 ต้องมี "คัดลอกจากเทอมก่อน") ถ้าต้องการให้เป็นข้อมูลระดับโรงเรียนเหมือนรายวิชา แจ้งได้
4. **StartPeriod/EndPeriod ของระดับชั้น** (SPEC ข้อ 3, กฎ H1) — เช่น ป.1 เรียนถึงคาบ 6 ในโหมด 0 (8 คาบ/วัน) แล้วในโหมด ร.ด. (10 คาบ/วัน) ป.1 เรียนถึงคาบไหน: ใช้เลขคาบเดิม (6) หรือต้องกำหนดแยกต่อโหมด? ตอนนี้ใช้ค่าเดียว (seed 1–10) แล้วจำกัดด้วย PeriodPerDay ของโหมด
5. Thai round-trip ผ่าน **PDF** (CLAUDE.md กฎข้อ 6) จะเพิ่มใน M6 พร้อม QuestPDF
