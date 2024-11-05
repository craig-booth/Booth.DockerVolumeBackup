import Volumes from './Volumes.tsx';

const VolumeRoutes = {
	path: 'volumes',
	children: [
		{
			index: true,
			element: <Volumes />,
		}
	]
};

export default VolumeRoutes;
