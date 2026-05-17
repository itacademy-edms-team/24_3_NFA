import React from 'react';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { oneDark } from 'react-syntax-highlighter/dist/esm/styles/prism';

export interface CodeBlockProps {
  code: string;
  language?: string;
  fileName?: string;
}

const LANGUAGE_MAP: Record<string, string> = {
  ts: 'typescript',
  tsx: 'typescript',
  js: 'javascript',
  jsx: 'javascript',
  py: 'python',
  rb: 'ruby',
  json: 'json',
  md: 'markdown',
  sql: 'sql',
  sh: 'bash',
  bash: 'bash',
  yaml: 'yaml',
  yml: 'yaml',
  html: 'html',
  css: 'css',
};

function detectLanguage(code: string, hint?: string): string {
  if (hint && LANGUAGE_MAP[hint.toLowerCase()]) {
    return LANGUAGE_MAP[hint.toLowerCase()];
  }
  if (hint) return hint;
  if (/^[\s\[\{].*[\]\}]$/.test(code.trim()) || code.trim().startsWith('{')) return 'json';
  if (/^(def |class |import |from |#)/m.test(code)) return 'python';
  if (/^(function |const |let |var |=>)/m.test(code)) return 'javascript';
  if (/^(<?xml|<[a-z]+)/m.test(code)) return 'html';
  return 'plaintext';
}

const CodeBlock: React.FC<CodeBlockProps> = ({ code, language, fileName }) => {
  const detectedLang = detectLanguage(code, language);

  return (
    <div className="my-3 rounded-xl overflow-hidden border border-slate-200">
      {fileName && (
        <div className="px-3 py-1.5 bg-slate-100 text-slate-600 text-xs font-medium">
          {fileName}
        </div>
      )}
      <SyntaxHighlighter
        language={detectedLang}
        style={oneDark}
        showLineNumbers
        customStyle={{
          margin: 0,
          padding: '1rem',
          fontSize: '0.8125rem',
          borderRadius: 0,
        }}
        codeTagProps={{ style: { fontFamily: 'ui-monospace, monospace' } }}
      >
        {code.trim()}
      </SyntaxHighlighter>
    </div>
  );
};

export default CodeBlock;

/** Разбивает текст на сегменты: текст | код */
export function parseCodeBlocks(text: string): Array<{ type: 'text' | 'code'; content: string; language?: string }> {
  const segments: Array<{ type: 'text' | 'code'; content: string; language?: string }> = [];
  const regex = /```(\w*)\n?([\s\S]*?)```/g;
  let lastIndex = 0;
  let m;

  while ((m = regex.exec(text)) !== null) {
    if (m.index > lastIndex) {
      segments.push({ type: 'text', content: text.slice(lastIndex, m.index) });
    }
    segments.push({
      type: 'code',
      content: m[2],
      language: m[1] || undefined,
    });
    lastIndex = regex.lastIndex;
  }

  if (lastIndex < text.length) {
    segments.push({ type: 'text', content: text.slice(lastIndex) });
  }

  return segments.length > 0 ? segments : [{ type: 'text', content: text }];
}
