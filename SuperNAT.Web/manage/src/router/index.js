import Vue from "vue"
import store from "../store"
import Router from "vue-router"
import {
  publicRoute
} from "./config"
import {
  getJwtToken
} from "@/util/auth"
import NProgress from "nprogress"
import "nprogress/nprogress.css"
import request from "@/util/request"
import {
  DefaultLayout
} from "@/components/layouts"

Vue.use(Router)
const router = new Router({
  mode: "hash",
  linkActiveClass: "active",
  routes: publicRoute
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
            //构造路由
            // 构造树形菜单
            var menuArr = data.Data.menu_list || []
            var parentArr = menuArr.filter(c => !c.pid)
            var routers = parentArr.map(v => {
              var children = menuArr.filter(c => c.pid === v.menu_id).map(m => {
                return {
                  path: m.path,
                  component: () => import(m.component),
                  name: m.name,
                  hidden: m.hidden,
                  meta: {
                    title: m.title,
                    icon: m.icon
                  }
                }
              })
              var redirect = v.path
              if (children.length > 0) {
                redirect += '/' + children[0].path
              }
              return {
                path: v.path,
                component: DefaultLayout,
                redirect: redirect,
                name: v.name,
                meta: {
                  title: v.title,
                  icon: v.icon
                },
                always_show: v.always_show,
                hidden: v.hidden,
                children: children
              }
            })
            console.log(routers)
            router.addRoutes(routers) // 动态添加可访问路由表
            store.dispatch("setRoutes", routers)
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