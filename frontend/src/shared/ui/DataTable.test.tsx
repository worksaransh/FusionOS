import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { DataTable, type DataTableColumn } from './DataTable';

interface Row {
  id: string;
  name: string;
}

const columns: DataTableColumn<Row>[] = [
  { header: 'Name', render: (row) => row.name },
];

describe('DataTable', () => {
  it('renders one row per item using the column render function', () => {
    const rows: Row[] = [
      { id: '1', name: 'Alpha' },
      { id: '2', name: 'Beta' },
    ];
    render(<DataTable columns={columns} rows={rows} isLoading={false} emptyMessage="Nothing here." rowKey={(r) => r.id} />);

    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(3); // header row + 2 data rows
  });

  it('shows the empty message when rows is an empty array', () => {
    render(<DataTable columns={columns} rows={[]} isLoading={false} emptyMessage="Nothing here." rowKey={(r) => r.id} />);

    expect(screen.getByText('Nothing here.')).toBeInTheDocument();
  });

  it('shows a loading indicator and no table while isLoading is true and rows is undefined', () => {
    render(<DataTable columns={columns} rows={undefined} isLoading emptyMessage="Nothing here." rowKey={(r) => r.id} />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading');
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it('announces a fetch error via role=alert', () => {
    render(
      <DataTable
        columns={columns}
        rows={undefined}
        isLoading={false}
        isError
        errorMessage="Could not load rows."
        emptyMessage="Nothing here."
        rowKey={(r) => r.id}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Could not load rows.');
  });

  it('gives every header cell scope="col" for screen-reader table navigation', () => {
    render(<DataTable columns={columns} rows={[]} isLoading={false} emptyMessage="Nothing here." rowKey={(r) => r.id} />);

    expect(screen.getByRole('columnheader', { name: 'Name' })).toHaveAttribute('scope', 'col');
  });
});
