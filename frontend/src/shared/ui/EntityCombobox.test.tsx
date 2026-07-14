import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { EntityCombobox, type EntityOption } from './EntityCombobox';

const options: EntityOption[] = [
  { id: '1', label: 'Apple' },
  { id: '2', label: 'Banana' },
  { id: '3', label: 'Cherry' },
];

describe('EntityCombobox', () => {
  it('shows the selected option label when closed', () => {
    render(<EntityCombobox value="2" onChange={vi.fn()} options={options} />);
    expect(screen.getByRole('combobox')).toHaveValue('Banana');
  });

  it('filters client-side on typed input when no onSearchChange is provided', async () => {
    const user = userEvent.setup();
    render(<EntityCombobox value="" onChange={vi.fn()} options={options} />);

    await user.click(screen.getByRole('combobox'));
    await user.type(screen.getByRole('combobox'), 'ban');

    expect(screen.getByRole('option', { name: 'Banana' })).toBeInTheDocument();
    expect(screen.queryByRole('option', { name: 'Apple' })).not.toBeInTheDocument();
  });

  it('forwards every keystroke to onSearchChange instead of filtering locally when provided', async () => {
    const user = userEvent.setup();
    const onSearchChange = vi.fn();
    // Server-side search: `options` is already the caller's filtered result set,
    // so all three should stay visible even though "xyz" matches none of them —
    // narrowing that list is the caller's job now, not this component's.
    render(<EntityCombobox value="" onChange={vi.fn()} options={options} onSearchChange={onSearchChange} />);

    await user.click(screen.getByRole('combobox'));
    await user.type(screen.getByRole('combobox'), 'xyz');

    expect(onSearchChange).toHaveBeenCalledWith('xyz');
    expect(screen.getByRole('option', { name: 'Apple' })).toBeInTheDocument();
  });

  it('calls onChange and closes the popup when an option is clicked', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<EntityCombobox value="" onChange={onChange} options={options} />);

    await user.click(screen.getByRole('combobox'));
    await user.click(screen.getByRole('option', { name: 'Cherry' }));

    expect(onChange).toHaveBeenCalledWith('3');
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument();
  });

  it('supports Arrow Down + Enter to select without ever clicking an option', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<EntityCombobox value="" onChange={onChange} options={options} />);

    const input = screen.getByRole('combobox');
    await user.click(input);
    await user.keyboard('{ArrowDown}{ArrowDown}{Enter}');

    // First ArrowDown just opens+highlights index 0 (Apple); the popup was
    // already open from the click, so the two ArrowDowns move the highlight
    // from Apple (0) to Banana (1) before Enter commits it.
    expect(onChange).toHaveBeenCalledWith('2');
  });

  it('closes on Escape without selecting anything', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<EntityCombobox value="" onChange={onChange} options={options} />);

    await user.click(screen.getByRole('combobox'));
    expect(screen.getByRole('listbox')).toBeInTheDocument();

    await user.keyboard('{Escape}');

    expect(screen.queryByRole('listbox')).not.toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalled();
  });

  it('exposes an accessible "Clear selection" button that resets the value', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<EntityCombobox value="1" onChange={onChange} options={options} />);

    await user.click(screen.getByRole('button', { name: 'Clear selection' }));

    expect(onChange).toHaveBeenCalledWith('');
  });

  it('sets aria-expanded and aria-activedescendant to reflect combobox state', async () => {
    const user = userEvent.setup();
    render(<EntityCombobox value="" onChange={vi.fn()} options={options} />);

    const input = screen.getByRole('combobox');
    expect(input).toHaveAttribute('aria-expanded', 'false');

    await user.click(input);
    expect(input).toHaveAttribute('aria-expanded', 'true');
    expect(input.getAttribute('aria-activedescendant')).toMatch(/-option-0$/);
  });
});
