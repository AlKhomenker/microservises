import { useEffect, useState, type FormEvent } from 'react'
import {
  createTemporaryTodo,
  deleteAllTemporaryTodos,
  deleteTemporaryTodo,
  getTemporaryTodos,
  type TemporaryTodo,
} from './api/temporaryTodos'

// Start dates have no time zone, so format them as UTC to avoid showing the previous day.
const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeZone: 'UTC' })

const statusLabels = { active: 'Active', expired: 'Expired' } as const

function todayIso(): string {
  const now = new Date()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${now.getFullYear()}-${month}-${day}`
}

function TemporaryTodoList() {
  const [todos, setTodos] = useState<TemporaryTodo[]>([])
  const [text, setText] = useState('')
  const [startDate, setStartDate] = useState(todayIso)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getTemporaryTodos()
      .then(setTodos)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!text.trim() || !startDate) return

    setSaving(true)
    setError(null)
    try {
      const created = await createTemporaryTodo(text, startDate)
      // Keep the server order: latest start date first.
      setTodos((current) =>
        [created, ...current].sort((a, b) => b.startDate.localeCompare(a.startDate)),
      )
      setText('')
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id: string) {
    setError(null)
    try {
      await deleteTemporaryTodo(id)
      setTodos((current) => current.filter((todo) => todo.id !== id))
    } catch (e) {
      setError((e as Error).message)
    }
  }

  async function handleDeleteAll() {
    if (!window.confirm('Remove all temporary todos?')) return

    setError(null)
    try {
      await deleteAllTemporaryTodos()
      setTodos([])
    } catch (e) {
      setError((e as Error).message)
    }
  }

  return (
    <section className="temporary-todos" aria-labelledby="temporary-todos-title">
      <h2 id="temporary-todos-title">Temporary todos</h2>

      <form className="add-form" onSubmit={handleSubmit}>
        <input
          aria-label="New temporary todo"
          placeholder="What needs to be done that day?"
          value={text}
          maxLength={500}
          onChange={(e) => setText(e.target.value)}
        />
        <input
          type="date"
          aria-label="Start date"
          value={startDate}
          required
          onChange={(e) => setStartDate(e.target.value)}
        />
        <button type="submit" disabled={saving || !text.trim() || !startDate}>
          Add
        </button>
      </form>

      {error && (
        <p role="alert" className="error">
          {error}
        </p>
      )}

      {loading ? (
        <p>Loading…</p>
      ) : todos.length === 0 ? (
        <p className="empty">No temporary todos yet.</p>
      ) : (
        <>
          <button
            type="button"
            className="delete delete-all"
            aria-label="Remove all temporary todos"
            onClick={handleDeleteAll}
          >
            Remove all
          </button>
          <ul className="todo-list">
            {todos.map((todo) => (
              <li key={todo.id} className={todo.status}>
                <div>
                  <span className="text">{todo.text}</span>
                  <time dateTime={todo.startDate}>{dateFormat.format(new Date(todo.startDate))}</time>
                  <span className={`status status-${todo.status}`}>{statusLabels[todo.status]}</span>
                </div>
                <button
                  type="button"
                  className="delete"
                  aria-label={`Delete "${todo.text}"`}
                  onClick={() => handleDelete(todo.id)}
                >
                  Delete
                </button>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  )
}

export default TemporaryTodoList
