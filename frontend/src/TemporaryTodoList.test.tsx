import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import TemporaryTodoList from './TemporaryTodoList'
import type { TemporaryTodo } from './api/temporaryTodos'

const existing: TemporaryTodo = { id: '1', text: 'Existing temporary', startDate: '2026-10-20', status: 'expired' }

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

describe('TemporaryTodoList', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  it('shows temporary todos with their status', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([existing]))

    render(<TemporaryTodoList />)

    expect(await screen.findByText('Existing temporary')).toBeInTheDocument()
    expect(screen.getByText('Expired')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith('/api/temporary-todos')
  })

  it('shows an empty state', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([]))

    render(<TemporaryTodoList />)

    expect(await screen.findByText('No temporary todos yet.')).toBeInTheDocument()
  })

  it('adds a temporary todo with the chosen start date, latest date first', async () => {
    const created: TemporaryTodo = { id: '2', text: 'New temporary', startDate: '2026-10-25', status: 'active' }
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(jsonResponse(created, 201))

    render(<TemporaryTodoList />)
    await screen.findByText('Existing temporary')

    await userEvent.type(screen.getByLabelText('New temporary todo'), 'New temporary')
    const dateInput = screen.getByLabelText('Start date')
    await userEvent.clear(dateInput)
    await userEvent.type(dateInput, '2026-10-25')
    await userEvent.click(screen.getByRole('button', { name: 'Add' }))

    const items = await screen.findAllByRole('listitem')
    expect(items[0]).toHaveTextContent('New temporary')
    expect(items[0]).toHaveTextContent('Active')
    expect(screen.getByLabelText('New temporary todo')).toHaveValue('')
    expect(fetchMock).toHaveBeenLastCalledWith(
      '/api/temporary-todos',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ text: 'New temporary', startDate: '2026-10-25' }),
      }),
    )
  })

  it('deletes a temporary todo', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(new Response(null, { status: 204 }))

    render(<TemporaryTodoList />)
    await userEvent.click(await screen.findByRole('button', { name: 'Delete "Existing temporary"' }))

    expect(await screen.findByText('No temporary todos yet.')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/temporary-todos/1', { method: 'DELETE' })
  })

  it('removes all temporary todos after confirmation', async () => {
    vi.stubGlobal('confirm', vi.fn(() => true))
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(new Response(null, { status: 204 }))

    render(<TemporaryTodoList />)
    await userEvent.click(await screen.findByRole('button', { name: 'Remove all temporary todos' }))

    expect(await screen.findByText('No temporary todos yet.')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/temporary-todos', { method: 'DELETE' })
  })

  it('shows the validation message from the API', async () => {
    fetchMock
      .mockResolvedValueOnce(jsonResponse([]))
      .mockResolvedValueOnce(jsonResponse({ title: 'Validation', errors: { startDate: ['Start date is required.'] } }, 400))

    render(<TemporaryTodoList />)
    await screen.findByText('No temporary todos yet.')

    await userEvent.type(screen.getByLabelText('New temporary todo'), 'x')
    await userEvent.click(screen.getByRole('button', { name: 'Add' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Start date is required.')
  })
})
