// Phần tương tác không phụ thuộc GSAP: dựng nội dung động, co giãn cảnh minh hoạ, phím, preset.
import { THUMBS, pixelColors } from './data.js';

const SPRITE = 'assets/icons/sprite.svg';
const icon = (id, size = 16) =>
  `<svg class="icon" width="${size}" height="${size}" aria-hidden="true"><use href="${SPRITE}#${id}"/></svg>`;

/** Điền các ô màu khảm vào mọi `.pixel-cells[data-cells]`. */
export function renderPixelCells(root) {
  root.querySelectorAll('.pixel-cells[data-cells]').forEach((box) => {
    const attr = `data-${box.dataset.cellAttr || 'cell'}`;
    box.innerHTML = pixelColors(+box.dataset.cells)
      .map((c) => `<span ${attr} style="background:${c}"></span>`)
      .join('');
  });
}

const MOCKS = {
  code: () => `<div class="thumb__mock thumb__mock--code">
      <span style="width:44%;height:6px;background:#7FB4E6;opacity:.8"></span>
      <span style="width:70%;height:6px;background:#3A3F48"></span>
      <span style="width:58%;height:6px;background:#B5D19A;opacity:.7;margin-left:14px"></span>
      <span style="width:64%;height:6px;background:#3A3F48;margin-left:14px"></span>
      <span style="width:38%;height:6px;background:#E6C07B;opacity:.7;margin-left:28px"></span>
      <span style="width:20%;height:6px;background:#3A3F48"></span></div>`,
  form: () => `<div class="thumb__mock thumb__mock--form">
      <span style="width:30%;height:5px;background:#9AA1AB"></span>
      <span style="height:16px;background:#fff;border:1px solid #D5D9DF;border-radius:3px"></span>
      <span style="width:24%;height:5px;background:#9AA1AB"></span>
      <span style="height:16px;background:#fff;border:1px solid #D5D9DF;border-radius:3px"></span>
      <span style="width:34%;height:16px;background:#0078D4;border-radius:3px;align-self:flex-end"></span></div>`,
  chart: (t) => `<div class="thumb__mock thumb__mock--chart">${t.bars
      .map((b) => `<span style="flex:1;height:${b.h};background:${b.c};border-radius:2px 2px 0 0"></span>`)
      .join('')}</div>`,
  chat: () => `<div class="thumb__mock thumb__mock--chat">
      <span style="width:58%;height:18px;background:#EEF0F3;border-radius:9px"></span>
      <span style="width:42%;height:18px;background:#0078D4;border-radius:9px;align-self:flex-end"></span>
      <span style="width:66%;height:18px;background:#EEF0F3;border-radius:9px"></span>
      <span style="width:30%;height:18px;background:#0078D4;border-radius:9px;align-self:flex-end"></span></div>`
};

/** Dựng lưới thumbnail của mục Gallery. */
export function renderGallery(root) {
  const grid = root.querySelector('#gal-grid');
  if (!grid) return;
  grid.innerHTML = THUMBS.map((t) => `
    <div class="thumb" data-thumb>
      <div class="thumb__shot">
        ${MOCKS[t.kind](t)}
        <span class="handles"><span class="h-tl"></span><span class="h-tr"></span><span class="h-bl"></span><span class="h-br"></span></span>
      </div>
      <div class="thumb__actions">
        <button class="thumb__action" type="button" aria-label="Pin to screen">${icon('i-pin')}</button>
        <button class="thumb__action" type="button" aria-label="Copy">${icon('i-copy')}</button>
        <button class="thumb__action thumb__action--danger" type="button" aria-label="Delete">${icon('i-trash')}</button>
      </div>
      <div class="thumb__meta"><span class="thumb__name">${t.name}</span><span class="thumb__size">${t.size}</span></div>
    </div>`).join('');

  const count = root.querySelector('#gal-count');
  if (count) count.textContent = `· ${THUMBS.length}`;
}

