import { useState } from 'react';
import { Routes, Route, useNavigate } from 'react-router-dom';
import { FavoritesProvider } from './contexts/FavoritesContext';
import MobileLayout from './components/mobile/MobileLayout';
import Sidebar from './components/Sidebar';
import TopBar, { type TopBarPeriod } from './components/TopBar';
import NewsList from './components/NewsList';
import AddSourceForm from './components/AddSourceForm';
import SourcesList from './components/SourcesList';
import EditSourceForm from './components/EditSourceForm';
import FavoritesPage from './components/FavoritesPage';
import { type PeriodFilter } from './services/newsService';
import './App.css';

function App() {
  const [timeFilter, setTimeFilter] = useState<PeriodFilter>('' as PeriodFilter);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [sourceTypeFilter, setSourceTypeFilter] = useState<string | undefined>(undefined);
  const [appliedFilters, setAppliedFilters] = useState({
    sources: [] as number[],
    categories: [] as string[],
    period: '' as TopBarPeriod
  });
  const navigate = useNavigate();

  const handlePeriodChange = (value: TopBarPeriod) => {
    setTimeFilter(value);
  };

  const handleLogoClick = () => {
    setSourceTypeFilter(undefined);
    navigate('/');
  };

  return (
    <FavoritesProvider>
      {/* Mobile Layout (< 768px) */}
      <div className="md:hidden">
        <MobileLayout />
      </div>

      {/* Desktop Layout (>= 768px) */}
      <div className="hidden md:flex min-h-screen items-center justify-center bg-slate-200 text-slate-900">
        <div className="h-[90vh] w-[95vw] max-w-6xl bg-white rounded-3xl shadow-2xl overflow-hidden flex">
          <Sidebar
            onSourceTypeChange={setSourceTypeFilter}
            onLogoClick={handleLogoClick}
          />
          <div className="flex-1 flex flex-col bg-slate-50">
            <TopBar
              searchQuery={searchQuery}
              onSearchChange={setSearchQuery}
              period={timeFilter as TopBarPeriod}
              onPeriodChange={handlePeriodChange}
              onFiltersChange={setAppliedFilters}
            />
            <main className="flex-1 px-8 py-6 overflow-y-auto overflow-x-hidden">
              <Routes>
                <Route
                  path="/"
                  element={
                    <NewsList
                      timeFilter={timeFilter}
                      searchQuery={searchQuery}
                      sourceType={sourceTypeFilter}
                      appliedFilters={appliedFilters}
                    />
                  }
                />
                <Route path="/favorites" element={<FavoritesPage />} />
                <Route path="/add-source" element={<AddSourceForm />} />
                <Route path="/sources" element={<SourcesList />} />
                <Route path="/edit-source/:id" element={<EditSourceForm />} />
              </Routes>
            </main>
          </div>
        </div>
      </div>
    </FavoritesProvider>
  );
}

export default App;
