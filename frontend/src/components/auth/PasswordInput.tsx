import React, { useState } from 'react';
import { FiEye, FiEyeOff } from 'react-icons/fi';

interface PasswordInputProps {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  minLength?: number;
  autoComplete?: string;
}

const PasswordInput: React.FC<PasswordInputProps> = ({
  id,
  label,
  value,
  onChange,
  required,
  minLength,
  autoComplete = 'current-password',
}) => {
  const [visible, setVisible] = useState(false);

  return (
    <div className="auth-field">
      <label htmlFor={id} className="auth-label auth-display">
        {label}
      </label>
      <div className="auth-input-wrap">
        <input
          id={id}
          type={visible ? 'text' : 'password'}
          className="auth-input"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          required={required}
          minLength={minLength}
          autoComplete={autoComplete}
        />
        <button
          type="button"
          className="auth-eye"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Скрыть пароль' : 'Показать пароль'}
          tabIndex={-1}
        >
          {visible ? <FiEyeOff /> : <FiEye />}
        </button>
      </div>
    </div>
  );
};

export default PasswordInput;
