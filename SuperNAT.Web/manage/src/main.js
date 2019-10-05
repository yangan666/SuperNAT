import Vue from "vue"
import App from "./App.vue"
import Vuetify from "vuetify"
import VuetifyDialog from "vuetify-dialog"
import VeeValidate from "vee-validate"
import "./theme/default.styl"
import "vuetify-dialog/dist/vuetify-dialog.css"
import router from "./router/"
import store from "./store"
import LoginForm from "./LoginForm.vue"
import "./registerServiceWorker"
import "roboto-fontface/css/roboto/roboto-fontface.css"
import "font-awesome/css/font-awesome.css"
import "./style/app.css"
import i18n from "./lang/lang"
import '@mdi/font/css/materialdesignicons.css' // Ensure you are using css-loader
// import "./util/general.js"

Vue.use(Vuetify, {
  theme: {
    primary: "#ee44aa",
    secondary: "#424242",
    accent: "#82B1FF",
    error: "#FF5252",
    info: "#2196F3",
    success: "#4CAF50",
    warning: "#FFC107"
  },
  customProperties: true
})

Vue.use(Vuetify, {
  iconfont: 'mdi'
})

Vue.use(VuetifyDialog, {
  confirm: {
    actions: {
      false: "No",
      true: {
        text: "Yes",
        color: "primary"
      }
    },
    icon: false, // to disable icon just put false
    width: 500
  },
  warning: {},
  error: {},
  prompt: {}
})

Vue.use(VeeValidate)

Vue.config.productionTip = false
window.show = false
let vm = new Vue({
  router,
  store,
  i18n,
  render: h => h(App)
}).$mount("#app")

window.showLogin = () => {
  vm.$dialog.show(LoginForm, { persistent: true }, {})
  show = true
}
