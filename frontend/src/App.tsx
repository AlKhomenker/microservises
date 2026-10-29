import { useEffect, useState, type FormEvent } from 'react'
import { createTodo, deleteAllTodos, deleteTodo, getTodos, type Todo } from './api/todos'
import TemporaryTodoList from './TemporaryTodoList'
import './App.css'

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' })

function App() {
  const [todos, setTodos] = useState<Todo[]>([])
  const [text, setText] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getTodos()
      .then(setTodos)
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false))
  }, [])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!text.trim()) return

    setSaving(true)
    setError(null)
    try {
      const created = await createTodo(text)
      setTodos((current) => [created, ...current])
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
      await deleteTodo(id)
      setTodos((current) => current.filter((todo) => todo.id !== id))
    } catch (e) {
      setError((e as Error).message)
    }
  }

  async function handleDeleteAll() {
    if (!window.confirm('Remove all todos?')) return

    setError(null)
    try {
      await deleteAllTodos()
      setTodos([])
    } catch (e) {
      setError((e as Error).message)
    }
  }

  return (
    <main className="app">
      <h1>Todo list</h1>

      <form className="add-form" onSubmit={handleSubmit}>
        <input
          aria-label="New todo"
          placeholder="What needs to be done?"
          value={text}
          maxLength={500}
          onChange={(e) => setText(e.target.value)}
        />
        <button type="submit" disabled={saving || !text.trim()}>
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
        <p className="empty">No todos yet.</p>
      ) : (
        <>
          <button type="button" className="delete delete-all" onClick={handleDeleteAll}>
            Remove all
          </button>
          <ul className="todo-list">
            {todos.map((todo) => (
              <li key={todo.id}>
                <div>
                  <span className="text">{todo.text}</span>
                  <time dateTime={todo.createDate}>{dateFormat.format(new Date(todo.createDate))}</time>
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

      <TemporaryTodoList />
    </main>
  )
}

export default App
