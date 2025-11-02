import { useState, useEffect } from 'react';
import { Button, Dialog, Flex, Text, TextField, Callout } from '@radix-ui/themes';
import { ExclamationTriangleIcon, InfoCircledIcon } from '@radix-ui/react-icons';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useApi } from '@/api/api';
import { formatLongDateTime } from '@/utils/Formatting';

interface RestoreButtonProps {
	backupvolumeid: number;
	volume: string;
	backuptime?: Date;
}

interface RestoreRequestParamaters {
	backupVolumeId: number,
	restoreVolumeName: string
}

export function RestoreButton({ backupvolumeid, volume, backuptime }: RestoreButtonProps) {

	const [dialogOpen, setDialogOpen] = useState(false);
	const [restoreVolumeName, setRestoreVolumeName] = useState(volume);
	const [existingVolume, setExistingVolume] = useState(true);

	const { getVolume, restoreVolume } = useApi();

	const { isSuccess, data: volumeDetail } = useQuery({
		queryKey: ['volume', restoreVolumeName],
		queryFn: () => getVolume(restoreVolumeName),
		enabled: dialogOpen
	});

	const restoreRequest = useMutation({
		mutationFn: (params: RestoreRequestParamaters) => {
			return restoreVolume(params.backupVolumeId, params.restoreVolumeName);
		},
		onSuccess: () => {
		//	setBackupRequested(false);
		//	setBackupId(data);
		//	setShowToast(true);
		}
	});

	const onDialogOpen = (open: boolean) => {
		setDialogOpen(open)
	}

	const volumeNameChanged = (value: string) => {
		setRestoreVolumeName(value);	
	}


	useEffect(() => {
		if (isSuccess) {
			setExistingVolume(isSuccess && (volumeDetail != null));
		}
	}, [isSuccess, volumeDetail]);

	return (
		<Dialog.Root onOpenChange={onDialogOpen}>
			<Dialog.Trigger>
				<Button>Restore</Button>
			</Dialog.Trigger>
			<Dialog.Content>
				<Dialog.Title>Restore</Dialog.Title>
				<Dialog.Description>{'Restore volume ' + volume + ' from backup on ' + formatLongDateTime(backuptime)}</Dialog.Description>

				<Flex align="start" direction="column" gap="3" py="5">
					<Flex direction="row" gap="5">
						<Text as="label">Restore volume as</Text>
						<TextField.Root value={restoreVolumeName} onChange={(e) => volumeNameChanged(e.target.value)} />
					</Flex>

					{
						existingVolume ? 
							<Callout.Root color="red">
								<Callout.Icon><ExclamationTriangleIcon /></Callout.Icon>
								<Callout.Text>
									Existing contents in the volume will be overwritten with contents from the backup.
									<br /><br />
									Any services using the volume will be stopped before restoring and restarted once complete.
								</Callout.Text>
							</Callout.Root>
							:		
							<Callout.Root>
								<Callout.Icon><InfoCircledIcon /></Callout.Icon>
								<Callout.Text>
									The volume does not exist.
									<br /><br />
									It will be created first and then the backup contents restored into the new volume.
								</Callout.Text>
							</Callout.Root>  
					}


				</Flex>


				<Flex gap="3" mt="4" justify="end">
					<Dialog.Close>
						<Button variant="soft" color="gray">
							Cancel
						</Button>
					</Dialog.Close>
					<Dialog.Close>
						<Button color="red" onClick={() => restoreRequest.mutate({ backupVolumeId: backupvolumeid, restoreVolumeName })} disabled={restoreRequest.isPending}>Restore</Button>
					</Dialog.Close>
				</Flex>
			</Dialog.Content>
		</Dialog.Root>
		
	);
}