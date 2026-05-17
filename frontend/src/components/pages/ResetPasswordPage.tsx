import React, { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import api from '../../services/api';
import toast from 'react-hot-toast';
import AuthLayout from '../auth/AuthLayout';
import PasswordInput from '../auth/PasswordInput';

const ResetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const [password, setPassword] = useState('');
  const [repeatPassword, setRepeatPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const token = searchParams.get('token');
    if (!token) {
      toast.error('Токен не указан');
      return;
    }
    if (password !== repeatPassword) {
      toast.error('Пароли не совпадают');
      return;
    }
    setLoading(true);
    try {
      const res = await api.post('/api/auth/reset-password', { token, newPassword: password });
      toast.success(res.data.message);
      navigate('/login');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg ?? 'Ошибка сброса пароля');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Новый пароль" subtitle="Придумайте новый пароль для входа">
      <form onSubmit={handleSubmit} className="auth-card">
        <PasswordInput
          id="reset-password"
          label="Password"
          value={password}
          onChange={setPassword}
          required
          minLength={6}
          autoComplete="new-password"
        />
        <PasswordInput
          id="reset-repeat"
          label="Repeat password"
          value={repeatPassword}
          onChange={setRepeatPassword}
          required
          minLength={6}
          autoComplete="new-password"
        />
        <button type="submit" className="auth-btn auth-display" disabled={loading}>
          Сохранить
        </button>
        <Link to="/login" className="auth-back">
          &lt; Вернуться ко входу
        </Link>
      </form>
    </AuthLayout>
  );
};

export default ResetPasswordPage;
