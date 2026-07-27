import { Sparkles, Heart, Rocket } from 'lucide-react';

export function CareersPage() {
  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-purple-500/10 px-3.5 py-1 text-xs font-bold text-purple-600 dark:text-purple-400 uppercase tracking-wider">
          Join Our Mission
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Build the Future of Enterprise Software
        </h1>
        <p className="text-base text-text-muted">
          We are looking for ambitious engineers, product designers, and solution architects to shape the next era of business operating systems.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Rocket className="text-primary" size={28} />
          <h3 className="text-lg font-bold text-text">High-Impact Engineering</h3>
          <p className="text-xs text-text-muted">Work on real-time event-driven systems powering global supply chains.</p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Heart className="text-rose-500" size={28} />
          <h3 className="text-lg font-bold text-text">Remote-First Culture</h3>
          <p className="text-xs text-text-muted">Flexible work hours, competitive equity, and health coverage.</p>
        </div>
        <div className="rounded-2xl border border-border bg-surface p-6 space-y-3">
          <Sparkles className="text-purple-600" size={28} />
          <h3 className="text-lg font-bold text-text">AI Innovation</h3>
          <p className="text-xs text-text-muted">Directly contribute to cutting-edge AI models for enterprise optimization.</p>
        </div>
      </div>
    </div>
  );
}
