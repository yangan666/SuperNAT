<template>
  <div>
    <v-card class="elevation-1 pa-3 login-card">
      <v-card-text>
        <div class="layout column align-center">
          <img src="/static/m.png"
               alt="SuperNAT内网穿透"
               width="120"
               height="120" />

          <h1 class="flex my-4 primary--text">SuperNAT内网穿透</h1>
        </div>

        <v-form>
          <v-text-field append-icon="person"
                        name="login"
                        :label="$t('login.account')"
                        type="text"
                        @keyup.enter.native="login"
                        v-model="model.user_name"
                        v-validate="'required'"
                        data-vv-name="账号"
                        :error-messages="errors.collect('账号')" />

          <v-text-field v-show="register"
                        append-icon="email"
                        name="email"
                        label="邮箱"
                        type="text"
                        @keyup.enter.native="login"
                        v-model="model.email"
                        v-validate="register ? 'required|email' : ''"
                        data-vv-name="邮箱"
                        :error-messages="errors.collect('邮箱')" />

          <v-text-field append-icon="lock"
                        name="password"
                        :label="$t('login.password')"
                        id="password"
                        type="password"
                        @keyup.enter.native="login"
                        v-model="model.password"
                        v-validate="'required'"
                        data-vv-name="密码"
                        :error-messages="errors.collect('密码')"
                        ref="password" />

          <v-text-field v-show="register"
                        append-icon="lock"
                        name="confirmPassword"
                        label="确认密码"
                        id="confirmPassword"
                        type="password"
                        @keyup.enter.native="login"
                        v-model="model.confirmPassword"
                        v-validate="register ? 'required|confirmed:password' : ''"
                        data-vv-name="确认密码"
                        :error-messages="errors.collect('确认密码')" />
        </v-form>
      </v-card-text>

      <div class="login-btn">
        <!-- <v-btn icon>
        <v-icon color="blue">fa fa-facebook-square fa-lg</v-icon>
      </v-btn>

      <v-btn icon>
        <v-icon color="red">fa fa-google fa-lg</v-icon>
      </v-btn>

      <v-btn icon>
        <v-icon color="light-blue">fa fa-twitter fa-lg</v-icon>
      </v-btn>-->

        <v-spacer />

        <v-btn v-if="!register"
               block
               color="primary"
               @click="login"
               :loading="loading">{{ $t("login.submit") }}</v-btn>
        <v-btn block
               color="primary"
               v-else
               @click="addUser"
               :loading="loading">注册</v-btn>

        <v-btn style="float:right"
               v-if="!register"
               flat
               color="primary"
               @click="register = true">还没有帐号，立即注册</v-btn>

        <v-btn style="float:right"
               v-else
               flat
               color="primary"
               @click="register = false">使用已有帐号登录</v-btn>
        <div style="clear:both"></div>
      </div>
    </v-card>
    <p class="copy-right">©{{new Date().getFullYear()}}&nbsp;SuperNAT&nbsp;&nbsp;粤ICP备19095189号-1&nbsp;</p>
  </div>
</template>

<script>
import request from "@/util/request"
import { setJwtToken } from "@/util/auth"
export default {
  $_veeValidate: {
    validator: "new"
  },
  data: () => ({
    register: false,
    loading: false,
    model: {}
  }),
  mounted () { },
  methods: {
    addUser () {
      this.loading = true
      // handle login
      this.$validator.validateAll().then(res => {
        if (res) {
          request({
            url: "/Api/User/Register",
            method: "post",
            data: this.model
          }).then(({ data }) => {
            this.loading = false
            if (data.Result) {
              this.model = {}
              this.register = false
              this.$dialog.message.success(data.Message, {
                position: "top"
              })
            } else {
              this.$dialog.message.error(data.Message, {
                position: "top"
              })
            }
          })
        } else {
          this.loading = false
        }
      })
    },
    login () {
      this.loading = true
      // handle login
      this.$validator.validateAll().then(res => {
        if (res) {
          request({
            url: "/Api/User/Login",
            method: "post",
            data: this.model
          }).then(({ data }) => {
            this.loading = false
            if (data.Result) {
              this.$dialog.message.success(data.Message, {
                position: "top"
              })
              //记录到cookies
              setJwtToken(data.Data.token)
              this.$router.push("/dashboard")
            } else {
              this.$dialog.message.error(data.Message, {
                position: "top"
              })
            }
          })
        } else {
          this.loading = false
        }
      })
    }
  }
}
</script>

<style scoped lang="css">
.copy-right {
  position: absolute;
  bottom: 5px;
  text-align: center;
  width: 620px;
  color: #999;
}
</style>

