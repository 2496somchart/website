import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// ตอน dev: proxy /api ไปที่ API (dotnet run ค่าเริ่มต้น http://localhost:5080)
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: { '/api': process.env.VITE_API_PROXY ?? 'http://localhost:5080' },
  },
})
