import { DataTable, DataTableColumn } from '@/components/DataTable';
import { StatusBadge } from '@/components/StatusBadge';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { Backup } from '@/models/Backup';

const getBackupScheduleName = (backup: Backup): string => {
	if (backup.backupType == 'Scheduled')
		return backup.scheduleName
	else if (backup.backupType == 'Adhoc')
		return '(adhoc)'
	else if (backup.backupType == 'Unmanaged')
		return '(unmanaged)'
	else
        return '(unmanaged)'
}


function Backups() {


	const { getBackups } = useApi();

	const { isPending, isError, data: backups } = useQuery({
		queryKey: ['backups'],
		queryFn: () => getBackups()
	});


	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	const columns: DataTableColumn<Backup>[] = [
		{ id: 1, heading: 'Schedule', value: (x) => x.scheduleName, sortable: true, render: (backup) => { return (<Link to={'/backups/' + backup.backupId}>{getBackupScheduleName(backup)}</Link>) } },
		{ id: 2, heading: 'Status', value: (x) => x.status, sortable: true, render: (backup) => { return (<StatusBadge status={backup.status}/>) } },
		{ id: 3, heading: 'Backup Time', value: (x) => x.backupTime, sortable: true }
	];

	return (
		<>
			<DataTable columns={columns} data={backups} keyField='backupId' defaultSortColumn={3} defaultSortAscending={false}  />
		</>
	)
}

export default Backups;