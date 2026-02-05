import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaRss, FaGithub, FaReddit, FaPen } from 'react-icons/fa';
import MobileHeader from './MobileHeader';
import EditSourceModal from './EditSourceModal';

interface Source {
  id: number;
  name: string;
  type: string;
  isActive: boolean;
  lastPolledAtUtc: string | null;
  lastError: string | null;
}

const MobileSettings: React.FC = () => {
  const navigate = useNavigate();
  const [sources, setSources] = useState<Source[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingSourceId, setEditingSourceId] = useState<number | null>(null);

  useEffect(() => {
    loadSources();
  }, []);

  const loadSources = async () => {
    try {
      const response = await fetch('http://localhost:5043/api/sources');
      if (response.ok) {
        const data: Source[] = await response.json();
        setSources(data);
      }
    } catch (error) {
      console.error('Error loading sources:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = () => {
    navigate('/add-source');
  };

  const handleSourceClick = (id: number) => {
    setEditingSourceId(id);
  };

  const getSourceIcon = (type: string) => {
    const typeLower = type.toLowerCase();
    if (typeLower === 'github') return FaGithub;
    if (typeLower === 'reddit') return FaReddit;
    if (typeLower === 'rss') return FaRss;
    return FaRss;
  };

  return (
    <div>
      <MobileHeader title="Настройки" showSearch={false} showMenu={false} />

      <div className="px-4 py-3">
        {/* Заголовок с кнопкой добавления */}
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg font-semibold text-slate-900">Источники</h2>
          <button
            onClick={handleEdit}
            className="px-3 py-1.5 bg-indigo-600 text-white text-xs font-medium rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-1"
          >
            <FaPen className="w-3 h-3" />
            Добавить
          </button>
        </div>

        {/* Таблица источников */}
        {loading ? (
          <div className="text-center py-8">
            <p className="text-slate-500 text-sm">Загрузка...</p>
          </div>
        ) : sources.length === 0 ? (
          <div className="text-center py-8">
            <p className="text-slate-500 text-sm">Нет добавленных источников</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-slate-200">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 uppercase">
                    Имя
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 uppercase">
                    Тип
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 uppercase">
                    Статус
                  </th>
                  <th className="px-3 py-2 text-left text-xs font-medium text-slate-500 uppercase">
                    Действия
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-slate-200">
                {sources.map((source) => {
                  const SourceIcon = getSourceIcon(source.type);
                  return (
                    <tr key={source.id} className="text-sm">
                      <td className="px-3 py-3 font-medium text-slate-900">
                        {source.name}
                      </td>
                      <td className="px-3 py-3 text-slate-500 capitalize">
                        <div className="flex items-center gap-1.5">
                          <SourceIcon className="w-4 h-4" />
                          {source.type}
                        </div>
                      </td>
                      <td className="px-3 py-3">
                        <span className={`px-2 py-0.5 text-xs font-semibold rounded-full ${
                          source.isActive 
                            ? 'bg-green-100 text-green-800' 
                            : 'bg-red-100 text-red-800'
                        }`}>
                          {source.isActive ? '✓' : '✗'}
                        </span>
                      </td>
                      <td className="px-3 py-3">
                        <button
                          onClick={() => handleSourceClick(source.id)}
                          className="text-indigo-600 hover:text-indigo-800 text-xs font-medium"
                        >
                          Ред.
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {editingSourceId && (
        <EditSourceModal
          sourceId={editingSourceId}
          onClose={() => setEditingSourceId(null)}
        />
      )}
    </div>
  );
};

export default MobileSettings;
