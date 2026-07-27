import { useState } from 'react';
import { Mail, Phone, MapPin, Send, CheckCircle2 } from 'lucide-react';
import { Button } from '../../shared/ui/Button';

export function ContactPage() {
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitted(true);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12 space-y-16">
      <div className="text-center max-w-3xl mx-auto space-y-4">
        <span className="rounded-full bg-primary/10 px-3.5 py-1 text-xs font-bold text-primary uppercase tracking-wider">
          Get in Touch
        </span>
        <h1 className="text-4xl font-extrabold tracking-tight text-text sm:text-5xl">
          Contact FusionOS Sales & Support
        </h1>
        <p className="text-base text-text-muted">
          Our team of enterprise solutions specialists is available to discuss custom deployments, platform migration, and demos.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-12 lg:grid-cols-12">
        <div className="lg:col-span-5 space-y-6">
          <div className="rounded-2xl border border-border bg-surface p-6 space-y-4">
            <h3 className="text-lg font-bold text-text">Global Headquarters</h3>
            <div className="space-y-3 text-xs text-text-muted">
              <p className="flex items-center gap-2">
                <MapPin size={16} className="text-primary" /> 100 Enterprise Way, Suite 400, San Francisco, CA
              </p>
              <p className="flex items-center gap-2">
                <Mail size={16} className="text-primary" /> contact@fusionos.io
              </p>
              <p className="flex items-center gap-2">
                <Phone size={16} className="text-primary" /> +1 (800) 555-FUSION
              </p>
            </div>
          </div>
        </div>

        <div className="lg:col-span-7 rounded-2xl border border-border bg-surface p-8 shadow-xs">
          {submitted ? (
            <div className="py-12 text-center space-y-3">
              <CheckCircle2 size={48} className="text-emerald-500 mx-auto" />
              <h3 className="text-xl font-bold text-text">Message Sent Successfully</h3>
              <p className="text-xs text-text-muted">Our enterprise support team will respond to your inquiry within 24 hours.</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <label className="flex flex-col gap-1 font-medium text-text">
                  Your Name *
                  <input required type="text" placeholder="John Smith" className="rounded-lg border border-border bg-surface p-2.5 text-sm" />
                </label>
                <label className="flex flex-col gap-1 font-medium text-text">
                  Work Email *
                  <input required type="email" placeholder="john@company.com" className="rounded-lg border border-border bg-surface p-2.5 text-sm" />
                </label>
              </div>
              <label className="flex flex-col gap-1 font-medium text-text">
                Subject
                <input type="text" placeholder="Enterprise ERP Query" className="rounded-lg border border-border bg-surface p-2.5 text-sm" />
              </label>
              <label className="flex flex-col gap-1 font-medium text-text">
                Message *
                <textarea required rows={4} placeholder="Describe your business needs..." className="rounded-lg border border-border bg-surface p-2.5 text-sm" />
              </label>
              <Button type="submit" className="w-full py-3 text-sm font-semibold flex items-center justify-center gap-2">
                <Send size={16} /> Send Message
              </Button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
