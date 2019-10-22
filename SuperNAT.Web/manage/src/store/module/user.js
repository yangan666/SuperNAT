import { publicRoute } from "@/router"

const user = {
  state: {
    dialog: false,
    user: {},
    routes: publicRoute,
    addRouters: []
  },
  mutations: {
    setDialog(state, data) {
      state.dialog = data
    },
    setUser(state, data) {
      state.user = data
    },
    setRoutes(state, data) {
      state.routes = publicRoute.concat(data)
      state.addRouters = data
    }
  },
  actions: {
    setDialog({ commit }, data) {
      commit("setDialog", data)
    },
    setUser({ commit }, data) {
      commit("setUser", data)
    },
    setRoutes({ commit }, data) {
      commit("setRoutes", data)
    }
  }
}

export default user
