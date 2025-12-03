import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { updateSource, type RssSourceConfiguration } from '../services/newsService';

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
  const [rssConfig, setRssConfig] = useState<RssSourceConfiguration>({
    url: '',
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
        setSource(sourceData);
        setName(sourceData.name);

        // Парсим конфигурацию
        const config: RssSourceConfiguration = JSON.parse(sourceData.configuration);
        setRssConfig({
          url: config.url || '',
          limit: config.limit || 10,
          category: config.category || ''
        });
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
      const configuration: RssSourceConfiguration = {
        url: rssConfig.url,
        limit: rssConfig.limit,
        category: rssConfig.category || '',
      };

      const sourceData = {
        name: name,
        type: source.type,
        configuration: configuration,
        isActive: source.isActive,
      };

      await updateSource(source.id, sourceData);
      alert('Источник успешно обновлен!');
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

  return (
    <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm">
      <h2 className="text-lg font-semibold mb-4 text-slate-900">Редактировать RSS-канал</h2>
      {error && <div className="mb-4 p-2 bg-red-100 text-red-700 rounded text-xs">{error}</div>}
      <form onSubmit={handleSubmit}>
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
        <div className="mb-4">
          <label htmlFor="url" className="block text-xs font-medium text-gray-700 mb-1">
            URL RSS-ленты
          </label>
          <input
            type="text"
            id="url"
            value={rssConfig.url}
            disabled
            className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 text-gray-500 cursor-not-allowed"
          />
          <p className="mt-1 text-xs text-gray-500">URL нельзя изменить</p>
        </div>
        <div className="mb-4">
          <label htmlFor="limit" className="block text-xs font-medium text-gray-700 mb-1">
            Лимит новостей
          </label>
          <input
            type="number"
            id="limit"
            value={rssConfig.limit}
            onChange={(e) => setRssConfig(prev => ({ ...prev, limit: Number(e.target.value) }))}
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
            value={rssConfig.category || ''}
            onChange={(e) => setRssConfig(prev => ({ ...prev, category: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Например, Технологии"
          />
        </div>
        <div className="flex space-x-3">
          <button
            type="button"
            onClick={() => navigate('/sources')}
            className="flex-1 py-2 px-4 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Отмена
          </button>
          <button
            type="submit"
            disabled={saving}
            className={`flex-1 py-2 px-4 rounded-md text-white font-medium ${
              saving ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
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

