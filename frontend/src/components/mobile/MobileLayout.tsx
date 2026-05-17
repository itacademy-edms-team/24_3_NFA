import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import MobileBottomNav from './MobileBottomNav';
import MobileMain from './MobileMain';
import MobileCollections from './MobileCollections';
import MobileProfile from './MobileProfile';
import MobileSettings from './MobileSettings';
import LoginPage from '../pages/LoginPage';
import RegisterPage from '../pages/RegisterPage';
import ConfirmEmailPage from '../pages/ConfirmEmailPage';
import ForgotPasswordPage from '../pages/ForgotPasswordPage';
import ResetPasswordPage from '../pages/ResetPasswordPage';
import AddSourceForm from '../forms/AddSourceForm';

const MobileProtected: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user } = useAuth();
  return user ? <>{children}</> : <Navigate to="/login" replace />;
};

const MobileLayout: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-slate-200">
      <main className={user ? 'pb-[64px]' : ''}>
        <Routes>
          <Route path="/login" element={user ? <Navigate to="/" /> : <LoginPage />} />
          <Route path="/register" element={user ? <Navigate to="/" /> : <RegisterPage />} />
          <Route path="/confirm-email" element={<ConfirmEmailPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
          <Route
            path="/"
            element={
              <MobileProtected>
                <MobileMain />
              </MobileProtected>
            }
          />
          <Route
            path="/collections"
            element={
              <MobileProtected>
                <MobileCollections />
              </MobileProtected>
            }
          />
          <Route
            path="/profile"
            element={
              <MobileProtected>
                <MobileProfile />
              </MobileProtected>
            }
          />
          <Route
            path="/settings"
            element={
              <MobileProtected>
                <MobileSettings />
              </MobileProtected>
            }
          />
          <Route
            path="/add-source"
            element={
              <MobileProtected>
                <AddSourceForm />
              </MobileProtected>
            }
          />
          <Route path="*" element={<Navigate to={user ? '/' : '/login'} replace />} />
        </Routes>
      </main>
      {user && <MobileBottomNav />}
    </div>
  );
};

export default MobileLayout;
