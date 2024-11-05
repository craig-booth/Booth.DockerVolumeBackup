import { Volume } from '@/models/Volume';
import { VolumeBackupRequest } from '@/models/VolumeBackupRequest';

export interface UseApiResult {
    getVolumes(): Promise<Volume[]>;
    backupVolumes(backupRequest: VolumeBackupRequest): Promise<number>;
}

export const useApi = (): UseApiResult => {


    const getVolumes = async (): Promise<Volume[]> => {

        const volumes = fetch('api/volumes',
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
            .then(response => JSON.parse(response, customReviver))

        return volumes;
    }

    const backupVolumes = async (backupRequest: VolumeBackupRequest): Promise<number> => {

        const backupId = fetch('api/volumes/backup',
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

    return {
        getVolumes,
        backupVolumes
    };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function customReviver(key: string, value: any): any {

    if ((key == 'lastBackup') || (key == 'startTime') || (key == 'endTime') || (key == 'backupTime')) {
        if ((value != undefined) && (value != null)) {

            if (value == '9999-12-31')
                return undefined;
            else {
                const date = new Date(value);
                return date;
            }

        }
    }

    return value;
}