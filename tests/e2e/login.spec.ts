import { expect, test } from '@playwright/test'

const username = process.env.E2E_USERNAME ?? 'admin'
const password = process.env.E2E_PASSWORD ?? process.env.SEED_USER_PASSWORD

test.beforeAll(() => {
  if (!password) throw new Error('ตั้ง E2E_PASSWORD (หรือ SEED_USER_PASSWORD) ให้ตรงกับรหัสผ่าน seed ก่อนรัน')
})

test('หน้าแรกพาไปหน้า login', async ({ page }) => {
  await page.goto('/')
  await expect(page).toHaveURL(/\/login$/)
  await expect(page.getByRole('heading', { name: 'EasySchedule' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'เข้าสู่ระบบ' })).toBeVisible()
})

test('รหัสผ่านผิดแสดงข้อความภาษาไทย', async ({ page }) => {
  await page.goto('/login')
  await page.getByLabel('ชื่อผู้ใช้').fill(username)
  await page.getByLabel('รหัสผ่าน').fill('wrong-password')
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click()
  await expect(page.getByRole('alert')).toHaveText('ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง')
})

test('login แล้วเห็นระดับชั้น ป.1 ม.1 ม.4 เป็นภาษาไทย', async ({ page }) => {
  await page.goto('/login')
  await page.getByLabel('ชื่อผู้ใช้').fill(username)
  await page.getByLabel('รหัสผ่าน').fill(password!)
  await page.getByRole('button', { name: 'เข้าสู่ระบบ' }).click()

  await expect(page.getByRole('heading', { name: 'ระดับชั้น' })).toBeVisible()
  const levels = page.getByTestId('class-level')
  for (const name of ['ป.1', 'ม.1', 'ม.4']) {
    await expect(levels.filter({ has: page.getByText(name, { exact: true }) })).toHaveCount(1)
  }
  await expect(page.getByText('ม.4/1, ม.4/2')).toBeVisible()

  // ออกจากระบบแล้วกลับไปหน้า login
  await page.getByRole('button', { name: 'ออกจากระบบ' }).click()
  await expect(page).toHaveURL(/\/login$/)
})
