const c = document.getElementById('c');
const ctx = c.getContext('2d');
let W, H, ox, oy;

function resize() {
  W = c.width = innerWidth;
  H = c.height = innerHeight;
  ox = W * 0.5;
  oy = H * 0.45;
}
resize();
addEventListener('resize', resize);

// ===== ISO =====
const T = 46;
const C30 = Math.cos(Math.PI / 6);
const S30 = 0.5;
function iso(gx, gy) {
  return [ox + (gx - gy) * C30 * T, oy + (gx + gy) * S30 * T];
}

// ===== GRID =====
function drawGrid() {
  const N = 16;
  for (let i = -N; i <= N; i++) {
    const f = 1 - Math.abs(i) / (N + 1);
    const a = f * 0.25;
    if (a < 0.01) continue;
    ctx.strokeStyle = 'rgba(170,180,200,' + a + ')';
    ctx.lineWidth = 0.5;
    let [x1, y1] = iso(i, -N), [x2, y2] = iso(i, N);
    ctx.beginPath(); ctx.moveTo(x1, y1); ctx.lineTo(x2, y2); ctx.stroke();
    [x1, y1] = iso(-N, i); [x2, y2] = iso(N, i);
    ctx.beginPath(); ctx.moveTo(x1, y1); ctx.lineTo(x2, y2); ctx.stroke();
  }
}

// ===== NODES =====
const nodes = {
  load:    { gx: -3, gy: -4 },
  battery: { gx: 5,  gy: -4 },
  solar:   { gx: -3, gy: 4  },
  grid:    { gx: 5,  gy: 4  }
};
function nPos(key) { return iso(nodes[key].gx, nodes[key].gy); }

// ===== ELLIPSE PARAMS =====
// Each node has a large isometric dashed ellipse underneath
const ELLIPSE_RX = 120; // horizontal radius
const ELLIPSE_RY = 40;  // vertical radius (iso perspective squash)
const ELLIPSE_OFFSET_Y = 15; // shift down from iso center

// ===== DRAW DASHED ELLIPSE =====
function drawEllipse(cx, cy) {
  ctx.save();
  ctx.setLineDash([6, 5]);
  ctx.strokeStyle = 'rgba(180, 188, 200, 0.55)';
  ctx.lineWidth = 1.2;
  ctx.beginPath();
  ctx.ellipse(cx, cy + ELLIPSE_OFFSET_Y, ELLIPSE_RX, ELLIPSE_RY, 0, 0, Math.PI * 2);
  ctx.stroke();
  ctx.setLineDash([]);
  ctx.restore();
  
  // Subtle fill
  ctx.fillStyle = 'rgba(240, 243, 248, 0.3)';
  ctx.beginPath();
  ctx.ellipse(cx, cy + ELLIPSE_OFFSET_Y, ELLIPSE_RX, ELLIPSE_RY, 0, 0, Math.PI * 2);
  ctx.fill();
}

// ===== FIND POINT ON ELLIPSE EDGE =====
// Given ellipse center and a target point, find intersection of line from center to target with ellipse
function ellipseEdgePoint(ecx, ecy, targetX, targetY) {
  const ey = ecy + ELLIPSE_OFFSET_Y;
  const dx = targetX - ecx;
  const dy = targetY - ey;
  
  // Parametric: angle from center to target
  const angle = Math.atan2(dy * ELLIPSE_RX, dx * ELLIPSE_RY);
  
  return [
    ecx + ELLIPSE_RX * Math.cos(angle),
    ey + ELLIPSE_RY * Math.sin(angle)
  ];
}

// ===== CONNECTIONS =====
const conns = [
  { from: 'load', to: 'battery', label: '1.33 MW' },
  { from: 'solar', to: 'load',   label: '7.54 MW' },
  { from: 'solar', to: 'grid',   label: '0.41 MW' },
  { from: 'load',  to: 'grid',   label: '8.61 MW' },
  { from: 'grid',  to: 'battery', label: '0.22 MW' }
];

