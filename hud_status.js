const fs = require('fs');
const path = require('path');

const agentsPath = path.resolve('C:/work/my/github/actionrpgx/.agents/AGENTS.md');
const claudePath = path.resolve('C:/work/my/github/actionrpgx/CLAUDE.md');

function extractSection(file, heading) {
  const content = fs.readFileSync(file, 'utf8');
  const regex = new RegExp(`###\\s+${heading}[\\s\\S]*?(?=\\n\\n###|$)`, 'i');
  const match = content.match(regex);
  return match ? match[0].trim() : '';
}

const agentsSec = extractSection(agentsPath, '0\\. AI Operational Guidelines');
const claudeSec = extractSection(claudePath, '0\\. AI Operational Guidelines');

const result = {
  // HUD renders a "notice" field as a line; null hides it.
  notice: agentsSec === claudeSec
    ? null
    : '[AI-Optnl-Err] AI Operational Guidelines differ between AGENTS.md and CLAUDE.md'
};

console.log(JSON.stringify(result));
