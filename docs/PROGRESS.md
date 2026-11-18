# PROGRESS

## M0 — Scaffold (branch `feature/m0-scaffold`) — ✅ เสร็จ รอรีวิว

### สิ่งที่ทำ
1. **Solution** `EasySchedule.sln` (.NET 8, nullable enabled ผ่าน `Directory.Build.props`)
   - `src/EasySchedule.Domain` — entity 20 ตัว (School, Term, ScheduleMode, Period, ClassLevel, ClassSection, BreakSlot, Teacher, Room, Subject, StudyPlan, Lesson, LessonTeacher, LessonGroup, SubjectRule, Reservation, Snapshot, Placement, User, AuditLog) + enum `LoadType` (1–7), `UserRole`, `SchoolDay`
   - `src/EasySchedule.Infrastructure` — `EasyScheduleDbContext` (DB `EasyScheduleWeb`, collation `Thai_100_CI_AS`), global query filter ตาม `SchoolId`, ป้องกันเขียนข้ามโรงเรียนใน `SaveChanges`, FK ครบ (cascade เฉพาะ Placement→Snapshot/Lesson และ LessonTeacher→Lesson), migration `InitialCreate`, `DataSeeder`
   - `src/EasySchedule.Api` — Minimal API: `GET /api/health`, `POST /api/auth/login`, `GET /api/auth/me`, `GET /api/schools/{id}/classlevels`, `POST /api/schools/{id}/classlevels` (SchoolAdmin/SuperAdmin), Swagger + JWT bearer, policy ตาม role SuperAdmin / SchoolAdmin / Viewer
   - `tests/EasySchedule.Tests` — 24 tests (xUnit + Testcontainers SQL Server + WebApplicationFactory)
2. **Seed**: โรงเรียนตัวอย่าง `DEMO01` "โรงเรียนตัวอย่างวิทยา" — ป.1, ม.1, ม.4 ระดับละ 2 ห้อง, ครู 10, วิชา 8, ภาคเรียน 1/2569, คาบ 8 คาบ/วัน, BreakSlot ค่าเริ่มต้นทั้งโรงเรียน (พักเบรกหลังคาบ 2, พักกลางวันหลังคาบ 4), ห้องพิเศษ 2 ห้อง, user `superadmin` / `admin` / `viewer` (รหัสผ่านจาก `SEED_USER_PASSWORD`)
3. **web/**: React + Vite + Tailwind — หน้า login และหน้าแรกแสดงระดับชั้น (UI ภาษาไทย, ฟอนต์ Noto Sans Thai แบบ offline)
4. **Docker**: `docker-compose.yml` (db + api + web, เปิดที่ :8080) และ `docker-compose.onprem.yml` (SQL Express, ปิด seed/swagger, port 80, เก็บข้อมูลใน `./data`, restart always, จำกัด log)
5. **CI**: `.github/workflows/easyschedule-ci.yml` — `dotnet build` + `dotnet test`, `npm ci && npm run build`, แล้ว `docker compose up` + `npx playwright test`
6. `docs/PROGRESS.md`, `docs/DECISIONS.md`

### วิธีรัน
```bash
cp .env.example .env        # แล้วเปลี่ยนทุกค่า
docker compose up --build   # เปิด http://localhost:8080  (Swagger: /swagger)
```
login: `admin` (หรือ `viewer`, `superadmin`) / รหัสผ่าน = `SEED_USER_PASSWORD` ใน `.env`

ติดตั้งในโรงเรียน:
```bash
docker compose -f docker-compose.yml -f docker-compose.onprem.yml up -d --build
```

### ผลการตรวจเกณฑ์ M0 (2026-11-18)

| เกณฑ์ | คำสั่ง | ผล |
|---|---|---|
| compose ขึ้นแล้วเห็นหน้า login | `docker compose down -v && docker compose up --build -d` → `curl localhost:8080/api/health` | ✅ `{"status":"ok","database":"ok"}`, หน้า `/login` แสดงผล (มี screenshot) |
| login แล้วเห็น "ป.1", "ม.1", "ม.4" ไทยไม่เพี้ยน | Playwright + ตรวจ screenshot | ✅ |
| `dotnet test` ผ่าน (มี tenant isolation + Thai round-trip) | `dotnet test` | ✅ 24/24 passed — `TenantIsolationTests` (9), `ThaiTextRoundTripTests` (2), `ApiTests` (8), `ModelTests` (5) |
| `npx playwright test` ผ่าน | `cd web && set -a && . ../.env && set +a && npx playwright test` | ✅ 3/3 passed |
| ไม่มีรหัสผ่านใน git | `git grep -i "password="` | ✅ เจอแค่ `.env.example` และ `src/docs/deployment/android.md` (เอกสาร Flutter เดิมของ repo, เป็น placeholder — ดู D-018) |
| `npm run build` | `cd web && npm ci && npm run build` | ✅ |

หมายเหตุสภาพแวดล้อมที่ใช้ตรวจ: ใน sandbox ที่ทำงาน build container ต้องผ่าน proxy จึงใช้ compose override ชั่วคราว (เพิ่ม CA + proxy เฉพาะตอน build, **ไม่ได้ commit**) — Dockerfile ใน repo เป็นแบบมาตรฐาน; CI บน GitHub จะเป็นตัวยืนยันอีกชั้น

### คำถามค้าง
1. **ไม่พบ `CLAUDE.md`, `Spec.md.md`/`docs/SPEC.md`, `PROMPT-M0.md`** ใน repo — ขั้น `git mv` จึงทำไม่ได้ ช่วย push ไฟล์เหล่านี้ขึ้นมา แล้วจะปรับโครง/entity ให้ตรง SPEC (ข้อ [รอ SPEC] ใน DECISIONS.md)
2. repo `2496somchart/website` เป็น fork ของเว็บไซต์ Flutter (`src/` มีไฟล์เว็บเดิม 300MB+, มี workflow `build.yml` เดิม) — ต้องการย้าย EasySchedule ไป repo ใหม่ หรือลบเนื้อหาเดิมออกไหม?
3. ความหมายของ `LoadType` 1–7 และฟิลด์ที่ SPEC ต้องการในแต่ละ entity
4. DB engine: เลือก SQL Server (Express สำหรับ on-prem) — ถ้า SPEC กำหนดอย่างอื่น (เช่น PostgreSQL) แจ้งได้
5. on-prem ครั้งแรก: ตอนนี้ต้องเปิด `SEED_ENABLED=true` ครั้งแรกเพื่อให้มี user — ควรมีหน้าตั้งค่าครั้งแรก (setup wizard) แทนไหม?
6. API ใช้บัญชี `sa` ใน compose — production ควรสร้าง login เฉพาะแอป (ทำใน milestone ถัดไป?)
