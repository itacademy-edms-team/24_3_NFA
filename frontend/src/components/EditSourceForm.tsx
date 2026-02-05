import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { updateSource, type RssSourceConfiguration, type GitHubSourceConfiguration, type RedditSourceConfiguration } from '../services/newsService';

interface Source {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
}

const EditSourceForm: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [source, setSource] = useState<Source | null>(null);
  const [name, setName] = useState<string>('');
  
  // RSS Configuration
  const [rssConfig, setRssConfig] = useState<RssSourceConfiguration>({
    url: '',
    limit: 10,
    category: ''
  });
  
  // GitHub Configuration
  const [githubConfig, setGithubConfig] = useState<GitHubSourceConfiguration>({
    repositoryOwner: '',
    repositoryName: '',
    limit: 10,
  });
  
  // Reddit Configuration
  const [redditConfig, setRedditConfig] = useState<RedditSourceConfiguration>({
    subreddit: '',
    sortType: 'hot',
    limit: 10,
    category: ''
  });
  
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadSource = async () => {
      try {
        setLoading(true);
        const response = await fetch(`http://localhost:5043/api/sources/${id}`);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const sourceData: Source = await response.json();
        console.log('Loaded source:', sourceData);
        setSource(sourceData);
        setName(sourceData.name);

        // Парсим конфигурацию в зависимости от типа
        const config = JSON.parse(sourceData.configuration);
        console.log('Parsed config:', config);
        console.log('Source type:', sourceData.type);
        
        const sourceType = sourceData.type.toLowerCase();
        
        if (sourceType === 'rss') {
          console.log('Setting RSS config');
          setRssConfig({
            url: config.url || config.Url || '',
            limit: config.limit || config.Limit || 10,
            category: config.category || config.Category || ''
          });
        } else if (sourceType === 'github') {
          console.log('Setting GitHub config');
          setGithubConfig({
            repositoryOwner: config.repositoryOwner || config.RepositoryOwner || '',
            repositoryName: config.repositoryName || config.RepositoryName || '',
            limit: config.limit || config.Limit || 10,
          });
        } else if (sourceType === 'reddit') {
          console.log('Setting Reddit config');
          setRedditConfig({
            subreddit: config.subreddit || config.Subreddit || '',
            sortType: config.sortType || config.SortType || 'hot',
            limit: config.limit || config.Limit || 10,
            category: config.category || config.Category || ''
          });
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Произошла ошибка при загрузке источника.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      loadSource();
    }
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    if (!source) return;

    try {
      let configuration: any;

      if (source.type.toLowerCase() === 'rss') {
        configuration = {
          url: rssConfig.url,
          limit: rssConfig.limit,
          category: rssConfig.category || '',
        };
      } else if (source.type.toLowerCase() === 'github') {
        configuration = {
          RepositoryOwner: githubConfig.repositoryOwner,
          RepositoryName: githubConfig.repositoryName,
          Limit: githubConfig.limit,
        };
      } else if (source.type.toLowerCase() === 'reddit') {
        configuration = {
          Subreddit: redditConfig.subreddit,
          SortType: redditConfig.sortType,
          Limit: redditConfig.limit,
          Category: redditConfig.category || '',
        };
      } else {
        throw new Error('Неподдерживаемый тип источника');
      }

      const sourceData = {
        name: name,
        type: source.type,
        configuration: configuration,
        isActive: source.isActive,
      };

      console.log('Sending configuration:', configuration);
      await updateSource(source.id, sourceData);
      navigate('/sources');
    } catch (err) {
      console.error("Error updating source:", err);
      setError(err instanceof Error ? err.message : 'Произошла ошибка при обновлении источника.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Загрузка...</div>;
  }

  if (!source) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Источник не найден</div>;
  }

  const getSourceTypeLabel = (type: string) => {
    const typeLower = type.toLowerCase();
    if (typeLower === 'github') return 'GitHub';
    if (typeLower === 'reddit') return 'Reddit';
    if (typeLower === 'rss') return 'RSS';
    return type;
  };

  return (
    <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm">
      <h2 className="text-lg font-semibold mb-4 text-slate-900">
        Редактировать {getSourceTypeLabel(source.type)} источник
      </h2>
      {error && <div className="mb-4 p-2 bg-red-100 text-red-700 rounded text-xs">{error}</div>}
      <form onSubmit={handleSubmit}>
        {/* Название - общее для всех */}
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
            placeholder="Например, Хабрахабр"
          />
        </div>

        {/* RSS поля */}
        {source.type.toLowerCase() === 'rss' && (
          <>
            <div className="mb-4">
              <label htmlFor="url" className="block text-xs font-medium text-slate-700 mb-1">
                URL RSS-ленты
              </label>
              <input
                type="text"
                id="url"
                value={rssConfig.url}
                disabled
                className="w-full px-3 py-2 border border-slate-300 rounded-md bg-slate-100 text-slate-500 cursor-not-allowed"
              />
              <p className="mt-1 text-xs text-slate-500">URL нельзя изменить</p>
            </div>
            <div className="mb-4">
              <label htmlFor="rssLimit" className="block text-xs font-medium text-slate-700 mb-1">
                Лимит новостей
              </label>
              <input
                type="number"
                id="rssLimit"
                value={rssConfig.limit}
                onChange={(e) => setRssConfig(prev => ({ ...prev, limit: Number(e.target.value) }))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="category" className="block text-xs font-medium text-slate-700 mb-1">
                Категория
              </label>
              <input
                type="text"
                id="category"
                value={rssConfig.category || ''}
                onChange={(e) => setRssConfig(prev => ({ ...prev, category: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="Например, Технологии"
              />
            </div>
          </>
        )}

        {/* GitHub поля */}
        {source.type.toLowerCase() === 'github' && (
          <>
            <div className="mb-4">
              <label htmlFor="owner" className="block text-xs font-medium text-slate-700 mb-1">
                Владелец репозитория
              </label>
              <input
                type="text"
                id="owner"
                value={githubConfig.repositoryOwner}
                onChange={(e) => setGithubConfig(prev => ({ ...prev, repositoryOwner: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="Например, microsoft"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="repo" className="block text-xs font-medium text-slate-700 mb-1">
                Название репозитория
              </label>
              <input
                type="text"
                id="repo"
                value={githubConfig.repositoryName}
                onChange={(e) => setGithubConfig(prev => ({ ...prev, repositoryName: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="Например, vscode"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="githubLimit" className="block text-xs font-medium text-slate-700 mb-1">
                Лимит событий
              </label>
              <input
                type="number"
                id="githubLimit"
                value={githubConfig.limit}
                onChange={(e) => setGithubConfig(prev => ({ ...prev, limit: Number(e.target.value) }))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </>
        )}

        {/* Reddit поля */}
        {source.type.toLowerCase() === 'reddit' && (
          <>
            <div className="mb-4">
              <label htmlFor="subreddit" className="block text-xs font-medium text-slate-700 mb-1">
                Сабреддит
              </label>
              <input
                type="text"
                id="subreddit"
                value={redditConfig.subreddit}
                onChange={(e) => setRedditConfig(prev => ({ ...prev, subreddit: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="Например, technology"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="sortType" className="block text-xs font-medium text-slate-700 mb-1">
                Тип сортировки
              </label>
              <select
                id="sortType"
                value={redditConfig.sortType}
                onChange={(e) => setRedditConfig(prev => ({ ...prev, sortType: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                <option value="hot">Hot (Горячее)</option>
                <option value="new">New (Новое)</option>
                <option value="top">Top (Лучшее)</option>
              </select>
            </div>
            <div className="mb-4">
              <label htmlFor="redditLimit" className="block text-xs font-medium text-slate-700 mb-1">
                Лимит постов
              </label>
              <input
                type="number"
                id="redditLimit"
                value={redditConfig.limit}
                onChange={(e) => setRedditConfig(prev => ({ ...prev, limit: Number(e.target.value) }))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="redditCategory" className="block text-xs font-medium text-slate-700 mb-1">
                Категория
              </label>
              <input
                type="text"
                id="redditCategory"
                value={redditConfig.category || ''}
                onChange={(e) => setRedditConfig(prev => ({ ...prev, category: e.target.value }))}
                className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                placeholder="Например, Технологии"
              />
            </div>
          </>
        )}

        <div className="flex gap-3">
          <button
            type="button"
            onClick={() => navigate('/sources')}
            className="flex-1 py-2 px-4 border border-slate-300 rounded-full text-slate-700 hover:bg-slate-50 transition-colors"
          >
            Отмена
          </button>
          <button
            type="submit"
            disabled={saving}
            className={`flex-1 py-2 px-4 rounded-full text-white font-medium transition-colors ${
              saving ? 'bg-slate-400 cursor-not-allowed' : 'bg-indigo-600 hover:bg-indigo-700'
            }`}
          >
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default EditSourceForm;
