import { useEffect, useReducer } from 'react';
import { Text, Flex, Grid, Strong } from '@radix-ui/themes';
import { DataTable, DataTableColumn } from '@/components/DataTable';
import { StatusBadge } from '@/components/StatusBadge';
import { useParams } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { parseJson } from '@/api/jsonParser';
import { BackupDetail, BackupVolumeDetail } from '@/models/Backup';
import { BackupStatus } from '@/models/BackupStatus';
import { formatLongDateTime, formatDuration, formatStorageSize } from '@/utils/Formatting';

interface BackupAction {
	type: string;
	data: BackupDetail | BackupStatus;
}

const columns: DataTableColumn<BackupVolumeDetail>[] = [
	{ id: 1, heading: 'Volume', value: (x) => x.volume },
	{ id: 2, heading: 'Status', value: (x) => x.status, render: (backup) => { return (<StatusBadge status={backup.status} />) } },
	{ id: 3, heading: 'Size', value: (x) => x.backupSize, align: 'right', formatter: (x) => formatStorageSize(x as number) },
	{ id: 4, heading: 'Backup Time', value: (x) => x.startTime },
	{ id: 5, heading: 'Duration', value: (x) => x.startTime && x.endTime ? x.endTime.getTime() - x.startTime.getTime() : undefined, formatter: (x) => formatDuration(x as number) }
];


const initialBackup: BackupDetail = { backupId: -1, backupType: 'Unmanaged', scheduleName: '', status: 'Queued', volumes: [] };
function reducer(state: BackupDetail, action: BackupAction): BackupDetail {
	switch (action.type) {
		case 'set':
			return { ...action.data } as BackupDetail;
		case 'update': {
			const backupStatus = action.data as BackupStatus;

			const newState = { ...state };

			newState.status = backupStatus.status;
			newState.startTime = backupStatus.startTime;
			newState.endTime = backupStatus.endTime;
			newState.volumes.forEach((volume) => {
				const volumeStatus = backupStatus.volumes.find(x => x.backupVolumeId == volume.backupVolumeId);
				if (volumeStatus) {
					volume.status = volumeStatus.status;
					volume.startTime = volumeStatus.startTime;
					volume.endTime = volumeStatus.endTime;
				}
			});

			return newState;
		}
		default:
			throw new Error('Unknown action type');
	}
}

function BackupDetails() {

	const { backupId } = useParams();
	const { getBackup } = useApi();

	const [backup, dispatch] = useReducer(reducer, initialBackup);

	const { isPending, isError, isSuccess, data } = useQuery({
		queryKey: ['backup'],
		queryFn: () => getBackup(backupId ? Number.parseInt(backupId) : -1),
	});

	// Handle Backup being fetched
	useEffect(() => {
		if (isSuccess)
			dispatch({ type: 'set', data: data });
	}, [isSuccess, data]);

	// Handle backup status changing
	useEffect(() => {

		if (backup?.status == 'Active' || backup?.status == 'Queued') {
			const eventSource = new EventSource(`/api/backups/${backupId}/statusevents`);

			eventSource.onmessage = (e) => {
				const backupStatus = parseJson<BackupStatus>(e.data);

				if (backupStatus) {
					dispatch({ type: 'update', data: backupStatus });
				}

			};

			return () => eventSource.close();
		};

		return;
	}, [backup, backupId]);

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}


	

	return (
		<>
			<Flex direction="column" gap="2" px="20px" py="20px">
				<Flex direction="row" gap="5" >
					<Text>Backup: <Strong>{backup.scheduleName}</Strong> (#{backup.backupId})</Text>
					<StatusBadge status={backup.status}></StatusBadge>
				</Flex>
				<Grid columns="2">
					<Text align="left"> Started: {formatLongDateTime(backup.startTime)}</Text >
					<Text align="left"> Duration: {backup.startTime && backup.endTime ? formatDuration(backup.endTime.getTime() - backup.startTime.getTime()) : undefined}</Text >
				</Grid>
			</Flex>
			<DataTable columns={columns} data={backup.volumes} keyField='backupVolumeId' />
		</>

	)

}

export default BackupDetails;