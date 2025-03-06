import { DataTable, DataTableColumn } from '@/components/DataTable';
import { Link } from 'react-router-dom';
import { useNavigate } from 'react-router';
import { Badge, Flex, Button, Box } from '@radix-ui/themes';
import { PlusIcon } from '@radix-ui/react-icons';
import { useQuery } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { Schedule } from '@/models/Schedule';

function Schedules() {

	const navigate = useNavigate();
	const { getSchedules } = useApi();

	const { isPending, isError, data: schedules } = useQuery({
		queryKey: ['schedules'],
		queryFn: () => getSchedules()
	});


	if (isPending) {
		return <div>Loading...</div>;
	}

	if (isError) {
		return <div>Error</div>;
	}

	const onAddSchedule = () => {
		navigate('/schedules/new');
	}

	const columns: DataTableColumn<Schedule>[] = [
		{ id: 1, heading: 'Schedule', value: (x) => x.name, sortable: true, render: (schedule) => { return (<Link to={'/schedules/' + schedule.scheduleId}>{schedule.name}</Link>) } },
		{ id: 2, heading: 'Status', value: (x) => x.enabled, sortable: true, render: (schedule) => { return (<Badge color={schedule.enabled ? 'green' : 'gray'}>{ schedule.enabled ? 'Active' : 'Inactive'}</Badge>) } }
	];

	return (
		<>
			<Flex direction="row" justify="end" gap="5" py="20px">
				<Box>
					<Button onClick={() => onAddSchedule() }><PlusIcon height="16" width="16" />Add Schedule</Button>
				</Box>
			</Flex>
			<DataTable columns={columns} data={schedules} keyField='scheduleId' defaultSortColumn={1} />
		</>
	)
}

export default Schedules;