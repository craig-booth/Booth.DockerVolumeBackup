
export type Status = 'Queued' | 'Active' | 'Complete' | 'Error';
export interface Backup {
    backupId: number;
    scheduleId: number;
    scheduleName: string;
    status: Status;
    backupTime: Date;
}

export interface BackupDetail {
    backupId: number;
    scheduleId?: number;
    scheduleName: string;
    status: Status;
    startTime?: Date;
    endTime?: Date;
    volumes: BackupVolumeDetail[];
}

export interface BackupVolumeDetail {
    backupVolumeId: number;
    volume: string;
    status: Status;
    startTime?: Date;
    endTime?: Date;
    backupSize?: number;
}
