import { pad } from '@utilities/numbers';

export function getTimestamp() {
    var datetime = new Date();
    return `${pad(datetime.getFullYear(), 4)}-${pad(datetime.getMonth(), 2)}-${pad(datetime.getDate(), 2)}_${pad(datetime.getHours(), 2)}-${pad(datetime.getMinutes(), 2)}-${pad(datetime.getSeconds(), 2)}`;
}