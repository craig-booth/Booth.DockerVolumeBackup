import { Outlet } from 'react-router-dom';
import { Flex, Box } from '@radix-ui/themes';
import * as Toast from '@radix-ui/react-toast';

import Menu from './Menu.tsx';

export default function Layout() {

    return (
        <>
            <Flex direction="column">
                <Menu />
                <Box pt="12px">
                    <Toast.Provider>
                        <Toast.Viewport className="ToastViewport" />
                        <Outlet />
                    </Toast.Provider>
                </Box>
            </Flex>
        </>
    );
}