function drawConnection(fromCenter, toCenter, time) {
  // Get edge points on each ellipse
  const f = ellipseEdgePoint(fromCenter[0], fromCenter[1], toCenter[0], toCenter[1]);
  const t = ellipseEdgePoint(toCenter[0], toCenter[1], fromCenter[0], fromCenter[1]);
  
  const dx = t[0] - f[0], dy = t[1] - f[1];
  const len = Math.sqrt(dx * dx + dy * dy);
  const ux = dx / len, uy = dy / len;
  
  // Base line (thin gray)
  ctx.strokeStyle = 'rgba(190, 195, 210, 0.5)';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(f[0], f[1]);
  ctx.lineTo(t[0], t[1]);
  ctx.stroke();
  
  // Gradient flow animation - wave 1
  const speed = 65;
  const gradLen = 80;
  const cycle = len + gradLen * 2;
  
  function drawGradWave(offset, alpha) {
    const gStartT = Math.max(0, offset / len);
    const gEndT = Math.min(1, (offset + gradLen) / len);
    if (gEndT <= 0 || gStartT >= 1) return;
    
    const gx1 = f[0] + dx * Math.max(0, gStartT);
    const gy1 = f[1] + dy * Math.max(0, gStartT);
    const gx2 = f[0] + dx * Math.min(1, gEndT);
    const gy2 = f[1] + dy * Math.min(1, gEndT);
    
    const grad = ctx.createLinearGradient(gx1, gy1, gx2, gy2);
    grad.addColorStop(0, 'rgba(91,141,239,0)');
    grad.addColorStop(0.3, 'rgba(91,141,239,' + (alpha * 0.45) + ')');
    grad.addColorStop(0.5, 'rgba(120,170,255,' + (alpha * 0.6) + ')');
    grad.addColorStop(0.7, 'rgba(91,141,239,' + (alpha * 0.45) + ')');
    grad.addColorStop(1, 'rgba(91,141,239,0)');
    
    ctx.strokeStyle = grad;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(gx1, gy1);
    ctx.lineTo(gx2, gy2);
    ctx.stroke();
  }
  
  const offset1 = (time * speed) % cycle - gradLen;
  drawGradWave(offset1, 1);
  
  const offset2 = ((time * speed) + cycle * 0.5) % cycle - gradLen;
  drawGradWave(offset2, 0.8);
  
  // Blue junction dots at connection points on ellipse edges
  drawBlueDot(f[0], f[1]);
  drawBlueDot(t[0], t[1]);
}

// ===== BLUE JUNCTION DOT (small) =====
function drawBlueDot(x, y) {
  ctx.fillStyle = 'rgba(91, 141, 239, 0.25)';
  ctx.beginPath(); ctx.arc(x, y, 5, 0, Math.PI * 2); ctx.fill();
  ctx.fillStyle = 'rgba(91, 141, 239, 0.75)';
  ctx.beginPath(); ctx.arc(x, y, 2.5, 0, Math.PI * 2); ctx.fill();
}

// ===== POSITION UI =====
function posUI(nsp) {
  const s = (id, x, y) => {
    const e = document.getElementById(id);
    if (e) { e.style.left = x + 'px'; e.style.top = y + 'px'; e.style.position = 'absolute'; }
  };
  
  // Images centered on iso nodes
  s('img-load',    nsp.load[0] - 100, nsp.load[1] - 100);
  s('img-battery', nsp.battery[0] - 100, nsp.battery[1] - 100);
  s('img-solar',   nsp.solar[0] - 100, nsp.solar[1] - 75);
  s('img-grid',    nsp.grid[0] - 120, nsp.grid[1] - 95);
  
  // Stats
  s('s-load', nsp.load[0] - 210, nsp.load[1] - 120);
  s('s-bat',  nsp.battery[0] - 55, nsp.battery[1] - 160);
  s('s-sol',  nsp.solar[0] - 230, nsp.solar[1] + 5);
  s('s-grid', nsp.grid[0] - 240, nsp.grid[1] + 15);
  
  // Labels (below ellipse)
  s('l-load', nsp.load[0] - 10, nsp.load[1] + ELLIPSE_OFFSET_Y + ELLIPSE_RY + 8);
  s('l-bat',  nsp.battery[0] + 20, nsp.battery[1] + ELLIPSE_OFFSET_Y + ELLIPSE_RY + 8);
  s('l-sol',  nsp.solar[0] - 5, nsp.solar[1] + ELLIPSE_OFFSET_Y + ELLIPSE_RY + 8);
  s('l-grid', nsp.grid[0] + 15, nsp.grid[1] + ELLIPSE_OFFSET_Y + ELLIPSE_RY + 8);
  
  // Flow tags at midpoint of visible line segment
  const fids = ['f-lb', 'f-sl', 'f-sg', 'f-lg', 'f-gb'];
  conns.forEach((cn, i) => {
    const fc = nsp[cn.from], tc = nsp[cn.to];
    const fe = ellipseEdgePoint(fc[0], fc[1], tc[0], tc[1]);
    const te = ellipseEdgePoint(tc[0], tc[1], fc[0], fc[1]);
    const mx = (fe[0] + te[0]) / 2, my = (fe[1] + te[1]) / 2;
    s(fids[i], mx + 8, my - 16);
  });
}

// ===== MAIN =====
let last = performance.now(), time = 0;

function frame(now) {
  const dt = Math.min((now - last) / 1000, 0.05);
  last = now; time += dt;
  ctx.clearRect(0, 0, W, H);
  
  // BG
  const bg = ctx.createLinearGradient(0, 0, W, H);
  bg.addColorStop(0, '#edf0f5'); bg.addColorStop(0.5, '#e4e8f0'); bg.addColorStop(1, '#eef1f6');
  ctx.fillStyle = bg; ctx.fillRect(0, 0, W, H);
  
  drawGrid();
  
  const nsp = {};
  for (const k in nodes) nsp[k] = nPos(k);
  
  // Draw ellipses (behind everything)
  for (const k in nsp) drawEllipse(nsp[k][0], nsp[k][1]);
  
  // Draw connections (from ellipse edge to ellipse edge)
  conns.forEach(cn => {
    drawConnection(nsp[cn.from], nsp[cn.to], time);
  });
  
  posUI(nsp);
  requestAnimationFrame(frame);
}

requestAnimationFrame(frame);