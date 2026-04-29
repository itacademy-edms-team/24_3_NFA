import { useState } from 'react';
import { Routes, Route, useNavigate, Navigate } from 'react-router-dom';
import { FavoritesProvider } from './contexts/FavoritesContext';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import MobileLayout from './components/mobile/MobileLayout';
import Sidebar from './components/Sidebar';
import TopBar, { type TopBarPeriod } from './components/TopBar';
import NewsList from './components/NewsList';
import AddSourceForm from './components/AddSourceForm';
import SourcesList from './components/SourcesList';
import EditSourceForm from './components/EditSourceForm';
import FavoritesPage from './components/FavoritesPage';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import ProfilePage from './components/ProfilePage';
import { type PeriodFilter } from './services/newsService';
import './App.css';

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { user, loading } = useAuth();
  if (loading) return <div className="flex h-screen items-center justify-center">Загрузка...</div>;
  return user ? <>{children}</> : <Navigate to="/login" />;
};

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
  const { user } = useAuth();

  const handlePeriodChange = (value: TopBarPeriod) => {
    setTimeFilter(value);
  };

  const handleLogoClick = () => {
    setSourceTypeFilter(undefined);
    navigate('/');
  };

  return (
    <>
      {/* Mobile Layout (< 768px) */}
      <div className="md:hidden">
        <MobileLayout />
      </div>

      {/* Desktop Layout (>= 768px) */}
      <div className="hidden md:flex min-h-screen items-center justify-center bg-slate-200 text-slate-900">
        <div className="h-[90vh] w-[95vw] max-w-6xl bg-white rounded-3xl shadow-2xl overflow-hidden flex">
          {user && (
            <Sidebar
              onSourceTypeChange={setSourceTypeFilter}
              onLogoClick={handleLogoClick}
            />
          )}
          <div className="flex-1 flex flex-col bg-slate-50">
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
            <main className="flex-1 px-8 py-6 overflow-y-auto overflow-x-hidden">
              <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
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
              </Routes>
            </main>
          </div>
        </div>
      </div>
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
