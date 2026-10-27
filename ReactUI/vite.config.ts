import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  envPrefix: ['VITE_', 'REACT_APP_'],
  server: {
    port: 5173,
    proxy: {
      '/WeatherForecast': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
      '/hangfire': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      }
    }
  }
});
