# Spec: EasySchedule Web App (สำหรับ Claude Code)

Nov 18, 2026 · @สมชาติ หรั่งเจริญ

## 1. ภาพรวมและขอบเขต

สร้าง Web App จัดตารางสอนโรงเรียนมัธยมไทย (ป.1-ม.6) ที่ให้ผลการจัดตาราง **ดีเท่าหรือดีกว่า EasySchedule (Sch69)** โดยใช้เงื่อนไขชุดเดียวกัน แต่เขียน solver ใหม่เป็น C# ที่ทดสอบได้ ไม่พึ่ง SP ที่ WITH ENCRYPTION

**ระบบเดิมคงไว้ตามเดิม**: โปรแกรม Sch69.accde, EasySchedule\_Engine และฐาน Sch\* ทั้งหมดใช้งานต่อได้ตามปกติ ระบบใหม่สร้าง DB ใหม่แยกต่างหาก และอ่านฐานเดิมแบบ read-only เพื่อ import เท่านั้น

**เป้าหมายเวอร์ชันแรก (v1)**

- โรงเรียนกรอก/นำเข้า ครู ห้อง วิชา ชั้น-ห้อง ภาระสอน แล้วกดจัดตารางอัตโนมัติได้ใน 1 ปุ่ม
- แก้ตารางด้วยมือแบบ drag-drop โดยระบบบล็อกการวางที่ผิดเงื่อนไขทันที พร้อมบอกเหตุผลเป็นภาษาไทย
- พิมพ์/ส่งออก ตารางรายห้อง รายครู รายห้องเรียน (PDF + Excel)
- รองรับโหมด ร.ด. (4 วัน x 10 คาบ + 1 วัน ร.ด.) และวิชาเลือก
- หลายโรงเรียนในระบบเดียว (multi-tenant)

**ไม่ทำใน v1**

- ระบบทะเบียน-วัดผล และระบบรับสมัคร (เชื่อมทีหลังผ่าน API)
- จัดสอบ/จัดห้องสอบ, สอนแทน (เลื่อนไป v2)
- แอปมือถือ native (ใช้ responsive web แทน)

## 2. Tech stack และสถาปัตยกรรม

ใช้ .NET 8 + SQL Server เพราะต่อกับฐาน Sch69 เดิมได้ตรง และคุณดูแลต่อได้เอง

