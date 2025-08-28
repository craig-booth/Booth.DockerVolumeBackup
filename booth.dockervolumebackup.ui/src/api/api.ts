import { parseJson } from '@/api/jsonParser';
import { Volume } from '@/models/Volume';
import { VolumeBackupRequest } from '@/models/VolumeBackupRequest';
import { BackupDeleteRequest } from '@/models/BackupDeleteRequest';
import { Backup, BackupDetail } from '@/models/Backup';
import { Schedule, ScheduleDetail } from '@/models/Schedule';


export interface UseApiResult {
    getVolumes(): Promise<Volume[]>;
    backupVolumes(backupRequest: VolumeBackupRequest): Promise<number>;

    getBackups(): Promise<Backup[]>;
    getBackup(backupId: number): Promise<BackupDetail>;
    deleteBackup(backupId: number): Promise<boolean>;
    deleteBackups(backupIds: number[]): Promise<number>;
    runBackup(scheduleId: number): Promise<number>;

    getSchedules(): Promise<Schedule[]>;
    getSchedule(scheduleId: number): Promise<ScheduleDetail>;
    createSchedule(schedule: ScheduleDetail): Promise<number>;
    updateSchedule(schedule: ScheduleDetail): Promise<boolean>;
    deleteSchedule(scheduleId: number): Promise<boolean>;
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

    const deleteBackup = async (backupId: number): Promise<boolean> => {

        const result = fetch('/api/backups/' + backupId,
            {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return true;
            })

        return result;
    }

    const deleteBackups = async (backupIds: number[]): Promise<number> => {

        const backupDeleteRequest: BackupDeleteRequest = { backupIds: backupIds };

        const result = fetch('/api/backups/delete',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(backupDeleteRequest)
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => Number.parseInt(response))

        return result;
    }

    const runBackup = async (scheduleId: number): Promise<number> => {

        const backupId = fetch('/api/backups/' + scheduleId + '/run',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: null
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => Number.parseInt(response))

        return backupId;
    }


    const getSchedules = async (): Promise<Schedule[]> => {

        const schedules = fetch('/api/schedules',
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
            .then(response => parseJson<Schedule[]>(response))

        return schedules;
    }

    const getSchedule = async (scheduleId: number): Promise<ScheduleDetail> => {

        const schedule = fetch('/api/schedules/' + scheduleId,
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
            .then(response => parseJson<ScheduleDetail>(response))

        return schedule;
    }

    const createSchedule = async (schedule: ScheduleDetail): Promise<number> => {

        const scheduleId = fetch('/api/schedules/',
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(schedule),
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return response.text();
            })
            .then(response => parseJson<number>(response))

        return scheduleId;
    }

    const updateSchedule = async (schedule: ScheduleDetail): Promise<boolean> => {
        const result = fetch('/api/schedules/' + schedule.scheduleId,
            {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(schedule),
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return true;
            })

        return result;
    }

    const deleteSchedule = async (scheduleId: number): Promise<boolean> => {

        const result = fetch('/api/schedules/' + scheduleId,
            {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
            })
            .then(response => {
                if (!response.ok)
                    throw new Error(response.statusText)

                return true;
            })

        return result;
    }

    return {
        getVolumes,
        backupVolumes,
        getBackups,
        getBackup,
        deleteBackup,
        deleteBackups,
        runBackup,
        getSchedules,
        getSchedule,
        createSchedule,
        updateSchedule,
        deleteSchedule
    };
}