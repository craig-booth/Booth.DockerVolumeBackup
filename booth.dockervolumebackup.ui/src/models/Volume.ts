export interface Volume
{
    name: string;
    size: number;
    lastBackup?: Date;
    active: boolean;
}