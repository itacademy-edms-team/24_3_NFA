import React, { useEffect } from 'react';
import '../../styles/auth.css';

interface AuthLayoutProps {
  title: string;
  subtitle?: string;
  children: React.ReactNode;
  showSocial?: boolean;
  onSocialClick?: (provider: 'google' | 'yandex' | 'vk') => void;
}

const AuthLayout: React.FC<AuthLayoutProps> = ({
  title,
  subtitle,
  children,
  showSocial = false,
  onSocialClick,
}) => {
  useEffect(() => {
    const prevHtmlOverflow = document.documentElement.style.overflow;
    const prevBodyOverflow = document.body.style.overflow;
    document.documentElement.style.overflow = 'hidden';
    document.body.style.overflow = 'hidden';
    return () => {
      document.documentElement.style.overflow = prevHtmlOverflow;
      document.body.style.overflow = prevBodyOverflow;
    };
  }, []);

  return (
    <div className="auth-page auth-body">
      <div className="auth-page-inner">
        <h1 className="auth-title auth-display">{title}</h1>
        {subtitle && <p className="auth-subtitle">{subtitle}</p>}
        {showSocial && (
          <div className="auth-social" aria-label="Вход через соцсети">
            <button
              type="button"
              className="auth-social-btn"
              aria-label="Google"
              onClick={() => onSocialClick?.('google')}
            >
              G
            </button>
            <button
              type="button"
              className="auth-social-btn"
              aria-label="Яндекс"
              onClick={() => onSocialClick?.('yandex')}
            >
              Я
            </button>
            <button
              type="button"
              className="auth-social-btn"
              aria-label="ВКонтакте"
              onClick={() => onSocialClick?.('vk')}
            >
              VK
            </button>
          </div>
        )}
        {children}
      </div>
    </div>
  );
};

export default AuthLayout;
