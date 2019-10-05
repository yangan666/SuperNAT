import { AuthLayout, DefaultLayout, ChatLayout } from "@/components/layouts"

export const publicRoute = [
  { path: "*", component: () => import("@/views/error/NotFound.vue") },
  {
    path: "/auth",
    component: AuthLayout,
    meta: { title: "Login" },
    redirect: "/auth/login",
    hidden: true,
    children: [
      {
        path: "login",
        name: "login",
        meta: { title: "Login" },
        component: () => import("@/views/auth/Login.vue")
      }
    ]
  },
  {
    path: "/403",
    name: "Forbidden",
    hidden: true,
    meta: { title: "Access Denied" },
    component: () => import("@/views/error/Deny.vue")
  },
  {
    path: "/404",
    name: "404",
    meta: { title: "Not Found" },
    component: () => import("@/views/error/NotFound.vue")
  },
  {
    path: "/500",
    name: "500",
    meta: { title: "Server Error" },
    component: () => import("@/views/error/Error.vue")
  }
]

export const protectedRoute = [
  {
    path: "/",
    component: DefaultLayout,
    meta: { title: "首页", icon: "home" },
    redirect: "/dashboard",
    children: [
      {
        path: "/dashboard",
        name: "Dashboard",
        meta: { title: "请求统计", icon: "home" },
        component: () => import("@/views/Dashboard.vue")
      }
    ]
  },
  {
    path: "/sys",
    name: "System",
    component: DefaultLayout,
    meta: { title: "系统管理", icon: "dashboard" },
    redirect: "/sys/user",
    children: [
      {
        path: "/menu",
        name: "Menu",
        meta: { title: "菜单管理", icon: "dashboard" },
        component: () => import("@/views/menu/Menu.vue")
      },
      {
        path: "/role",
        name: "Role",
        meta: { title: "角色管理", icon: "dashboard" },
        component: () => import("@/views/role/Role.vue")
      },
      {
        path: "/user",
        name: "User",
        meta: { title: "用户管理", icon: "dashboard" },
        component: () => import("@/views/user/UserList.vue")
      }
    ]
  },
  {
    path: "/nat",
    name: "Nat",
    component: DefaultLayout,
    meta: { title: "内网穿透", icon: "dashboard" },
    redirect: "/nat/client",
    children: [
      {
        path: "/client",
        name: "Client",
        meta: { title: "主机管理", icon: "dashboard" },
        component: () => import("@/views/client/ClientList.vue")
      },
      {
        path: "/map",
        name: "Map",
        meta: { title: "端口映射", icon: "dashboard" },
        component: () => import("@/views/map/MapList.vue")
      }
    ]
  }
]
