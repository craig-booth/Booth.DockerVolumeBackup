export function parseJson<T>(json: string): T  {
    return JSON.parse(json, customReviver) as T;
} 


// eslint-disable-next-line @typescript-eslint/no-explicit-any
const customReviver = (key: string, value: any): any => {

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