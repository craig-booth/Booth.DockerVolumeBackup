import { useState, useEffect } from 'react';
import { Grid, Flex, Text, TextField, Switch, Checkbox, Button } from '@radix-ui/themes';
import { DataTable, DataTableColumn } from '@/components/DataTable';
import { useParams, useNavigate } from 'react-router';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { QueuedBackupToast } from '@/components/QueuedBackupToast';
import { ScheduleDetail } from '@/models/Schedule';
import { Volume } from '@/models/Volume';

const columns: DataTableColumn<Volume>[] = [
	{ id: 1, heading: 'Volume', sortable: true, value: (x) => x.name }
];

const newSchedule: ScheduleDetail = {
	scheduleId: 0,
	name: '',
	enabled: true,
	days: { sunday: false, monday: false, tuesday: false, wednesday: false, thursday: false, friday: false, saturday: false},
	time: '',
    keepLast: 0,
	volumes: []
};

function ScheduleDetails() {

	const [ schedule, setSchedule] = useState<ScheduleDetail>(); 
	const [ selection, setSelection] = useState<Set<string|number>>(new Set()); 
	const [ changeMade, setChangeMade] = useState(false); 
	const [ backupRequested, setBackupRequested] = useState(false); 
	const [ backupId, setBackupId] = useState(0); 
	const [ showToast, setShowToast] = useState(false);

	const { getSchedule, getVolumes, createSchedule, updateSchedule, deleteSchedule, runBackup } = useApi();
	const navigate = useNavigate();


	const { scheduleId } = useParams();
	const isNew = scheduleId == 'new';

	const { isPending: isPending1, isError: isError1, isSuccess: isSuccess1, data } = useQuery({
		queryKey: ['schedule'],
		queryFn: !isNew ? () => getSchedule(scheduleId ? Number.parseInt(scheduleId) : -1) : () => new Promise<ScheduleDetail>((resolve) => resolve(newSchedule)) 
	});

	const { isPending: isPending2, isError: isError2, isSuccess: isSuccess2, data: volumes } = useQuery({
		queryKey: ['volumes'],
		queryFn: () => getVolumes(),
	});

	useEffect(() => {
		if (isSuccess1) {
			setSchedule(data);

			if (isSuccess2) {
				const selectedVolumes = new Set(data.volumes.map((x) => x));
				setSelection(selectedVolumes);
			}
		}
	}, [isNew, isSuccess1, isSuccess2, data, volumes]);

	const createScheduleRequest = useMutation({
		mutationFn: (schedule: ScheduleDetail) => {
			return createSchedule(schedule);
		},
		onSuccess: () => {
			navigate('/schedules');
		}
	});

	const updateScheduleRequest = useMutation({
		mutationFn: (schedule: ScheduleDetail) => {
			return updateSchedule(schedule);
		},
		onSuccess: () => {
			setChangeMade(false);
		}
	});

	const deleteScheduleRequest = useMutation({
		mutationFn: (scheduleId: number) => {
			return deleteSchedule(scheduleId);
		},
		onSuccess: () => {
			navigate('/schedules');
		}
	});

	const runScheduleRequest = useMutation({
		mutationFn: (scheduleId: number) => {
			return runBackup(scheduleId);
		},
		onSuccess: (data: number) => {
			setBackupRequested(false);
			setBackupId(data);
			setShowToast(true);
		}
	});

	if (isPending1 || isPending2) {
		return <div>Loading...</div>;
	}

	if (isError1 || isError2) {
		return <div>Error</div>;
	}

	const onSave = () => {
		if (schedule) {
			if (isNew) {
				createScheduleRequest.mutate({ ...schedule, volumes: Array.from(selection) as string[] });
			}
			else {
				updateScheduleRequest.mutate({ ...schedule, volumes: Array.from(selection) as string[] });
			}
		}	
	}

	const onDelete = () => {
		if (schedule) {
			deleteScheduleRequest.mutate(schedule.scheduleId);
		}
	}

	const onRunNow = () => {
		if (schedule) {
			setBackupRequested(true);
			runScheduleRequest.mutate(schedule.scheduleId);
		}
	}

	const selectionChanged = (selection: Set<string|number>) => {
		setSelection(selection);
		setChangeMade(true);
	}

	const nameChanged = (value: string) => {
		const newSchedule = { ...schedule } as ScheduleDetail;
		newSchedule.name = value;
		setSchedule(newSchedule);
		setChangeMade(true);
	}

	const enableChanged = (value: boolean) => {
		const newSchedule = { ...schedule } as ScheduleDetail;
		newSchedule.enabled = value;
		setSchedule(newSchedule);
		setChangeMade(true);
	}

	const timeChanged = (value: string) => {
		const newSchedule = { ...schedule } as ScheduleDetail;
		newSchedule.time = value;
		setSchedule(newSchedule);
		setChangeMade(true);
	}

	const dayChanged = (day: string,  value: boolean) => {
		const newSchedule = { ...schedule } as ScheduleDetail;
		if (day == 'sunday') newSchedule.days.sunday = value;
		else if (day == 'monday') newSchedule.days.monday = value;
		else if (day == 'tuesday') newSchedule.days.tuesday = value;
		else if (day == 'wednesday') newSchedule.days.wednesday = value;
		else if (day == 'thursday') newSchedule.days.thursday = value;
		else if (day == 'friday') newSchedule.days.friday = value;
		else if (day == 'saturday') newSchedule.days.saturday = value;

		setSchedule(newSchedule);
		setChangeMade(true);
	}

	const keepLastChanged = (value: string) => {
		const newSchedule = { ...schedule } as ScheduleDetail;
		newSchedule.keepLast = value ? Number.parseInt(value) : 0;
		setSchedule(newSchedule);
		setChangeMade(true);
    }

	return (
		<>
			<QueuedBackupToast backupId={backupId} open={showToast} onOpenChange={setShowToast} />
			<Flex direction="column" gap="4">
				<Flex justify="center" gap="4">
					<Button onClick={() => onSave()} disabled={!changeMade || backupRequested}>Save</Button>
					<Button onClick={() => onDelete()} disabled={isNew || backupRequested} color="red">Delete</Button>
					<Button onClick={() => onRunNow()} disabled={isNew || backupRequested}>Run Now</Button>
				</Flex>
				<Grid columns="200px 200px" rows="5" gap="2">
					<Text as="label" align="left">Name</Text>
					<TextField.Root name="name" value={schedule?.name} onChange={(e) => nameChanged(e.target.value)} />
					<Text as="label" align="left">Enabled</Text>
					<Switch name="enabled" checked={schedule?.enabled} onCheckedChange={(x) => enableChanged(x)} />
					<Text as="label" align="left">Days</Text>
					<Flex gap="3">
						<Checkbox checked={schedule?.days.sunday} onCheckedChange={(x) => dayChanged('sunday', x === true)} />Sunday
						<Checkbox checked={schedule?.days.monday} onCheckedChange={(x) => dayChanged('monday', x === true)} />Monday
						<Checkbox checked={schedule?.days.tuesday} onCheckedChange={(x) => dayChanged('tuesday', x === true)} />Tuesday
						<Checkbox checked={schedule?.days.wednesday} onCheckedChange={(x) => dayChanged('wednesday', x === true)} />Wednesday
						<Checkbox checked={schedule?.days.thursday} onCheckedChange={(x) => dayChanged('thursday', x === true)} />Thursday
						<Checkbox checked={schedule?.days.friday} onCheckedChange={(x) => dayChanged('friday', x === true)} />Friday
						<Checkbox checked={schedule?.days.saturday} onCheckedChange={(x) => dayChanged('saturday', x === true)} />Saturday
					</Flex>
					<Text as="label" align="left">Time</Text>
					<TextField.Root type="time" value={schedule?.time} onChange={(e) => timeChanged(e.target.value)} />
					<Text as="label" align="left">Backups to Keep</Text>
					<TextField.Root type="number" value={schedule?.keepLast} onChange={(e) => keepLastChanged(e.target.value)} />
				</Grid>
				<DataTable includeCheckBox columns={columns} data={volumes} keyField='name' selection={selection} onSelectionChange={selectionChanged} defaultSortColumn={1} />
			</Flex>
		</>

	)

}

export default ScheduleDetails;