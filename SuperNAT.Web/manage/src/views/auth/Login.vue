<template>
  <v-card class="elevation-1 pa-3 login-card">
    <v-card-text>
      <div class="layout column align-center">
        <img src="/static/m.png" alt="SuperNAT内网穿透" width="120" height="120" />

        <h1 class="flex my-4 primary--text">SuperNAT内网穿透</h1>
      </div>

      <v-form>
        <v-text-field
          append-icon="person"
          name="login"
          :label="$t('login.account')"
          type="text"
          @keyup.enter.native="login"
          v-model="model.user_name"
          v-validate="'required'"
          :error-messages="errors.collect('user_name')"
          data-vv-name="user_name"
          required
        />

        <v-text-field
          append-icon="lock"
          name="password"
          :label="$t('login.password')"
          id="password"
          type="password"
          @keyup.enter.native="login"
          v-model="model.password"
          v-validate="'required'"
          :error-messages="errors.collect('password')"
          data-vv-name="password"
          required
        />

        <v-text-field
          v-show="register"
          append-icon="lock"
          name="password"
          label="确认密码"
          id="password2"
          type="password"
          @keyup.enter.native="login"
          v-model="model.password2"
          v-validate="{'required': 'true', 'is': model.password}"
          data-vv-name="password2"
          required
        />
        <div v-show="errors.has('password2')">
          <p style="color:red">{{ errors.first('password2') }}</p>
        </div>
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

      <v-btn
        v-if="!register"
        block
        color="primary"
        @click="login"
        :loading="loading"
      >{{ $t("login.submit") }}</v-btn>
      <v-btn block color="primary" v-else @click="register" :loading="loading">注册</v-btn>

      <v-btn
        style="float:right"
        v-if="!register"
        flat
        color="primary"
        @click="register = true"
      >还没有帐号，立即注册</v-btn>
      <v-btn style="float:right" v-else flat color="primary" @click="register = false">使用已有帐号登录</v-btn>
      <div style="clear:both"></div>
    </div>
  </v-card>
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
    model: {
      user_name: "",
      password: ""
    },
    dictionary: {
      custom: {
        user_name: {
          required: () => "账号不能为空"
        },
        password: {
          required: () => "密码不能为空"
        },
        password2: {
          required: () => "确认密码不能为空"
        }
      }
    }
  }),
  mounted() {
    this.$validator.localize("zh", this.dictionary)
  },
  methods: {
    register() {
      this.register = true
    },
    login() {
      this.loading = true
      // handle login
      this.$validator.validateAll(this.formItem).then(res => {
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

<style scoped lang="css"></style>

