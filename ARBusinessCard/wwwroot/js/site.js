/* =============================================================
   AR Business Card — Global JS
   ============================================================= */

document.addEventListener('DOMContentLoaded', () => {

  // ── Auto-dismiss alerts ──────────────────────────────────────
  document.querySelectorAll('.alert').forEach(alert => {
    setTimeout(() => {
      alert.style.transition = 'opacity 0.5s, transform 0.5s';
      alert.style.opacity = '0';
      alert.style.transform = 'translateY(-10px)';
      setTimeout(() => alert.remove(), 500);
    }, 4000);
  });

  // ── Animate cards on scroll ──────────────────────────────────
  const observerOpts = { threshold: 0.1, rootMargin: '0px 0px -30px 0px' };
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        e.target.style.opacity = '1';
        e.target.style.transform = 'translateY(0)';
      }
    });
  }, observerOpts);

  document.querySelectorAll('.animate-in').forEach(el => {
    el.style.opacity = '0';
    el.style.transform = 'translateY(20px)';
    el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
    observer.observe(el);
  });

  // ── Biz-card 3D tilt on hover ────────────────────────────────
  document.querySelectorAll('.biz-card').forEach(card => {
    card.addEventListener('mousemove', e => {
      const rect = card.getBoundingClientRect();
      const x = (e.clientX - rect.left) / rect.width  - 0.5;
      const y = (e.clientY - rect.top)  / rect.height - 0.5;
      card.style.transform = `rotateY(${x * 14}deg) rotateX(${-y * 10}deg) scale(1.03)`;
    });
    card.addEventListener('mouseleave', () => {
      card.style.transform = 'rotateY(0) rotateX(0) scale(1)';
    });
  });

  // ── Live card tilt in creator ────────────────────────────────
  const liveCard = document.getElementById('liveCard');
  if (liveCard) {
    liveCard.addEventListener('mousemove', e => {
      const rect = liveCard.getBoundingClientRect();
      const x = (e.clientX - rect.left) / rect.width  - 0.5;
      const y = (e.clientY - rect.top)  / rect.height - 0.5;
      liveCard.style.transform = `rotateY(${x * 18}deg) rotateX(${-y * 12}deg) scale(1.02)`;
      liveCard.style.transition = 'transform 0.1s';
    });
    liveCard.addEventListener('mouseleave', () => {
      liveCard.style.transform = 'rotateY(0) rotateX(0) scale(1)';
      liveCard.style.transition = 'transform 0.4s ease';
    });
  }

  // ── Navbar active link highlight ─────────────────────────────
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-link').forEach(link => {
    const href = link.getAttribute('href')?.toLowerCase();
    if (href && currentPath.includes(href) && href !== '/') {
      link.classList.add('active');
    }
  });

  // ── Button ripple effect ─────────────────────────────────────
  document.querySelectorAll('.btn').forEach(btn => {
    btn.addEventListener('click', function(e) {
      const ripple = document.createElement('span');
      const rect = this.getBoundingClientRect();
      const size = Math.max(rect.width, rect.height);
      ripple.style.cssText = `
        position:absolute; border-radius:50%;
        width:${size}px; height:${size}px;
        left:${e.clientX - rect.left - size/2}px;
        top:${e.clientY - rect.top - size/2}px;
        background:rgba(255,255,255,0.15);
        transform:scale(0); animation:rippleAnim 0.5s ease;
        pointer-events:none;
      `;
      if (getComputedStyle(this).position === 'static')
        this.style.position = 'relative';
      this.style.overflow = 'hidden';
      this.appendChild(ripple);
      ripple.addEventListener('animationend', () => ripple.remove());
    });
  });

  // Inject ripple keyframe
  if (!document.getElementById('rippleStyle')) {
    const s = document.createElement('style');
    s.id = 'rippleStyle';
    s.textContent = `@keyframes rippleAnim { to { transform: scale(2.5); opacity: 0; } }`;
    document.head.appendChild(s);
  }

  // ── Confirm delete with custom dialog ────────────────────────
  // (using native confirm for simplicity; already in deleteCard fn)

  // ── Copy to clipboard for share URL ─────────────────────────
  window.copyLink = function(url) {
    navigator.clipboard.writeText(url).then(() => {
      showToast('Link copied! 🔗');
    }).catch(() => {
      const inp = document.createElement('input');
      inp.value = url;
      document.body.appendChild(inp);
      inp.select();
      document.execCommand('copy');
      inp.remove();
      showToast('Link copied! 🔗');
    });
  };

  // ── Toast notification ────────────────────────────────────────
  window.showToast = function(msg, type = 'success') {
    const toast = document.createElement('div');
    const color = type === 'success' ? '#10b981' : '#ef4444';
    toast.style.cssText = `
      position:fixed; bottom:24px; right:24px; z-index:9999;
      background:rgba(10,15,35,0.95); border:1px solid ${color};
      border-radius:12px; padding:12px 20px;
      color:${color}; font-family:'Rajdhani',sans-serif;
      font-size:0.85rem; font-weight:700; letter-spacing:1px;
      box-shadow:0 10px 40px rgba(0,0,0,0.5);
      transform:translateY(60px); opacity:0;
      transition:all 0.3s cubic-bezier(0.34,1.56,0.64,1);
    `;
    toast.textContent = msg;
    document.body.appendChild(toast);
    requestAnimationFrame(() => {
      toast.style.transform = 'translateY(0)';
      toast.style.opacity = '1';
    });
    setTimeout(() => {
      toast.style.transform = 'translateY(60px)';
      toast.style.opacity = '0';
      setTimeout(() => toast.remove(), 300);
    }, 3000);
  };

  // ── Number counter animation for stats ───────────────────────
  document.querySelectorAll('.stat-value').forEach(el => {
    const raw = el.textContent.trim();
    const num = parseInt(raw);
    if (!isNaN(num) && num > 0) {
      let current = 0;
      const step = Math.ceil(num / 30);
      const timer = setInterval(() => {
        current = Math.min(current + step, num);
        el.textContent = current;
        if (current >= num) clearInterval(timer);
      }, 40);
    }
  });

});
