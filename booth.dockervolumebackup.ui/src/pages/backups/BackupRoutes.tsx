import Backups from './Backups.tsx';
import BackupDetails from './BackupDetails.tsx';

const BackupRoutes = {
	path: 'backups',
	children: [
		{
			index: true,
			element: <Backups />,
		},
		{
			path: ':backupId',
			element: <BackupDetails />,
		}
	]
};

export default BackupRoutes;
