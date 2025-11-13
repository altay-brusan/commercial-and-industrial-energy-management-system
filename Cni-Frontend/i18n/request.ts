import { getUserLocale } from '@/src/services/locale';
import {getRequestConfig} from 'next-intl/server';

export default getRequestConfig(async () => {

  const locale = await getUserLocale();

  return {
    locale,
    messages: (await import(`@/src/messages/${locale}.json`)).default
  };
});
