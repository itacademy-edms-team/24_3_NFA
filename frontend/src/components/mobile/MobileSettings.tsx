import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaRss, FaGithub, FaReddit, FaPen } from 'react-icons/fa';
import MobileHeader from './MobileHeader';
import EditSourceModal from './EditSourceModal';
import {
  fetchSources,
  setSourceActive,
  syncSource,
  type NewsSourceListItem,
} from '../../services/newsService';
import toast from 'react-hot-toast';

const MobileSettings: React.FC = () => {
  const navigate = useNavigate();
  const [sources, setSources] = useState<NewsSourceListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingSourceId, setEditingSourceId] = useState<number | null>(null);
  const [syncingId, setSyncingId] = useState<number | null>(null);

  const loadSources = async () => {
    try {
      setSources(await fetchSources());
    } catch (error) {
      console.error('Error loading sources:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSources();
  }, []);

  const handleToggleActive = async (source: NewsSourceListItem) => {
    try {
      await setSourceActive(source.id, !source.isActive);
      await loadSources();
      toast.success(source.isActive ? 'На паузе' : 'Возобновлено');
    } catch {
      toast.error('Не удалось изменить статус');
    }
  };

  const handleSync = async (id: number) => {
    setSyncingId(id);
    try {
      const result = await syncSource(id);
      await loadSources();
      toast.success(result.lastError ? result.lastError : `Добавлено: ${result.itemsAdded}`);
    } catch {
      toast.error('Ошибка обновления');
      await loadSources();
    } finally {
      setSyncingId(null);
    }
  };

  const getSourceIcon = (type: string) => {
    const typeLower = type.toLowerCase();
    if (typeLower === 'github') return FaGithub;
    if (typeLower === 'reddit') return FaReddit;
    return FaRss;
  };

  return (
    <div>
      <MobileHeader title="Настройки" showSearch={false} showMenu={false} />

      <div className="px-4 py-3">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg font-semibold text-slate-900">Источники</h2>
          <button
            onClick={() => navigate('/add-source')}
            className="px-3 py-1.5 bg-indigo-600 text-white text-xs font-medium rounded-lg flex items-center gap-1"
          >
            <FaPen className="w-3 h-3" />
            Добавить
          </button>
        </div>

        {loading ? (
          <p className="text-center text-slate-500 text-sm py-8">Загрузка...</p>
        ) : sources.length === 0 ? (
          <p className="text-center text-slate-500 text-sm py-8">Нет источников</p>
        ) : (
          <div className="space-y-3">
            {sources.map((source) => {
              const SourceIcon = getSourceIcon(source.type);
              return (
                <div key={source.id} className="bg-white rounded-lg p-3 shadow-sm border border-slate-100">
                  <div className="flex items-center gap-2 mb-1">
                    <SourceIcon className="w-4 h-4 text-slate-500" />
                    <span className="font-medium text-sm">{source.name}</span>
                    <span className="text-xs text-slate-400 capitalize">{source.type}</span>
                  </div>
                  {source.lastError && (
                    <p className="text-xs text-red-600 mb-2 line-clamp-2">{source.lastError}</p>
                  )}
                  <div className="flex flex-wrap gap-2">
                    <button
                      onClick={() => handleSync(source.id)}
                      disabled={syncingId === source.id}
                      className="text-xs text-indigo-600"
                    >
                      {syncingId === source.id ? '...' : 'Обновить'}
                    </button>
                    <button
                      onClick={() => handleToggleActive(source)}
                      className="text-xs text-amber-700"
                    >
                      {source.isActive ? 'Пауза' : 'Старт'}
                    </button>
                    <button
                      onClick={() => setEditingSourceId(source.id)}
                      className="text-xs text-slate-600"
                    >
                      Ред.
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {editingSourceId && (
        <EditSourceModal
          sourceId={editingSourceId}
          onClose={() => {
            setEditingSourceId(null);
            loadSources();
          }}
        />
      )}
    </div>
  );
};

export default MobileSettings;