| ชั้น | เทคโนโลยี | หมายเหตุ |
| --- | --- | --- |
| Frontend | React + TypeScript + Vite, Tailwind | ตาราง drag-drop ด้วย dnd-kit, ภาษาไทยเป็นค่าเริ่มต้น |
| API | ASP.NET Core 8 Web API (C#) | REST + JWT, OpenAPI/Swagger |
| Solver | C# class library `EasySchedule.Solver` | ไม่ผูกกับ DB — รับ input model, คืน placement ทดสอบด้วย unit test ได้ |
| DB | SQL Server 2019+ (Express ได้) | DB ใหม่ EasyScheduleWeb แยกจากฐานเดิม, DB เดียว + SchoolId |
| Data access | EF Core (CRUD) + Dapper (อ่านก้อนใหญ่ให้ solver) | ห้ามใช้ synonym routing |
| Report | QuestPDF (PDF), ClosedXML (Excel) | ฟอนต์ TH Sarabun New ฝังใน PDF |
| Test | xUnit, Playwright (E2E) | รันได้ด้วย `dotnet test` / `npx playwright test` |
| Deploy | Docker Compose (api + web + db) — เลือกได้: Cloud (VPS) หรือติดตั้งในโรงเรียน | ชุดติดตั้งเดียวกันทั้ง 2 แบบ ต่างกันแค่ config |

**หลักสถาปัตยกรรม**

1. กฎการจัดตารางทั้งหมดอยู่ใน C# (`ConstraintChecker`) ที่เดียว — ทั้ง solver และการวางด้วยมือเรียกตัวเดียวกัน ไม่ใช้ INSTEAD OF trigger
2. Solver ทำงานบน in-memory model แล้วเขียนผลลง DB ใน transaction เดียว (all-or-nothing)
3. ทุกครั้งที่จัดตาราง เก็บเป็น Snapshot ใหม่ ย้อนกลับได้ (แทน TblZPlanPark)
4. งานจัดตารางรันเป็น background job แสดง progress ผ่าน SignalR

## 3. Data model

แยก "ภาระสอน" (Lesson = TblXPlan) ออกจาก "การวางลงตาราง" (Placement = TblZPlan) เหมือนระบบเดิม ทุกตารางมี `SchoolId` + `TermId`

| Entity | เทียบกับระบบเดิม | ฟิลด์หลัก |
| --- | --- | --- |
| School | TblSchoolInfo | Name, SchCode, LicenseTier |
| Term | SchYr/SchTr | Year (พ.ศ.), Term (1/2), IsActive — **ปี/เทอมจัดตาราง ไม่ใช่ปีทะเบียน** |
| ScheduleMode | TblSchedulePlan | Mode (0 ปกติ / 1 ร.ด.), DaysList, PeriodPerDay, MilitaryDay |
| Period | TblPeriod | PeriodNo 0-12, Start, End (P0 = โฮมรูม) |
| ClassLevel | ClassLevelID | Code (11-16 = ป.1-ป.6, 21-23 = ม.1-ม.3, 34-36 = ม.4-ม.6; รหัสลงท้าย 0 เช่น 10/20 = รหัสรวมช่วงชั้น ไม่ใช่ห้องจริง), Name, StartPeriod, EndPeriod |
| ClassSection | ClassSectionNo | LevelId, SectionNo, HomeroomId — จำนวนห้องมาจาก record จริง ไม่ใช่ตัวเลข ToClassRoomID |
| Teacher | TblTeacher | Code, Name, MaxConsecPeriods (default 2) |
| Room | TblRoom | Type (R ห้องเรียน / L ปฏิบัติการ), Capacity |
| Subject | TblClassSubjects | Code, Title, SubjectGroup (G), Credits, PPW, PT |
| StudyPlan | PlanTitleList | แผนการเรียน — **ตารางลูก ไม่ใช่ string คั่นด้วย comma** |
| Lesson | TblXPlan | SectionId, SubjectId, LoadType, PPW, PT, RoomId, LabId, AllowSplit, MaxConsec, LessonGroupId |
| LessonTeacher | TblClassTeacher / TeacIDList | LessonId, TeacherId, Ordinal (1 = อาจารย์หลัก) |
| LessonGroup | PlanGroup / fn\_XPlanIDList | กลุ่มที่ต้องวางคาบเดียวกัน (team teaching, วิชาเลือกคู่ขนาน) |
| SubjectRule | TblXPlanConstraint | Subject1, Subject2, Type (C ต่อเนื่อง / U ห้ามวันเดียวกัน / S ลำดับ) |
| Reservation | TblEPlan + LoadType 3/4 | บล็อกเวลา รายแผนการเรียน / รายครู (สอนคณะ, พบแพทย์) / รายห้อง |
| Snapshot | (ใหม่) + TblZPlanPark | Id, ModeId, CreatedAt, CreatedBy, Note, IsLive |
| Placement | TblZPlan | SnapshotId, LessonId, DayNo (2-6), PeriodNo, RoomId, IsLocked, CreatedBy, CreatedAt |

**BreakSlot (ใหม่)**: SectionId, DayNo, PeriodNo — คาบพักเก็บเป็นช่องรายห้อง-รายวัน ค่าเริ่มต้นตั้งให้ตรงกันทั้งโรงเรียน (ประถม ม.ต้น ม.ปลาย) แล้วปรับแก้ได้อิสระทุกระดับ ทุกห้อง ทุกวัน มีปุ่ม "ใช้ค่าเดียวกันทั้งระดับ/ทั้งโรงเรียน"

**LoadType (คงค่าเดิมเพื่อ import ได้ตรง)**: 1 ภาระบริหาร (ไม่จัด) · 2 กิจกรรม เช่น ร.ด. · 3/4 การกันเวลาครู · 5 วิชาปกติ · 6 วิชาเลือก · 7 คาบพัก (แยกจาก 5 ให้ชัดเจน — ระบบเดิมปนกันจนเกิดบั๊ก Guard 10)

**Importer**: อ่านจาก Sch69 (TblXPlan, TblZPlan, TblClassTeacher, TblClassPlan, TblRoom, TblXPlanConstraint, TblEPlan) → แปลงเข้า model ใหม่ เพื่อใช้เป็นข้อมูลทดสอบและ benchmark ต้องจับ Year/Term จาก SchYr/SchTr และแยก PlanTitleList ทั้งแบบ ',ม.ต้น,' และ 'ม.ต้น'

**ข้อมูลทดสอบ**: Sch69 + SchKUS — Importer ต้องแทนชื่อครูจริงด้วยชื่อสมมติ (เช่น ครู 001) ก่อนเขียนลง DB ใหม่ และห้ามเขียนกลับฐานเดิม

## 4. Hard constraints (ห้ามผิดเด็ดขาด)

ทุกข้อมาจาก Guard ใน Tg\_Prevent\_ZPlanConflict v3.9 ต้องมี unit test อย่างน้อย 1 กรณีผ่าน + 1 กรณีไม่ผ่าน และคืนข้อความภาษาไทยที่บอกเหตุผล

| รหัส | กฎ | ข้อยกเว้น |
| --- | --- | --- |
| H1 | PeriodNo ≤ PeriodPerDay ของโหมด และอยู่ใน StartPeriod-EndPeriod ของระดับชั้น | P0 = โฮมรูม ไม่นับเป็นคาบสอน |
| H2 | ไม่ทับ Reservation (เวลาที่กันไว้ของแผนการเรียน/ครู/ห้อง) | วิชาเลือกที่เป็นเจ้าของ Reservation นั้น |
| H3 | ห้องเรียน 1 ห้อง มีได้ 1 วิชาต่อคาบ | LessonGroup เดียวกัน และวิชาเลือก (นักเรียนเลือกเรียนวิชาเดียว) |
| H4 | ครูสอนติดกันไม่เกิน MaxConsec (default 2) | MaxConsec = 0 ปิดการตรวจ; ข้าม พัก/HR และ ClassLevel 99 |
| H5 | วิชาเดียวกันในวันเดียวกันต้องต่อเนื่อง ไม่แยกช่วง | AllowSplit = 1 |
| H6 | ห้อง/แล็บ 1 ห้อง ใช้ได้ 1 กลุ่มต่อคาบ | LessonGroup เดียวกัน |
| H7 | ครู 1 คนอยู่ได้ 1 ที่ต่อคาบ (รวมทุกคนใน team teaching) | สอนหลายห้องพร้อมกันใน LessonGroup เดียวกัน |
| H8 | SubjectRule U: สองวิชาห้ามอยู่วันเดียวกัน | — |
| H9 | SubjectRule C/S: ต้องต่อเนื่อง / ต้องเรียงลำดับ | — |
| H10 | LoadType 5/6: ติดกันได้ไม่เกิน 2 คาบ และห้ามคร่อมพักเที่ยง (เช้า+บ่ายวันเดียวกัน) | ใช้กับ LoadType 5/6 เท่านั้น ไม่เกี่ยวกับคาบพัก/กิจกรรม |
| H11 | ช่องที่เป็นคาบพัก (BreakSlot) ห้ามวางวิชา — ค่าเริ่มต้นตรงกันทั้งโรงเรียน ปรับได้ทุกระดับ ทุกห้อง ทุกวัน — ตรวจ**รายห้อง** ไม่ใช่รายกลุ่ม | กิจกรรมที่ AllowSplit = 1 (ร.ด.) |
| H12 | จำนวนคาบที่วาง ≤ PPW ของ Lesson | — |
| H13 | Placement ที่ IsLocked = 1 solver ห้ามย้าย/ลบ | ผู้ใช้ปลดล็อกเอง |
| H14 | Lesson LoadType 1 ไม่ถูกจัดลงตารางเลย | — |
| H15 | License tier: จำนวนห้องต่อระดับชั้น ≤ 4/8/12/16/20 ตาม tier | ตรวจก่อนเริ่ม solver แล้วหยุดพร้อมข้อความไทย |

ทุกกฎที่มีค่าตัวเลข (MaxConsec, BreakPeriod, PeriodPerDay) อ่านจาก DB ห้าม hardcode

## 5. Soft constraints และ scoring

Solver ต้องวางครบ PPW ก่อน (เป้าหมายอันดับ 1) แล้วค่อยลดคะแนนโทษด้านล่าง น้ำหนักตั้งค่าได้ต่อโรงเรียน

| รหัส | กฎ | น้ำหนักเริ่มต้น |
| --- | --- | --- |
| S0 | คาบที่ยังวางไม่ได้ (shortfall = PPW − คาบที่วาง) | 1000 ต่อคาบ |
| S1 | คณิต (G=2) / วิทย์ (3) / อังกฤษ (8) ควรอยู่ช่วงเช้า (≤ P4) | 5 ต่อคาบที่ไม่ได้ |
| S2 | พละ/ศิลปะ/กิจกรรม (G=5, 6, 9, A) ไม่ควรอยู่ P1 | 5 |
| S3 | ครูคนเดิมสอน P1 ของห้องเดิมซ้ำหลายวัน (P1 teacher spread) | 3 ต่อวันที่ซ้ำ |
| S4 | ตารางห้องเรียนมีคาบว่างตรงกลางวัน (gap) | 10 ต่อ gap |
| S5 | P1 ของห้องเรียนว่าง | 8 |
| S6 | วิชาเดียวกันกระจุกอยู่บางวัน แทนที่จะกระจายทั้งสัปดาห์ | 2 |
| S7 | ภาระครูต่อวันไม่สมดุล (วันหนึ่งเต็ม อีกวันว่าง) | 1 |

**อัลกอริทึมแนะนำ** (ให้ Claude Code เลือกได้ แต่ต้องผ่าน benchmark ข้อ 9)

1. Greedy เรียงตามความยาก (ครูที่ภาระหนัก / team teaching / คาบคู่ ก่อน) — เทียบเท่า sp\_AllStep
2. Repair ด้วย direct move → indirect swap → multi-level chain — เทียบเท่า sp\_AutoSwapXPlanID
3. Local search / simulated annealing ลด S1-S7 โดยไม่ทำให้ S0 แย่ลง
4. ตั้ง time limit ได้ (default 60 วินาที) และ seed คงที่เพื่อให้ผล deterministic

ทางเลือก: Google OR-Tools CP-SAT (มี NuGet สำหรับ .NET) — ถ้าใช้ ต้องทำเป็น `ISolver` implementation ที่สลับกับ greedy ได้ และวัดผลเทียบกัน

## 6. โหมด ร.ด. วิชาเลือก และลำดับงานจัดตาราง

โรงเรียนมี 2 โหมด แต่ละโหมดเป็น Snapshot คนละชุด สลับไปมาได้โดยไม่ต้องจัดใหม่

| โหมด | วัน | คาบ/วัน | คาบสอนสุทธิ/สัปดาห์ |
| --- | --- | --- | --- |
| 0 ปกติ | จ-ศ (5 วัน) | 8 | 40 − 5 พัก = 35 |
| 1 ร.ด. | 4 วัน + วัน ร.ด. 1 วัน | 10 | 40 − 4 พัก = 36 (+ กิจกรรม ร.ด. 9 คาบ) |

**กฎโหมด ร.ด.**

- วัน ร.ด. อ่านจาก ScheduleMode.MilitaryDay ห้าม hardcode วันอังคาร — 1 ปีการศึกษาเรียน ร.ด. เพียงเทอมเดียว (เลือกได้ว่าเทอม 1 หรือ 2)
- โหมดใช้ตรงกันทั้งโรงเรียน (ป.1-ม.6 เป็น 4 วัน x 10 คาบ + วัน ร.ด. พร้อมกัน) · ม.4-6: วางวิชา "รักษาดินแดน" เต็มวัน · ป.1-ม.3: วางกิจกรรม "หยุด รด." เต็มวัน
- กิจกรรม ร.ด. เป็น LoadType 2, AllowSplit = 1 (คร่อมพักได้), MaxConsec ≥ จำนวนคาบต่อเนื่องที่ยาวที่สุด, IsLocked = 1 — ระบบตั้งค่าเองทุกครั้ง ไม่พึ่งค่าที่ค้างอยู่
- คาบพักของวัน ร.ด. คงอยู่ตาม BreakSlot ของแต่ละห้อง
- สลับโหมด = เปลี่ยน Snapshot ที่ IsLive ตัดสินโหมดปัจจุบันจาก**ข้อมูล** ไม่ใช่ flag ที่เก็บแยก (บทเรียน IsActive เพี้ยนในระบบเดิม)

**กฎวิชาเลือก (LoadType 6)**

- จองเวลาราย**แผนการเรียน** ไม่ใช่รายวิชา — ห้องเดียวมีหลายแผนได้
- วิชาเลือกในแผนเดียวกันวางซ้อนคาบเดียวกันได้ — นับความต้องการเป็น MAX(PPW) ต่อแผน ไม่ใช่ SUM
- วิชาเลือกที่ไม่มีแผนการเรียน จัดไม่ได้ — แสดงใน pre-flight check
- จำกัดวิชาเลือกไว้ที่ P1-P8 และไม่ลงวัน ร.ด.

**ลำดับงานที่ solver ต้องทำตาม (ห้ามสลับ)**

1. Pre-flight check: ห้องเกินจำนวนห้องจริง (orphan section), วิชาไม่มีครู, วิชาเลือกไม่มีแผน, ความจุไม่พอ, license tier — พบปัญหาให้หยุดและบอกเป็นภาษาไทย ไม่ใช่คืนตารางว่างเงียบๆ
2. วางคาบพัก + โฮมรูม (lock)
3. (โหมด 1) วางกิจกรรม ร.ด. เต็มวัน (lock)
4. วาง Reservation และวิชาเลือก (หลีกเลี่ยงวัน ร.ด.)
5. จัดวิชาปกติ LoadType 5
6. Repair + optimise soft constraints
7. บันทึกเป็น Snapshot ใหม่ พร้อมรายงาน shortfall + เหตุผลที่วางไม่ได้รายคาบ

## 7. หน้าจอและฟีเจอร์

| หน้าจอ | ทำอะไร | Milestone |
| --- | --- | --- |
| เข้าสู่ระบบ / เลือกโรงเรียน / เลือกปี-เทอม | Login, สลับปี/เทอมจัดตาราง | M1 |
| ข้อมูลพื้นฐาน | ครู, ห้อง, วิชา, ชั้น-ห้อง, คาบเวลา, แผนการเรียน (CRUD + import Excel) | M1 |
| มอบหมายผู้สอน | ตาราง วิชา × ห้อง คลิกเซลล์เลือกครู (คลิกซ้ำ = ยกเลิก, ลำดับคลิก = Ordinal) แทน listbox + ปุ่มลูกศร | M2 |
| กฎการจัด | SubjectRule (C/U/S), Reservation, น้ำหนัก soft constraint | M2 |
| ตรวจความพร้อม (pre-flight) | รายการปัญหาก่อนจัด คลิกแล้วไปที่ข้อมูลที่ต้องแก้ | M3 |
| จัดตารางอัตโนมัติ | ปุ่มเดียว + progress bar + สรุปผล (วางได้/ขาด/เหตุผล) | M3 |
| ตารางสอน (grid) | มุมมองรายห้อง/รายครู/รายห้องเรียน วัน × คาบ, เซลล์ = รหัสวิชา + ครู + ห้อง, สีตามกลุ่มสาระ | M3 |
| แก้ด้วยมือ | drag-drop, ไฮไลต์ช่องที่วางได้ (เขียว) / ต้องสลับ (เหลือง) / ผิดกฎ (แดง + tooltip เหตุผล), ล็อก/ปลดล็อก, undo/redo | M4 |
| เติมคาบว่าง | หา gap + P1 ว่าง แล้วเสนอผู้สมัครเรียงตามคะแนน (เทียบ FrmFillPeriod v2) | M4 |
| โหมด ร.ด. | กำหนดวันและเทอม ร.ด., สร้าง/สลับโหมด | M5 |
| Snapshot | รายการเวอร์ชัน, เทียบ 2 เวอร์ชัน, กู้คืน | M5 |
| รายงาน | ตารางรายห้อง/รายครู/รายห้องเรียน PDF (A4 แนวนอน, พิมพ์ทุกห้องในไฟล์เดียว), Excel, สรุปภาระครู | M6 |
| ผู้ดูแลระบบ | จัดการโรงเรียน, ผู้ใช้, license | M7 |
| หน้าจอจัดตารางสอน | ตามแบบเดิม รายวิชา ตารางครู ตารางนักเรียน ตารางการใช้ห้อง |  M3 |

UI ภาษาไทยทั้งหมด วันที่แสดงเป็น พ.ศ. ต้องใช้งานได้บนจอ 1366×768 และ tablet

## 8. Multi-school, license และสิทธิ์ผู้ใช้

บนเว็บไม่ต้องมี fingerprint, poison file หรือ DbDefence — ควบคุมที่ server ทั้งหมด

**Tenant**

- ทุก query กรอง SchoolId ผ่าน EF Core global query filter + ทดสอบว่าโรงเรียน A มองไม่เห็นข้อมูลโรงเรียน B
- ห้ามมี state กลางแบบ "โรงเรียนที่ active" (บทเรียน synonym ชี้ผิด DB ในระบบเดิม) — SchoolId มาจาก token ของผู้ใช้แต่ละ request

**License (เก็บที่ตาราง School)**

| ค่า | ความหมาย |
| --- | --- |
| Plan | Trial / Licensed / Expired |
| MaxSectionsPerLevel | 4, 8, 12, 16, 20 (Trial = 4) |
| ExpiresAt | หมดอายุแล้วดูตารางเดิมได้ แต่จัดใหม่ไม่ได้ |
| TrialPlacementCap | เพดานจำนวนคาบที่วางได้ตอนทดลอง (ค่าเริ่ม 200 เทียบ Trial Gate v2) |

**บทบาทผู้ใช้**

| Role | สิทธิ์ |
| --- | --- |
| SuperAdmin (คุณ) | สร้างโรงเรียน, กำหนด license, ดูทุกโรงเรียน |
| SchoolAdmin (ฝ่ายวิชาการ) | แก้ไขทุกอย่างในโรงเรียนตัวเอง, จัดตาราง, เพิ่มผู้ใช้ |
| Viewer (ครู) | ดู/พิมพ์ตารางที่ publish แล้ว |

ทุกการเปลี่ยนแปลงตารางเก็บ audit log (ใคร ทำอะไร เมื่อไร) ห้ามใส่ secret ใน source code — ใช้ environment variable / user-secrets

## 9. Milestones และ acceptance criteria

Claude Code ทำทีละ milestone จะขึ้น milestone ถัดไปได้เมื่อผ่านทุกข้อในคอลัมน์เกณฑ์แล้วเท่านั้น

| # | งาน | เกณฑ์ผ่าน (ตรวจได้ด้วยคำสั่ง) | ประมาณ |
| --- | --- | --- | --- |
| M0 | Repo, Docker Compose, CI, schema + migration | `docker compose up` แล้วเปิดเว็บได้; `dotnet test` ผ่าน | 2-3 วัน |
| M1 | ข้อมูลพื้นฐาน + login + **Importer จาก Sch69** | import Sch69 + SchKUS (ตัดชื่อครูจริงแล้ว) แล้วจำนวนครู/ห้อง/Lesson/PPW รวมตรงกับ query ต้นทาง 100% | 1 สัปดาห์ |
| M2 | มอบหมายผู้สอน, SubjectRule, Reservation | E2E test สร้าง team teaching 2 ครู + ลำดับ Ordinal ถูก | 1 สัปดาห์ |
| M3 | ConstraintChecker + Solver + pre-flight + grid แสดงผล | ดู benchmark ด้านล่าง | 2-4 สัปดาห์ |
| M4 | Drag-drop, lock, undo, เติมคาบว่าง | วางผิดกฎทุกข้อ H1-H13 ต้องถูกปฏิเสธพร้อมเหตุผลไทย (Playwright) | 1-2 สัปดาห์ |
| M5 | โหมด ร.ด. + Snapshot | สลับ 0→1→0 แล้วตารางโหมด 0 กลับมาเหมือนเดิมทุกเซลล์; วัน ร.ด. เต็ม (กิจกรรม + พัก = ทุกห้อง × 10) | 1 สัปดาห์ |
| M6 | รายงาน PDF/Excel | PDF ภาษาไทยไม่เพี้ยน, 43 ห้องในไฟล์เดียว < 10 วินาที | 1 สัปดาห์ |
| M7 | Multi-school, license, role, deploy VPS | test แยก tenant ผ่าน; Trial จัดห้องที่ 5 ไม่ได้ | 1 สัปดาห์ |

**Benchmark ของ M3 (สำคัญที่สุด)** — ใช้ข้อมูล Sch69 ที่ import มา เทียบกับผลของ sp\_AllStep เดิม (วัดเมื่อ 14 ส.ค. 2569)

| ตัววัด | ระบบเดิม โหมด 0 | ระบบเดิม โหมด 1 | เป้าหมายระบบใหม่ |
| --- | --- | --- | --- |
| Hard constraint ที่ผิด (ครูชนกัน) | 28 | 26 | **0** |
| คาบที่วางไม่ได้ (shortfall) | 19 | 3 | **≤ 19 / ≤ 3** |
| เวลาจัดทั้งโรงเรียน | \~2 วินาที | \~2 วินาที | ≤ 60 วินาที |
| ผลซ้ำได้ (seed เดิม = ผลเดิม) | — | — | ใช่ |

ก่อนเริ่ม M3 ให้วัดตัวเลขของระบบเดิมใหม่จากฐานปัจจุบันก่อน เพราะข้อมูลเปลี่ยนไปแล้ว (เช่น ม.1 เคยเป็น 8 ห้อง)

## 10. กฎการทำงานอัตโนมัติของ Claude Code

คัดลอกส่วนนี้ไปไว้ใน `CLAUDE.md` ที่ root ของ repo

**ทำเองได้เลย (ไม่ต้องถาม)**

- เขียน/แก้โค้ด, เพิ่ม package, รัน test, แก้ test ที่ fail
- เลือกชื่อ class/ไฟล์/โครง folder เอง
- commit บน branch `feature/m<N>-*` ทุกครั้งที่ test ผ่าน
- เมื่อ spec ไม่บอก ให้เลือกทางที่ตรงกับระบบเดิม แล้วบันทึกเหตุผลใน `docs/DECISIONS.md`

**ต้องหยุดและรอคนอนุมัติ**

- merge เข้า `main`, deploy, แก้ DB ที่ไม่ใช่ dev container
- เชื่อมต่อ Sch69 จริง — อ่านจากไฟล์ export / .bak ที่ restore เป็น `Sch69_Import` เท่านั้น **ห้ามเขียนกลับ**
- เปลี่ยน hard constraint, ตัด feature ออก, เปลี่ยน tech stack
- ลดเกณฑ์ benchmark หรือแก้ test ให้ผ่านโดยไม่แก้โค้ดจริง

**รอบการทำงานต่อ milestone**

1. อ่าน spec + `docs/PROGRESS.md`
2. เขียน test ตามเกณฑ์ผ่านก่อน แล้วจึงเขียนโค้ด
3. วนจน test ผ่านทั้งหมด (รวม milestone ก่อนหน้า)
4. อัปเดต `docs/PROGRESS.md`: ทำอะไรไป, ตัวเลข benchmark, คำถามที่ค้าง
5. เปิด PR แล้วหยุด รอคุณ review (สัปดาห์ละ 1-2 ครั้ง)

ติดปัญหาเดิมเกิน 3 รอบ หรือเจอกฎที่ขัดกันใน spec → เขียนคำถามไว้ใน PROGRESS.md แล้วทำงานส่วนอื่นต่อ ห้ามเดาเอง

## 11. บทเรียนจากระบบเดิมที่ห้ามทำซ้ำ

| ปัญหาที่เคยเกิด | กฎในระบบใหม่ |
| --- | --- |
| ลดจำนวนห้อง (8→7) แต่ไม่ลบภาระสอน → sp\_AllStep วนไม่จบ ตารางว่างทั้งโรงเรียนโดยไม่มี error | ลบห้อง = ลบ Lesson/Placement ของห้องนั้นใน transaction เดียว (FK cascade) และ solver ต้องจบเสมอพร้อมรายงาน |
| คาบพักถูกตั้ง LoadType 5 → Guard 10 ปฏิเสธ ร.ด. หลัง 14 แถว | คาบพักมี LoadType ของตัวเอง (7) ที่ไม่เข้ากฎ H10 |
| Trigger rollback ทำให้ transaction doomed → ดูเหมือนหยุดทั้งชุด | ไม่ใช้ INSTEAD OF trigger; ตรวจกฎใน C# ก่อนเขียน |
| IsActive flag เพี้ยน → สลับโหมดเงียบ / stamp โหมดผิด | Placement ผูก SnapshotId — โหมดมาจาก Snapshot ไม่ใช่ flag แยก |
| ลบ XPlan แล้ว park อ้าง ID ที่ไม่มีแล้ว (dangling) | FK จาก Placement → Lesson; ลบ Lesson ที่มีใน Snapshot ใดๆ ต้องถามก่อน |
| OP เก็บซ้ำซ้อนแล้วไม่ตรง (tg\_DelTblZPlan ปิดอยู่) | ไม่เก็บ OP — คำนวณจาก COUNT Placement ทุกครั้ง |
| PlanTitleList เก็บทั้ง ',ม.ต้น,' และ 'ม.ต้น' → LIKE หลุดครึ่งหนึ่ง | ตารางลูก + FK ไม่มี list คั่น comma ใน DB |
| ปีทะเบียน (Yr/Tr) ≠ ปีจัดตาราง (SchYr/SchTr) → อ่านผิดปี | มี Term ของงานจัดตารางแยกชัดเจน |
| Synonym ชี้ผิด DB หลังสลับฐาน กระทบทุก session | SchoolId ต่อ request, ไม่มี state กลาง |
| ภาษาไทยเพี้ยนเป็น ? ตอน export ด้วย sqlcmd / PS 5.1 | ทุกไฟล์ UTF-8, คอลัมน์ nvarchar, มี test ที่ round-trip ข้อความไทย |
| โรงเรียนไม่ backup | ทุกครั้งที่จัดตารางเก็บ Snapshot ใหม่; backup DB อัตโนมัติทุกคืนบน server |

## 12. เรื่องที่ต้องให้คุณตัดสินใจก่อนเริ่ม

ตอบ 6 ข้อนี้แล้ว Claude Code เริ่ม M0 ได้ทันที (ค่าที่เสนอคือค่าที่ spec นี้ใช้อยู่)

- [ ] **Solver ใหม่ใน C# ~~หรือเรียก sp\_AllStep~~ เดิม?** เสนอ C# — AI ทดสอบและแก้เองได้ (SP เดิม WITH ENCRYPTION แก้ไม่ได้) แต่ใช้เวลา M3 มากขึ้น 2-3 สัปดาห์
- [ ] **DB ~~แยกต่อโรงเรียน หรือ~~ DB เดียว + SchoolId?** เสนอ DB เดียว — deploy/migrate ง่ายกว่ามากสำหรับ \~100 โรงเรียน
- [ ] **Cloud (VPS ของคุณ) หรือติดตั้งในโรงเรียน?** ตัดสินแล้ว: รองรับทั้ง 2 แบบ ให้โรงเรียนเลือก
- [ ] **React~~ หรือต่อยอด Wisej (VB.NET) ที่มีอยู่?~~** เสนอ React — ทำ drag-drop หรือ ติ๊ก ได้ดีกว่า และ AI เขียนได้แม่นกว่า
- [ ] **ข้อมูลทดสอบ**: ใช้ .bak ของ Sch69 ปัจจุบัน + อีก 1 โรงเรียน (SchKUS **) **— ต้องตัดชื่อครูจริงออก~~ก่อนหรือไม่?~~
- [ ] **ตารางตัวอย่างที่ฝ่ายวิชาการยอมรับแล้ว** 1 ชุด ใช้ตรวจว่า soft constraint ตั้งน้ำหนักถูกทาง
