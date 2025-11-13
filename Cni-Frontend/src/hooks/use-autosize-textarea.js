import { useEffect } from 'react';

export const useAutosizeTextArea = (textAreaRef, value) => {
  useEffect(() => {
    if (textAreaRef) {
      textAreaRef.style.height = '50px';
      const { scrollHeight } = textAreaRef;

      textAreaRef.style.height = `${scrollHeight}px`;
    }
  }, [textAreaRef, value]);
};
