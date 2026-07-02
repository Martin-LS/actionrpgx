const { execSync } = require('child_process');
const path = require('path');

let input = '';
process.stdin.on('data', chunk => input += chunk);
process.stdin.on('end', () => {
  const data = JSON.parse(input);
  const model = data.model.display_name;
  const dir = path.basename(data.workspace.current_dir);
  const pct = Math.floor(data.context_window?.used_percentage || 0);

  let branch = '';
  try {
    branch = execSync('git branch --show-current', { encoding: 'utf8', stdio: ['pipe', 'pipe', 'ignore'] }).trim();
    branch = branch ? ` | 🌿 ${branch}` : '';
  } catch {}

  const BLUE = '\x1b[94m', GREEN = '\x1b[32m', YELLOW = '\x1b[33m', RED = '\x1b[31m', RESET = '\x1b[0m';
  const colorFor = p => p >= 90 ? RED : p >= 70 ? YELLOW : GREEN;

  const formatRemaining = resetsAt => {
    const sec = Math.max(0, resetsAt - Date.now() / 1000);
    const d = Math.floor(sec / 86400);
    const h = Math.floor((sec % 86400) / 3600);
    const m = Math.floor((sec % 3600) / 60);
    const parts = [];
    if (d > 0) parts.push(`${d}d`);
    if (d > 0 || h > 0) parts.push(`${h}h`);
    parts.push(`${m}m`);
    return parts.join(' ');
  };

  const fiveH = data.rate_limits?.five_hour?.used_percentage;
  const fiveHResetsAt = data.rate_limits?.five_hour?.resets_at;
  const week = data.rate_limits?.seven_day?.used_percentage;
  const weekResetsAt = data.rate_limits?.seven_day?.resets_at;

  let limits = '';
  if (fiveH != null && fiveHResetsAt != null) limits += ` | ${BLUE}${formatRemaining(fiveHResetsAt)}:${RESET} ${colorFor(fiveH)}${Math.round(fiveH)}%${RESET}`;
  if (week != null && weekResetsAt != null) limits += ` | ${BLUE}${formatRemaining(weekResetsAt)}:${RESET} ${colorFor(week)}${Math.round(week)}%${RESET}`;

  console.log(`${BLUE}[${model}] 📁 ${dir}${branch}${RESET} | ${BLUE}ctx:${RESET} ${colorFor(pct)}${pct}%${RESET}${limits}`);
});
