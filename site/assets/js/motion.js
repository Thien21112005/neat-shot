// Toàn bộ chuyển động GSAP của trang. Tôn trọng prefers-reduced-motion.

const OPTIONS = {
  loopHero: false,   // Lặp lại demo ở hero
  scrubLength: 2600  // Độ dài cuộn (px) cho câu chuyện Annotate được ghim
};

export function initMotion(root, { toolList }) {
  const gsap = window.gsap;
  const ScrollTrigger = window.ScrollTrigger;
  if (!gsap || !ScrollTrigger) return null;

  gsap.registerPlugin(ScrollTrigger);
  const hasDraw = !!window.DrawSVGPlugin;
  const hasSplit = !!window.SplitText;
  if (hasDraw) gsap.registerPlugin(window.DrawSVGPlugin);
  if (hasSplit) gsap.registerPlugin(window.SplitText);
  gsap.defaults({ ease: 'power3.out', duration: .6 });

  const $ = (s) => root.querySelector(s);
  const $$ = (s) => Array.from(root.querySelectorAll(s));

  // Vẽ nét SVG: dùng DrawSVG nếu có, ngược lại fallback stroke-dashoffset
  const drawSet = (el) => {
    if (!el) return;
    if (hasDraw) gsap.set(el, { drawSVG: '0%' });
    else {
      const len = el.getTotalLength();
      gsap.set(el, { strokeDasharray: len, strokeDashoffset: len });
    }
  };
  const drawTo = (vars = {}) => Object.assign(hasDraw ? { drawSVG: '100%' } : { strokeDashoffset: 0 }, vars);

  let splits = [];

  const headline = () => {
    if (!hasSplit) return undefined;
    splits = $$('[data-split]').map((el) => window.SplitText.create(el, { type: 'lines,words', mask: 'lines' }));
    splits.forEach((s) => {
      if (s.masks?.length) gsap.set(s.masks, { paddingBottom: '0.14em', marginBottom: '-0.14em' });
    });
    gsap.from(splits.flatMap((s) => s.words), { yPercent: 110, duration: .8, stagger: .035, delay: .05 });
    return () => { splits.forEach((s) => s.revert()); splits = []; };
  };

  const hero = () => {
    const stage = $('#hero-fit'), sel = $('#hero-sel'), cur = $('#hero-cursor'), badge = $('#hero-badge');
    const bars = $$('#hero-toolbar .tb'), toast = $('#hero-toast'), hint = $('#hero-hint');
    const arrow = $('#hero-arrow'), head = $('#hero-head');
    const steps = $$('[data-hstep]'), cells = $$('[data-hcell]');
    const wEl = $('#hero-w'), hEl = $('#hero-h');
    const S = { x: 266, y: 140, w: 412, h: 236 };
    const size = { w: 0, h: 0 };
    const writeSize = () => { wEl.textContent = Math.round(size.w); hEl.textContent = Math.round(size.h); };

    gsap.set(sel, { width: 0, height: 0, opacity: 0 });
    gsap.set(badge, { opacity: 0 });
    gsap.set(bars, { opacity: 0, y: -10, scale: .97, transformOrigin: '50% 0' });
    drawSet(arrow);
    gsap.set(head, { opacity: 0 });
    gsap.set(steps, { scale: 0, transformOrigin: '50% 50%' });
    gsap.set(cells, { opacity: 0 });
    gsap.set(toast, { opacity: 0, y: 16 });
    gsap.set(hint, { opacity: 0, y: -6 });
    gsap.set(cur, { x: 640, y: 560, opacity: 0 });
    writeSize();

    const tl = gsap.timeline({ paused: true });
    if (OPTIONS.loopHero) tl.repeat(-1).repeatDelay(1.2);
    tl.to(cur, { opacity: 1, duration: .2 })
      .to(cur, { x: S.x, y: S.y, duration: .7, ease: 'power2.inOut' }, '<')
      .to(hint, { opacity: 1, y: 0, duration: .3 })
      .set(sel, { opacity: 1 }, '<')
      .to(badge, { opacity: 1, duration: .15 }, '<')
      .addLabel('drag')
      .to(sel, { width: S.w, height: S.h, duration: .9, ease: 'power3.inOut' }, 'drag')
      .to(cur, { x: S.x + S.w, y: S.y + S.h, duration: .9, ease: 'power3.inOut' }, 'drag')
      .to(size, { w: S.w, h: S.h, duration: .9, ease: 'power3.inOut', onUpdate: writeSize }, 'drag')
      .to(bars, { opacity: 1, y: 0, scale: 1, duration: .45, ease: 'back.out(1.4)', stagger: .08 }, '+=.08')
      .to(hint, { opacity: 0, duration: .3 }, '<')
      .to(cur, { x: S.x + 112, y: S.y + 214, duration: .5, ease: 'power2.inOut' })
      .addLabel('arrow')
      .to(arrow, drawTo({ duration: .55, ease: 'power2.inOut' }), 'arrow')
      .to(cur, { x: S.x + 242, y: S.y + 188, duration: .55, ease: 'power2.inOut' }, 'arrow')
      .to(head, { opacity: 1, duration: .12 })
      .to(steps, { scale: 1, duration: .3, ease: 'back.out(1.7)', stagger: .2 }, '+=.15')
      .to(cur, { opacity: 0, duration: .3 }, '+=.1')
      .to(cells, { opacity: 1, duration: .15, stagger: { each: .01, from: 'random' } }, '<')
      .to(toast, { opacity: 1, y: 0, duration: .45 }, '+=.25')
      .to(toast, { opacity: 0, y: 8, duration: .4 }, '+=2.6');

    const start = gsap.delayedCall(.6, () => tl.play(0));
    const replay = () => { if (!tl.isActive()) tl.restart(); };
    stage.addEventListener('mouseenter', replay);
    return () => { stage.removeEventListener('mouseenter', replay); start.kill(); tl.kill(); };
  };

  const beforeAfter = () => {
    $$('[data-annoy]').forEach((row) => {
      const tl = gsap.timeline({ scrollTrigger: { trigger: row, start: 'top 75%', once: true } });
      const before = row.querySelector('[data-side="before"]');
      const after = row.querySelector('[data-side="after"]');
      const q = (el, s) => Array.from(el.querySelectorAll(s));

      switch (row.dataset.annoy) {
        case 'tray':
          tl.fromTo(q(before, '[data-flash]'), { opacity: .7 }, { opacity: 0, duration: .5 }, .3)
            .fromTo(q(after, '[data-menu]'), { opacity: 1, scale: 1 }, { opacity: 0, scale: .96, duration: .3, transformOrigin: '100% 100%' }, .3)
            .fromTo(q(after, '[data-flash]'), { opacity: .7 }, { opacity: 0, duration: .5 }, .75)
            .from(q(after, '[data-ok]'), { opacity: 0, scale: .8, duration: .35, ease: 'back.out(1.7)' }, 1);
          break;
        case 'keys':
          tl.to(q(before, '[data-press]'), { y: 3, duration: .08, yoyo: true, repeat: 1 }, .3)
            .from(q(before, '[data-pop]'), { opacity: 0, y: 10, duration: .4 }, .55)
            .to(q(after, '[data-press]'), { y: 3, duration: .08, yoyo: true, repeat: 1, stagger: .12 }, .3)
            .from(q(after, '[data-pop]'), { opacity: 0, scale: .92, duration: .4, ease: 'back.out(1.4)' }, .85)
            .from(q(after, '[data-ok]'), { opacity: 0, duration: .3 }, 1.15);
          break;
        default:
          tl.from(q(before, '[data-shift]'), { x: -28, y: -18, duration: .7, ease: 'power2.inOut' }, .3)
            .from(q(after, '[data-shift]'), { x: 28, y: 18, duration: .6 }, .3)
            .from(q(after, '[data-ok]'), { opacity: 0, scale: .8, duration: .35, ease: 'back.out(1.7)' }, .95);
      }
    });
  };

  const annotate = (desktop) => {
    const arrow = $('#ann-arrow'), head = $('#ann-head'), rect = $('#ann-rect');
    const label = $('#ann-label'), blur = $('#ann-blur');
    const steps = $$('[data-astep]'), cells = $$('[data-acell]');
    drawSet(arrow);
    drawSet(rect);
    gsap.set(head, { opacity: 0 });
    gsap.set(label, { opacity: 0, y: 8 });
    gsap.set(steps, { scale: 0, transformOrigin: '50% 50%' });
    gsap.set(cells, { opacity: 0 });
    gsap.set(blur, { opacity: 0 });
    toolList.set(0);

    const last = toolList.count - 1;
    const tl = gsap.timeline(desktop
      ? {
          defaults: { ease: 'none' },
          scrollTrigger: {
            trigger: '#annotate-pin', start: 'top top', end: `+=${OPTIONS.scrubLength}`,
            pin: true, scrub: .6, anticipatePin: 1,
            onUpdate: (s) => {
              const i = Math.min(last, Math.floor(s.progress * toolList.count));
              if (i !== toolList.active) toolList.set(i);
            }
          }
        }
      : { scrollTrigger: { trigger: '#ann-canvas', start: 'top 70%', once: true } });

    const D = desktop ? 1 : .5;
    tl.to(arrow, drawTo({ duration: .75 * D }), 0)
      .to(head, { opacity: 1, duration: .1 * D }, .75 * D)
      .to(rect, drawTo({ duration: .8 * D }), 1 * D)
      .to(label, { opacity: 1, y: 0, duration: .6 * D }, 2 * D)
      .to(steps, { scale: 1, duration: .25 * D, stagger: .25 * D }, 3 * D)
      .to(cells, { opacity: 1, duration: .1 * D, stagger: { each: .025 * D, from: 'random' } }, 4 * D)
      .to(blur, { opacity: 1, duration: .7 * D }, 5 * D)
      .to({}, { duration: .3 * D }, 5.7 * D);
    if (!desktop) {
      for (let i = 0; i < toolList.count; i++) tl.call(() => toolList.set(i), null, i * D);
    }
    return () => toolList.set(-1);
  };

  const beautify = () => {
    const grads = $('#bf-grads'), shot = $('#bf-shot'), shadow = $('#bf-shadow-l'), card = $('#bf-card');
    const pad = $('#bf-pad'), rad = $('#bf-rad'), shadowLabel = $('#bf-shv');
    const p = { pad: 0, rad: 0 };
    const write = () => { pad.textContent = `${Math.round(p.pad)}px`; rad.textContent = `${Math.round(p.rad)}px`; };

    gsap.set(grads, { xPercent: -100 });
    gsap.set(shot, { scale: 1 });
    gsap.set([shadow, card], { borderRadius: 0 });
    gsap.set(shadow, { opacity: 0 });
    write();
    shadowLabel.textContent = 'none';

    // Giá trị đúng với mặc định của app: Padding 20px, CornerRadius 12px
    gsap.timeline({ scrollTrigger: { trigger: '#bf-fit', start: 'top 70%', once: true } })
      .to(shot, { scale: .9, duration: .8, ease: 'power3.inOut' }, 0)
      .to(p, { pad: 20, duration: .8, ease: 'power3.inOut', onUpdate: write }, 0)
      .to([shadow, card], { borderRadius: 12, duration: .5 }, .7)
      .to(p, { rad: 12, duration: .5, onUpdate: write }, .7)
      .to(shadow, { opacity: 1, duration: .6 }, 1.1)
      .call(() => { shadowLabel.textContent = 'soft'; }, null, 1.1)
      .to(grads, { xPercent: 0, duration: .8 }, 1.4);
    return () => { shadowLabel.textContent = 'soft'; p.pad = 20; p.rad = 12; write(); };
  };

  const pin = () => {
    const frame = $('#pin-fit'), main = $('#pin-main'), hud = $('#pin-hud'), pen = $('#pin-pen');
    const p = { z: 100, o: 100 };
    const write = () => {
      gsap.set(main, { scale: p.z / 100, opacity: p.o / 100 });
      hud.textContent = `Zoom ${Math.round(p.z)}% · Opacity ${Math.round(p.o)}%`;
    };
    write();
    drawSet(pen);
    gsap.from($$('[data-pin-other]'), {
      opacity: 0, y: 30, duration: .6, stagger: .15,
      scrollTrigger: { trigger: frame, start: 'top 75%', once: true }
    });
    gsap.timeline({ defaults: { ease: 'none' }, scrollTrigger: { trigger: frame, start: 'top 70%', end: 'bottom 40%', scrub: .6 } })
      .to(p, { z: 120, duration: 1, onUpdate: write })
      .to(p, { o: 70, duration: 1, onUpdate: write })
      .to(pen, drawTo({ duration: 1 }));
    return () => { hud.textContent = 'Zoom 120% · Opacity 70%'; };
  };

  const ocr = () => {
    const scan = $('#ocr-scan'), boxes = $$('[data-obox]'), lines = $$('[data-oline]');
    const H = 400, T = 2.4;
    gsap.set(boxes, { opacity: 0 });
    const lineSplits = lines.map((o) => (hasSplit ? window.SplitText.create(o, { type: 'chars' }) : null));
    lines.forEach((o, i) => {
      if (lineSplits[i]) gsap.set(lineSplits[i].chars, { opacity: 0 });
      else gsap.set(o, { opacity: 0, x: -6 });
    });

    const tl = gsap.timeline({ scrollTrigger: { trigger: '#ocr-fit', start: 'top 65%', once: true } });
    tl.fromTo(scan, { y: 0, opacity: 1 }, { y: H, duration: T, ease: 'none' }, 0).to(scan, { opacity: 0, duration: .3 });
    boxes.forEach((b, i) => {
      const t = (+b.dataset.y / H) * T;
      tl.to(b, { opacity: 1, duration: .2 }, t);
      if (lineSplits[i]) tl.to(lineSplits[i].chars, { opacity: 1, duration: .01, stagger: .014 }, t);
      else if (lines[i]) tl.to(lines[i], { opacity: 1, x: 0, duration: .3 }, t);
    });
    return () => lineSplits.forEach((s) => s?.revert());
  };

  const gallery = () => {
    gsap.from($$('[data-thumb]'), {
      opacity: 0, y: 24, scale: .97, duration: .6, stagger: .06,
      scrollTrigger: { trigger: '#gal-grid', start: 'top 80%', once: true }
    });
  };

  const reveals = () => {
    const els = $$('[data-reveal]');
    gsap.set(els, { opacity: 0, y: 20 });
    ScrollTrigger.batch(els, {
      start: 'top 88%', once: true,
      onEnter: (batch) => gsap.to(batch, { opacity: 1, y: 0, duration: .7, stagger: .08, overwrite: true })
    });
  };

  const magnetic = () => {
    const cleanups = [];
    $$('[data-magnetic]').forEach((el) => {
      const xTo = gsap.quickTo(el, 'x', { duration: .4, ease: 'power3.out' });
      const yTo = gsap.quickTo(el, 'y', { duration: .4, ease: 'power3.out' });
      const move = (e) => {
        const r = el.getBoundingClientRect();
        xTo((e.clientX - r.left - r.width / 2) * .22);
        yTo((e.clientY - r.top - r.height / 2) * .3);
      };
      const leave = () => { xTo(0); yTo(0); };
      el.addEventListener('mousemove', move);
      el.addEventListener('mouseleave', leave);
      cleanups.push(() => { el.removeEventListener('mousemove', move); el.removeEventListener('mouseleave', leave); });
    });
    return () => cleanups.forEach((f) => f());
  };

  const mm = gsap.matchMedia();
  const ready = document.fonts?.ready ?? Promise.resolve();
  ready.then(() => {
    mm.add({
      motion: '(prefers-reduced-motion: no-preference)',
      desktop: '(min-width: 900px)',
      hover: '(hover: hover)'
    }, (ctx) => {
      const c = ctx.conditions;
      if (!c.motion) { toolList.set(-1); return undefined; }

      const cleanups = [];
      const safe = (fn) => {
        try {
          const r = fn();
          if (typeof r === 'function') cleanups.push(r);
        } catch (err) {
          console.warn('[NeatShot motion]', err);
        }
      };
      safe(headline); safe(hero); safe(beforeAfter);
      safe(() => annotate(c.desktop)); safe(beautify); safe(pin);
      safe(ocr); safe(gallery); safe(reveals);
      if (c.hover) safe(magnetic);
      requestAnimationFrame(() => ScrollTrigger.refresh());
      return () => cleanups.forEach((f) => f());
    });
  });

  return {
    // SplitText phải được gỡ trước khi đổi ngôn ngữ, nếu không chữ mới sẽ bị cắt vụn
    revertSplits: () => { splits.forEach((s) => s.revert()); splits = []; }
  };
}
