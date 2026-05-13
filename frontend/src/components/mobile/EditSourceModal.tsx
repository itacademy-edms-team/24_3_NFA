import React, { useState, useEffect } from 'react';
import { updateSource, type SourceConfiguration } from '../../services/newsService';
import api from '../../services/api';
import { FaTimes } from 'react-icons/fa';

interface Source {
  id: number;
  name: string;
  type: string;
  configuration: string;
  isActive: boolean;
}

interface EditSourceModalProps {
  sourceId: number;
  onClose: () => void;
}

const EditSourceModal: React.FC<EditSourceModalProps> = ({ sourceId, onClose }) => {
  const [source, setSource] = useState<Source | null>(null);
  const [name, setName] = useState<string>('');
  const [isActive, setIsActive] = useState<boolean>(true);
  const [tumblrBlog, setTumblrBlog] = useState('');
  const [tumblrLimit, setTumblrLimit] = useState(10);
  const [loading, setLoading] = useState<boolean>(true);
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadSource = async () => {
      try {
        setLoading(true);
        const response = await api.get<Source>(`/api/sources/${sourceId}`);
        const sourceData = response.data;
        setSource(sourceData);
        setName(sourceData.name);
        setIsActive(sourceData.isActive);

        const config = sourceData.configuration ? JSON.parse(sourceData.configuration) : {};
        const typeLower = (sourceData.type ?? '').toLowerCase();
        if (typeLower === 'tumblr') {
          setTumblrBlog(config.blogName ?? '');
          setTumblrLimit(Number(config.limit ?? 10));
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Ошибка при загрузке');
      } finally {
        setLoading(false);
      }
    };

    loadSource();
  }, [sourceId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    if (!source) return;

    try {
      const typeLower = source.type.toLowerCase();
      let configuration: SourceConfiguration;

      if (typeLower === 'tumblr') {
        configuration = { blogName: tumblrBlog, limit: tumblrLimit };
      } else {
        configuration = JSON.parse(source.configuration);
      }

      await updateSource(source.id, {
        name,
        type: source.type,
        configuration,
        isActive,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка при сохранении');
    } finally {
      setSaving(false);
    }
  };

  const typeLower = source?.type.toLowerCase() ?? '';

  if (loading) {
    return (
      <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/50">
        <div className="bg-white rounded-t-2xl w-full p-6">
          <p className="text-center text-slate-500">Загрузка...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/50" onClick={onClose}>
      <div
        className="bg-white rounded-t-2xl w-full max-h-[80vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="sticky top-0 bg-white border-b border-slate-200 px-4 py-3 flex items-center justify-between">
          <h3 className="text-[16px] font-semibold text-slate-900">Редактировать</h3>
          <button onClick={onClose} className="p-2 text-slate-600 active:bg-slate-100 rounded-lg">
            <FaTimes className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-4 space-y-4">
          {error && (
            <div className="p-3 bg-red-100 text-red-700 rounded-lg text-sm">{error}</div>
          )}

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Название *</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex items-center gap-2">
            <input
              id="mobile-isActive"
              type="checkbox"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="h-4 w-4 rounded border-gray-300 text-indigo-600"
            />
            <label htmlFor="mobile-isActive" className="text-sm text-slate-700">
              Источник активен
            </label>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Тип</label>
            <input
              type="text"
              value={source?.type || ''}
              disabled
              className="w-full px-3 py-2 border border-slate-300 rounded-lg bg-slate-100 text-slate-500"
            />
          </div>

          {typeLower === 'tumblr' && (
            <>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Имя блога *</label>
                <input
                  type="text"
                  value={tumblrBlog}
                  onChange={(e) => setTumblrBlog(e.target.value)}
                  required
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg"
                  placeholder="staff"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Лимит постов</label>
                <input
                  type="number"
                  value={tumblrLimit}
                  min={1}
                  max={100}
                  onChange={(e) => setTumblrLimit(Number(e.target.value))}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg"
                />
              </div>
            </>
          )}

          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2.5 border border-slate-300 rounded-lg text-slate-700 font-medium hover:bg-slate-50"
            >
              Отмена
            </button>
            <button
              type="submit"
              disabled={saving}
              className={`flex-1 py-2.5 rounded-lg text-white font-medium ${
                saving ? 'bg-slate-400' : 'bg-indigo-600 hover:bg-indigo-700'
              }`}
            >
              {saving ? 'Сохранение...' : 'Сохранить'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditSourceModal;
