import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import type { Todo } from './api/todos'

// The temporary todos section has its own tests and its own requests.
vi.mock('./TemporaryTodoList', () => ({ default: () => null }))

const existing: Todo = { id: '1', text: 'Existing todo', createDate: '2026-10-21T08:00:00+00:00' }

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } })
}

describe('App', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  it('shows todos from the API', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([existing]))

    render(<App />)

    expect(await screen.findByText('Existing todo')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith('/api/todos')
  })

  it('shows an empty state', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([]))

    render(<App />)

    expect(await screen.findByText('No todos yet.')).toBeInTheDocument()
  })

  it('adds a new todo to the top of the list', async () => {
    const created: Todo = { id: '2', text: 'New todo', createDate: '2026-10-21T09:00:00+00:00' }
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(jsonResponse(created, 201))

    render(<App />)
    await screen.findByText('Existing todo')

    await userEvent.type(screen.getByLabelText('New todo'), 'New todo')
    await userEvent.click(screen.getByRole('button', { name: 'Add' }))

    const items = await screen.findAllByRole('listitem')
    expect(items[0]).toHaveTextContent('New todo')
    expect(screen.getByLabelText('New todo')).toHaveValue('')
    expect(fetchMock).toHaveBeenLastCalledWith(
      '/api/todos',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ text: 'New todo' }) }),
    )
  })

  it('deletes a todo', async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(new Response(null, { status: 204 }))

    render(<App />)
    await userEvent.click(await screen.findByRole('button', { name: 'Delete "Existing todo"' }))

    expect(await screen.findByText('No todos yet.')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/todos/1', { method: 'DELETE' })
  })

  it('removes all todos after confirmation', async () => {
    vi.stubGlobal('confirm', vi.fn(() => true))
    fetchMock.mockResolvedValueOnce(jsonResponse([existing])).mockResolvedValueOnce(new Response(null, { status: 204 }))

    render(<App />)
    await userEvent.click(await screen.findByRole('button', { name: 'Remove all' }))

    expect(await screen.findByText('No todos yet.')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenLastCalledWith('/api/todos', { method: 'DELETE' })
  })

  it('keeps todos when remove all is cancelled', async () => {
    vi.stubGlobal('confirm', vi.fn(() => false))
    fetchMock.mockResolvedValueOnce(jsonResponse([existing]))

    render(<App />)
    await userEvent.click(await screen.findByRole('button', { name: 'Remove all' }))

    expect(screen.getByText('Existing todo')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('shows the validation message from the API', async () => {
    fetchMock
      .mockResolvedValueOnce(jsonResponse([]))
      .mockResolvedValueOnce(jsonResponse({ title: 'Validation', errors: { text: ['Text is too long.'] } }, 400))

    render(<App />)
    await screen.findByText('No todos yet.')

    await userEvent.type(screen.getByLabelText('New todo'), 'x')
    await userEvent.click(screen.getByRole('button', { name: 'Add' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Text is too long.')
  })
})
