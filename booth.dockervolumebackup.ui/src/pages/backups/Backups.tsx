import { useState } from 'react';
import { DataTable, DataTableColumn } from '@/components/DataTable';
import { StatusBadge } from '@/components/StatusBadge';
import { Link } from 'react-router-dom';
import { Flex, Button, Box, AlertDialog } from '@radix-ui/themes';
import { TrashIcon } from '@radix-ui/react-icons';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
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

const columns: DataTableColumn<Backup>[] = [
	{ id: 1, heading: 'Schedule', value: (x) => x.scheduleName, sortable: true, render: (backup) => { return (<Link to={'/backups/' + backup.backupId}>{getBackupScheduleName(backup)}</Link>) } },
	{ id: 2, heading: 'Status', value: (x) => x.status, sortable: true, render: (backup) => { return (<StatusBadge status={backup.status} />) } },
	{ id: 3, heading: 'Backup Time', value: (x) => x.backupTime, sortable: true }
];

function Backups() {

	const [selection, setSelection] = useState<Set<string | number>>(new Set());
	const { getBackups, deleteBackups } = useApi();
	const queryClient = useQueryClient()

	const { isPending, isError, data: backups } = useQuery({
		queryKey: ['backups'],
		queryFn: () => getBackups()
	});

	const selectionChanged = (selection: Set<string | number>) => {
		setSelection(selection);
	}

	const deleteBackupsRequest = useMutation({
		mutationFn: (backupIds: number[]) => {
			return deleteBackups(backupIds);
		},
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ['backups'] })
		}
	});

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	const onDeleteBackups = () => {
		deleteBackupsRequest.mutate(Array.from(selection) as number[]);
	}

	return (
		<>
			<Flex direction="row" justify="end" gap="5" py="20px">
				<Box>
					<AlertDialog.Root>
						<AlertDialog.Trigger>
							<Button disabled={selection.size == 0} color="red"><TrashIcon height="16" width="16" />Delete Backup</Button>
						</AlertDialog.Trigger>
						<AlertDialog.Content maxWidth="450px">
							<AlertDialog.Title>Delete Backup</AlertDialog.Title>
							<AlertDialog.Description size="2">
								Are you sure? Deleted backups cannot be recovered.
							</AlertDialog.Description>

							<Flex gap="3" mt="4" justify="end">
								<AlertDialog.Cancel>
									<Button variant="soft" color="gray">
										Cancel
									</Button>
								</AlertDialog.Cancel>
								<AlertDialog.Action>
									<Button variant="solid" color="red" onClick={() => onDeleteBackups()}>
										Delete
									</Button>
								</AlertDialog.Action>
							</Flex>
						</AlertDialog.Content>
					</AlertDialog.Root>					
				</Box>
			</Flex>
			<DataTable columns={columns} data={backups} keyField='backupId' defaultSortColumn={3} defaultSortAscending={false} includeCheckBox={true} selection={selection} onSelectionChange={selectionChanged} />
		</>
	)
}

export default Backups;