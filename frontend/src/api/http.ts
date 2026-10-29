export const apiUrl = import.meta.env.VITE_API_URL ?? ''

export async function ensureOk(response: Response): Promise<Response> {
  if (response.ok) return response

  // The API returns RFC 9457 problem details on errors.
  let message = `Request failed with status ${response.status}`
  try {
    const problem = await response.json()
    const firstError = problem.errors && Object.values<string[]>(problem.errors)[0]?.[0]
    message = firstError ?? problem.detail ?? problem.title ?? message
  } catch {
    // Body is not JSON; keep the default message.
  }
  throw new Error(message)
}
