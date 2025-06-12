import { TabNav, Flex, Box, Text } from '@radix-ui/themes';
import { Link, useMatches } from 'react-router-dom';


export default function Menu() {

	const matches = useMatches();

	const isActive = (path: string): boolean =>
	{
		const index = matches.findIndex(x => x.pathname.startsWith(path));
		return (index != -1);
	};

	return (
		<Flex direction="column" >
			<Flex direction="row">
				<Box px="3"><img src="/Booth.png" width="20px" height="20px" /></Box>
				<Box><Text size="3" weight="bold">Volume Backup</Text></Box>
			</Flex>
			<Box width="100%">
				<TabNav.Root>
					<TabNav.Link asChild active={isActive('/backups/')}>
						<Link to={'backups'}>Backups</Link>
					</TabNav.Link>
					<TabNav.Link asChild active={isActive('/volumes/')}>
						<Link to={'volumes'}>Volumes</Link>
					</TabNav.Link>
					<TabNav.Link asChild active={isActive('/schedules/')}>
						<Link to={'schedules'}>Schedules</Link>
					</TabNav.Link>
				</TabNav.Root>
			</Box>
		</Flex>


	);
}