// Dữ liệu minh hoạ: thumbnail thư viện ảnh và ô màu cho hiệu ứng khảm (pixelate).

const BLUE = ['#0078D4', '#3B9BEB', '#9CC8EF'];

const bars = (heights, offset) => heights.map((h, i) => ({ h: `${h}%`, c: BLUE[(i + offset) % 3] }));

export const THUMBS = [
  { name: 'NeatShot_2026-10-24_142305.png', size: '412 × 236', kind: 'form' },
  { name: 'NeatShot_2026-10-24_143012.png', size: '1280 × 720', kind: 'code' },
  { name: 'NeatShot_2026-10-24_150844.png', size: '640 × 360', kind: 'chart', bars: bars([40, 65, 52, 80, 58, 92], 0) },
  { name: 'NeatShot_2026-10-23_091520.png', size: '388 × 512', kind: 'chat' },
  { name: 'NeatShot_2026-10-23_101147.png', size: '960 × 540', kind: 'code' },
  { name: 'NeatShot_2026-10-22_173402.png', size: '720 × 405', kind: 'chart', bars: bars([78, 54, 66, 38, 72, 48], 1) },
  { name: 'NeatShot_2026-10-22_112958.png', size: '412 × 236', kind: 'form' },
  { name: 'NeatShot_2026-10-21_164233.png', size: '540 × 300', kind: 'chat' }
];

const GREYS = ['#C9CED6', '#AEB4BD', '#DDE1E6', '#9AA1AB', '#BFC5CD', '#D0D4DA', '#A7AEB8', '#B8BEC6'];

/** Dãy màu xám giả ngẫu nhiên nhưng cố định (để mỗi lần tải trang trông giống nhau). */
export const pixelColors = (count) =>
  Array.from({ length: count }, (_, i) => GREYS[(i * 7 + ((i * i) >> 1)) % GREYS.length]);
