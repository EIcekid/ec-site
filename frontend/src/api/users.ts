import http from './http'
import type { Me } from '../types'

export const usersApi = {
  me() {
    return http.get<Me>('/users/me').then((r) => r.data)
  },
}
