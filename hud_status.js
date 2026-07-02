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
  const barColor = colorFor(pct);

  const filled = Math.floor(pct * 10 / 100);
  const bar = '▓'.repeat(filled) + '░'.repeat(10 - filled);

  const fiveH = data.rate_limits?.five_hour?.used_percentage;
  const week = data.rate_limits?.seven_day?.used_percentage;

  let limits = '';
  if (fiveH != null) limits += ` | ${BLUE}5h:${RESET} ${colorFor(fiveH)}${Math.round(fiveH)}%${RESET}`;
  if (week != null) limits += ` | ${BLUE}7d:${RESET} ${colorFor(week)}${Math.round(week)}%${RESET}`;

  console.log(`${BLUE}[${model}] 📁 ${dir}${branch}${RESET} | ${barColor}${bar} ${pct}%${RESET}${limits}`);
});
