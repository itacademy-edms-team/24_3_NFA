import React from 'react';
import {
  FaTelegram,
  FaVk,
  FaRss,
  FaFolder,
  FaPlus,
  FaBars,
  FaCog,
} from 'react-icons/fa';

interface SidebarItem {
  title: string;
  icon: React.ComponentType<{ className?: string }>;
}

interface SidebarGroup {
  name: string;
  items: SidebarItem[];
}

const sidebarData: SidebarGroup[] = [
  {
    name: 'Источники',
    items: [
      { title: 'Telegram', icon: FaTelegram },
      { title: 'ВКонтакте', icon: FaVk },
      { title: 'RSS каналы', icon: FaRss },
    ],
  },
  {
    name: 'Ваши коллекции',
    items: [
      { title: 'Коллекция 1', icon: FaFolder },
      { title: 'Коллекция 2', icon: FaFolder },
      { title: 'Коллекция 3', icon: FaFolder },
    ],
  },
];

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ collapsed, onToggle }) => {
  return (
    <div
      className={`h-full bg-white text-slate-900 flex flex-col border-r border-slate-200 transition-[width] duration-300 ${
        collapsed ? 'w-14' : 'w-56'
      } shadow-sm`}
    >
      <div className="h-20 px-4 flex items-center gap-2 border-b border-slate-200">
        <button
          onClick={onToggle}
          className="p-2 rounded-lg hover:bg-slate-100 focus:outline-none transition-colors text-slate-600"
          aria-label="Toggle sidebar"
        >
          <FaBars />
        </button>
        {!collapsed && (
          <span className="text-xl font-black tracking-tight">Svodka</span>
        )}
      </div>

      <div className="flex-grow p-3 overflow-y-auto space-y-4 bg-white">
        {sidebarData.map((group) => (
          <div key={group.name} className="space-y-2">
            <p
              className={`text-xs font-semibold text-slate-400 uppercase transition-opacity duration-200 ${
                collapsed ? 'opacity-0' : 'opacity-100'
              }`}
            >
              {group.name}
            </p>
            <ul className="space-y-1">
              {group.items.map((item) => (
                <li key={item.title}>
                  <a
                    href="#"
                    className="flex items-center p-2 text-sm font-medium text-slate-600 rounded-xl hover:bg-slate-100 transition-colors"
                  >
                    <item.icon className="text-lg text-slate-500" />
                    <span
                      className={`transition-all duration-200 ${
                        collapsed ? 'opacity-0 w-0' : 'opacity-100 w-auto'
                      }`}
                      style={{ marginLeft: collapsed ? 0 : '0.75rem' }}
                    >
                      {item.title}
                    </span>
                  </a>
                </li>
              ))}
            </ul>
          </div>
        ))}
        {!collapsed && (
          <button className="flex items-center w-full p-3 text-sm font-medium text-white bg-slate-900 rounded-2xl hover:bg-slate-800 transition-colors shadow-sm">
            <FaPlus className="text-lg" />
            <span className="ml-3 whitespace-nowrap">Добавить коллекцию</span>
          </button>
        )}
      </div>

      <div className="p-3 border-t border-slate-200">
        <a
          href="#"
          className="flex items-center p-2 text-sm font-medium text-slate-600 rounded-xl hover:bg-slate-100 transition-colors"
        >
          <FaCog className="text-lg text-slate-500" />
          <span
            className={`ml-3 whitespace-nowrap transition-all duration-200 ${
              collapsed ? 'opacity-0 w-0' : 'opacity-100 w-auto'
            }`}
          >
            Настройки
          </span>
        </a>
      </div>
    </div>
  );
};

export default Sidebar;

