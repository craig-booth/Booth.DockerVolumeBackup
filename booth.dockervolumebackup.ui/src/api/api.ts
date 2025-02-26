import { parseJson } from '@/api/jsonParser';
import { Volume } from '@/models/Volume';
import { VolumeBackupRequest } from '@/models/VolumeBackupRequest';
import { Backup, BackupDetail } from '@/models/Backup';


export interface UseApiResult {
    getVolumes(): Promise<Volume[]>;
    backupVolumes(backupRequest: VolumeBackupRequest): Promise<number>;
    getBackups(): Promise<Backup[]>;
    getBackup(backupId: number): Promise<BackupDetail>;
}

export const useApi = (): UseApiResult => {


    const getVolumes = async (): Promise<Volume[]> => {

        const volumes = fetch('/api/volumes',
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => parseJson<Volume[]>(response))

        return volumes;
    }

    const backupVolumes = async (backupRequest: VolumeBackupRequest): Promise<number> => {
        const backupId = fetch('/api/backups/run',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(backupRequest)
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => Number.parseInt(response))

        return backupId;
    }

    const getBackup = async (backupId: number): Promise<BackupDetail> => {

        const backup = fetch('/api/backups/' + backupId,
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => parseJson<BackupDetail>(response))

        return backup;
    }

    const getBackups = async (): Promise<Backup[]> => {

        const backups = fetch('/api/backups',
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => parseJson<Backup[]>(response))

        return backups;
    }

    return {
        getVolumes,
        backupVolumes,
        getBackups,
        getBackup
    };
}