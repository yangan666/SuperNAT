import axios from 'axios'
import {
  getToken
} from '@/utils/auth'

// create an axios instance
const service = axios.create({
  baseURL: 'http://localhost:8088', // api的base_url
  timeout: -1 // request timeout
})

// request interceptor
service.interceptors.request.use(config => {
  // Do something before request is sent]
  config.headers['X-Token'] = getToken() // 让每个请求携带token-- ['X-Token']为自定义key 请根据实际情况自行修改
  return config
}, error => {
  // Do something with request error
  Promise.reject(error)
})

export default service