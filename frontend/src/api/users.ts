import http from './http'
import type { Me } from '../types'

export const usersApi = {
  me() {
    return http.get<Me>('/users/me').then((r) => r.data)
  },
  updateProfile(name: string) {
    return http.put<Me>('/users/me', { name }).then((r) => r.data)
  },
  changePassword(currentPassword: string, newPassword: string) {
    return http.post('/users/me/change-password', { currentPassword, newPassword })
  },
}
