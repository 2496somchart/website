# DECISIONS — บันทึกการตัดสินใจ

> หมายเหตุ: ตอนทำ M0 **ไม่มีไฟล์ `CLAUDE.md`, `docs/SPEC.md` (เดิม `Spec.md.md`), `PROGRESS.md`, `DECISIONS.md` และ `PROMPT-M0.md` อยู่ใน repo**
> (ตรวจทุก branch แล้ว) จึงทำตาม prompt M0 ที่ได้รับในแชท และตัดสินใจเองในจุดที่ SPEC ควรกำหนด — ทุกข้อด้านล่างที่มีป้าย **[รอ SPEC]** ควรถูกทบทวนเมื่อมี SPEC จริง

| # | เรื่อง | การตัดสินใจ | เหตุผล |
|---|---|---|---|
| D-001 | ฐานข้อมูล | SQL Server 2022 (dev: Developer, on-prem: **Express** ฟรี), DB ชื่อ `EasyScheduleWeb` | ชื่อ DB ตาม prompt; ข้อความไทยเก็บใน `nvarchar` ทั้งหมด **[รอ SPEC]** |
| D-002 | Collation | ฐานข้อมูลสร้างด้วย `Thai_100_CI_AS` (ผ่าน `modelBuilder.UseCollation`) | เรียงลำดับภาษาไทยถูกต้อง; มี test ตรวจ |
| D-003 | โครง repo | `EasySchedule.sln` ที่ root, `src/EasySchedule.{Domain,Infrastructure,Api}`, `tests/EasySchedule.Tests`, `web/` | ไม่มี CLAUDE.md ข้อ 2 ให้ดู **[รอ CLAUDE.md]** |
| D-004 | `LoadType` | enum ตาม LoadTypeID ที่ได้รับ: 1 บริหาร (`Administration`), 2 กิจกรรม (`Activity`), 3 สอนคณะ (`TeamTeaching`), 4 ส่วนบุคคล (`Personal`), 5 สอน (`Teaching`), 6 เลือกเสรี (`FreeElective`), 7 พัก (`Break`); เก็บเป็น int, ชื่อไทยผ่าน `LoadType.ToThai()`; ค่าเริ่มต้นของ Lesson/Subject = 5 สอน | ยืนยันโดยผู้ใช้ (2026-11-18); ค่าตัวเลขเก็บใน DB ห้ามเปลี่ยน |
| D-005 | ฟิลด์ของ entity | ออกแบบเองให้ครอบคลุมการจัดตาราง (ดู `src/EasySchedule.Domain/Entities`) | **[รอ SPEC ข้อ 3]** |
| D-006 | Multi-tenant | ทุก entity ที่มี `SchoolId` implement `ITenantEntity` → global query filter อัตโนมัติ (`BypassFilter \|\| SchoolId == CurrentSchoolId`); tenant ไม่ทราบ = ไม่เห็นอะไรเลย (fail-closed); `SaveChanges` เติม `SchoolId` ให้และ**ปฏิเสธการเขียนข้ามโรงเรียน**; endpoint ตรวจ `schoolId` ใน URL กับ claim อีกชั้น → 403 | กันข้อมูลรั่ว 2 ชั้น |
| D-007 | SuperAdmin | `User.SchoolId = null`, ข้าม filter ได้ (เห็นทุกโรงเรียน) | ตาม role ใน prompt |
| D-008 | Cascade delete | ปิด cascade ทั้งหมด ยกเว้น `Placement→Snapshot`, `Placement→Lesson`, `LessonTeacher→Lesson` | ตาม prompt; กันลบโรงเรียน/ภาคเรียนแล้วข้อมูลหายเป็นทอด ๆ |
| D-009 | Auth | JWT (HS256) ใน `Authorization: Bearer`, claim `role` + `school_id`; รหัสผ่าน hash ด้วย PBKDF2 ของ ASP.NET Identity `PasswordHasher` | ง่ายสำหรับ on-prem ไม่ต้องพึ่ง IdP ภายนอก |
| D-010 | ความลับ | ไม่มีรหัสผ่าน/connection string ใน git: compose ประกอบ connection string จาก `Database__Server/User/Password` ที่ดึงจาก `.env`; รหัสผ่าน user ตัวอย่างมาจาก `SEED_USER_PASSWORD`; test สุ่มค่าเองทุกครั้ง | เกณฑ์ M0 |
| D-011 | Seed | seed ตอน runtime (ไม่ใช้ `HasData`) เมื่อ `Seed__Enabled=true`, idempotent; user ตัวอย่าง `superadmin`, `admin`, `viewer` | ต้องใช้รหัสผ่านจาก env จึงใส่ใน migration ไม่ได้ |
| D-012 | Migration | API รัน `Migrate()` ตอนเริ่มเมื่อ `Database__MigrateOnStartup=true` (เปิดใน compose) | ติดตั้งในโรงเรียนได้ด้วยคำสั่งเดียว |
| D-013 | Integration test | Testcontainers (SQL Server จริง) + `WebApplicationFactory`; ตั้ง `ES_TEST_SQLSERVER` เพื่อใช้ server ที่มีอยู่แทน | ทดสอบ Thai round-trip / collation กับ engine จริง ไม่ใช่ SQLite |
| D-014 | Web | React 18 + Vite 5 + Tailwind 4 + react-router; ฟอนต์ Noto Sans Thai bundle ผ่าน `@fontsource` (ไม่โหลดจาก Google) | on-prem อาจไม่มีอินเทอร์เน็ต |
| D-015 | Web serving | nginx เสิร์ฟไฟล์ static และ proxy `/api`, `/swagger` ไป API → origin เดียว ไม่ต้องตั้ง CORS | |
| D-016 | Node image | build stage ใช้ `node:22-bookworm-slim` ไม่ใช่ alpine | rollup บน musl build ไม่ผ่าน ("Code is nested too deeply") |
| D-017 | E2E | Playwright รันกับ stack จริงจาก `docker compose` (baseURL `E2E_BASE_URL`, ค่าเริ่มต้น `http://localhost:8080`) | ตรวจเกณฑ์ "login → เห็นระดับชั้น" แบบ end-to-end |
| D-018 | ข้อความ password+`=` ใน repo | `src/docs/deployment/android.md` (เนื้อหาเว็บไซต์ Flutter เดิมของ repo) มีบรรทัด storePassword ตัวอย่างพร้อม placeholder `<password from previous step>` — เป็นเอกสารตัวอย่าง ไม่ใช่ความลับ ไม่ได้แก้ | อยู่นอกขอบเขต M0 |
