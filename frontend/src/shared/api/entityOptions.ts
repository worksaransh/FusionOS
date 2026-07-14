import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from './client';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import type { PagedResult } from './types';
import type { EntityOption } from '../ui/EntityCombobox';

/**
 * Backs every EntityCombobox in the app (shared/ui/EntityCombobox.tsx) — one
 * hook per pickable entity type. Product, Supplier, Customer, Warehouse, and
 * Account now search server-side (their list endpoints accept a `search`
 * query param — 08_API_STANDARDS.md), debounced here so keystrokes don't
 * each fire a request; the returned `onSearchChange` is passed straight to
 * EntityCombobox. Zone, Sales Order, and Purchase Order list endpoints don't
 * have search support yet, so those three hooks still fetch one page and
 * fall back to EntityCombobox's client-side filtering (fine at their current
 * scale — a warehouse's zones, one company's orders — unlike the
 * company-wide Product/Customer/Supplier lists this was actually fixing).
 */
const OPTIONS_PAGE_SIZE = 20;
const SEARCH_DEBOUNCE_MS = 250;

function useEntityOptions<T>(
  queryKeyPrefix: string,
  basePath: string,
  companyId: string | undefined,
  mapToOption: (item: T) => EntityOption,
): { options: EntityOption[]; isLoading: boolean; onSearchChange: (search: string) => void } {
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search, SEARCH_DEBOUNCE_MS);
  const enabled = Boolean(companyId);

  const query = useQuery({
    queryKey: [queryKeyPrefix, companyId, debouncedSearch],
    queryFn: () => {
      const params = new URLSearchParams({ companyId: companyId!, page: '1', pageSize: String(OPTIONS_PAGE_SIZE) });
      if (debouncedSearch.trim()) params.set('search', debouncedSearch.trim());
      return apiClient.get<PagedResult<T>>(`${basePath}?${params.toString()}`);
    },
    enabled,
  });

  return {
    options: query.data?.data.map(mapToOption) ?? [],
    isLoading: query.isLoading,
    onSearchChange: setSearch,
  };
}

/** For entities whose list endpoint has no `search` param yet — one generous page, client-side filtered by EntityCombobox itself. */
function useEntityOptionsWithoutSearch<T>(
  queryKey: readonly unknown[],
  path: string,
  enabled: boolean,
  mapToOption: (item: T) => EntityOption,
): { options: EntityOption[]; isLoading: boolean } {
  const query = useQuery({
    queryKey,
    queryFn: () => apiClient.get<PagedResult<T>>(path),
    enabled,
  });

  return {
    options: query.data?.data.map(mapToOption) ?? [],
    isLoading: query.isLoading,
  };
}

interface ProductLite {
  id: string;
  sku: string;
  name: string;
}

export function useProductOptions(companyId: string | undefined) {
  return useEntityOptions<ProductLite>(
    'product-options',
    '/inventory/products',
    companyId,
    (p) => ({ id: p.id, label: `${p.sku} — ${p.name}` }),
  );
}

interface SupplierLite {
  id: string;
  code: string;
  name: string;
}

export function useSupplierOptions(companyId: string | undefined) {
  return useEntityOptions<SupplierLite>(
    'supplier-options',
    '/procurement/suppliers',
    companyId,
    (s) => ({ id: s.id, label: `${s.code} — ${s.name}` }),
  );
}

interface CustomerLite {
  id: string;
  code: string;
  name: string;
}

export function useCustomerOptions(companyId: string | undefined) {
  return useEntityOptions<CustomerLite>(
    'customer-options',
    '/sales/customers',
    companyId,
    (c) => ({ id: c.id, label: `${c.code} — ${c.name}` }),
  );
}

interface WarehouseLite {
  id: string;
  code: string;
  name: string;
}

export function useWarehouseOptions(companyId: string | undefined) {
  return useEntityOptions<WarehouseLite>(
    'warehouse-options',
    '/warehouse/warehouses',
    companyId,
    (w) => ({ id: w.id, label: `${w.code} — ${w.name}` }),
  );
}

interface ZoneLite {
  id: string;
  code: string;
  name: string;
}

const ZONE_OPTIONS_PAGE_SIZE = 200;

export function useZoneOptions(companyId: string | undefined, warehouseId: string | undefined) {
  return useEntityOptionsWithoutSearch<ZoneLite>(
    ['zone-options', companyId, warehouseId],
    `/warehouse/warehouses/${warehouseId}/zones?companyId=${companyId}&page=1&pageSize=${ZONE_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId && warehouseId),
    (z) => ({ id: z.id, label: `${z.code} — ${z.name}` }),
  );
}

interface AccountLite {
  id: string;
  code: string;
  name: string;
}

export function useAccountOptions(companyId: string | undefined) {
  return useEntityOptions<AccountLite>(
    'account-options',
    '/finance/accounts',
    companyId,
    (a) => ({ id: a.id, label: `${a.code} — ${a.name}` }),
  );
}

interface OrderLite {
  id: string;
  status: string;
  orderDate: string;
  totalAmount: number;
}

const ORDER_OPTIONS_PAGE_SIZE = 200;

export function useSalesOrderOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<OrderLite>(
    ['sales-order-options', companyId],
    `/sales/sales-orders?companyId=${companyId}&page=1&pageSize=${ORDER_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (o) => ({
      id: o.id,
      label: `${new Date(o.orderDate).toLocaleDateString()} · ${o.status} · ${o.totalAmount.toLocaleString()}`,
    }),
  );
}

export function usePurchaseOrderOptions(companyId: string | undefined) {
  return useEntityOptionsWithoutSearch<OrderLite>(
    ['purchase-order-options', companyId],
    `/procurement/purchase-orders?companyId=${companyId}&page=1&pageSize=${ORDER_OPTIONS_PAGE_SIZE}`,
    Boolean(companyId),
    (o) => ({
      id: o.id,
      label: `${new Date(o.orderDate).toLocaleDateString()} · ${o.status} · ${o.totalAmount.toLocaleString()}`,
    }),
  );
}
