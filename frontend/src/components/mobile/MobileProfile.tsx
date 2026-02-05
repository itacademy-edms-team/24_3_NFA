import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaRss, FaGithub, FaReddit, FaChevronRight, FaPen, FaClock } from 'react-icons/fa';
import MobileHeader from './MobileHeader';

interface Source {
  id: number;
  name: string;
  type: string;
  isActive: boolean;
}

interface UserProfile {
  nickname: string;
  fio: string;
  birthday: string;
  status: string;
}

const MobileProfile: React.FC = () => {
  const navigate = useNavigate();
  const [sources, setSources] = useState<Source[]>([]);
  const [loading, setLoading] = useState(true);
  const [user] = useState<UserProfile>({
    nickname: 'Nickname',
    fio: 'FIO',
    birthday: 'Birthday date',
    status: 'Status',
  });

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

  const getSourceIcon = (type: string) => {
    const typeLower = type.toLowerCase();
    if (typeLower === 'github') return FaGithub;
    if (typeLower === 'reddit') return FaReddit;
    if (typeLower === 'rss') return FaRss;
    return FaRss;
  };

  const handleEdit = () => {
    console.log('Edit profile');
  };

  const handleSourceClick = (id: number) => {
    navigate('/', { state: { sourceFilter: id } });
  };

  return (
    <div>
      <MobileHeader title="Your Profile" showSearch={false} onMenuClick={handleEdit} />

      {/* Profile Header */}
      <div className="px-4 py-4 bg-white border-b border-[#E5E5EA]">
        <div className="flex items-start gap-4">
          {/* Avatar */}
          <div className="w-20 h-20 bg-[#E8F0FE] rounded-full flex items-center justify-center flex-shrink-0">
            <svg className="w-14 h-14 text-[#6B5B95]" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
            </svg>
          </div>

          {/* User Info */}
          <div className="flex-1 pt-1">
            <div className="flex items-center justify-between mb-1">
              <h2 className="text-[16px] font-semibold text-[#1A1A1A]">{user.nickname}</h2>
              <button onClick={handleEdit} className="p-1 text-[#8E8E93] hover:text-[#1A1A1A]">
                <FaPen className="w-4 h-4" />
              </button>
            </div>
            <p className="text-[13px] text-[#6B6B6B]">{user.fio}</p>
            <p className="text-[12px] text-[#8E8E93]">{user.birthday}</p>
            <p className="text-[12px] text-[#8E8E93]">{user.status}</p>
          </div>
        </div>
      </div>

      {/* Section Title */}
      <div className="px-4 py-3">
        <h3 className="text-[14px] font-semibold text-[#1A1A1A] mb-3 flex items-center gap-2">
          <span>Мои каналы</span>
          <span className="px-2 py-0.5 bg-[#E5E5EA] text-[#6B6B6B] text-[10px] rounded-full">
            {sources.length}
          </span>
        </h3>

        {loading ? (
          <div className="flex items-center justify-center py-10">
            <p className="text-[14px] text-[#6B6B6B]">Загрузка...</p>
          </div>
        ) : sources.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-10">
            <p className="text-[14px] text-[#6B6B6B] text-center mb-3">
              Каналы не добавлены
            </p>
            <button
              onClick={() => navigate('/add-source')}
              className="px-6 py-2.5 bg-[#6B5B95] text-white text-[13px] font-medium rounded-full"
            >
              Добавить канал
            </button>
          </div>
        ) : (
          <div className="space-y-2">
            {sources.map((source) => {
              const SourceIcon = getSourceIcon(source.type);
              return (
                <div
                  key={source.id}
                  onClick={() => handleSourceClick(source.id)}
                  className="bg-white rounded-xl p-3 shadow-sm flex items-center gap-3 cursor-pointer active:bg-[#F5F5F7] transition-colors"
                >
                  {/* Thumbnail */}
                  <div className="w-16 h-16 bg-[#F5F5F7] rounded-lg flex-shrink-0 flex items-center justify-center">
                    <SourceIcon className="w-7 h-7 text-[#6B5B95]" />
                  </div>

                  {/* Info */}
                  <div className="flex-1 min-w-0">
                    <h4 className="text-[14px] font-semibold text-[#1A1A1A] truncate mb-1">
                      {source.name}
                    </h4>
                    <p className="text-[12px] text-[#6B6B6B] line-clamp-2">
                      {source.type} {source.isActive ? '' : '• Неактивен'}
                    </p>
                    <div className="flex items-center gap-1 mt-1 text-[#8E8E93]">
                      <FaClock className="w-3 h-3" />
                      <span className="text-[11px]">Today • 23 min</span>
                    </div>
                  </div>

                  {/* Arrow */}
                  <FaChevronRight className="w-4 h-4 text-[#8E8E93] flex-shrink-0" />
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default MobileProfile;
