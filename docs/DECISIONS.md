# DECISIONS — ทางเลือกที่ตัดสินเอง + เหตุผล

Source of truth = `docs/SPEC.md` · กฎการทำงาน = `CLAUDE.md`

| # | เรื่อง | การตัดสินใจ | เหตุผล |
|---|---|---|---|
| D-001 | DB | SQL Server 2022 (dev: Developer, on-prem: Express), DB `EasyScheduleWeb` เดียว + `SchoolId` | ตาม SPEC ข้อ 2 / CLAUDE.md ข้อ 1 |
| D-002 | Collation | สร้าง DB ด้วย `Thai_100_CI_AS`; ทุกคอลัมน์ข้อความเป็น `nvarchar` (มี test ตรวจทั้ง schema) | ภาษาไทยไม่เพี้ยน (CLAUDE.md กฎข้อ 6) |
| D-003 | TermId | ตารางงานจัดตารางมี `SchoolId` + `TermId` (ClassLevel, ClassSection, Teacher, Room, Lesson, Placement, Reservation, Snapshot, ScheduleMode, Period, BreakSlot, LessonGroup, SubjectRule). **ยกเว้น `Subject`, `StudyPlan`, `StudyPlanSubject` เป็นข้อมูลระดับโรงเรียน** | SPEC ข้อ 3 + ผู้ใช้ตัดสิน (2026-11-18): วิชาเรียนตามแผน วิชาพื้นฐานเรียนตามเทอมที่แผนกำหนด วิชาเพิ่มเติม/เพิ่มเติมเลือกแล้วแต่โรงเรียนจัด — ดังนั้นรายวิชาและแผนใช้ข้ามปี/เทอม ส่วนการสอนจริงต่อเทอมอยู่ที่ Lesson |
| D-004 | `LoadType` | enum 1–7: 1 บริหาร (`Administration`), 2 กิจกรรม (`Activity`), 3 สอนคณะ (`TeamTeaching`), 4 ส่วนบุคคล (`Personal`), 5 สอน (`Teaching`), 6 เลือกเสรี (`FreeElective`), 7 พัก (`Break`); เก็บเป็น int; ชื่อไทย `ToThai()`; ค่าเริ่มต้นของ Lesson = 5 | ผู้ใช้ยืนยัน + SPEC ข้อ 3 (คงค่าเดิมเพื่อ import) |
| D-005 | `Term.TermNo` | ใช้ชื่อ `TermNo` แทน `Term` ตาม SPEC | C# ห้ามมี member ชื่อเดียวกับ class |
| D-006 | `ScheduleMode.DaysList` | `List<int>` ของ DayNo เก็บเป็น JSON array (EF8 primitive collection) | SPEC ห้าม string คั่น comma; JSON มีชนิดข้อมูลชัด ไม่ต้อง LIKE |
| D-007 | `ClassLevel.IsAggregate` | คำนวณจาก `Code % 10 == 0` ไม่เก็บใน DB | CLAUDE.md กฎข้อ 9 ไม่เก็บค่าที่คำนวณได้ |
| D-008 | ชื่อห้องเรียน | ไม่เก็บ เช่น "ม.4/2" ประกอบจาก `ClassLevel.Name` + `SectionNo` | ไม่เก็บค่าซ้ำซ้อน |
| D-009 | `ClassSection.HomeroomId` | FK → `Room` (ห้องเรียนประจำ) | ผู้ใช้ยืนยัน (2026-11-18) |
| D-010 | แผนการเรียน | `StudyPlan` (ระดับโรงเรียน) + `StudyPlanSubject` (LevelCode × Semester 1/2 → Subject, PPW ตามแผน) + `StudyPlanSection` (ห้องของเทอมนั้นที่ใช้แผน, ห้องเดียวหลายแผนได้); โรงเรียนที่ไม่ทำแผนไม่ต้องมี record; `Lesson.StudyPlanId` สำหรับวิชาเลือก; `Subject.Category` = พื้นฐาน / เพิ่มเติม / เพิ่มเติมเลือก / กิจกรรมพัฒนาผู้เรียน | ผู้ใช้ตอบข้อ 2 + SPEC ข้อ 3, 6. `StudyPlanSubject` อ้าง `LevelCode` ไม่ใช่ ClassLevelId เพราะแผนใช้หลายปี — จะปรับรายละเอียดตาม PDF นครสวรรค์ใน M1 |
| D-011 | `Reservation` | เป้าหมาย = StudyPlan / Teacher / Room (ต้องมีอย่างน้อย 1 — CHECK constraint), มี `LoadType` (3/4) และ `DayNo`/`PeriodNo` | SPEC ข้อ 3 |
| D-012 | `BreakSlot` | `ScheduleModeId` + `SectionId` + `DayNo` + `PeriodNo` (unique) — แยกตามโหมดเพราะโหมด 0/1 คาบต่างกัน | SPEC ข้อ 3, 6 |
| D-013 | License | เก็บที่ `School`: `Plan` (Trial/Licensed/Expired), `MaxSectionsPerLevel` (=license tier), `ExpiresAt`, `TrialPlacementCap` (200); ไม่มีคอลัมน์ `LicenseTier` แยก | SPEC ข้อ 8 — `LicenseTier` กับ `MaxSectionsPerLevel` คือค่าเดียวกัน |
| D-014 | Enum ใน DB | `RoomType` (R/L), `SubjectRule.Type` (C/U/S), `School.Plan` เก็บเป็นตัวอักษร; `LoadType`, `ScheduleMode.Mode` เก็บเป็นตัวเลข | อ่านง่ายและตรงกับระบบเดิม |
| D-015 | Cascade delete | Restrict ทั้งหมด ยกเว้น: ClassSection→Lesson/BreakSlot/StudyPlanSection, Lesson→Placement/LessonTeacher, Snapshot→Placement, StudyPlan→StudyPlanSection/StudyPlanSubject | SPEC ข้อ 11: ลบห้อง = ลบ Lesson/Placement ใน transaction เดียว; ไม่มี ID ห้อย. "ลบ Lesson ที่อยู่ใน Snapshot ต้องถามก่อน" จะทำที่ API (M1) |
| D-016 | Multi-tenant | global query filter (`BypassFilter \|\| SchoolId == CurrentSchoolId`) อ่าน tenant จาก JWT ต่อ request (`HttpTenantContext`, scoped, ไม่มี static); tenant ไม่ทราบ = ไม่เห็นอะไร; `SaveChanges` ปฏิเสธการเขียนข้ามโรงเรียน; endpoint ตรวจ `schoolId` ใน URL อีกชั้น (403) | SPEC ข้อ 8 / CLAUDE.md กฎข้อ 3 |
| D-017 | Auth | JWT HS256, claim `role` + `school_id`; รหัสผ่าน PBKDF2 (ASP.NET Identity `PasswordHasher`); SuperAdmin มี `SchoolId = null` | |
| D-018 | Secret | dev: `dotnet user-secrets` (Api มี `UserSecretsId`) หรือ `.env` สำหรับ compose; deploy: env var; compose ประกอบ connection string จาก `Database__Server/User/Password` จึงไม่มี connection string ใน git | CLAUDE.md กฎข้อ 8 |
| D-019 | Seed | runtime seeder (idempotent) เมื่อ `Seed__Enabled=true`: โรงเรียน DEMO01 (Trial), เทอม 2569/1, โหมด 0 (8 คาบ, พัก P5) และโหมด 1 (10 คาบ, พัก P6, วัน ร.ด. = 3), ระดับ 11/21/34 ระดับละ 2 ห้อง, ครู `ครู 001`–`ครู 010`, วิชา 8 (G 1–8), แผน วิทย์-คณิต/ศิลป์-ภาษา; user `superadmin`/`admin`/`viewer` รหัสจาก `SEED_USER_PASSWORD` | รหัสผ่านต้องมาจาก env จึงใส่ใน migration ไม่ได้; คาบพักเป็นข้อมูล seed ไม่ hardcode ในกฎ |
| D-020 | Importer guard | `ImportSourceGuard.Validate` ยอมรับเฉพาะ `Sch69_Import`/`SchKUS_Import` + `ApplicationIntent=ReadOnly` (มี test) | CLAUDE.md กฎเหล็กข้อ 1 — ตัว importer จริงทำใน M1 |
| D-021 | Solver | M0 มีแค่สัญญา `ISolver`/`SolverInput`/`SolverResult` + test ว่า assembly ไม่อ้าง EF/SqlClient/Dapper/Infrastructure | CLAUDE.md ข้อ 1, 8 |
| D-022 | Test | xUnit + FluentAssertions **7.0.0** (v8 เปลี่ยนเป็น license เชิงพาณิชย์) + Testcontainers SQL Server; ตั้ง `ES_TEST_SQLSERVER` เพื่อใช้ server ที่มีอยู่ | |
| D-023 | E2E | Playwright อยู่ใน `tests/e2e` แต่ติดตั้ง/รันจาก `web/` (`cd web && npx playwright test`), `tests/e2e/tsconfig.json` ชี้ `@playwright/test` ไปที่ `web/node_modules`; รันกับ stack จริงจาก compose | ตรง layout และคำสั่งใน CLAUDE.md |
| D-024 | Web | React 18 + TS + Vite 5 + Tailwind 4 + TanStack Query; ข้อความ UI ใน `web/src/i18n/th.ts`; ESLint ห้าม `any`; ฟอนต์ Noto Sans Thai bundle ผ่าน `@fontsource` (on-prem ไม่มีเน็ตก็ใช้ได้); dnd-kit จะเพิ่มตอน M4 | CLAUDE.md ข้อ 1, 8 |
| D-025 | Web serving | nginx เสิร์ฟ static + proxy `/api`, `/swagger` → origin เดียว ไม่ต้อง CORS; build stage ใช้ `node:22-bookworm-slim` (rollup บน alpine/musl build ไม่ผ่าน) | |
| D-026 | Commit | ข้อความ commit `m0: …` | CLAUDE.md ข้อ 8 |
| D-027 | ข้อความ password ตามด้วย = ใน repo | `src/docs/deployment/android.md` เป็นเอกสาร Flutter เดิมของ repo มีบรรทัด storePassword ตัวอย่าง (placeholder) — ไม่แก้ | อยู่นอกขอบเขต |
| D-028 | ติดตั้งครั้งแรก | on-prem ปิด seed; ตั้ง `BOOTSTRAP_SUPERADMIN_PASSWORD` แล้ว API สร้าง `superadmin` ให้ **เฉพาะเมื่อยังไม่มีผู้ใช้เลย** (ครั้งต่อไปไม่มีผล ไม่สร้างข้อมูลตัวอย่าง); สร้างโรงเรียน/ผู้ใช้ต่อผ่านหน้าผู้ดูแลระบบ (M7) | ผู้ใช้ให้ตัดสินเอง — ง่ายสุด ไม่ต้องมีหน้า setup wizard และไม่มีรหัสค้างในระบบ |
| D-029 | บัญชีฐานข้อมูล | service `db-init` (ใช้ sa ครั้งเดียว) สร้างฐาน `EasyScheduleWeb` COLLATE `Thai_100_CI_AS` + login `easyschedule_app` เป็น `db_owner` เฉพาะฐานนี้ (ไม่ใช่ sysadmin); API ใช้ login นี้ (`DB_APP_PASSWORD`) | ผู้ใช้ให้ตัดสินเอง — ลดสิทธิ์ของแอป; db_owner จำเป็นเพราะ API รัน migration ตอนเริ่ม |
| D-030 | โฟลเดอร์ข้อมูล on-prem | service `db-data-perms` (busybox) `chown 10001` ให้ `${DATA_DIR}/mssql` ก่อน SQL Server เริ่ม — SQL Server ยังรันแบบ non-root | ทดสอบแล้ว: ถ้าไม่ทำ SQL Server เริ่มไม่ได้ (Permission denied) |
