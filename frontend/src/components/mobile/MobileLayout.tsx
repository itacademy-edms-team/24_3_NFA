import React from 'react';
import { Routes, Route } from 'react-router-dom';
import MobileBottomNav from './MobileBottomNav';
import MobileMain from './MobileMain';
import MobileCollections from './MobileCollections';
import MobileProfile from './MobileProfile';
import MobileSettings from './MobileSettings';

const MobileLayout: React.FC = () => {
  return (
    <div className="min-h-screen bg-[#F5F5F7]">
      <main className="pb-[64px]">
        <Routes>
          <Route path="/" element={<MobileMain />} />
          <Route path="/collections" element={<MobileCollections />} />
          <Route path="/profile" element={<MobileProfile />} />
          <Route path="/settings" element={<MobileSettings />} />
        </Routes>
      </main>
      <MobileBottomNav />
    </div>
  );
};

export default MobileLayout;
