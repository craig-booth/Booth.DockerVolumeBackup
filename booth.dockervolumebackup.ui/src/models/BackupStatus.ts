import { Status } from '@/models/Backup';
export interface BackupStatus {
    backupId: number;
    status: Status;
    startTime?: Date;
    endTime?: Date;
    volumes: VolumeBackupStatus[];
}

export interface VolumeBackupStatus {
    backupVolumeId: number;
    status: Status;
    startTime?: Date;
    endTime?: Date;
}