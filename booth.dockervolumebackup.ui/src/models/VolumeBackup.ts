import { Status } from '@/models/Backup';
export interface VolumeBackup {
    volumeName: string;
    backupId: number;
    backupVolumeId: number;
    scheduleId?: number;
    scheduleName: string;
    status: Status;
    backupTime: Date;
    size: number;
}