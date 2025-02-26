import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

import './App.css'

import Layout from '@/layout/Layout.tsx';
import BackupRoutes from '@/pages/backups/BackupRoutes.tsx';
import VolumeRoutes from '@/pages/volumes/VolumeRoutes.tsx';
import ScheduleRoutes from '@/pages/schedules/ScheduleRoutes.tsx';


function App() {

    const queryClient = new QueryClient();

    const router = createBrowserRouter([
        {
            path: '/',
            element: <Layout />,
            children: [
                {
                    path: '/',
                    element: <Navigate to="backups" replace={true} />
                },
                BackupRoutes,
                VolumeRoutes,
                ScheduleRoutes,
            ]
        }

    ]);


    return (
        <QueryClientProvider client={queryClient}>
            <RouterProvider router={router} />
            <ReactQueryDevtools initialIsOpen />
        </QueryClientProvider>
    )
}

export default App
