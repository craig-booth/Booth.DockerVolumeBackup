import { Link } from 'react-router-dom';
import * as Toast from '@radix-ui/react-toast';
import { Cross2Icon } from '@radix-ui/react-icons';

export interface QueuedBackupToastProps extends Toast.ToastProps {
	backupId: number
}

export function QueuedBackupToast(props: QueuedBackupToastProps) {
	
	return (
		<Toast.Root className="ToastRoot" {...props}>
			Backup Queued. Click to <Link to={'/backups/' + props.backupId}>view</Link>
			<Toast.Close><Cross2Icon height="16" width="16" cursor="default" /></Toast.Close>
		</Toast.Root>
	);
}


