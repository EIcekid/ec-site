import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthResponse } from '../types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const email = ref<string | null>(localStorage.getItem('email'))
  const name = ref<string | null>(localStorage.getItem('name'))
  const role = ref<string | null>(localStorage.getItem('role'))

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => role.value === 'Admin')

  function setAuth(auth: AuthResponse) {
    token.value = auth.token
    email.value = auth.email
    name.value = auth.name
    role.value = auth.role
    localStorage.setItem('token', auth.token)
    localStorage.setItem('email', auth.email)
    localStorage.setItem('name', auth.name)
    localStorage.setItem('role', auth.role)
  }

  function logout() {
    token.value = null
    email.value = null
    name.value = null
    role.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('email')
    localStorage.removeItem('name')
    localStorage.removeItem('role')
  }

  return { token, email, name, role, isLoggedIn, isAdmin, setAuth, logout }
})
