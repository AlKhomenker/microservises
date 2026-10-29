import { apiUrl, ensureOk } from './http'

export interface Todo {
  id: string
  text: string
  createDate: string
}

const baseUrl = `${apiUrl}/api/todos`

export async function getTodos(): Promise<Todo[]> {
  const response = await ensureOk(await fetch(baseUrl))
  return response.json()
}

export async function createTodo(text: string): Promise<Todo> {
  const response = await ensureOk(
    await fetch(baseUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text }),
    }),
  )
  return response.json()
}

export async function deleteTodo(id: string): Promise<void> {
  await ensureOk(await fetch(`${baseUrl}/${id}`, { method: 'DELETE' }))
}

export async function deleteAllTodos(): Promise<void> {
  await ensureOk(await fetch(baseUrl, { method: 'DELETE' }))
}
