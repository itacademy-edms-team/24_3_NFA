import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import toast from 'react-hot-toast';
import AuthLayout from '../auth/AuthLayout';
import TextInput from '../auth/TextInput';

const ForgotPasswordPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await api.post('/api/auth/forgot-password', { email });
      setSent(true);
      toast.success(res.data.message);
    } catch {
      toast.error('Не удалось отправить запрос');
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      title="Забыли пароль?"
      subtitle="Введите Ваш email, используемый для входа. Мы вышлем письмо с инструкцией."
    >
      <form onSubmit={handleSubmit} className="auth-card">
        {sent ? (
          <p className="auth-message">
            Если email зарегистрирован, на него отправлена ссылка для восстановления доступа.
          </p>
        ) : (
          <TextInput
            id="forgot-email"
            label="Email"
            type="email"
            value={email}
            onChange={setEmail}
            required
            autoComplete="email"
          />
        )}
        {!sent && (
          <button type="submit" className="auth-btn auth-display" disabled={loading}>
            Восстановить доступ
          </button>
        )}
        <Link to="/login" className="auth-back">
          &lt; Вернуться ко входу
        </Link>
      </form>
    </AuthLayout>
  );
};

export default ForgotPasswordPage;
