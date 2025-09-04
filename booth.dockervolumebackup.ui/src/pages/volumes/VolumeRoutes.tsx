import Volumes from './Volumes.tsx';
import VolumeBackups from './VolumeBackups.tsx';

const VolumeRoutes = {
	path: 'volumes',
	children: [
		{
			index: true,
			element: <Volumes />,
		},
		{
			path: ':volume/backups',
			element: <VolumeBackups />,
		}
	]
};

export default VolumeRoutes;
