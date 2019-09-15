import Cookies from 'js-cookie'

const token = 'token'
const user_id = 'user_id'

export function getJwtToken() {
  return Cookies.get(token) || ''
}

export function setJwtToken(data) {
  return Cookies.set(token, data)
}

export function getUserId() {
  return Cookies.get(user_id) || ''
}

export function setUserId(data) {
  return Cookies.set(user_id, data)
}

export function removeToken() {
  Cookies.remove(token)
  Cookies.remove(user_id)
}
