import { useEffect, useState } from 'react';
import { Table, Flex, Box, Button, TextField, Checkbox, Text, Progress } from '@radix-ui/themes';
import { MagnifyingGlassIcon, Cross2Icon, ArrowUpIcon, ArrowDownIcon } from '@radix-ui/react-icons';
import { BackupStatus } from '@/models/BackupStatus';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useApi } from '@/api/api';

interface ColumnSort {
	field: string;
	ascending: boolean;
} 

interface VolumeState {
	name: string;
	selected: boolean;
	backupStatus: string;
}

const datetimeFormatter = new Intl.DateTimeFormat(undefined, { dateStyle: 'full', timeStyle: 'short' });
const kiloByteSizeFormatter = new Intl.NumberFormat(undefined, { style: 'unit', unit: 'kilobyte', unitDisplay: 'short', maximumFractionDigits: 0 });
const megaByteSizeFormatter = new Intl.NumberFormat(undefined, { style: 'unit', unit: 'megabyte', unitDisplay: 'short', maximumFractionDigits: 0 });

const _1KB = 1024;
const _1MB = 1024 * _1KB;

function Volumes() {

	const [selectAll, setSelectAll] = useState(false);
	const [filter, setFilter] = useState('');
	const [sortOrder, setSortOrder] = useState<ColumnSort>({ field: 'name', ascending: true })
	const [volumeStates, setVolumeStates] = useState<VolumeState[]>([]);
	const [backupId, setBackupId] = useState(-1);
	const [activeBackup, setActiveBackup] = useState<BackupStatus|null>(null);

	const { getVolumes, backupVolumes } = useApi();

	const { isPending, isError, data: volumes } = useQuery({
		queryKey: ['volumes'],
		queryFn: () => getVolumes()
	});

	useEffect(() => {

		if (volumes) {
			const newVolumeStates = volumes.map<VolumeState>(x => { return { name: x.name, selected: false, backupStatus: '' } });
			setVolumeStates(newVolumeStates);
		}
			

	}, [volumes]);

	const backupRequest = useMutation({
		mutationFn: (requestedVolumes: string[]) => {	
			const initialStatus: BackupStatus = { backupId: 0, status: 'Queued', volumes: requestedVolumes.map(x => {return { volumeName: x, status: 'Queued' }; }) };
			setActiveBackup(initialStatus); 
			return backupVolumes({ volumes: requestedVolumes });
		},
		onSuccess: (data: number) => setBackupId(data)
	});

	useEffect(() => {

		if (backupId >= 0) {
			const eventSource = new EventSource(`/api/backup/${backupId}`);

			eventSource.onmessage = (e) => {
				const data = JSON.parse(e.data);
				setActiveBackup(data);
			};

			return () => eventSource.close();
		};

		return;
	}, [backupId]);



	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	const toggleSelectAll = () => {
		const newValue = !selectAll;
		setSelectAll(newValue);

		const newVolumeStates = volumeStates.map(x => { return {...x, selected: newValue } });
		setVolumeStates(newVolumeStates);
	};

	const toggleVolumeSelected = (name: string) => {
		const newVolumeStates = [...volumeStates];
		const selectedVolume = newVolumeStates.find(x => x.name == name);
		if (selectedVolume)
			selectedVolume.selected = !selectedVolume.selected;

		setVolumeStates(newVolumeStates);
	}

	const filterChanged = (value: string) => {
		setFilter(value);
	}

	const sortOrderChanged = (field: string) => {
		if (sortOrder.field == field)
			setSortOrder({ field: sortOrder.field, ascending: !sortOrder.ascending });
		else
			setSortOrder({ field: field, ascending: true });
	}

	let tableData;
	if (filter == '')
		tableData = [...volumes];
	else
		tableData = volumes.filter(x => x.name.includes(filter));
	tableData.sort((a, b) => {

		let order = 0;
		if (sortOrder.field == 'name') {
			order = a.name.localeCompare(b.name);
		}
		else if (sortOrder.field == 'size') {
			order = a.size - b.size;
		}
		else if (sortOrder.field == 'lastBackup') {
			order = (a.lastBackup ? a.lastBackup.getTime() : 0) - (b.lastBackup ? b.lastBackup.getTime() : 0);
		}

		if (!sortOrder.ascending)
			order = -order;

		return order;
	});

	const sortDisplay = (field: string, ascendering: boolean) => {
		if ((sortOrder.field == field) && (sortOrder.ascending == ascendering))
			return 'inline';
		else
			return 'none';
	}

	const formatSize = (size?: number) => {
		if (!size)
			return '-';

		if (size < _1MB)
			return kiloByteSizeFormatter.format(size / _1KB);
		else
			return megaByteSizeFormatter.format(size / _1MB);
	}

	const formatDate = (date?: Date) => {	
		if (!date)
			return '-';

		return datetimeFormatter.format(date);			
	}

	const percentComplete = (): number => {
		if (activeBackup != null) {
			const volumesComplete = activeBackup.volumes.reduce((accumulator, currentValue) => accumulator + (currentValue.status == 'Complete' ? 1 : 0), 0);
			return (volumesComplete / activeBackup.volumes.length) * 100;
		}
		else {
			return 0;
		}
	}

	const startBackup = () => {
		const selectedVolumes = volumeStates.filter(x => x.selected).map(x => x.name);

		const newVolumeStates = volumeStates.map(x => { return { ...x, selected: false, backupStatus: x.selected? 'Queued': '' } });
		setVolumeStates(newVolumeStates);

		backupRequest.mutate(selectedVolumes);
	}

	return (
		<>
			<Flex direction="row" justify="end" gap="5" py="20px">
				<Text style={{ display: activeBackup? 'block' : 'none' }} mt="1">Backup {activeBackup?.status}</Text>
				<Progress style={{ display: activeBackup ? 'block' : 'none' }}  size="3" mt="3" value={percentComplete()} />
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
					<Button disabled={!volumeStates.some(x => x.selected) || (activeBackup != null && activeBackup.status != 'Complete')} onClick={() => startBackup()}>Backup Now</Button>
				</Box>
			</Flex>
			<Table.Root variant="surface">
				<Table.Header>
					<Table.Row>
						<Table.ColumnHeaderCell width="1px"><Checkbox checked={selectAll} onClick={() => toggleSelectAll()} /></Table.ColumnHeaderCell>
						<Table.ColumnHeaderCell onClick={() => sortOrderChanged('name')}>
							<Flex direction="row" gap="3">
								<Text size="3">Volume</Text>
								<ArrowUpIcon display={sortDisplay('name', true)} height="16" width="16" cursor="default" />
								<ArrowDownIcon display={sortDisplay('name', false)} height="16" width="16" cursor="default" />
							</Flex>
						</Table.ColumnHeaderCell>
						<Table.ColumnHeaderCell align="right" onClick={() => sortOrderChanged('size')}>
							<Flex direction="row" justify="end" gap="3">
								<Text size="3">Size</Text>
								<ArrowUpIcon display={sortDisplay('size', true)} height="16" width="16" cursor="default" />
								<ArrowDownIcon display={sortDisplay('size', false)} height="16" width="16" cursor="default" />
							</Flex>
						</Table.ColumnHeaderCell>
						<Table.ColumnHeaderCell onClick={() => sortOrderChanged('lastBackup')}>
							<Flex direction="row" gap="3">
								<Text size="3">Last Backup</Text>
								<ArrowUpIcon display={sortDisplay('lastBackup', true)} height="16" width="16" cursor="default" />
								<ArrowDownIcon display={sortDisplay('lastBackup', false)} height="16" width="16" cursor="default" />
							</Flex>
						</Table.ColumnHeaderCell>
					</Table.Row>
				</Table.Header>
				<Table.Body>
					{
						tableData.map((volume) => {

							let volumeState = volumeStates.find(x => x.name == volume.name);
							if (volumeState == null) {
								volumeState = { name: volume.name, selected: false, backupStatus: '' };
							}

							return (
								<Table.Row>
									<Table.Cell width="1px"><Checkbox checked={volumeState?.selected} onClick={() => toggleVolumeSelected(volume.name)} /></Table.Cell>
									<Table.RowHeaderCell>{volume.name}</Table.RowHeaderCell>
									<Table.Cell align="right">{formatSize(volume.size)}</Table.Cell>
									<Table.Cell>{formatDate(volume.lastBackup)}</Table.Cell>
								</Table.Row>
							)
						})
					}
				</Table.Body>
			</Table.Root>
		</>
	)
}

export default Volumes;