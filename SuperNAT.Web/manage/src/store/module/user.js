const user = {
  state: {
    dialog: false,
    user: {}
  },
  mutations: {
    setDialog(state, data) {
      state.dialog = data
    },
    setUser(state, data) {
      state.user = data
    }
  },
  actions: {
    setDialog({ commit }, data) {
      commit("setDialog", data)
    },
    setUser({ commit }, data) {
      commit("setUser", data)
    }
  }
}

export default user
