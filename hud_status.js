const { execSync } = require('child_process');
const path = require('path');
let input = '';
process.stdin.on('data', chunk => input += chunk);
process.stdin.on('end', () => {
  try {
    const data = JSON.parse(input);
    
    // Resolve model name
    const model = (data.model?.display_name) || data.active_model || (typeof data.model === 'string' ? data.model : 'Model');
    
    // Resolve directory name
    const currentDir = data.workspace?.current_dir || data.cwd || process.cwd();
    const dir = path.basename(currentDir);
    
    // Resolve context percentage
    const pct = Math.floor(
      data.context_window?.used_percentage ?? 
      data.context_window?.percentage ?? 
      0
    );

    // Resolve git branch (use VCS info if available, fallback to execSync)
    let branch = '';
    if (data.vcs && data.vcs.branch) {
      branch = ` | 🌿 ${data.vcs.branch}`;
    } else {
      try {
        const cmdOut = execSync('git branch --show-current', { encoding: 'utf8', stdio: ['pipe', 'pipe', 'ignore'] }).trim();
        branch = cmdOut ? ` | 🌿 ${cmdOut}` : '';
      } catch {}
    }

    const BLUE = '\x1b[94m', GREEN = '\x1b[32m', YELLOW = '\x1b[33m', RED = '\x1b[31m', RESET = '\x1b[0m';
    const colorFor = p => p >= 90 ? RED : p >= 70 ? YELLOW : GREEN;

    const formatRemainingSeconds = sec => {
      sec = Math.max(0, sec);
      const d = Math.floor(sec / 86400);
      const h = Math.floor((sec % 86400) / 3600);
      const m = Math.floor((sec % 3600) / 60);
      const parts = [];
      if (d > 0) parts.push(`${d}d`);
      if (d > 0 || h > 0) parts.push(`${h}h`);
      parts.push(`${m}m`);
      return parts.join(' ');
    };

    // Claude CLI / Antigravity rate limits & quotas
    let fiveHUsed = data.rate_limits?.five_hour?.used_percentage;
    let fiveHRemainingSec = data.rate_limits?.five_hour?.resets_at 
      ? Math.max(0, data.rate_limits.five_hour.resets_at - Date.now() / 1000)
      : null;

    let weekUsed = data.rate_limits?.seven_day?.used_percentage;
    let weekRemainingSec = data.rate_limits?.seven_day?.resets_at 
      ? Math.max(0, data.rate_limits.seven_day.resets_at - Date.now() / 1000)
      : null;

    // Fallback to Antigravity quotas
    if (fiveHUsed == null && data.quota) {
      const isGemini = /gemini/i.test(model);
      const q5h = isGemini 
        ? (data.quota["gemini-5h"] || data.quota["3p-5h"])
        : (data.quota["3p-5h"] || data.quota["gemini-5h"]);
      if (q5h) {
        fiveHUsed = (1 - q5h.remaining_fraction) * 100;
        fiveHRemainingSec = q5h.reset_in_seconds;
      }
    }

    if (weekUsed == null && data.quota) {
      const isGemini = /gemini/i.test(model);
      const qWeekly = isGemini 
        ? (data.quota["gemini-weekly"] || data.quota["3p-weekly"])
        : (data.quota["3p-weekly"] || data.quota["gemini-weekly"]);
      if (qWeekly) {
        weekUsed = (1 - qWeekly.remaining_fraction) * 100;
        weekRemainingSec = qWeekly.reset_in_seconds;
      }
    }

    let limits = '';
    if (fiveHUsed != null) {
      const timeStr = fiveHRemainingSec != null ? `${formatRemainingSeconds(fiveHRemainingSec)}:` : '5h:';
      limits += ` | ${BLUE}${timeStr}${RESET} ${colorFor(fiveHUsed)}${Math.round(fiveHUsed)}%${RESET}`;
    }
    if (weekUsed != null) {
      const timeStr = weekRemainingSec != null ? `${formatRemainingSeconds(weekRemainingSec)}:` : '7d:';
      limits += ` | ${BLUE}${timeStr}${RESET} ${colorFor(weekUsed)}${Math.round(weekUsed)}%${RESET}`;
    }

    // Antigravity Quota Usage
    if (data.quota_usage_percentage != null) {
      const quota = data.quota_usage_percentage;
      limits += ` | ${BLUE}quota:${RESET} ${colorFor(quota)}${Math.round(quota)}%${RESET}`;
    }

    // Antigravity Agent State
    let state = '';
    if (data.agent_state && data.agent_state !== 'idle') {
      const stateColor = data.agent_state === 'thinking' ? YELLOW : GREEN;
      state = ` | ${stateColor}${data.agent_state.toUpperCase()}${RESET}`;
    }

    console.log(`${BLUE}[${model}] 📁 ${dir}${branch}${RESET} | ${BLUE}ctx:${RESET} ${colorFor(pct)}${pct}%${RESET}${limits}${state}`);
  } catch (err) {
    console.log(`📁 actionrpgx | ctx: 0%`);
  }
});
