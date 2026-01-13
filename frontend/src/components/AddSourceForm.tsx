import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom'; 
import { createSource } from '../services/newsService';
import type { RssSourceConfiguration, GitHubSourceConfiguration, RedditSourceConfiguration } from '../services/newsService'; 

const AddSourceForm: React.FC = () => {
  const [sourceType, setSourceType] = useState<string>('rss');
  const [name, setName] = useState<string>('');
  
  // RSS fields
  const [url, setUrl] = useState<string>('');
  const [rssLimit, setRssLimit] = useState<number>(10); 
  const [category, setCategory] = useState<string>('');
  
  // GitHub fields
  const [repositoryOwner, setRepositoryOwner] = useState<string>('');
  const [repositoryName, setRepositoryName] = useState<string>('');
  const [token, setToken] = useState<string>('');
  const [githubLimit, setGithubLimit] = useState<number>(10);
  
  // Reddit fields
  const [subreddit, setSubreddit] = useState<string>('');
  const [sortType, setSortType] = useState<string>('hot');
  const [redditLimit, setRedditLimit] = useState<number>(10);
  const [redditCategory, setRedditCategory] = useState<string>('');
  
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate(); 

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      let configuration: RssSourceConfiguration | GitHubSourceConfiguration | RedditSourceConfiguration;

      if (sourceType === 'rss') {
        configuration = {
          url: url,
          limit: rssLimit,
          category: category || "",
        };
      } else if (sourceType === 'github') {
        configuration = {
          repositoryOwner: repositoryOwner,
          repositoryName: repositoryName,
          token: token || undefined,
          limit: githubLimit,
        };
      } else if (sourceType === 'reddit') {
        configuration = {
          subreddit: subreddit,
          sortType: sortType,
          limit: redditLimit,
          category: redditCategory || undefined,
        };
      } else {
        throw new Error('Неподдерживаемый тип источника');
      }

      const newSourceData = {
        name: name,
        type: sourceType, 
        configuration: configuration,
        isActive: true, 
      };

      await createSource(newSourceData);

      // Reset form
      setName('');
      setUrl('');
      setRssLimit(10);
      setCategory('');
      setRepositoryOwner('');
      setRepositoryName('');
      setToken('');
      setGithubLimit(10);
      setSubreddit('');
      setSortType('hot');
      setRedditLimit(10);
      setRedditCategory('');

      alert('Источник успешно добавлен!');
      // Navigate to home page to see the new news
      navigate('/');
    } catch (err) {
      console.error("Error adding source:", err);
      setError(err instanceof Error ? err.message : 'Произошла ошибка при добавлении источника.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm z-50">
      <h2 className="text-lg font-semibold mb-4 text-slate-900">Добавить новый источник</h2>
      {error && <div className="mb-4 p-2 bg-red-100 text-red-700 rounded text-xs">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="sourceType" className="block text-xs font-medium text-gray-700 mb-1">
            Тип источника *
          </label>
          <select
            id="sourceType"
            value={sourceType}
            onChange={(e) => setSourceType(e.target.value)}
            required
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="rss">RSS</option>
            <option value="github">GitHub</option>
            <option value="reddit">Reddit</option>
          </select>
        </div>
        
        <div className="mb-4">
          <label htmlFor="name" className="block text-xs font-medium text-gray-700 mb-1">
            Название источника *
          </label>
          <input
            type="text"
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Например, Хабрахабр"
          />
        </div>

        {sourceType === 'rss' && (
          <>
            <div className="mb-4">
              <label htmlFor="url" className="block text-xs font-medium text-gray-700 mb-1">
                URL RSS-ленты *
              </label>
              <input
                type="url"
                id="url"
                value={url}
                onChange={(e) => setUrl(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="https://example.com/rss"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="rssLimit" className="block text-xs font-medium text-gray-700 mb-1">
                Лимит новостей
              </label>
              <input
                type="number"
                id="rssLimit"
                value={rssLimit}
                onChange={(e) => setRssLimit(Number(e.target.value))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="category" className="block text-xs font-medium text-gray-700 mb-1">
                Категория
              </label>
              <input
                type="text"
                id="category"
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Например, Технологии"
              />
            </div>
          </>
        )}

        {sourceType === 'github' && (
          <>
            <div className="mb-4">
              <label htmlFor="repositoryOwner" className="block text-xs font-medium text-gray-700 mb-1">
                Владелец репозитория (username или organization) *
              </label>
              <input
                type="text"
                id="repositoryOwner"
                value={repositoryOwner}
                onChange={(e) => setRepositoryOwner(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="microsoft"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="repositoryName" className="block text-xs font-medium text-gray-700 mb-1">
                Название репозитория *
              </label>
              <input
                type="text"
                id="repositoryName"
                value={repositoryName}
                onChange={(e) => setRepositoryName(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="vscode"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="token" className="block text-xs font-medium text-gray-700 mb-1">
                GitHub Personal Access Token (опционально)
              </label>
              <input
                type="password"
                id="token"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="ghp_..."
              />
            </div>
            <div className="mb-4">
              <label htmlFor="githubLimit" className="block text-xs font-medium text-gray-700 mb-1">
                Лимит событий
              </label>
              <input
                type="number"
                id="githubLimit"
                value={githubLimit}
                onChange={(e) => setGithubLimit(Number(e.target.value))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </>
        )}

        {sourceType === 'reddit' && (
          <>
            <div className="mb-4">
              <label htmlFor="subreddit" className="block text-xs font-medium text-gray-700 mb-1">
                Название сабреддита (без r/) *
              </label>
              <input
                type="text"
                id="subreddit"
                value={subreddit}
                onChange={(e) => setSubreddit(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="programming"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="sortType" className="block text-xs font-medium text-gray-700 mb-1">
                Тип сортировки
              </label>
              <select
                id="sortType"
                value={sortType}
                onChange={(e) => setSortType(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="hot">Hot</option>
                <option value="new">New</option>
                <option value="top">Top</option>
              </select>
            </div>
            <div className="mb-4">
              <label htmlFor="redditLimit" className="block text-xs font-medium text-gray-700 mb-1">
                Лимит постов
              </label>
              <input
                type="number"
                id="redditLimit"
                value={redditLimit}
                onChange={(e) => setRedditLimit(Number(e.target.value))}
                min="1"
                max="100"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="mb-4">
              <label htmlFor="redditCategory" className="block text-xs font-medium text-gray-700 mb-1">
                Категория
              </label>
              <input
                type="text"
                id="redditCategory"
                value={redditCategory}
                onChange={(e) => setRedditCategory(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Например, Технологии"
              />
            </div>
          </>
        )}

        <button
          type="submit"
          disabled={loading}
          className={`w-full py-2 px-4 rounded-md text-white font-medium ${
            loading ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
          }`}
        >
          {loading ? 'Добавление...' : 'Добавить источник'}
        </button>
      </form>
    </div>
  );
};

export default AddSourceForm;