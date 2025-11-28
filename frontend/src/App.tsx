import { useState } from 'react';
import { Routes, Route } from 'react-router-dom';
import Sidebar from './components/Sidebar';
import TopBar, { type TopBarPeriod } from './components/TopBar';
import NewsList from './components/NewsList';
import AddSourceForm from './components/AddSourceForm';
import { type PeriodFilter } from './services/newsService';
import './App.css';

function App() {
  const [timeFilter, setTimeFilter] = useState<PeriodFilter>('week');
  const [searchQuery, setSearchQuery] = useState<string>('');

  const handlePeriodChange = (value: TopBarPeriod) => {
    setTimeFilter(value);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-200 text-slate-900">
      <div className="h-[90vh] w-[95vw] max-w-6xl bg-white rounded-3xl shadow-2xl overflow-hidden flex">
        <Sidebar />
        <div className="flex-1 flex flex-col bg-slate-50">
          <TopBar
            searchQuery={searchQuery}
            onSearchChange={setSearchQuery}
            period={(timeFilter as TopBarPeriod) ?? 'week'}
            onPeriodChange={handlePeriodChange}
          />
          <main className="flex-1 px-8 py-6 overflow-y-auto">
            <Routes>
              <Route path="/" element={<NewsList timeFilter={timeFilter} searchQuery={searchQuery} />} />
              <Route path="/add-source" element={<AddSourceForm />} />
            </Routes>
          </main>
        </div>
      </div>
    </div>
  );
}

export default App;
