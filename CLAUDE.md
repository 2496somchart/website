# CLAUDE.md — EasySchedule Web

Web App จัดตารางสอนโรงเรียนไทย ป.1-ม.6 (multi-school) — สร้างใหม่ทั้งหมด แยกจากระบบเดิม

**Source of truth = `docs/SPEC.md`** (export จาก Spec doc). ถ้า CLAUDE.md กับ SPEC ขัดกัน ให้ SPEC ชนะ แล้วบันทึกใน `docs/PROGRESS.md` หัวข้อ "คำถามค้าง"

---

## 1. Stack (ห้ามเปลี่ยนเองโดยไม่ถาม)

| ชั้น | เทคโนโลยี |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API (C#), JWT, Swagger |
| Solver | C# class library `EasySchedule.Solver` — ไม่อ้าง DB/EF เลย |
| Data | SQL Server 2019+ (Express ได้), EF Core 8 (CRUD + migrations), Dapper (bulk read) |
| Frontend | React 18 + TypeScript + Vite + Tailwind, dnd-kit, TanStack Query |
| Report | QuestPDF (ฝังฟอนต์ TH Sarabun New), ClosedXML |
| Test | xUnit + FluentAssertions, Testcontainers (SQL Server), Playwright |
| Deploy | Docker Compose — `docker-compose.yml` (cloud) และ `docker-compose.onprem.yml` (ในโรงเรียน) ต่างกันแค่ config |

## 2. Repo layout

```
/src
  EasySchedule.Domain          entities, enums, value objects (ไม่มี dependency)
  EasySchedule.Solver          ConstraintChecker + ISolver + GreedySolver + RepairSolver + LocalSearch
  EasySchedule.Infrastructure  EF Core DbContext, migrations, repositories
  EasySchedule.Importer        อ่าน Sch69 / SchKUS (read-only) → DB ใหม่ + anonymize ชื่อครู
  EasySchedule.Api             controllers, auth, SignalR progress hub, report endpoints
/web                           React app
/tests
  EasySchedule.Solver.Tests    unit test ทุก hard constraint H1-H15 (ผ่าน 1 + ไม่ผ่าน 1 ต่อข้อ)
  EasySchedule.Api.Tests       integration test (Testcontainers)
  EasySchedule.Benchmark       รัน solver กับข้อมูล import แล้วพิมพ์ shortfall / violations / เวลา
  e2e                          Playwright
/docs  SPEC.md  PROGRESS.md  DECISIONS.md
```

## 3. Commands

```bash
docker compose up -d db                    # SQL Server dev
dotnet build
dotnet test                                # ต้องผ่านทั้งหมดก่อน commit
dotnet run --project tests/EasySchedule.Benchmark -- --school Sch69 --mode 0
cd web && npm ci && npm run dev
cd web && npx playwright test
docker compose up --build                  # ทั้งระบบ → http://localhost:8080
```

## 4. กฎเหล็ก (ห้ามละเมิด)

1. **ระบบเดิมห้ามแตะ** — Sch69.accde, EasySchedule_Engine, ฐาน Sch* ทั้งหมดใช้งานต่อตามปกติ
   - Importer อ่านจาก DB ที่ restore จาก .bak เป็น `Sch69_Import` / `SchKUS_Import` เท่านั้น
   - connection string ของ Importer ต้องมี `ApplicationIntent=ReadOnly` และ login ที่มีแค่ `db_datareader`
   - ห้าม INSERT/UPDATE/DELETE/ALTER ใดๆ กับฐาน `*_Import`
2. **DB ใหม่ชื่อ `EasyScheduleWeb`** — DB เดียว ทุกตารางมี `SchoolId` + EF Core global query filter
3. **ไม่มี state กลาง** — SchoolId มาจาก JWT ของแต่ละ request เท่านั้น ห้าม static/singleton เก็บ "โรงเรียนที่ active"
4. **กฎการจัดตารางอยู่ใน `ConstraintChecker` ที่เดียว** — solver, drag-drop, API ใช้ตัวเดียวกัน ห้ามใช้ trigger ตรวจกฎใน DB
5. **ห้าม hardcode** ค่าที่โรงเรียนตั้งได้: PeriodPerDay, BreakSlot, MaxConsec, วัน ร.ด., จำนวนห้อง
6. **ภาษาไทยต้องไม่เพี้ยน** — ไฟล์ทั้งหมด UTF-8, คอลัมน์ข้อความเป็น `nvarchar`, มี test round-trip ข้อความไทยผ่าน API และ PDF
7. **ข้อความ error ที่ผู้ใช้เห็นเป็นภาษาไทย** และบอกเหตุผล เช่น "ครู ก. สอน ม.2/3 อยู่แล้วในคาบนี้"
8. **ห้ามใส่ secret ใน source** — ใช้ `dotnet user-secrets` (dev) / environment variable (deploy)
9. **ไม่เก็บค่าที่คำนวณได้** — จำนวนคาบที่วางแล้ว (OP) คำนวณจาก COUNT(Placement) เสมอ
10. **ชื่อครูจริงห้ามเข้า DB ใหม่** — Importer แทนด้วย `ครู 001`, `ครู 002` … (เก็บ mapping ไว้ในไฟล์นอก repo เท่านั้น)

## 5. Domain glossary

| คำ | ความหมาย |
|---|---|
| ClassLevel code | 11-16 = ป.1-ป.6, 21-23 = ม.1-ม.3, 34-36 = ม.4-ม.6 · ลงท้าย 0 (10, 20, 30) = รหัสรวมช่วงชั้น ไม่ใช่ห้องจริง · 99 = การกันเวลาครู |
| DayNo | 2-6 = จันทร์-ศุกร์ |
| PeriodNo | 0 = โฮมรูม (ไม่นับเป็นคาบสอน), 1-12 |
| LoadType | 1 ภาระบริหาร (ไม่จัด) · 2 กิจกรรม/ร.ด. · 3,4 กันเวลาครู · 5 วิชาปกติ · 6 วิชาเลือก · 7 คาบพัก |
| Lesson | ภาระสอน 1 วิชา × 1 ห้อง (= TblXPlan เดิม) มี PPW = คาบต่อสัปดาห์, PT = คาบต่อครั้ง |
| Placement | การวาง Lesson ลงช่อง วัน × คาบ (= TblZPlan เดิม) ผูกกับ Snapshot |
| LessonGroup | Lesson ที่ต้องอยู่คาบเดียวกัน (team teaching, วิชาเลือกคู่ขนาน) |
| BreakSlot | คาบพักรายห้อง-รายวัน ค่าเริ่มต้นตรงกันทั้งโรงเรียน แก้ได้อิสระ |
| Mode 0 / 1 | 0 = 5 วัน × 8 คาบ · 1 = 4 วัน × 10 คาบ + วัน ร.ด. — ใช้พร้อมกันทั้งโรงเรียน |
| Term | ปี/เทอมของ**งานจัดตาราง** (= SchYr/SchTr เดิม) ไม่ใช่ปีทะเบียน (Yr/Tr) |

## 6. วิธีทำงาน (autonomous loop)

**ทำเองได้ ไม่ต้องถาม**: เขียน/แก้โค้ด, เพิ่ม NuGet/npm package, รัน test, แก้ test ที่ fail (ด้วยการแก้โค้ด), ตั้งชื่อ class/ไฟล์, commit บน branch `feature/m<N>-<slug>`

**ต้องหยุดรอคนอนุมัติ**: merge เข้า `main`, deploy, แก้ DB ที่ไม่ใช่ dev container, เปลี่ยน hard constraint, ตัด feature, เปลี่ยน stack, ลดเกณฑ์ benchmark

**ห้ามเด็ดขาด**: แก้ test ให้ผ่านโดยไม่แก้โค้ดจริง, `[Skip]` test ที่ fail, ลบ assertion, เขียนลงฐานเดิม

**รอบต่อ milestone**
1. อ่าน `docs/SPEC.md` ส่วนของ milestone นั้น + `docs/PROGRESS.md`
2. เขียน test ตามเกณฑ์ผ่านก่อน (red)
3. เขียนโค้ดจน test ผ่านทั้งหมด รวม milestone ก่อนหน้า (green)
4. refactor, `dotnet format`, `npm run lint`
5. อัปเดต `docs/PROGRESS.md` (ทำอะไร, ตัวเลข benchmark, คำถามค้าง) และ `docs/DECISIONS.md` (ทางเลือกที่ตัดสินเอง + เหตุผล)
6. commit, push branch, เปิด PR แล้ว**หยุด**

**ติดปัญหาเดิมเกิน 3 รอบ หรือ spec ขัดกันเอง** → เขียนคำถามใน `PROGRESS.md` หัวข้อ "คำถามค้าง" แล้วไปทำส่วนอื่นต่อ ห้ามเดา

**Spec ไม่ได้บอก** → เลือกทางที่ตรงกับพฤติกรรมระบบเดิม แล้วบันทึกใน `DECISIONS.md`

## 7. Benchmark (เกณฑ์ M3)

ใช้ข้อมูล Sch69 ที่ import แล้ว ต้องได้:
- hard constraint violation = **0**
- shortfall ≤ ระบบเดิม (วัดใหม่จากฐานปัจจุบันก่อนเริ่ม M3 แล้วจดใน PROGRESS.md)
- เวลา ≤ 60 วินาที, seed เดิมได้ผลเดิม (deterministic)

## 8. Code style

- C#: nullable enabled, file-scoped namespace, `record` สำหรับ DTO, async ตลอด, ไม่ใช้ `dynamic`
- Solver: pure function เท่าที่ทำได้ — input `SolverInput`, output `SolverResult` (placements + unplaced พร้อมเหตุผล)
- React: function component + hooks, ไม่มี `any`, ข้อความ UI อยู่ใน `web/src/i18n/th.ts`
- Commit message: `m<N>: <สิ่งที่ทำ>` เช่น `m3: add H4 teacher max-consecutive check`
