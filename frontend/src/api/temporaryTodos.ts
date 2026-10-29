import { apiUrl, ensureOk } from './http'

export type TemporaryTodoStatus = 'active' | 'expired'

export interface TemporaryTodo {
  id: string
  text: string
  /** Date only, `yyyy-MM-dd`. */
  startDate: string
  status: TemporaryTodoStatus
}

const baseUrl = `${apiUrl}/api/temporary-todos`

export async function getTemporaryTodos(): Promise<TemporaryTodo[]> {
  const response = await ensureOk(await fetch(baseUrl))
  return response.json()
}

export async function createTemporaryTodo(text: string, startDate: string): Promise<TemporaryTodo> {
  const response = await ensureOk(
    await fetch(baseUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text, startDate }),
    }),
  )
  return response.json()
}

export async function deleteTemporaryTodo(id: string): Promise<void> {
  await ensureOk(await fetch(`${baseUrl}/${id}`, { method: 'DELETE' }))
}

export async function deleteAllTemporaryTodos(): Promise<void> {
  await ensureOk(await fetch(baseUrl, { method: 'DELETE' }))
}
