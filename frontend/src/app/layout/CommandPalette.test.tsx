import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CommandPalette } from './CommandPalette';
import type { ModuleNavEntry } from '../modules';
import { apiClient } from '../../shared/api/client';
import { useActiveCompany } from '../../shared/company/useActiveCompany';

// The palette fans out real apiClient.get calls once a query is 2+ characters
// and a company is active — mocked here so every test (old and new) runs
// against controlled responses instead of a real network call.
vi.mock('../../shared/api/client', () => ({
  apiClient: { get: vi.fn() },
}));

const modules: ModuleNavEntry[] = [
  { name: 'sales', label: 'Sales', schema: 'sales', phase: 'Phase 1', implemented: true },
  { name: 'finance', label: 'Finance', schema: 'finance', phase: 'Phase 2', implemented: true },
  { name: 'crm', label: 'CRM', schema: 'crm', phase: 'Phase 4', implemented: true },
  { name: 'inventory', label: 'Inventory', schema: 'inventory', phase: 'Phase 1', implemented: true },
];

function renderPalette(open: boolean, onClose: () => void) {
  return render(
    <MemoryRouter initialEntries={['/dashboard']}>
      <CommandPalette open={open} onClose={onClose} modules={modules} />
      <Routes>
        <Route path="/dashboard" element={<p>Dashboard page</p>} />
        <Route path="/finance" element={<p>Finance page</p>} />
        <Route path="/inventory" element={<p>Inventory page</p>} />
      </Routes>
    </MemoryRouter>,
  );
}

const mockGet = vi.mocked(apiClient.get);

beforeEach(() => {
  mockGet.mockReset();
  // Default: every endpoint resolves to an empty page unless a test
  // overrides it — keeps the pre-existing module-only tests below from
  // depending on the fan-out feature at all (no company is active, so no
  // fetch fires for them either way).
  mockGet.mockResolvedValue({ data: [], page: 1, pageSize: 5, totalCount: 0 });
});

afterEach(() => {
  // Reset the shared zustand store back to its default so a companyId set by
  // one test can't leak into the next.
  useActiveCompany.setState({ companyId: '' });
});

