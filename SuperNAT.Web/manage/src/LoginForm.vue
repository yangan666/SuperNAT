<template>
  <DialogCard title="重新登录"
              :actions="actions">
    <form>
      <v-flex>
        <v-text-field clearable
                      v-model="formItem.user_name"
                      label="用户名"></v-text-field>
      </v-flex>
      <v-flex>
        <v-text-field clearable
                      v-model="formItem.password"
                      label="密码"
                      type="password"></v-text-field>
      </v-flex>
    </form>
  </DialogCard>
</template>
<script>
import request from "./util/request"
import { setJwtToken } from "./util/auth"
import store from "./store"
export default {
  data () {
    return {
      formItem: {
        user_name: store.getters.user.user_name || '',
        password: ""
      }
    }
  },
  computed: {
    actions () {
      return {
        login: {
          flat: true,
          text: '登录',
          handle: () => {
            show = false
            request({
              url: '/Api/User/Login',
              method: 'post',
              data: this.formItem
            }).then(({ data }) => {
              if (data.Result) {
                this.$dialog.message.success(data.Message, {
                  position: 'top'
                })
                //记录到cookies
                setJwtToken(data.Data.token)
                store.dispatch('setUser', data.Data)
                //刷新页面
                location.reload()
              } else {
                this.$dialog.message.error(data.Message, {
                  position: 'top'
                })
              }
            })
          }
        }
      }
    }
  },
  methods: {

  }
}
</script>
