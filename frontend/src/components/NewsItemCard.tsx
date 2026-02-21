import React, { useState } from 'react';
import { type NewsItem } from '../types/NewsItem';
import { FaRss, FaGithub, FaReddit, FaHeart } from 'react-icons/fa';
import SafeImage from './SafeImage';
import { useFavorites } from '../contexts/FavoritesContext';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeRaw from 'rehype-raw';

interface NewsItemCardProps {
  newsItem: NewsItem;
}

const markdownComponents = {
  h1: ({ node, ...props }: any) => (
    <h1 className="text-lg font-bold text-slate-900 mt-4 mb-2" {...props} />
  ),
  h2: ({ node, ...props }: any) => (
    <h2 className="text-base font-bold text-slate-900 mt-3 mb-2" {...props} />
  ),
  h3: ({ node, ...props }: any) => (
    <h3 className="text-sm font-bold text-slate-900 mt-2 mb-1" {...props} />
  ),
  p: ({ node, ...props }: any) => (
    <p className="text-slate-700 text-sm leading-relaxed mb-2" {...props} />
  ),
  strong: ({ node, ...props }: any) => (
    <strong className="font-bold text-slate-900" {...props} />
  ),
  em: ({ node, ...props }: any) => (
    <em className="italic text-slate-700" {...props} />
  ),
  ul: ({ node, ...props }: any) => (
    <ul className="list-disc list-inside ml-4 text-slate-700 text-sm space-y-1" {...props} />
  ),
  ol: ({ node, ...props }: any) => (
    <ol className="list-decimal list-inside ml-4 text-slate-700 text-sm space-y-1" {...props} />
  ),
  li: ({ node, ...props }: any) => (
    <li className="text-slate-700 text-sm" {...props} />
  ),
  blockquote: ({ node, ...props }: any) => (
    <blockquote className="border-l-4 border-indigo-300 pl-4 py-2 my-2 bg-slate-50 text-slate-600 italic text-sm" {...props} />
  ),
  code: ({ node, inline, className, children, ...props }: any) => (
    inline ? (
      <code className="bg-slate-100 rounded px-1 py-0.5 text-sm font-mono text-indigo-600 break-all" {...props}>
        {children}
      </code>
    ) : (
      <div className="max-w-[668px] overflow-x-auto">
        <code className={`${className} block`} {...props}>
          {children}
        </code>
      </div>
    )
  ),
  pre: ({ node, ...props }: any) => (
    <div className="max-w-[668px] overflow-x-auto my-2">
      <pre className="bg-slate-900 text-slate-100 rounded-lg p-4 whitespace-pre-wrap break-words" {...props} />
    </div>
  ),
  a: ({ node, ...props }: any) => {
    const href = props.href || '';
    const displayText = typeof props.children === 'string' ? props.children : href;

    // Обрезаем длинные URL в тексте ссылки
    const truncateUrlText = (url: string, maxLength: number = 40) => {
      if (url.length <= maxLength) return url;
      try {
        const urlObj = new URL(url);
        const domain = urlObj.hostname;
        const path = urlObj.pathname;
        if (domain.length + path.length > maxLength) {
          return `${domain}${path.length > 10 ? path.substring(0, 10) + '...' : path}...`;
        }
        return `${domain}${path}...`;
      } catch {
        return url.substring(0, maxLength) + '...';
      }
    };

    return (
      <a
        className="text-indigo-600 hover:text-indigo-800 underline underline-offset-2 break-all"
        target="_blank"
        rel="noopener noreferrer"
        {...props}
      >
        {truncateUrlText(displayText)}
      </a>
    );
  },
  hr: ({ node, ...props }: any) => (
    <hr className="border-slate-200 my-4" {...props} />
  ),
  table: ({ node, ...props }: any) => (
    <div className="overflow-x-auto rounded-xl border border-slate-200 my-3">
      <table className="min-w-full text-xs" {...props} />
    </div>
  ),
  thead: ({ node, ...props }: any) => (
    <thead className="bg-slate-50" {...props} />
  ),
  th: ({ node, ...props }: any) => (
    <th className="px-3 py-2 border-b border-slate-200 font-semibold text-slate-700 text-center" {...props} />
  ),
  td: ({ node, ...props }: any) => (
    <td className="px-3 py-2 border-t border-slate-100 text-center align-top" {...props} />
  ),
  tr: ({ node, ...props }: any) => (
    <tr className="even:bg-slate-50" {...props} />
  ),
  img: ({ node, ...props }: any) => (
    <img
      className="w-full max-w-[668px] max-h-[374px] h-auto object-contain rounded-lg my-2 mx-auto"
      loading="lazy"
      {...props}
    />
  ),
  details: ({ node, ...props }: any) => (
    <details
      className="my-2 border border-slate-200 rounded-lg bg-slate-50 px-3 py-2 text-slate-700 text-sm"
      {...props}
    />
  ),
  summary: ({ node, ...props }: any) => (
    <summary
      className="cursor-pointer font-medium text-slate-900 mb-1 list-none"
      {...props}
    />
  ),
};

