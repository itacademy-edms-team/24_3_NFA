import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import { updateSource, type RssSourceConfiguration, type GitHubSourceConfiguration, type RedditSourceConfiguration } from '../services/newsService';
import toast from 'react-hot-toast';

type ApiTag = {
  tag?: { name?: string };
};

type SourceFromApi = {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
  newsSourceTags?: ApiTag[];
};

const EditSourceForm: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const sourceId = useMemo(() => {
    const parsed = Number(id);
    return Number.isFinite(parsed) ? parsed : null;
  }, [id]);

  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);

  const [sourceType, setSourceType] = useState<string>('rss');
  const [isActive, setIsActive] = useState<boolean>(true);
  const [name, setName] = useState<string>('');

  const [rssConfig, setRssConfig] = useState<RssSourceConfiguration>({ url: '', limit: 10, category: undefined });
  const [githubConfig, setGithubConfig] = useState<GitHubSourceConfiguration>({
    repositoryOwner: '',
    repositoryName: '',
    token: undefined,
    limit: 10,
    eventTypes: undefined,
    category: undefined
  });
  const [redditConfig, setRedditConfig] = useState<RedditSourceConfiguration>({
    subreddit: '',
    sortType: 'hot',
    limit: 10,
    category: undefined
  });

  const [tags, setTags] = useState<string[]>([]);
  const [tagInput, setTagInput] = useState<string>('');

  const normalizeTag = (value: string): string => value.trim().replace(/\s+/g, ' ').toLowerCase();

  const addTag = (rawTag: string) => {
    const normalizedTag = normalizeTag(rawTag);
    if (!normalizedTag) return;
    setTags((prev) => (prev.includes(normalizedTag) ? prev : [...prev, normalizedTag]));
  };

  const removeTag = (tagToRemove: string) => {
    setTags((prev) => prev.filter((t) => t !== tagToRemove));
  };

  const commitTagFromInput = () => {
    addTag(tagInput);
    setTagInput('');
  };

  const handleTagKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === ' ' || e.key === ',' || e.key === 'Enter') {
      e.preventDefault();
      commitTagFromInput();
    }
  };

  useEffect(() => {
    if (!sourceId) return;

    const loadSource = async () => {
      try {
        setLoading(true);
        const response = await api.get<SourceFromApi>(`/api/sources/${sourceId}`);
        const sourceData = response.data;

        setName(sourceData.name ?? '');
        setSourceType(sourceData.type ?? 'rss');
        setIsActive(!!sourceData.isActive);

        const config = sourceData.configuration ? JSON.parse(sourceData.configuration) : {};

        const typeLower = (sourceData.type ?? '').toLowerCase();
        if (typeLower === 'rss') {
          setRssConfig({
            url: config.url ?? '',
            limit: Number(config.limit ?? 10),
            category: config.category ?? undefined
          });
        } else if (typeLower === 'github') {
          setGithubConfig({
            repositoryOwner: config.repositoryOwner ?? '',
            repositoryName: config.repositoryName ?? '',
            token: config.token ?? undefined,
            limit: Number(config.limit ?? 10),
            eventTypes: config.eventTypes ?? undefined,
            category: config.category ?? undefined
          });
        } else if (typeLower === 'reddit') {
          setRedditConfig({
            subreddit: config.subreddit ?? '',
            sortType: config.sortType ?? 'hot',
            limit: Number(config.limit ?? 10),
            category: config.category ?? undefined
          });
        }

        const serverTags = (sourceData.newsSourceTags ?? [])
          .map((t) => t.tag?.name)
          .filter((x): x is string => typeof x === 'string' && x.trim().length > 0);

        // На клиенте храним нормализованные строки для дедупликации
        const normalized = Array.from(new Set(serverTags.map(normalizeTag)));
        setTags(normalized);
      } catch (err) {
        toast.error(err instanceof Error ? err.message : 'Ошибка при загрузке источника');
      } finally {
        setLoading(false);
      }
    };

    loadSource();
  }, [sourceId]);

  const typeLower = sourceType.toLowerCase();

  const getSourceTypeLabel = (type: string) => {
    if (type === 'github') return 'GitHub';
    if (type === 'reddit') return 'Reddit';
    if (type === 'rss') return 'RSS';
    return type;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!sourceId) return;

    setSaving(true);
    try {
      let configuration: any;
      if (typeLower === 'rss') {
        configuration = rssConfig;
      } else if (typeLower === 'github') {
        configuration = githubConfig;
      } else {
        configuration = redditConfig;
      }

      await updateSource(sourceId, {
        name,
        type: typeLower,
        configuration,
        isActive,
        tags
      });

      toast.success('Источник успешно обновлен!');
      navigate('/sources');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Ошибка обновления.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm">
        Загрузка...
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm z-50">
      <h2 className="text-lg font-semibold mb-4 text-slate-900">
        Редактировать {getSourceTypeLabel(typeLower)} источник
      </h2>

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="name" className="block text-xs font-medium text-slate-700 mb-1">
            Название источника *
          </label>
          <input
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
        </div>

        <div className="mb-4">
          <label htmlFor="sourceTags" className="block text-xs font-medium text-gray-700 mb-1">
            Теги источника
          </label>
          <div className="w-full rounded-md border border-gray-300 px-2 py-2 focus-within:ring-2 focus-within:ring-blue-500">
            <div className="mb-2 flex flex-wrap gap-2">
              {tags.map((tag) => (
                <span
                  key={tag}
                  className="inline-flex items-center gap-1 rounded-full bg-blue-100 px-2 py-1 text-xs font-medium text-blue-800"
                >
                  {tag}
                  <button
                    type="button"
                    onClick={() => removeTag(tag)}
                    className="rounded-full px-1 text-blue-700 hover:bg-blue-200"
                    aria-label={`Удалить тег ${tag}`}
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
            <input
              type="text"
              id="sourceTags"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleTagKeyDown}
              onBlur={commitTagFromInput}
              className="w-full border-0 p-0 text-sm focus:outline-none"
              placeholder="Введите тег и нажмите пробел или запятую"
            />
          </div>
        </div>

        {typeLower === 'rss' && (
          <div className="mb-4">
            <label htmlFor="rssLimit" className="block text-xs font-medium text-gray-700 mb-1">
              Лимит новостей
            </label>
            <input
              type="number"
              id="rssLimit"
              value={rssConfig.limit}
              min={1}
              max={100}
              onChange={(e) => setRssConfig((prev) => ({ ...prev, limit: Number(e.target.value) }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        )}

        {typeLower === 'github' && (
          <div className="mb-4">
            <label htmlFor="githubLimit" className="block text-xs font-medium text-gray-700 mb-1">
              Лимит событий
            </label>
            <input
              type="number"
              id="githubLimit"
              value={githubConfig.limit}
              min={1}
              max={100}
              onChange={(e) => setGithubConfig((prev) => ({ ...prev, limit: Number(e.target.value) }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        )}

        {typeLower === 'reddit' && (
          <div className="mb-4">
            <label htmlFor="redditLimit" className="block text-xs font-medium text-gray-700 mb-1">
              Лимит постов
            </label>
            <input
              type="number"
              id="redditLimit"
              value={redditConfig.limit}
              min={1}
              max={100}
              onChange={(e) => setRedditConfig((prev) => ({ ...prev, limit: Number(e.target.value) }))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        )}

        <button
          type="submit"
          disabled={saving}
          className={`w-full py-2 px-4 rounded-md text-white font-medium ${
            saving ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
          }`}
        >
          {saving ? 'Сохранение...' : 'Сохранить'}
        </button>
      </form>
    </div>
  );
};

export default EditSourceForm;