/** Scale mỗi `.stage` cho vừa khung `.stage-frame` (kích thước gốc lấy từ biến CSS --w). */
export function initStageFit(root) {
  const frames = Array.from(root.querySelectorAll('.stage-frame'));
  const apply = (frame) => {
    const stage = frame.querySelector('.stage');
    const base = parseFloat(getComputedStyle(frame).getPropertyValue('--w'));
    if (stage && base) stage.style.transform = `scale(${frame.clientWidth / base})`;
  };
  frames.forEach(apply);
  if ('ResizeObserver' in window) {
    const ro = new ResizeObserver((entries) => entries.forEach((e) => apply(e.target)));
    frames.forEach((f) => ro.observe(f));
  }
}

/** Làm sáng keycap tương ứng khi người dùng nhấn phím thật. */
export function initKeycaps(root) {
  const keys = ['control', 'shift', 'a', 'printscreen', 'escape', 'z', 'y', 'enter', 's', 'o'];
  const set = (k, on) => root.querySelectorAll(`[data-key="${k}"]`)
    .forEach((el) => (on ? el.setAttribute('data-on', '') : el.removeAttribute('data-on')));
  const keyOf = (e) => {
    const k = (e.key || '').toLowerCase();
    return keys.includes(k) ? k : null;
  };

  window.addEventListener('keydown', (e) => {
    const k = keyOf(e);
    if (k) set(k, true);
  });
  window.addEventListener('keyup', (e) => {
    const k = keyOf(e);
    if (!k) return;
    if (k === 'printscreen') {
      // Windows chỉ gửi keyup cho PrtSc -> nháy sáng ngắn
      set(k, true);
      setTimeout(() => set(k, false), 240);
    } else {
      set(k, false);
    }
  });
  window.addEventListener('blur', () => keys.forEach((k) => set(k, false)));
}

/** Nút chọn preset Beautify: đổi lớp gradient phía sau ảnh minh hoạ. */
export function initPresets(root) {
  const buttons = Array.from(root.querySelectorAll('[data-preset]'));
  const layers = Array.from(root.querySelectorAll('[data-grad]'));
  const name = root.querySelector('#bf-name');

  const pick = (preset) => {
    buttons.forEach((b) => b.setAttribute('aria-pressed', String(b.dataset.preset === preset)));
    layers.forEach((layer) => {
      const opacity = layer.dataset.grad === preset ? 1 : 0;
      if (window.gsap) window.gsap.to(layer, { opacity, duration: .45, ease: 'power2.out', overwrite: true });
      else layer.style.opacity = opacity;
    });
    if (name) name.textContent = preset;
  };

  buttons.forEach((b) => b.addEventListener('click', () => pick(b.dataset.preset)));
}

/** Nhãn kích thước "W × H" của khung tải về luôn khớp kích thước thật. */
export function initDownloadSize(root) {
  const box = root.querySelector('#dl-box');
  const out = root.querySelector('#dl-size');
  if (!box || !out || !('ResizeObserver' in window)) return;
  new ResizeObserver(() => {
    out.textContent = `${Math.round(box.offsetWidth)} × ${Math.round(box.offsetHeight)}`;
  }).observe(box);
}

/** Danh sách công cụ ở mục Annotate: đánh dấu công cụ đang được "vẽ". */
export function createToolList(root) {
  const items = Array.from(root.querySelectorAll('[data-tool]'));
  const out = root.querySelector('#ann-step');
  let active = -1;

  const set = (index) => {
    active = index;
    items.forEach((li, j) => (j === index ? li.setAttribute('data-on', '') : li.removeAttribute('data-on')));
    const shown = index < 0 ? items.length - 1 : index;
    if (out && items[shown]) {
      out.textContent = `${shown + 1} / ${items.length} · ${items[shown].querySelector('[data-tname]').textContent}`;
    }
  };

  return {
    set,
    refresh: () => set(active),
    get active() { return active; },
    get count() { return items.length; }
  };
}
