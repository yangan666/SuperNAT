import Vue from "vue"
import store from "../store"
import Router from "vue-router"
import {
  publicRoute,
  protectedRoute
} from "./config"
import {
  getJwtToken
} from "@/util/auth"
import NProgress from "nprogress"
import "nprogress/nprogress.css"
import request from "@/util/request"
const routes = publicRoute.concat(protectedRoute)

Vue.use(Router)
const router = new Router({
  mode: "hash",
  linkActiveClass: "active",
  routes: routes
})
// router gards
router.beforeEach((to, from, next) => {
  NProgress.start()
  var token = getJwtToken()
  if (to.path === "/auth/login") {
    next()
  } else {
    //auth route is authenticated
    if (token) {
      var user = store.getters.user
      if (JSON.stringify(user) == "{}") {
        //重新获取用户信息
        request({
          url: "/Api/User/GetUserInfo",
          method: "post"
        }).then(({
          data
        }) => {
          if (data.Result) {
            data.Data.token = token
            store.dispatch("setUser", data.Data)
            next()
          } else {
            //跳转到登录
            next("/auth/login")
          }
        })
      } else {
        next()
      }
    } else {
      //跳转到登录
      next("/auth/login")
    }
  }
})

router.afterEach((to, from) => {
  NProgress.done()
})

export default router