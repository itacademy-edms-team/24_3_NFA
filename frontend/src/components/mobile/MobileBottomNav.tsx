import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { FaHeart, FaHome, FaUser, FaCog } from 'react-icons/fa';

const MobileBottomNav: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const tabs = [
    { path: '/collections', icon: FaHeart, label: 'Избранное' },
    { path: '/', icon: FaHome, label: 'Главная' },
    { path: '/profile', icon: FaUser, label: 'Профиль' },
    { path: '/settings', icon: FaCog, label: 'Настройки' },
  ];

  const isActive = (path: string) => {
    if (path === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(path);
  };

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-[#E5E5EA] px-2 pb-safe">
      <div className="flex justify-around items-center h-16">
        {tabs.map((tab) => {
          const active = isActive(tab.path);
          return (
            <button
              key={tab.path}
              onClick={() => navigate(tab.path)}
              className={`flex flex-col items-center justify-center w-full py-1 ${
                active ? 'text-[#6B5B95]' : 'text-[#8E8E93]'
              }`}
            >
              <tab.icon className="w-6 h-6 mb-0.5" />
              <span className="text-[10px] font-medium">{tab.label}</span>
            </button>
          );
        })}
      </div>
    </nav>
  );
};

export default MobileBottomNav;
