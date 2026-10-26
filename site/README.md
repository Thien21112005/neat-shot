# NeatShot — Landing page

Static marketing site for NeatShot, published with GitHub Pages at
<https://thien21112005.github.io/neat-shot/>. No build step: plain HTML, CSS and ES modules, plus
GSAP 3.13 (ScrollTrigger, SplitText, DrawSVG) loaded from jsDelivr.

## Structure

```text
site/
├── index.html              # Markup for every section (hero → download → footer)
└── assets/
    ├── css/
    │   ├── tokens.css      # Design tokens: colors, fonts, spacing
    │   ├── base.css        # Reset, base typography, keyframes, reduced-motion
    │   ├── components.css  # Reusable pieces: buttons, keycaps, badges, handles, stage frames, mock toolbar
    │   └── sections.css    # Section-specific layout, in page order
    ├── js/
    │   ├── main.js         # Entry point: wires everything together
    │   ├── i18n.js         # EN/VI switcher + Vietnamese dictionary
    │   ├── data.js         # Demo data (gallery thumbnails, pixelate colors)
    │   ├── ui.js           # Non-animated behavior: stage scaling, keycaps, presets, gallery rendering
    │   └── motion.js       # All GSAP animations (respects prefers-reduced-motion)
    └── icons/
        ├── sprite.svg      # Line icon set (24×24, stroke = currentColor)
        └── favicon.svg
```

### Conventions

- **Illustrations are code, not images.** Mock desktops and app windows are HTML/CSS + SVG inside a
  `.stage-frame` (`style="--w:<width>;--h:<height>"`). `ui.js` scales the inner `.stage` to fit, so
  absolute coordinates inside a stage are its drawing geometry and stay inline.
- **Icons** come from the sprite: `<svg class="icon" width="18" height="18"><use href="assets/icons/sprite.svg#i-pin"/></svg>`.
- **Translations:** English lives in the HTML; add `data-i18n="key"` and the Vietnamese string in `VI` (`i18n.js`).
- **Motion hooks** use `id` / `data-*` attributes (`data-reveal`, `data-split`, `data-hstep`…), never class names.

## Preview locally

ES modules and the external SVG sprite need an HTTP server (opening the file directly won't work):

```bash
cd site
python -m http.server 8080
# then open http://localhost:8080
```

## Design source

The original Claude Design export this site was built from is kept in
[`docs/design/landing-page-export/`](../docs/design/landing-page-export/).
