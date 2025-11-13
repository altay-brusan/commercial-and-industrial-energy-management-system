import { css } from 'styled-components';

export const sizes: Record<string, number> = {
  xl: 2800,
  lg: 1199,
  md: 991,
  sm: 767,
  xs: 575,
  iphone5: 320
};

// Media fonksiyonunun tipini tanımlayın
const media: Record<string, (...args: [TemplateStringsArray, ...any[]]) => ReturnType<typeof css>> = Object.keys(sizes).reduce((accumulator, label) => {
  // use em in breakpoints to work properly cross-browser and support users
  // changing their browsers font-size: https://zellwk.com/blog/media-query-units/
  const emSize = sizes[label] / 16;
  accumulator[label] = (...args: [TemplateStringsArray, ...any[]]) => css`
    @media (max-width: ${emSize}em) {
      ${css(...args)};
    }
  `;
  return accumulator;
}, {} as Record<string, (...args: [TemplateStringsArray, ...any[]]) => ReturnType<typeof css>>);

export default media;
