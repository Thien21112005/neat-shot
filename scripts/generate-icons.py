"""
Sinh toàn bộ icon của NeatShot từ một thiết kế duy nhất (giống site/assets/icons/favicon.svg):
ô vuông bo góc tối, bốn góc khung chọn màu xanh, ô sáng ở giữa.

Đầu ra:
  src/NeatShot/Assets/NeatShot.ico          icon exe + khay hệ thống (16 → 256 px)
  packaging/msix/Assets/*.png               logo cho gói MSIX (Start menu, taskbar, Store)
  docs/store/StoreLogo-300.png              logo vuông cho trang Store (Partner Center)

Chạy:  python scripts/generate-icons.py   (cần Pillow)
"""
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent

BG = (14, 15, 18, 255)        # #0E0F12
ACCENT = (0, 120, 212, 255)   # #0078D4
CENTER = (232, 234, 237, 255)  # #E8EAED
SUPERSAMPLE = 8


def draw_mark(size: int, plate: bool = True, padding: float = 0.0) -> Image.Image:
    """Vẽ logo trên lưới 32×32 (như favicon.svg), phóng to rồi thu nhỏ để khử răng cưa."""
    big = size * SUPERSAMPLE
    img = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    inner = big * (1 - 2 * padding)
    offset = big * padding
    u = inner / 32  # 1 đơn vị lưới

    def p(x, y):
        return (offset + x * u, offset + y * u)

    if plate:
        d.rounded_rectangle([p(0, 0), p(32, 32)], radius=7 * u, fill=BG)

    w = max(1, round(2.5 * u))
    # Bốn góc khung chọn: M7 12V7h5 · M20 7h5v5 · M25 20v5h-5 · M12 25H7v-5
    for pts in ([(7, 12), (7, 7), (12, 7)], [(20, 7), (25, 7), (25, 12)],
                [(25, 20), (25, 25), (20, 25)], [(12, 25), (7, 25), (7, 20)]):
        d.line([p(*q) for q in pts], fill=ACCENT, width=w, joint="curve")
        # đầu nét vuông (stroke-linecap: square nhẹ) để góc không bị khuyết
        for q in (pts[0], pts[-1]):
            cx, cy = p(*q)
            d.rectangle([cx - w / 2, cy - w / 2, cx + w / 2, cy + w / 2], fill=ACCENT)
    d.rectangle([p(13, 13), p(19, 19)], fill=CENTER)

    return img.resize((size, size), Image.LANCZOS)


def save_png(img: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path, "PNG", optimize=True)
    print(f"  {path.relative_to(ROOT)}  {img.width}x{img.height}")


def tile(width: int, height: int, mark_ratio: float) -> Image.Image:
    """Ô logo nền trong suốt, logo nằm giữa (Store tự đặt màu nền theo theme)."""
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    mark = draw_mark(round(min(width, height) * mark_ratio))
    canvas.alpha_composite(mark, ((width - mark.width) // 2, (height - mark.height) // 2))
    return canvas


def main() -> None:
    print("ICO:")
    ico_path = ROOT / "src/NeatShot/Assets/NeatShot.ico"
    ico_path.parent.mkdir(parents=True, exist_ok=True)
    sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    frames = [draw_mark(s) for s in sizes]
    frames[-1].save(ico_path, format="ICO", sizes=[(s, s) for s in sizes], append_images=frames[:-1])
    print(f"  {ico_path.relative_to(ROOT)}  {sizes}")

    print("MSIX:")
    assets = ROOT / "packaging/msix/Assets"
    for scale in (100, 125, 150, 200, 400):
        f = scale / 100
        save_png(tile(round(150 * f), round(150 * f), 0.6), assets / f"Square150x150Logo.scale-{scale}.png")
        save_png(tile(round(310 * f), round(150 * f), 0.6), assets / f"Wide310x150Logo.scale-{scale}.png")
        save_png(tile(round(71 * f), round(71 * f), 0.75), assets / f"Square71x71Logo.scale-{scale}.png")
        save_png(draw_mark(round(44 * f)), assets / f"Square44x44Logo.scale-{scale}.png")
        save_png(draw_mark(round(50 * f)), assets / f"StoreLogo.scale-{scale}.png")
    # Icon taskbar / Start (không bị Windows đặt lên nền màu)
    for target in (16, 20, 24, 30, 32, 36, 40, 48, 60, 64, 72, 80, 96, 256):
        mark = draw_mark(target)
        save_png(mark, assets / f"Square44x44Logo.targetsize-{target}.png")
        save_png(mark, assets / f"Square44x44Logo.targetsize-{target}_altform-unplated.png")
        save_png(mark, assets / f"Square44x44Logo.targetsize-{target}_altform-lightunplated.png")

    print("Store:")
    save_png(draw_mark(300), ROOT / "docs/store/StoreLogo-300.png")


if __name__ == "__main__":
    main()
