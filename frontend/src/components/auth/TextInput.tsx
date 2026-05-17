import React from 'react';

interface TextInputProps {
  id: string;
  label: string;
  type?: string;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  autoComplete?: string;
}

const TextInput: React.FC<TextInputProps> = ({
  id,
  label,
  type = 'text',
  value,
  onChange,
  required,
  autoComplete,
}) => (
  <div className="auth-field">
    <label htmlFor={id} className="auth-label auth-display">
      {label}
    </label>
    <input
      id={id}
      type={type}
      className="auth-input auth-input--plain"
      value={value}
      onChange={(e) => onChange(e.target.value)}
      required={required}
      autoComplete={autoComplete}
    />
  </div>
);

export default TextInput;
