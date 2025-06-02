
export interface Schedule {
    scheduleId: number;
    name: string;
    enabled: boolean
}

export interface ScheduleDetail {
    scheduleId: number;
    name: string;
    enabled: boolean;
    days: ScheduleDays;
    time: string;
    keepLast: number;
    volumes: string[];
}

export interface ScheduleDays {
    sunday: boolean;
    monday: boolean;
    tuesday: boolean;
    wednesday: boolean;
    thursday: boolean;
    friday: boolean;
    saturday: boolean;
}
