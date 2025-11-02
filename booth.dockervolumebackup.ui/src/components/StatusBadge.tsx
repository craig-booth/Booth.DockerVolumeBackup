import { Badge, BadgeProps } from '@radix-ui/themes';
import { Status } from '@/models/Backup';


export type StatusBadgeProps = {
	status: Status;
}

export function StatusBadge({ status }: StatusBadgeProps) {

	const statusColor = (status: Status): BadgeProps['color'] => {
		switch (status) {
			case 'Queued':
				return 'gray';
			case 'Active':
				return 'blue';
			case 'Complete':
				return 'green';
			case 'Error':
				return 'red';
		}

	};

	return (
		<>
			<Badge color={statusColor(status)} size="2" variant="outline">{status}</Badge>
		</>
	);
}