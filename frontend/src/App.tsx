import { useState } from 'react';
import { Routes, Route, useNavigate, Navigate, useLocation } from 'react-router-dom';
import { FavoritesProvider } from './contexts/FavoritesContext';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import MobileLayout from './components/mobile/MobileLayout';
import Sidebar from './components/layout/Sidebar';
import TopBar, { type TopBarPeriod } from './components/layout/TopBar';
import NewsList from './components/lists/NewsList';
import AddSourceForm from './components/forms/AddSourceForm';
import SourcesList from './components/lists/SourcesList';
import EditSourceForm from './components/forms/EditSourceForm';
import FavoritesPage from './components/pages/FavoritesPage';
import LoginPage from './components/pages/LoginPage';
import RegisterPage from './components/pages/RegisterPage';
import ConfirmEmailPage from './components/pages/ConfirmEmailPage';
import ForgotPasswordPage from './components/pages/ForgotPasswordPage';
import ResetPasswordPage from './components/pages/ResetPasswordPage';
import ProfilePage from './components/pages/ProfilePage';
import { type PeriodFilter } from './services/newsService';
import './App.css';

const AUTH_ROUTES = ['/login', '/register', '/confirm-email', '/forgot-password', '/reset-password'] as const;

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { user, loading } = useAuth();
  if (loading) return <div className="flex h-screen items-center justify-center">Загрузка...</div>;
  return user ? <>{children}</> : <Navigate to="/login" />;
};

const DesktopAuthRoutes = () => (
  <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/register" element={<RegisterPage />} />
    <Route path="/confirm-email" element={<ConfirmEmailPage />} />
    <Route path="/forgot-password" element={<ForgotPasswordPage />} />
    <Route path="/reset-password" element={<ResetPasswordPage />} />
    <Route path="/" element={<Navigate to="/login" replace />} />
    <Route path="*" element={<Navigate to="/login" replace />} />
  </Routes>
);

function AppContent() {
  const [timeFilter, setTimeFilter] = useState<PeriodFilter>('' as PeriodFilter);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [sourceTypeFilter, setSourceTypeFilter] = useState<string | undefined>(undefined);
  const [appliedFilters, setAppliedFilters] = useState({
    sources: [] as number[],
    tags: [] as string[],
    period: '' as TopBarPeriod
  });
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const isAuthRoute = AUTH_ROUTES.includes(location.pathname as (typeof AUTH_ROUTES)[number]);

  const handlePeriodChange = (value: TopBarPeriod) => {
    setTimeFilter(value);
  };

  const handleLogoClick = () => {
    setSourceTypeFilter(undefined);
    navigate('/');
  };

  if (user && isAuthRoute) {
    return <Navigate to="/" replace />;
  }

  return (
    <>
      {/* Mobile Layout (< 768px) */}
      <div className="md:hidden">
        <MobileLayout />
      </div>

      {/* Desktop: гость без сессии → страницы входа/регистрации/ссылки из письма */}
      {!user && (
        <div className="hidden md:block fixed inset-0 z-50 overflow-hidden bg-white">
          <DesktopAuthRoutes />
        </div>
      )}

      {/* Desktop Layout (>= 768px), только для авторизованных */}
      {user && !isAuthRoute && (
        <div className="hidden md:flex min-h-screen items-center justify-center bg-slate-200 text-slate-900">
          <div className="h-[90vh] w-[95vw] max-w-6xl bg-white rounded-3xl shadow-2xl overflow-hidden flex">
            {user && (
              <Sidebar
                onSourceTypeChange={setSourceTypeFilter}
                onLogoClick={handleLogoClick}
              />
            )}
            <div className="flex-1 flex flex-col bg-slate-50 min-w-0">
              {user && (
                <TopBar
                  searchQuery={searchQuery}
                  onSearchChange={setSearchQuery}
                  period={timeFilter as TopBarPeriod}
                  onPeriodChange={handlePeriodChange}
                  onFiltersChange={setAppliedFilters}
                  sourceType={sourceTypeFilter}
                />
              )}
              <main className="flex-1 px-8 py-6 overflow-y-auto overflow-x-hidden min-h-0">
                <Routes>
                  <Route
                    path="/"
                    element={
                      <ProtectedRoute>
                        <NewsList
                          timeFilter={timeFilter}
                          searchQuery={searchQuery}
                          sourceType={sourceTypeFilter}
                          appliedFilters={appliedFilters}
                        />
                      </ProtectedRoute>
                    }
                  />
                  <Route path="/favorites" element={<ProtectedRoute><FavoritesPage /></ProtectedRoute>} />
                  <Route path="/add-source" element={<ProtectedRoute><AddSourceForm /></ProtectedRoute>} />
                  <Route path="/sources" element={<ProtectedRoute><SourcesList /></ProtectedRoute>} />
                  <Route path="/edit-source/:id" element={<ProtectedRoute><EditSourceForm /></ProtectedRoute>} />
                  <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
                  <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
              </main>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <FavoritesProvider>
          <AppContent />
        </FavoritesProvider>
      </ToastProvider>
    </AuthProvider>
  );
}

export default App;
