import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import SourcesList from './SourcesList';
import toast from 'react-hot-toast';
import { FaChevronDown, FaChevronUp } from 'react-icons/fa';

const ProfilePage: React.FC = () => {
  const { user, logout } = useAuth();
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [isAccordionOpen, setIsAccordionOpen] = useState(false);

  const handlePasswordChange = async () => {
    const loadingToast = toast.loading('Смена пароля...');
    try {
      const response = await fetch('http://localhost:5043/api/auth/change-password', {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ oldPassword, newPassword }),
      });
      if (!response.ok) throw new Error('Ошибка смены пароля');
      toast.success('Пароль успешно изменен', { id: loadingToast });
      setOldPassword('');
      setNewPassword('');
      setIsAccordionOpen(false);
    } catch (err: any) {
      toast.error(err.message, { id: loadingToast });
    }
  };

  return (
    <div className="max-w-4xl mx-auto mt-6 p-6 bg-white rounded-2xl shadow-md">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-slate-900">Профиль</h2>
        <button 
          onClick={logout} 
          className="px-4 py-2 bg-slate-500 text-white rounded-xl hover:bg-slate-600 transition-colors text-sm font-medium"
        >
          Выйти
        </button>
      </div>

      <div className="mb-6 space-y-1">
        <p className="text-sm text-slate-500 uppercase font-semibold tracking-wider">Email аккаунта</p>
        <p className="text-lg text-slate-900 font-medium">{user?.email}</p>
      </div>
      
      <div className="mb-8 border border-slate-100 rounded-xl overflow-hidden">
        <button 
          onClick={() => setIsAccordionOpen(!isAccordionOpen)}
          className="w-full flex items-center justify-between p-4 bg-slate-50 hover:bg-slate-100 transition-colors"
        >
          <span className="font-semibold text-slate-700">Смена пароля</span>
          {isAccordionOpen ? <FaChevronUp className="text-slate-400" /> : <FaChevronDown className="text-slate-400" />}
        </button>
        
        {isAccordionOpen && (
          <div className="p-4 bg-white border-t border-slate-100 space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className="text-xs text-slate-500 font-medium ml-1">Текущий пароль</label>
                <input 
                  type="password" 
                  placeholder="••••••••" 
                  className="w-full p-2 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500" 
                  value={oldPassword} 
                  onChange={(e) => setOldPassword(e.target.value)} 
                />
              </div>
              <div className="space-y-1">
                <label className="text-xs text-slate-500 font-medium ml-1">Новый пароль</label>
                <input 
                  type="password" 
                  placeholder="••••••••" 
                  className="w-full p-2 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500" 
                  value={newPassword} 
                  onChange={(e) => setNewPassword(e.target.value)} 
                />
              </div>
            </div>
            <div className="flex justify-end">
              <button 
                onClick={handlePasswordChange} 
                className="px-6 py-2 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 transition-colors font-medium shadow-sm"
              >
                Обновить пароль
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="mt-8 border-t border-slate-100 pt-8">
        <SourcesList />
      </div>
    </div>
  );
};

export default ProfilePage;
