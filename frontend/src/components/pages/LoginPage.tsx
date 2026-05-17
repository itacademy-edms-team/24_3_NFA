import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../services/api';
import toast from 'react-hot-toast';
import AuthLayout from '../auth/AuthLayout';
import TextInput from '../auth/TextInput';
import PasswordInput from '../auth/PasswordInput';

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    const loadingToast = toast.loading('Вход...');
    try {
      const response = await api.post('/api/auth/login', { email, password });
      login(response.data.token, email);
      toast.success('Вход выполнен', { id: loadingToast });
      navigate('/');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg ?? 'Неверный email или пароль', { id: loadingToast });
    } finally {
      setLoading(false);
    }
  };

  const handleSocial = () => {
    toast('Вход через соцсети будет доступен позже', { icon: 'ℹ️' });
  };

  return (
    <AuthLayout title="Вход" showSocial onSocialClick={handleSocial}>
      <form onSubmit={handleSubmit} className="auth-card auth-card--wide">
        <TextInput
          id="login-email"
          label="Login"
          type="email"
          value={email}
          onChange={setEmail}
          required
          autoComplete="email"
        />
        <PasswordInput
          id="login-password"
          label="Password"
          value={password}
          onChange={setPassword}
          required
          autoComplete="current-password"
        />
        <Link to="/forgot-password" className="auth-forgot">
          Забыли пароль?
        </Link>
        <button type="submit" className="auth-btn auth-display" disabled={loading}>
          Войти
        </button>
        <p className="auth-footer">
          нет аккаунта{' '}
          <Link to="/register" className="auth-link">
            Зарегистрируйтесь
          </Link>
        </p>
      </form>
    </AuthLayout>
  );
};

export default LoginPage;
