import React from 'react';
import { Toaster } from 'react-hot-toast';

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <>
      {children}
      <Toaster 
        position="top-right"
        toastOptions={{
          duration: 3000,
          style: {
            background: '#334155',
            color: '#fff',
            borderRadius: '0.75rem',
            fontSize: '14px',
          },
        }} 
      />
    </>
  );
};
