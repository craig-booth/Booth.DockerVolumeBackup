const _1KB = 1024;
const _1MB = 1024 * _1KB;

const datetimeFormatter = new Intl.DateTimeFormat(undefined, { dateStyle: 'full', timeStyle: 'short' });
const kiloByteSizeFormatter = new Intl.NumberFormat(undefined, { style: 'unit', unit: 'kilobyte', unitDisplay: 'short', maximumFractionDigits: 0 });
const megaByteSizeFormatter = new Intl.NumberFormat(undefined, { style: 'unit', unit: 'megabyte', unitDisplay: 'short', maximumFractionDigits: 0 });

export function formatLongDateTime(date?: Date) {
	if (!date)
		return '-';

	return datetimeFormatter.format(date);
}

export function formatDuration(duration?: number) {
	if (!duration)
		return '-';
	duration += 34000;

	const durationSeconds = Math.trunc(duration / 1000);

	const seconds = Math.trunc(durationSeconds % 60);
	const minutes = Math.trunc((durationSeconds - seconds) % 3600) / 60;
	const hours = Math.trunc(durationSeconds / 3600);
	
	return (hours > 0 ? hours + 'hr ' + minutes + 'min ' + seconds + 's' : (minutes > 0 ? minutes + 'min ' + seconds + 's' : seconds + 's')) ;
}

export function formatStorageSize(size?: number) {
	if (!size)
		return '-';

	if (size < _1MB)
		return kiloByteSizeFormatter.format(size / _1KB);
	else
		return megaByteSizeFormatter.format(size / _1MB);
}