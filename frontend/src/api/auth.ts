import http from './http'
import type { AuthResponse } from '../types'

export const authApi = {
  register(email: string, password: string, name: string) {
    return http.post<AuthResponse>('/auth/register', { email, password, name }).then((r) => r.data)
  },
  login(email: string, password: string) {
    return http.post<AuthResponse>('/auth/login', { email, password }).then((r) => r.data)
  },
}
