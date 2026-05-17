import React, { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import api from '../../services/api';
import toast from 'react-hot-toast';
import AuthLayout from '../auth/AuthLayout';

type ConfirmState = 'pending' | 'loading' | 'success' | 'error';

const ConfirmEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [tokenInput, setTokenInput] = useState('');
  const [state, setState] = useState<ConfirmState>('pending');
  const [message, setMessage] = useState('');
  const [resending, setResending] = useState(false);

  const emailFromRegister = (location.state as { email?: string } | null)?.email;

  useEffect(() => {
    const token = searchParams.get('token');
    if (!token) return;

    setState('loading');
    api
      .get('/api/auth/confirm-email', { params: { token } })
      .then((res) => {
        setMessage(res.data.message);
        setState('success');
        toast.success(res.data.message);
      })
      .catch((err) => {
        const msg = err.response?.data?.message ?? 'Ошибка подтверждения';
        setMessage(msg);
        setState('error');
        toast.error(msg);
      });
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = tokenInput.trim();
    if (!trimmed) {
      toast.error('Вставьте токен из ссылки в письме');
      return;
    }

    setState('loading');
    try {
      const res = await api.get('/api/auth/confirm-email', { params: { token: trimmed } });
      setMessage(res.data.message);
      setState('success');
      toast.success(res.data.message);
    } catch {
      toast.error(
        'Недействительная или устаревшая ссылка. Откройте письмо заново или запросите новую ссылку.'
      );
      setState('pending');
    }
  };

  const handleResend = async () => {
    const email = emailFromRegister?.trim();
    if (!email) {
      toast.error('Укажите email на странице регистрации или войдите снова');
      return;
    }
    setResending(true);
    try {
      const res = await api.post('/api/auth/resend-confirmation', { email });
      toast.success(res.data.message);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      toast.error(msg ?? 'Не удалось отправить письмо');
    } finally {
      setResending(false);
    }
  };

  if (state === 'loading' && searchParams.get('token')) {
    return (
      <AuthLayout title="Подтверждение" subtitle="Подтверждаем ваш email...">
        <div className="auth-card">
          <p className="auth-message">Подождите...</p>
        </div>
      </AuthLayout>
    );
  }

  if (state === 'success') {
    return (
      <AuthLayout title="Подтверждение" subtitle={message}>
        <div className="auth-card">
          <button
            type="button"
            className="auth-btn auth-display"
            onClick={() => navigate('/login')}
          >
            Войти
          </button>
          <Link to="/" className="auth-back">
            &lt; Вернуться на главную
          </Link>
        </div>
      </AuthLayout>
    );
  }

  if (state === 'error') {
    return (
      <AuthLayout title="Подтверждение" subtitle={message}>
        <div className="auth-card">
          {emailFromRegister && (
            <button
              type="button"
              className="auth-btn auth-display mb-4"
              disabled={resending}
              onClick={handleResend}
            >
              {resending ? 'Отправка...' : 'Отправить ссылку повторно'}
            </button>
          )}
          <Link to="/login" className="auth-back">
            &lt; Вернуться ко входу
          </Link>
        </div>
      </AuthLayout>
    );
  }

  const subtitle = emailFromRegister
    ? `Мы отправили ссылку для подтверждения на ${emailFromRegister}`
    : 'Мы отправили на вашу почту ссылку для подтверждения регистрации';

  return (
    <AuthLayout title="Подтверждение" subtitle={subtitle}>
      <form onSubmit={handleSubmit} className="auth-card">
        <p className="auth-message text-sm mb-4">
          Откройте письмо и перейдите по ссылке. Если ссылка не открывается, скопируйте часть после{' '}
          <code className="text-xs">token=</code> из адресной строки и вставьте ниже.
        </p>
        <div className="auth-field">
          <label htmlFor="confirm-token" className="auth-label auth-display">
            Токен из ссылки
          </label>
          <input
            id="confirm-token"
            type="text"
            className="auth-input auth-input--plain"
            value={tokenInput}
            onChange={(e) => setTokenInput(e.target.value)}
            autoComplete="off"
            placeholder="вставьте токен"
          />
        </div>
        <button type="submit" className="auth-btn auth-display">
          Подтвердить
        </button>
        {emailFromRegister && (
          <button
            type="button"
            className="auth-btn auth-display mt-3 opacity-90"
            disabled={resending}
            onClick={handleResend}
          >
            {resending ? 'Отправка...' : 'Отправить ссылку повторно'}
          </button>
        )}
        <Link to="/" className="auth-back">
          &lt; Вернуться на главную
        </Link>
      </form>
    </AuthLayout>
  );
};

export default ConfirmEmailPage;
