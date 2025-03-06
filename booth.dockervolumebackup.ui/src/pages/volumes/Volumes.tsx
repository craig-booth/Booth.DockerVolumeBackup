import { useState, useMemo } from 'react';
import { Flex, Box, Button, TextField } from '@radix-ui/themes';
import { MagnifyingGlassIcon, Cross2Icon } from '@radix-ui/react-icons';
import * as Toast from '@radix-ui/react-toast';
import { DataTable, DataTableColumn } from '@/components/DataTable';
import { Volume } from '@/models/Volume';
import { Link } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { formatStorageSize } from '@/utils/Formatting';

const columns: DataTableColumn<Volume>[] = [
	{ id: 1, heading: 'Volume', value: (x) => x.name, sortable: true },
	{ id: 2, heading: 'Size', value: (x) => x.size, align: 'right', formatter: (x) => formatStorageSize(x as number), sortable: true },
	{ id: 3, heading: 'Last Backup', value: (x) => x.lastBackup, sortable: true }
];
function Volumes() {

	const [filter, setFilter] = useState('');
	const [selection, setSelection] = useState<Set<string>>(new Set()); 
	const [backupRequested, setBackupRequested] = useState(false); 
	const [backupId, setBackupId] = useState(0); 
	const [showToast, setShowToast] = useState(false);

	const { getVolumes, backupVolumes } = useApi();

	const { isPending, isError, data: volumes } = useQuery({
		queryKey: ['volumes'],
		queryFn: () => getVolumes()
	});

	const filteredVolumes = useMemo(() => volumes ? (filter == '') ? [...volumes] : volumes.filter(x => x.name.includes(filter)) : [], [volumes, filter]);

	const backupRequest = useMutation({
		mutationFn: (requestedVolumes: string[]) => {
			return backupVolumes({ volumes: requestedVolumes });
		},
		onSuccess: (data: number) => {
			setBackupRequested(false);
			setBackupId(data);
			setShowToast(true);
		}
	});

	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	const filterChanged = (value: string) => {
		setFilter(value);
	}

	const selectionChanged = (selection: Set<string>) => {
		setSelection(selection); 
	}

	const startBackup = () => {
		setBackupRequested(true);
		backupRequest.mutate(Array.from(selection));
	}

	return (
		<>
			<Toast.Provider>
				<Toast.Root className="ToastRoot" open={showToast} onOpenChange={setShowToast}>
					Backup Queued. Click to <Link to={'/backups/' + backupId}>view</Link>
					<Toast.Close><Cross2Icon height="16" width="16" cursor="default" /></Toast.Close>
				</Toast.Root>
				<Toast.Viewport className="ToastViewport" />
			</Toast.Provider>
			<Flex direction="row" justify="end" gap="5" py="20px">
				<Box width="300px">
					<TextField.Root value={filter} onChange={(e) => filterChanged(e.target.value)} placeholder="Search...">
						<TextField.Slot side="left">
							<MagnifyingGlassIcon height="16" width="16" />
						</TextField.Slot>
						<TextField.Slot side="right" onClick={() => filterChanged('')}>
							<Cross2Icon height="16" width="16" cursor="default" />
						</TextField.Slot>
					</TextField.Root>
				</Box>
				<Box>
					<Button disabled={selection.size == 0 || backupRequested} onClick={() => startBackup()}>Backup Now</Button>
				</Box>
			</Flex>
			<DataTable columns={columns} data={filteredVolumes} keyField='name' defaultSortColumn={1} includeCheckBox={true} selection={selection} onSelectionChange={selectionChanged} />
		</>
	)
}

export default Volumes;