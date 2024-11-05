import { TabNav } from '@radix-ui/themes';
import { Link, useMatches } from 'react-router-dom';


export default function Menu() {

	const matches = useMatches();

	const isActive = (path: string): boolean =>
	{
		const index = matches.findIndex(x => x.pathname.startsWith(path));
		return (index != -1);
	};

	return (
		<TabNav.Root>
			<TabNav.Link asChild active={isActive('/volumes/')}>
				<Link to={'volumes'}>Volumes</Link>
			</TabNav.Link>
			<TabNav.Link asChild active={isActive('/schedules/')}>
				<Link to={'schedules'}>Schedules</Link>
			</TabNav.Link>
		</TabNav.Root>

	);
}