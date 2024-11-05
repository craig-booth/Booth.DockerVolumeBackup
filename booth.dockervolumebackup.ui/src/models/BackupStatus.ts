
export interface BackupStatus {
    backupId: number;
    status: string;
    volumes: VolumeBackupStatus[];
}

export interface VolumeBackupStatus {
    volumeName: string;
    status: string;
    backupTime?: Date;
}