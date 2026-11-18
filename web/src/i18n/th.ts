// ข้อความ UI ทั้งหมดอยู่ที่นี่ (CLAUDE.md ข้อ 8)
export const th = {
  appName: 'EasySchedule',
  appTagline: 'ระบบจัดตารางสอน',
  login: {
    title: 'เข้าสู่ระบบ',
    subtitle: 'ระบบจัดตารางสอน — เข้าสู่ระบบ',
    username: 'ชื่อผู้ใช้',
    password: 'รหัสผ่าน',
    submit: 'เข้าสู่ระบบ',
    submitting: 'กำลังเข้าสู่ระบบ…',
    failed: 'เข้าสู่ระบบไม่สำเร็จ',
  },
  header: {
    logout: 'ออกจากระบบ',
  },
  roles: {
    SuperAdmin: 'ผู้ดูแลระบบกลาง',
    SchoolAdmin: 'ฝ่ายวิชาการ',
    Viewer: 'ครู',
  },
  classLevels: {
    title: 'ระดับชั้น',
    listLabel: 'รายชื่อระดับชั้น',
    loading: 'กำลังโหลด…',
    empty: 'ยังไม่มีระดับชั้น',
    sections: (count: number, names: string) => `${count} ห้องเรียน: ${names}`,
    noSchool: 'บัญชีนี้ไม่ได้ผูกกับโรงเรียน (SuperAdmin จะเลือกโรงเรียนได้ใน M1)',
  },
  errors: {
    generic: (status: number) => `เกิดข้อผิดพลาด (${status})`,
    forbidden: 'ไม่มีสิทธิ์เข้าถึงข้อมูลนี้',
    loadFailed: 'โหลดข้อมูลไม่สำเร็จ',
  },
} as const
