import store from "./store"
import router from "./router"
import { getJwtToken } from "@/util/auth"
import NProgress from "nprogress"
import "nprogress/nprogress.css"
import request from "@/util/request"
import { DefaultLayout } from "@/components/layouts"
const _import = require("@/router/_import_" + process.env.NODE_ENV)

NProgress.configure({ showSpinner: false }) // NProgress Configuration

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
        }).then(({ data }) => {
          if (data.Result) {
            data.Data.token = token
            //构造路由
            // 构造树形菜单
            var menuArr = data.Data.menu_list || []
            var parentArr = menuArr.filter(c => !c.pid)
            var routers = parentArr.map(v => {
              var children =
                menuArr
                  .filter(c => c.pid === v.menu_id)
                  .map(m => {
                    return {
                      path: m.path,
                      component: _import(m.component), //import("@views/menu/Menu.vue")
                      name: m.name,
                      hidden: m.hidden,
                      meta: {
                        title: m.title,
                        icon: m.icon
                      }
                    }
                  }) || []
              var redirect = v.path == "/" ? "" : v.path
              if (children.length > 0) {
                redirect += children[0].path
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
            // console.log(routers)
            router.addRoutes(routers) // 动态添加可访问路由表
            store.dispatch("setRoutes", routers)
            store.dispatch("setUser", data.Data)
            // console.log("to", to) // 当前路由
            if (routers.length > 0) {
              var has = false
              routers.map(v => {
                v.children.map(c => {
                  if (c.path === to.path) {
                    has = true
                  }
                })
              })
              if (has) {
                next({ ...to, replace: true }) // hack方法 确保addRoutes已完成 ,set the replace: true so the navigation will not leave a history record
              } else {
                var children = routers[0].children
                if (children.length > 0) {
                  next({ path: routers[0].redirect })
                } else {
                  next({ path: "/login" })
                }
              }
            } else {
              //跳转到登录
              next("/auth/login")
            }
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