const NewsItemCard: React.FC<NewsItemCardProps> = ({ newsItem }) => {
  const [showFullText, setShowFullText] = useState(false);
  const favorites = useFavorites();
  const isFav = favorites?.isFavorite(newsItem.id) ?? false;
  const formattedDate = new Date(newsItem.publishedAtUtc).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });

  const description = newsItem.description || '';
  const sourceType = newsItem.sourceType?.toLowerCase();
  const isGitHub = sourceType === 'github';
  const isReddit = sourceType === 'reddit';
  const shouldTruncate = !isGitHub && !isReddit && description.length > 300;
  const displayText = showFullText || !shouldTruncate
    ? description
    : description.substring(0, 300) + '...';

  const gitHubMetadata = isGitHub && newsItem.metadata ? JSON.parse(newsItem.metadata) : null;

  const getSourceIcon = () => {
    const sourceType = newsItem.sourceType?.toLowerCase();
    
    const iconWrapperClass = "flex items-center justify-center w-8 h-8 bg-white rounded-full"; 

    if (sourceType === 'rss') {
        return (
            <div className={iconWrapperClass}>
                <FaRss className="w-5 h-5 text-orange-500" />
            </div>
        );
    }
    if (sourceType === 'github') {
        return (
            <div className={iconWrapperClass}>
                <FaGithub className="w-5 h-5 text-gray-800" />
            </div>
        );
    }
    if (sourceType === 'reddit') {
        return (
            <div className={iconWrapperClass}>
                <FaReddit className="w-5 h-5 text-orange-600" />
            </div>
        );
    }
    return null;
  };

  return (
    <article className="bg-white rounded-2xl p-6 border border-slate-100 relative w-full max-w-[844px] min-w-0">
      <div className="absolute top-4 right-4 z-10 flex items-center gap-2">
        {favorites && (
          <button
            type="button"
            onClick={() => favorites.toggleFavorite(newsItem)}
            className="p-2 rounded-full hover:bg-slate-100 transition-colors"
            aria-label={isFav ? 'Убрать из избранного' : 'Добавить в избранное'}
          >
            <FaHeart
              className={`w-5 h-5 ${isFav ? 'text-red-500 fill-red-500' : 'text-slate-400'}`}
            />
          </button>
        )}
        {newsItem.sourceType && getSourceIcon()}
      </div>
      {newsItem.imageUrl && (
        <div className="mb-4 rounded-xl overflow-hidden bg-slate-50 flex justify-center">
          <SafeImage
            src={newsItem.imageUrl}
            alt={newsItem.title}
            className="w-full max-w-[668px] max-h-[374px] object-contain"
          />
        </div>
      )}

      <h2 className="text-xl font-bold text-slate-900 mb-3 leading-tight break-words min-w-0">
        {newsItem.title}
      </h2>
      
      {description && (
        <div className="mb-4">
          {isGitHub ? (
            <div className="text-slate-700 text-sm leading-relaxed">
              <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeRaw]}
                components={markdownComponents}
              >
                {description}
              </ReactMarkdown>
            </div>
          ) : isReddit ? (
            <div className="text-slate-700 text-sm leading-relaxed">
              <ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>
                {description}
              </ReactMarkdown>
            </div>
          ) : (
            <>
              <p className="text-slate-700 text-sm leading-relaxed whitespace-pre-wrap">
                {displayText}
              </p>
              {shouldTruncate && (
                <button
                  onClick={() => setShowFullText(!showFullText)}
                  className="mt-2 text-sm text-indigo-600 hover:text-indigo-800 font-medium"
                >
                  {showFullText ? 'Свернуть' : 'Читать полностью'}
                </button>
              )}
            </>
          )}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-3 mt-4 pt-4 border-t border-slate-100 min-w-0">
        {newsItem.category && (
          <span className="inline-block px-3 py-1 text-xs font-medium bg-indigo-100 text-indigo-700 rounded-full shrink-0">
            {newsItem.category}
          </span>
        )}
        {isGitHub && gitHubMetadata?.sha && (
          <span className="text-xs text-slate-500 font-mono bg-slate-100 px-2 py-1 rounded shrink-0">
            SHA: {gitHubMetadata.sha.substring(0, 7)}
          </span>
        )}
        {isGitHub && gitHubMetadata?.prNumber && (
          <span className="text-xs text-slate-500 shrink-0">
            PR #{gitHubMetadata.prNumber}
          </span>
        )}
        {newsItem.author && (
          <span className="text-xs text-slate-500 truncate">
            Автор: {newsItem.author}
          </span>
        )}
        <span className="text-xs text-slate-500 shrink-0">{formattedDate}</span>
        <a
          href={newsItem.link}
          target="_blank"
          rel="noopener noreferrer"
          className="ml-auto px-4 py-2 text-xs font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shrink-0"
        >
          Перейти к источнику →
        </a>
      </div>
    </article>
  );
};

export default NewsItemCard;
