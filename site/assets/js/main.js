// Điểm khởi chạy của landing page NeatShot.
import { initI18n } from './i18n.js';
import { initMotion } from './motion.js';
import {
  createToolList,
  initDownloadSize,
  initKeycaps,
  initPresets,
  initStageFit,
  renderGallery,
  renderPixelCells
} from './ui.js';

const root = document.getElementById('ns-root');

if (root) {
  // 1. Dựng nội dung động trước để i18n và GSAP nhìn thấy đầy đủ DOM
  renderPixelCells(root);
  renderGallery(root);

  // 2. Tương tác cơ bản (chạy được cả khi GSAP không tải được)
  const toolList = createToolList(root);
  let motion = null;
  initI18n(root, {
    beforeSwap: () => motion?.revertSplits(),
    onChange: () => toolList.refresh()
  });
  initStageFit(root);
  initKeycaps(root);
  initPresets(root);
  initDownloadSize(root);

  // 3. Chuyển động: script GSAP được nạp defer nên thường đã sẵn sàng; chờ thêm nếu CDN chậm
  let tries = 0;
  const startMotion = () => {
    if (window.gsap && window.ScrollTrigger) {
      motion = initMotion(root, { toolList });
    } else if (tries++ < 100) {
      setTimeout(startMotion, 60);
    } else {
      toolList.set(-1);
    }
  };
  startMotion();
}
