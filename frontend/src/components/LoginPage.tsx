import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const loadingToast = toast.loading('Вход...');
    try {
      const response = await fetch('http://localhost:5043/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      if (!response.ok) throw new Error('Неверный email или пароль');
      const data = await response.json();
      login(data.token, email);
      toast.success('Успешный вход!', { id: loadingToast });
      navigate('/');
    } catch (err: any) {
      toast.error(err.message, { id: loadingToast });
    }
  };

  return (
    <div className="flex items-center justify-center h-screen bg-slate-100">
      <form onSubmit={handleSubmit} className="p-8 bg-white rounded-xl shadow-md w-96">
        <h2 className="mb-6 text-xl font-bold">Вход</h2>
        <input type="email" placeholder="Email" className="w-full p-2 mb-4 border rounded" value={email} onChange={(e) => setEmail(e.target.value)} />
        <input type="password" placeholder="Пароль" className="w-full p-2 mb-4 border rounded" value={password} onChange={(e) => setPassword(e.target.value)} />
        <button type="submit" className="w-full p-2 text-white bg-indigo-600 rounded">Войти</button>
        <p className="mt-4 text-center text-sm text-slate-600">
          Нет аккаунта?{' '}
          <button
            type="button"
            onClick={() => navigate('/register')}
            className="text-indigo-600 hover:underline font-medium"
          >
            Зарегистрироваться
          </button>
        </p>
      </form>
    </div>
  );
};

export default LoginPage;
