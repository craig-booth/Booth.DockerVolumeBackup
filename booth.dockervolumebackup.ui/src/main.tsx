import React from 'react'
import ReactDOM from 'react-dom/client'
import { Theme } from '@radix-ui/themes'
import '@radix-ui/themes/styles.css'

import App from './App'

ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <Theme
            appearance="dark"
            accentColor="grass"
            grayColor="gray"
            panelBackground="solid"
            scaling="100%"
            radius="medium">
            <App />
        </Theme>
    </React.StrictMode>
)
