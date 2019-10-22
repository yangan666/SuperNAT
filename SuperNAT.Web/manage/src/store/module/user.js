import {
  publicRoute
} from "@/router/config"

const user = {
  state: {
    dialog: false,
    user: {},
    routes: publicRoute
  },
  mutations: {
    setDialog(state, data) {
      state.dialog = data
    },
    setUser(state, data) {
      state.user = data
    },
    setRoutes(state, data) {
      state.routes = data
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
