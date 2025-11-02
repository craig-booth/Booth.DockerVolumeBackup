import { Button } from '@radix-ui/themes';

interface DownloadButtonProps {
	backupvolumeid: number;
}
export function DownloadButton({ backupvolumeid }: DownloadButtonProps) {
	return (
		<a href={'/api/volumebackups/' + backupvolumeid + '/download'} target="_blank">
			<Button>Download</Button>
		</a>
	);
}