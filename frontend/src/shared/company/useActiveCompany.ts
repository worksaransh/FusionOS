import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Every Phase 1 command/query is company-scoped (04_DATABASE_GUIDELINES.md §6).
 * The signed-in session's company_id claim (07_SECURITY.md §1) now pre-fills
 * this on login (see LoginPage/AppShell), but it stays a separate, editable
 * store rather than reading straight off the JWT — useful for a user who
 * belongs to more than one company, and kept every Inventory/Warehouse/
 * Procurement/Sales/Finance page unchanged when Auth was added.
 */
interface ActiveCompanyState {
  companyId: string;
  setCompanyId: (id: string) => void;
}

export const useActiveCompany = create<ActiveCompanyState>()(
  persist(
    (set) => ({
      companyId: '',
      setCompanyId: (id) => set({ companyId: id }),
    }),
    { name: 'fusionos.active-company' },
  ),
);
