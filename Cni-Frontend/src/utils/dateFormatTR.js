import { formatISO, format } from 'date-fns';
import * as Locales from 'date-fns/locale';

export const formatDateISO = (date) => {
  return date ? formatISO(new Date(date)) : null;
};

export const formatDate = (date = '', formatType = 'dd-MM-yyyy | HH:mm:ss', options = {}) => {
  return date
    ? format(new Date(date), formatType, {
        ...options,
        locale: Locales.tr
      })
    : null;
};

export const hourMinute = (date = new Date()) => {
  return format(new Date(date), 'HH:mm');
};

export const hourMinuteSecond = (date = new Date()) => {
  return format(new Date(date), 'HH:mm:ss');
};

export const updateTimeToDate = (time) => {
  const date = new Date();
  const splitedHourMinute = time.split(':');
  const hour = Number(splitedHourMinute[0]);
  const minute = Number(splitedHourMinute[1]);

  date.setHours(hour);
  date.setMinutes(minute);
  return date;
};
