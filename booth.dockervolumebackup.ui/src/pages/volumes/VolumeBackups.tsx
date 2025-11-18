import { Flex, Text, Strong } from '@radix-ui/themes';
import { useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { DataTable, DataTableColumn } from '@/components/DataTable';
import { DownloadButton } from '@/components/DownloadButton';
import { RestoreButton } from '@/components/RestoreButton';
import { StatusBadge } from '@/components/StatusBadge';
import { VolumeBackup } from '@/models/VolumeBackup';
import { formatStorageSize } from '@/utils/Formatting';

const columns: DataTableColumn<VolumeBackup>[] = [
	{
		id: 1,
		heading: 'Backup Time',
		value: (x) => x.backupTime,
		sortable: true
	},	
	{
		id: 2,
		heading: 'Status',
		value: (x) => x.status,
		render: (backup) => { return (<StatusBadge status={backup.status} />) }
	},
	{
		id: 3,
		heading: 'Size',
		value: (x) => x.size,
		align: 'right',
		formatter: (x) => formatStorageSize(x as number)
	},
	{
		id: 4,
		heading: 'Actions',
		value: (x) => x.backupVolumeId, render: (backup) => {
			return (
				<Flex gap="3">
					<DownloadButton backupvolumeid={backup.backupVolumeId} />
					<RestoreButton backupvolumeid={backup.backupVolumeId} volume={backup.volumeName} backuptime={backup.backupTime} />
				</Flex >)
		}
	}

];

function VolumeBackups() {

	const { volume } = useParams();
	const { getVolumeBackups } = useApi();

	const { isPending, isError, data: backups } = useQuery({
		queryKey: ['volumebackups'],
		queryFn: () => getVolumeBackups(volume ? volume: ''),
	});

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	return (
		<>
			<Flex direction="column" gap="2" px="20px" py="20px">
				<Text align="left">Volume: <Strong>{volume}</Strong></Text>
			</Flex>
			<DataTable columns={columns} data={backups} defaultSortColumn={1} defaultSortAscending={false} keyField='backupId' />
		</>
	)
}

export default VolumeBackups;