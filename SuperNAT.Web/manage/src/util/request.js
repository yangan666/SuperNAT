import axios from "axios"
import { getJwtToken, removeToken } from "@/util/auth"

// create an axios instance
const service = axios.create({
  baseURL: "http://localhost:8088", // api的base_url  http://www.supernat.cn:8088  http://localhost:8088
  timeout: -1 // request timeout
  // withCredentials: true // 允许携带cookie
})

// request interceptor
service.interceptors.request.use(
  config => {
    // Do something before request is sent]
    var token = getJwtToken()
    if (token) {
      config.headers.Authorization = "Bearer " + token // 让每个请求携带token-- ['X-Token']为自定义key 请根据实际情况自行修改
    }
    return config
  },
  error => {
    // Do something with request error
    Promise.reject(error)
  }
)

service.interceptors.response.use(
  response => {
    var data = response.data
    //10000 未授权
    //10001 签名不正确
    //10002 会话超时
    //10003 授权验证失败
    if (data.Status != 0) {
      //输入密码重新登录
      if (show) {
        return response
      }
      showLogin()
    }
    return response
  },
  error => {
    console.log("err" + error) // for debug
    return Promise.reject(error)
  }
)

export default service
