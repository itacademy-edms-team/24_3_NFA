import React from 'react';
import {
  FaGithub,
  FaReddit,
  FaRss,
  FaTumblr,
  FaVk,
} from 'react-icons/fa';
import type { IconType } from 'react-icons';

type SourceTypeMeta = {
  Icon: IconType;
  label: string;
  colorClass: string;
};

const SOURCE_TYPE_META: Record<string, SourceTypeMeta> = {
  github: { Icon: FaGithub, label: 'GitHub', colorClass: 'text-slate-800' },
  reddit: { Icon: FaReddit, label: 'Reddit', colorClass: 'text-orange-600' },
  rss: { Icon: FaRss, label: 'RSS', colorClass: 'text-orange-500' },
  tumblr: { Icon: FaTumblr, label: 'Tumblr', colorClass: 'text-slate-700' },
  vk: { Icon: FaVk, label: 'VK', colorClass: 'text-blue-600' },
};

interface SourceTypeIconProps {
  type: string;
  className?: string;
}

const SourceTypeIcon: React.FC<SourceTypeIconProps> = ({ type, className = 'w-5 h-5' }) => {
  const key = type.toLowerCase();
  const meta = SOURCE_TYPE_META[key] ?? {
    Icon: FaRss,
    label: type,
    colorClass: 'text-slate-500',
  };
  const { Icon, label, colorClass } = meta;

  return (
    <span
      title={label}
      aria-label={label}
      className="inline-flex items-center justify-center"
    >
      <Icon className={`${className} ${colorClass}`} />
    </span>
  );
};

export default SourceTypeIcon;
