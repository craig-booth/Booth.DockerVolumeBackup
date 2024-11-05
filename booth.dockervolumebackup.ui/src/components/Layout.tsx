import { Outlet } from 'react-router-dom';
import { Flex, Box } from '@radix-ui/themes';

import Menu from './Menu.tsx';

export default function Layout() {

    return (
        <>
            <Flex direction="column">
                <Menu />
                <Box pt="12px">
                    <Outlet />
                </Box>
            </Flex>
        </>
    );
}