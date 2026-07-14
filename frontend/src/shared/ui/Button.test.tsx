import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Button } from './Button';

describe('Button', () => {
  it('renders its children and defaults to the primary variant', () => {
    render(<Button>Save</Button>);
    const button = screen.getByRole('button', { name: 'Save' });
    expect(button).toBeInTheDocument();
    expect(button.className).toContain('bg-primary');
  });

  it('applies the requested variant class', () => {
    render(<Button variant="danger">Delete</Button>);
    expect(screen.getByRole('button', { name: 'Delete' }).className).toContain('bg-danger');
  });

  it('fires onClick when enabled', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Click me</Button>);

    await user.click(screen.getByRole('button', { name: 'Click me' }));

    expect(onClick).toHaveBeenCalledOnce();
  });

  it('does not fire onClick when disabled', async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(
      <Button disabled onClick={onClick}>
        Click me
      </Button>,
    );

    await user.click(screen.getByRole('button', { name: 'Click me' }));

    expect(onClick).not.toHaveBeenCalled();
  });
});