describe('CommandPalette', () => {
  it('renders nothing when closed', () => {
    renderPalette(false, vi.fn());
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('lists every module it was given when opened', () => {
    renderPalette(true, vi.fn());

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Sales' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'Finance' })).toBeInTheDocument();
    expect(screen.getByRole('option', { name: 'CRM' })).toBeInTheDocument();
  });

  it('filters the module list as the user types', async () => {
    const user = userEvent.setup();
    renderPalette(true, vi.fn());

    await user.type(screen.getByRole('combobox'), 'fin');

    expect(screen.getByRole('option', { name: 'Finance' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Sales' })).not.toBeInTheDocument();
  });

  it('navigates to the highlighted module and closes on Enter', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderPalette(true, onClose);

    await user.type(screen.getByRole('combobox'), 'fin');
    await user.keyboard('{Enter}');

    expect(await screen.findByText('Finance page')).toBeInTheDocument();
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('moves the highlight with ArrowDown/ArrowUp and navigates to the highlighted option', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderPalette(true, onClose);

    // Sales(0) -> Finance(1) -> CRM(2) -> back to Finance(1)
    await user.keyboard('{ArrowDown}{ArrowDown}{ArrowUp}{Enter}');

    expect(await screen.findByText('Finance page')).toBeInTheDocument();
    expect(onClose).toHaveBeenCalledOnce();
  });

  it('closes without navigating on Escape', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderPalette(true, onClose);

    await user.keyboard('{Escape}');

    expect(onClose).toHaveBeenCalledOnce();
    expect(screen.queryByText('Finance page')).not.toBeInTheDocument();
  });

  it('shows a "no matching modules" message when the query matches nothing', async () => {
    const user = userEvent.setup();
    renderPalette(true, vi.fn());

    await user.type(screen.getByRole('combobox'), 'zzz');

    expect(screen.getByText('No matching modules.')).toBeInTheDocument();
  });

  describe('entity search fan-out', () => {
    beforeEach(() => {
      useActiveCompany.setState({ companyId: 'company-1' });
    });

    it('does not call apiClient below the 2-character threshold', async () => {
      const user = userEvent.setup();
      renderPalette(true, vi.fn());

      await user.type(screen.getByRole('combobox'), 'w');

      expect(mockGet).not.toHaveBeenCalled();
    });

    it('does not fan out when no company is active, even at 2+ characters', async () => {
      useActiveCompany.setState({ companyId: '' });
      const user = userEvent.setup();
      renderPalette(true, vi.fn());

      await user.type(screen.getByRole('combobox'), 'widget');
      await new Promise((resolve) => setTimeout(resolve, 350));

      expect(mockGet).not.toHaveBeenCalled();
    });

    it('fans out to RBAC-visible entity endpoints and renders grouped, clickable results', async () => {
      mockGet.mockImplementation((path: string) => {
        if (path.startsWith('/inventory/products')) {
          return Promise.resolve({
            data: [{ id: 'p1', sku: 'SKU-1', name: 'Widget' }],
            page: 1,
            pageSize: 5,
            totalCount: 1,
          });
        }
        return Promise.resolve({ data: [], page: 1, pageSize: 5, totalCount: 0 });
      });

      const user = userEvent.setup();
      const onClose = vi.fn();
      renderPalette(true, onClose);

      await user.type(screen.getByRole('combobox'), 'widget');

      expect(await screen.findByText('Products (1)')).toBeInTheDocument();
      const productOption = screen.getByRole('option', { name: 'SKU-1 — Widget' });
      expect(productOption).toBeInTheDocument();

      // Confirms the exact query-string contract (companyId/search/page/pageSize)
      // every curated endpoint is called with.
      expect(mockGet).toHaveBeenCalledWith(
        '/inventory/products?companyId=company-1&search=widget&page=1&pageSize=5',
      );

      await user.click(productOption);

      expect(await screen.findByText('Inventory page')).toBeInTheDocument();
      expect(onClose).toHaveBeenCalledOnce();
    });

    it('never fans out to a module the caller did not include in the RBAC-filtered `modules` prop', async () => {
      const user = userEvent.setup();
      renderPalette(true, vi.fn());

      await user.type(screen.getByRole('combobox'), 'widget');
      await waitFor(() => expect(mockGet).toHaveBeenCalled());

      // `modules` (module-level test fixture) never includes maintenance/hrms/
      // procurement/manufacturing/warehouse, so those endpoints must never be hit.
      expect(mockGet).not.toHaveBeenCalledWith(expect.stringContaining('/maintenance/assets'));
      expect(mockGet).not.toHaveBeenCalledWith(expect.stringContaining('/hrms/employees'));
    });

    it('keeps showing one group\'s results when another group\'s request rejects', async () => {
      mockGet.mockImplementation((path: string) => {
        if (path.startsWith('/inventory/products')) {
          return Promise.resolve({
            data: [{ id: 'p1', sku: 'SKU-1', name: 'Widget' }],
            page: 1,
            pageSize: 5,
            totalCount: 1,
          });
        }
        if (path.startsWith('/sales/customers')) {
          return Promise.reject(new Error('boom'));
        }
        return Promise.resolve({ data: [], page: 1, pageSize: 5, totalCount: 0 });
      });

      const user = userEvent.setup();
      renderPalette(true, vi.fn());

      await user.type(screen.getByRole('combobox'), 'widget');

      expect(await screen.findByText('Products (1)')).toBeInTheDocument();
      expect(await screen.findByText('Customers search failed.')).toBeInTheDocument();
    });

    it('lets Arrow keys reach into the entity results and Enter navigate to the matched module', async () => {
      mockGet.mockImplementation((path: string) => {
        if (path.startsWith('/inventory/products')) {
          return Promise.resolve({
            data: [{ id: 'p1', sku: 'SKU-1', name: 'Widget' }],
            page: 1,
            pageSize: 5,
            totalCount: 1,
          });
        }
        return Promise.resolve({ data: [], page: 1, pageSize: 5, totalCount: 0 });
      });

      const user = userEvent.setup();
      const onClose = vi.fn();
      renderPalette(true, onClose);

      // "widget" matches no module label/name, so the module section is empty
      // ("No matching modules.") and the only selectable row is the Product hit.
      await user.type(screen.getByRole('combobox'), 'widget');
      await screen.findByText('Products (1)');

      await user.keyboard('{ArrowDown}{Enter}');

      expect(await screen.findByText('Inventory page')).toBeInTheDocument();
      expect(onClose).toHaveBeenCalledOnce();
    });
  });
});
