import { fileURLToPath, URL } from 'url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';


// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '^/api': {
                target: 'http://localhost:5239',
                secure: false
            }
        },
        port: 5173
    }
})
