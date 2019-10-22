const getters = {
  device: state => state.app.device,
  theme: state => state.app.theme,
  color: state => state.app.color,
  user: state => state.user.user,
  dialog: state => state.user.dialog,
  routes: state => state.user.routes,
  addRouters: state => state.user.addRouters
}

export default getters
