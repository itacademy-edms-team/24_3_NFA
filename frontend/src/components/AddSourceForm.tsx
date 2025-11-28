import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom'; 
import { createSource } from '../services/newsService'; 

const AddSourceForm: React.FC = () => {
  const [name, setName] = useState<string>('');
  const [url, setUrl] = useState<string>('');
  const [limit, setLimit] = useState<number>(10); 
  const [category, setCategory] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate(); 

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const configuration = {
        url: url,
        limit: limit,
        category: category || null, 
      };


      const newSourceData = {
        name: name,
        type: 'rss', 
        configuration: JSON.stringify(configuration), 
        isActive: true, 
      };

      await createSource(newSourceData);

      setName('');
      setUrl('');
      setLimit(10);
      setCategory('');
      alert('Источник успешно добавлен!'); 
      navigate('/sources');
    } catch (err) {
      console.error("Error adding source:", err);
      setError(err instanceof Error ? err.message : 'Произошла ошибка при добавлении источника.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 text-sm">
      <h2 className="text-lg font-semibold mb-4 text-slate-900">Добавить новый RSS-канал</h2>
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
          <label htmlFor="limit" className="block text-xs font-medium text-gray-700 mb-1">
            Лимит новостей
          </label>
          <input
            type="number"
            id="limit"
            value={limit}
            onChange={(e) => setLimit(Number(e.target.value))}
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
        <button
          type="submit"
          disabled={loading}
          className={`w-full py-2 px-4 rounded-md text-white font-medium ${
            loading ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
          }`}
        >
          {loading ? 'Добавление...' : 'Добавить канал'}
        </button>
      </form>
    </div>
  );
};

export default AddSourceForm;