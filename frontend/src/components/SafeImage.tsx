import React, { useState } from 'react';

interface SafeImageProps {
  src: string;
  alt: string;
  className?: string;
  onError?: () => void;
}

const SafeImage: React.FC<SafeImageProps> = ({ src, alt, className = '', onError }) => {
  const [failed, setFailed] = useState(false);

  const handleError = () => {
    setFailed(true);
    onError?.();
  };

  if (failed) {
    return (
      <div
        className={`flex items-center justify-center bg-slate-100 text-slate-400 ${className}`}
        role="img"
        aria-label={alt}
      >
        <span className="text-sm">Изображение недоступно</span>
      </div>
    );
  }

  return (
    <img
      src={src}
      alt={alt}
      loading="lazy"
      className={className}
      onError={handleError}
    />
  );
};

export default SafeImage;
