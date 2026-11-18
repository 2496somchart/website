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
- **Docker**: `docker-compose.yml` (db + api + web → :8080), `docker-compose.onprem.yml` (SQL Express, ปิด seed/swagger, port 80, ข้อมูลใน `./data`, restart always)
- **CI** `.github/workflows/easyschedule-ci.yml`: `dotnet build` + `dotnet format --verify-no-changes` + `dotnet test`; `npm ci` + `npm run lint` + `npm run build`; `docker compose up --build` + `npx playwright test`

### วิธีรัน
```bash
cp .env.example .env              # แล้วเปลี่ยนทุกค่า
docker compose up --build         # http://localhost:8080 (Swagger: /swagger)
```
login `admin` / `viewer` / `superadmin` — รหัสผ่าน = `SEED_USER_PASSWORD` ใน `.env`

### ผลตรวจ (2026-11-18, DB ว่าง)

| เกณฑ์ | คำสั่ง | ผล |
|---|---|---|
| เปิดเว็บเห็นหน้า login | `docker compose down -v && docker compose up --build -d` | ✅ health `ok`, `/login` แสดงผล |
| login แล้วเห็น ป.1 ม.1 ม.4 ไทยไม่เพี้ยน | Playwright | ✅ |
| `dotnet test` | `dotnet test` | ✅ 54/54 (Solver.Tests 19, Api.Tests 35) รวม tenant isolation 10 ข้อ, Thai round-trip ผ่าน API + ตรวจทุกคอลัมน์เป็น nvarchar |
| `npx playwright test` | `cd web && npx playwright test` | ✅ 3/3 |
| lint / format | `npm run lint`, `dotnet format --verify-no-changes` | ✅ |
| ไม่มีรหัสผ่านใน git | `git grep -iE "password[=]"` | ✅ เหลือ `.env.example` และ `src/docs/deployment/android.md` (เอกสาร Flutter เดิม, placeholder — D-027) |

Benchmark: ยังไม่มี (solver = M3)

หมายเหตุ: sandbox ที่ใช้ตรวจต้องเพิ่ม proxy/CA ตอน docker build ผ่าน override ชั่วคราว (ไม่ได้ commit) — CI บน GitHub เป็นตัวยืนยันอีกชั้น

### คำถามค้าง
1. **repo**: `2496somchart/website` เป็น fork ของเว็บไซต์ Flutter (`src/` มีไฟล์เว็บเดิม ~300 MB, workflow `build.yml` เดิมรันบน PR ด้วย, default branch ชื่อ `master` ไม่ใช่ `main`) — ย้าย EasySchedule ไป repo ใหม่ไหม?
2. **TermId ในข้อมูลหลัก** (D-003): ทำให้ครู/ห้อง/วิชามี `TermId` ตาม SPEC ข้อ 3 — ต้องมีฟีเจอร์ "คัดลอกข้อมูลจากเทอมก่อน" ใน M1 ใช่ไหม หรือครู/ห้องควรเป็นข้อมูลระดับโรงเรียน (ไม่ผูกเทอม)?
3. **`ClassSection.HomeroomId`** (D-009): หมายถึงครูประจำชั้น หรือห้องประจำ (Room)?
4. **ClassLevel StartPeriod/EndPeriod** ใช้ค่าเดียวทั้งสองโหมด (seed = 1–10) หรือควรแยกตามโหมด?
5. **on-prem ครั้งแรก**: ตอนนี้ต้องเปิด `SEED_ENABLED=true` ครั้งแรกเพื่อให้มี user — ทำหน้าตั้งค่าครั้งแรกใน M7 แทนไหม?
6. compose ใช้บัญชี `sa` — production ควรมี login เฉพาะแอป (เสนอทำใน M7 deploy)
7. Thai round-trip ผ่าน **PDF** (CLAUDE.md กฎข้อ 6) จะเพิ่มใน M6 พร้อม QuestPDF
