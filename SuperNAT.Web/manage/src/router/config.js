import {
  AuthLayout
} from "@/components/layouts"

export const publicRoute = [{
    path: "*",
    hidden: true,
    component: () => import("@/views/error/NotFound.vue")
  },
  {
    path: "/auth",
    component: AuthLayout,
    meta: {
      title: "Login"
    },
    redirect: "/auth/login",
    hidden: true,
    children: [{
      path: "login",
      name: "login",
      meta: {
        title: "Login"
      },
      component: () => import("@/views/auth/Login.vue")
    }]
  },
  {
    path: "/403",
    name: "Forbidden",
    hidden: true,
    meta: {
      title: "Access Denied"
    },
    component: () => import("@/views/error/Deny.vue")
  },
  {
    path: "/404",
    name: "404",
    hidden: true,
    meta: {
      title: "Not Found"
    },
    component: () => import("@/views/error/NotFound.vue")
  },
  {
    path: "/500",
    name: "500",
    hidden: true,
    meta: {
      title: "Server Error"
    },
    component: () => import("@/views/error/Error.vue")
  }
]

export const protectedRoute = []