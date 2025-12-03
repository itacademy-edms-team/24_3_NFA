import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { emitSourcesChanged } from '../services/newsService';

interface Source {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
  lastPolledAtUtc: string | null;
  lastError: string | null;
}

const SourcesList: React.FC = () => {
  const [sources, setSources] = useState<Source[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<number | null>(null); // ID источника для подтверждения удаления
  const navigate = useNavigate();

  useEffect(() => {
    loadSources();
  }, []);

  const loadSources = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await fetch('http://localhost:5043/api/sources');
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const sourcesData: Source[] = await response.json();
      setSources(sourcesData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Произошла ошибка при загрузке источников.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      const response = await fetch(`http://localhost:5043/api/sources/${id}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      // Удаляем источник из списка
      setSources(sources.filter(source => source.id !== id));
      setShowDeleteConfirm(null);
      emitSourcesChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Произошла ошибка при удалении источника.');
      console.error(err);
    }
  };

  const handleEdit = (id: number) => {
    navigate(`/edit-source/${id}`);
  };

  const handleReadNews = (id: number) => {
    navigate('/', { state: { sourceFilter: id } });
  };

  if (loading) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Загрузка источников...</div>;
  }

  if (error) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Ошибка: {error}</div>;
  }

  return (
    <div className="max-w-4xl mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-slate-900">Источники новостей</h2>
        <button
          onClick={() => navigate('/add-source')}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Добавить источник
        </button>
      </div>

      {sources.length === 0 ? (
        <div className="text-center py-8">
          <p className="text-slate-500">Нет добавленных источников</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Имя
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Тип
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Статус
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Последняя проверка
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Действия
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {sources.map((source) => (
                <tr key={source.id}>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">{source.name}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-500">{source.type}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      source.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                    }`}>
                      {source.isActive ? 'Активный' : 'Неактивный'}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {source.lastPolledAtUtc ? new Date(source.lastPolledAtUtc).toLocaleString() : 'Никогда'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex flex-col items-start space-y-2">
                      <button
                        onClick={() => handleReadNews(source.id)}
                        className="text-blue-600 hover:text-blue-800"
                      >
                        Читать новости
                      </button>
                      <button
                        onClick={() => handleEdit(source.id)}
                        className="text-slate-600 hover:text-slate-800"
                      >
                        Редактировать
                      </button>
                      <button
                        onClick={() => setShowDeleteConfirm(source.id)}
                        className="text-red-600 hover:text-red-900"
                      >
                        Удалить
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Модальное окно подтверждения удаления */}
      {showDeleteConfirm !== null && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-lg font-medium text-gray-900 mb-2">Подтверждение удаления</h3>
            <p className="text-gray-600 mb-4">
              Вы уверены, что хотите удалить этот источник? Все связанные новости будут безвозвратно удалены.
            </p>
            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={() => setShowDeleteConfirm(null)}
                className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              >
                Отмена
              </button>
              <button
                type="button"
                onClick={() => handleDelete(showDeleteConfirm)}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
              >
                Удалить
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SourcesList;