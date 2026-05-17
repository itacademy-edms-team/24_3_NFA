import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../services/api';
import toast from 'react-hot-toast';
import AuthLayout from '../auth/AuthLayout';
import TextInput from '../auth/TextInput';
import PasswordInput from '../auth/PasswordInput';

const RegisterPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [repeatPassword, setRepeatPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== repeatPassword) {
      toast.error('Пароли не совпадают');
      return;
    }
    setLoading(true);
    const loadingToast = toast.loading('Регистрация...');
    try {
      const response = await api.post('/api/auth/register', { email, password });
      const data = response.data;
      if (data.requiresEmailConfirmation) {
        toast.success(data.message, { id: loadingToast, duration: 6000 });
        navigate('/confirm-email', { state: { email } });
        return;
      }
      if (data.token) {
        login(data.token, email);
        toast.success(data.message || 'Регистрация успешна!', { id: loadingToast });
        navigate('/');
      }
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg ?? 'Ошибка регистрации', { id: loadingToast });
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Регистрация">
      <form onSubmit={handleSubmit} className="auth-card">
        <TextInput
          id="register-email"
          label="Email"
          type="email"
          value={email}
          onChange={setEmail}
          required
          autoComplete="email"
        />
        <PasswordInput
          id="register-password"
          label="Password"
          value={password}
          onChange={setPassword}
          required
          minLength={6}
          autoComplete="new-password"
        />
        <PasswordInput
          id="register-repeat"
          label="Repeat password"
          value={repeatPassword}
          onChange={setRepeatPassword}
          required
          minLength={6}
          autoComplete="new-password"
        />
        <button type="submit" className="auth-btn auth-display" disabled={loading}>
          Зарегистрироваться
        </button>
        <p className="auth-footer">
          Уже есть аккаунт?{' '}
          <Link to="/login" className="auth-link">
            Войти
          </Link>
        </p>
      </form>
    </AuthLayout>
  );
};

export default RegisterPage;
