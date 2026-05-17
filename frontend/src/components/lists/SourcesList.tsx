import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  fetchSources,
  deleteSource,
  setSourceActive,
  syncSource,
  type NewsSourceListItem,
} from '../../services/newsService';
import { SourceSkeleton } from '../ui/Skeleton';
import toast from 'react-hot-toast';
import {
  FaSync,
  FaPause,
  FaPlay,
  FaBookOpen,
  FaPencilAlt,
  FaTrash,
} from 'react-icons/fa';
import SourceTypeIcon from '../ui/SourceTypeIcon';
import EditSourceForm from '../forms/EditSourceForm';

type IconActionButtonProps = {
  onClick: () => void;
  title: string;
  disabled?: boolean;
  className: string;
  children: React.ReactNode;
};

const IconActionButton: React.FC<IconActionButtonProps> = ({
  onClick,
  title,
  disabled,
  className,
  children,
}) => (
  <button
    type="button"
    onClick={onClick}
    disabled={disabled}
    title={title}
    aria-label={title}
    className={`p-2 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed ${className}`}
  >
    {children}
  </button>
);

const SourcesList: React.FC = () => {
  const [sources, setSources] = useState<NewsSourceListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<number | null>(null);
  const [syncingId, setSyncingId] = useState<number | null>(null);
  const [editingSourceId, setEditingSourceId] = useState<number | null>(null);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    loadSources();
  }, []);

  const loadSources = async () => {
    try {
      setLoading(true);
      setError(null);
      const sourcesData = await fetchSources();
      setSources(sourcesData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Произошла ошибка при загрузке источников.');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteSource(id);
      setSources(sources.filter((source) => source.id !== id));
      setShowDeleteConfirm(null);
      toast.success('Источник удалён');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Ошибка при удалении');
    }
  };

  const handleToggleActive = async (source: NewsSourceListItem) => {
    try {
      const updated = await setSourceActive(source.id, !source.isActive);
      setSources((prev) => prev.map((s) => (s.id === updated.id ? { ...s, ...updated } : s)));
      toast.success(updated.isActive ? 'Источник возобновлён' : 'Источник на паузе');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Не удалось изменить статус');
    }
  };

  const handleSync = async (id: number) => {
    setSyncingId(id);
    const loadingToast = toast.loading('Обновление...');
    try {
      const result = await syncSource(id);
      await loadSources();
      if (result.lastError) {
        toast.error(result.lastError, { id: loadingToast });
      } else {
        toast.success(
          result.itemsAdded > 0
            ? `Добавлено новостей: ${result.itemsAdded}`
            : 'Новых новостей нет',
          { id: loadingToast }
        );
      }
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { detail?: string; message?: string } } })?.response?.data
          ?.detail ||
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        (err instanceof Error ? err.message : 'Ошибка обновления');
      toast.error(msg, { id: loadingToast });
      await loadSources();
    } finally {
      setSyncingId(null);
    }
  };

  if (loading) {
    return (
      <div className="max-w-5xl mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100 space-y-4">
        <SourceSkeleton />
        <SourceSkeleton />
      </div>
    );
  }

  if (error) {
    return <div className="text-center text-slate-500 mt-10 text-sm">Ошибка: {error}</div>;
  }

  return (
    <div className="max-w-5xl mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md border border-slate-100">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-slate-900">Источники новостей</h2>
        <button
          onClick={() => navigate('/add-source')}
          className="px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700"
        >
          Добавить источник
        </button>
      </div>

      {sources.length === 0 ? (
        <div className="text-center py-8 text-slate-500">Нет добавленных источников</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-slate-200">
            <thead className="bg-slate-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase">Имя</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase">Тип</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase">Статус</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase">Проверка</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase">Ошибка</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-500 uppercase w-[11rem]">Действия</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {sources.map((source) => (
                <tr key={source.id}>
                  <td className="px-4 py-4 text-sm font-medium text-slate-900">{source.name}</td>
                  <td className="px-4 py-4">
                    <SourceTypeIcon type={source.type} />
                  </td>
                  <td className="px-4 py-4">
                    <span
                      className={`px-2 py-0.5 text-xs font-semibold rounded-full ${
                        source.isActive ? 'bg-green-100 text-green-800' : 'bg-amber-100 text-amber-800'
                      }`}
                    >
                      {source.isActive ? 'Активный' : 'На паузе'}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-sm text-slate-500">
                    {source.lastPolledAtUtc
                      ? new Date(source.lastPolledAtUtc).toLocaleString()
                      : 'Никогда'}
                  </td>
                  <td className="px-4 py-4 text-sm text-red-600 max-w-[14rem] align-top">
                    {source.lastError ? (
                      <div className="whitespace-normal break-words leading-snug">
                        <span>{source.lastError}</span>
                        {source.lastErrorAtUtc && (
                          <span className="block mt-1 text-xs text-red-500/90">
                            {new Date(source.lastErrorAtUtc).toLocaleString()}
                          </span>
                        )}
                      </div>
                    ) : (
                      '—'
                    )}
                  </td>
                  <td className="px-4 py-4 align-top">
                    <div className="flex flex-wrap items-center gap-0.5">
                      <IconActionButton
                        onClick={() => handleSync(source.id)}
                        disabled={syncingId === source.id}
                        title="Обновить сейчас"
                        className="text-indigo-600 hover:bg-indigo-50"
                      >
                        <FaSync className={`w-4 h-4 ${syncingId === source.id ? 'animate-spin' : ''}`} />
                      </IconActionButton>
                      <IconActionButton
                        onClick={() => handleToggleActive(source)}
                        title={source.isActive ? 'Пауза' : 'Возобновить'}
                        className="text-amber-700 hover:bg-amber-50"
                      >
                        {source.isActive ? (
                          <FaPause className="w-4 h-4" />
                        ) : (
                          <FaPlay className="w-4 h-4" />
                        )}
                      </IconActionButton>
                      <IconActionButton
                        onClick={() => navigate('/', { state: { sourceFilter: source.id } })}
                        title="Читать"
                        className="text-blue-600 hover:bg-blue-50"
                      >
                        <FaBookOpen className="w-4 h-4" />
                      </IconActionButton>
                      <IconActionButton
                        onClick={() => setEditingSourceId(source.id)}
                        title="Изменить"
                        className="text-slate-600 hover:bg-slate-50"
                      >
                        <FaPencilAlt className="w-4 h-4" />
                      </IconActionButton>
                      <IconActionButton
                        onClick={() => setShowDeleteConfirm(source.id)}
                        title="Удалить"
                        className="text-red-600 hover:bg-red-50"
                      >
                        <FaTrash className="w-4 h-4" />
                      </IconActionButton>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editingSourceId !== null && (
        <div
          className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center z-[100] p-4 overflow-y-auto"
          onClick={() => setEditingSourceId(null)}
        >
          <div
            className="w-full max-w-md my-8"
            onClick={(e) => e.stopPropagation()}
          >
            <EditSourceForm
              sourceIdProp={editingSourceId}
              onClose={() => {
                setEditingSourceId(null);
                loadSources();
              }}
              returnTo={location.pathname}
            />
          </div>
        </div>
      )}

      {showDeleteConfirm !== null && (
        <div className="fixed inset-0 bg-slate-900/30 backdrop-blur-sm flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 max-w-md w-full mx-4 shadow-2xl">
            <h3 className="text-lg font-semibold mb-2">Подтверждение удаления</h3>
            <p className="text-slate-600 mb-6">
              Удалить источник? Связанные новости будут удалены.
            </p>
            <div className="flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShowDeleteConfirm(null)}
                className="px-4 py-2 border border-slate-300 rounded-full text-slate-700"
              >
                Отмена
              </button>
              <button
                type="button"
                onClick={() => handleDelete(showDeleteConfirm)}
                className="px-4 py-2 bg-red-600 text-white rounded-full"
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
