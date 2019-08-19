import axios from "axios";
import { getToken } from "@/utils/auth";

// create an axios instance
const service = axios.create({
  baseURL: "", // api的base_url  http://www.supernat.cn:8088
  timeout: -1, // request timeout
  // withCredentials: true // 允许携带cookie
});

// request interceptor
service.interceptors.request.use(
  (config) => {
    // Do something before request is sent]
    config.headers["X-Token"] = getToken(); // 让每个请求携带token-- ['X-Token']为自定义key 请根据实际情况自行修改
    return config;
  },
  error => {
    // Do something with request error
    Promise.reject(error);
  }
);

service.interceptors.response.use(
  response => {
    // 拿到接口返回的 res 数据，做一些格式转换处理，使其符合 DataBinder 的要求
    const body = response.data;
    const newBody = {
      status: body.Result ? "SUCCESS" : "ERROR",
      code: body.Status,
      message: body.Message,
      data: {
        dataSource: body.Data
      }
    };
    response.data = newBody;
    return response;
  },
  error => {
    console.log("err" + error); // for debug
    return Promise.reject(error);
  }
);

export default service;
