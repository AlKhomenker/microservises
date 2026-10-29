/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Each backend service has its own port (see launchSettings.json of each host).
    // More specific paths come first.
    proxy: {
      '/api/audit-logs': 'http://localhost:5001', // TodoApp.AuditLogs.Host
      '/api': 'http://localhost:5000', // TodoApp.Todos.Host
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    restoreMocks: true,
  },
})
