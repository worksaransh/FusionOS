import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { PublicHeader } from './PublicHeader';
import { PublicFooter } from './PublicFooter';
import { DemoRequestModal } from './DemoRequestModal';

export function PublicLayout() {
  const [isDemoOpen, setIsDemoOpen] = useState(false);

  return (
    <div className="min-h-screen flex flex-col bg-surface-muted text-text font-sans antialiased">
      <PublicHeader onOpenDemo={() => setIsDemoOpen(true)} />
      <main className="flex-1">
        <Outlet context={{ openDemoModal: () => setIsDemoOpen(true) }} />
      </main>
      <PublicFooter />
      <DemoRequestModal isOpen={isDemoOpen} onClose={() => setIsDemoOpen(false)} />
    </div>
  );
}
