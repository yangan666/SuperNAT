<template>
  <v-toolbar color="primary"
             fixed
             dark
             app>
    <v-toolbar-title>
      <v-toolbar-side-icon @click="handleDrawerToggle"></v-toolbar-side-icon>
    </v-toolbar-title>
    <!-- <v-text-field flat
                  solo-inverted
                  prepend-inner-icon="search"
                  :label="$t('toobar.search')"
                  clearable
                  class="search"></v-text-field> -->
    <v-spacer></v-spacer>
    <v-toolbar-items>
      <v-btn icon
             @click="github">
        <v-icon class="fa-2x">fa-github</v-icon>
      </v-btn>
      <v-btn icon
             @click="handleFullScreen()">
        <v-icon>fullscreen</v-icon>
      </v-btn>
      <!-- <v-menu offset-y
              origin="center center"
              class="elelvation-1"
              :nudge-bottom="14"
              transition="scale-transition">
        <v-btn icon
               flat
               slot="activator">
          <v-badge color="red"
                   overlap>
            <span slot="badge">3</span>
            <v-icon medium>notifications</v-icon>
          </v-badge>
        </v-btn>
        <notification-list></notification-list>
      </v-menu> -->
      <v-menu offset-y
              origin="center center"
              :nudge-bottom="10"
              transition="scale-transition">
        <!-- <v-btn icon
               large
               flat
               slot="activator">
          <v-avatar size="30px">
            <img src="/static/avatar/man_4.jpg"
                 alt="Michael Wang" />
          </v-avatar>
        </v-btn> -->
        <v-toolbar-title slot="activator">
          <v-avatar size="40">
            <img src="/static/avatar/man_4.jpg"
                 alt="">
          </v-avatar>
          <span style="margin-left: 10px;">{{ user.user_name }}</span>
          <v-icon>arrow_drop_down</v-icon>
        </v-toolbar-title>
        <v-list class="pa-0">
          <v-list-tile v-for="(item, index) in items"
                       :to="!item.href ? { name: item.name } : null"
                       :href="item.href"
                       @click="item.click"
                       ripple="ripple"
                       :disabled="item.disabled"
                       :target="item.target"
                       rel="noopener"
                       :key="index">
            <v-list-tile-action v-if="item.icon">
              <v-icon>{{ item.icon }}</v-icon>
            </v-list-tile-action>
            <v-list-tile-content>
              <v-list-tile-title>{{ item.title }}</v-list-tile-title>
            </v-list-tile-content>
          </v-list-tile>
        </v-list>
      </v-menu>
      <v-dialog v-model="dialog"
                persistent
                max-width="500px">
        <v-card>
          <v-card-title>
            <span class="headline">资料</span>
          </v-card-title>

          <v-card-text>
            <v-container grid-list-md>
              <v-form>
                <v-text-field box
                              readonly
                              append-icon="person"
                              name="login"
                              :label="$t('login.account')"
                              type="text"
                              @keyup.enter.native="save"
                              v-model="model.user_name"
                              v-validate="'required'"
                              data-vv-name="账号"
                              :error-messages="errors.collect('账号')" />

                <v-text-field append-icon="email"
                              name="email"
                              label="邮箱"
                              type="text"
                              @keyup.enter.native="save"
                              v-model="model.email"
                              v-validate="'required|email'"
                              data-vv-name="邮箱"
                              :error-messages="errors.collect('邮箱')" />

                <v-text-field append-icon="lock"
                              name="password"
                              label="新密码"
                              id="password"
                              type="password"
                              @keyup.enter.native="save"
                              v-model="model.password"
                              ref="password" />

                <v-text-field append-icon="lock"
                              name="confirmPassword"
                              label="确认密码"
                              id="confirmPassword"
                              type="password"
                              @keyup.enter.native="save"
                              v-model="model.confirmPassword"
                              v-validate="'confirmed:password'"
                              data-vv-name="确认密码"
                              :error-messages="errors.collect('确认密码')" />
              </v-form>
            </v-container>
          </v-card-text>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1"
                   flat
                   @click="save"
                   :loading="loading">保存</v-btn>
            <v-btn color="blue darken-1"
                   flat
                   @click="close">取消</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-toolbar-items>
  </v-toolbar>
</template>
<script>
import NotificationList from "@/components/widgets/list/NotificationList"
import Util from "@/util"
import request from "@/util/request"
import { removeToken } from "@/util/auth"
export default {
  $_veeValidate: {
    validator: "new"
  },
  name: "AppToolbar",
  components: {
    NotificationList
  },
  data () {
    return {
      dialog: false,
      model: {},
      loading: false,
      items: [
        {
          icon: "account_circle",
          href: "#",
          title: this.$t('toobar.profile'),
          click: this.handleProfile
        },
        // {
        //   icon: "settings",
        //   href: "#",
        //   title: this.$t('toobar.settings'),
        //   click: this.handleSetting
        // },
        {
          icon: "fullscreen_exit",
          href: "#",
          title: this.$t('toobar.logout'),
          click: this.handleLogut
        }
      ]
    }
  },
  computed: {
    toolbarColor () {
      return this.$vuetify.options.extra.mainNav
    },
    user () {
      return this.$store.getters.user
    }
  },
  methods: {
    github () {
      window.open('https://github.com/yangan666/SuperNAT')
    },
    handleDrawerToggle () {
      this.$emit("side-icon-click")
    },
    handleFullScreen () {
      Util.toggleFullScreen()
    },
    handleLogut () {
      //handle logout
      removeToken()
      this.$store.dispatch("setUser", {})
      this.$router.push('/auth/login')
    },
    handleSetting () {

    },
    handleProfile () {
      this.dialog = true
      this.model = this.$store.getters.user
    },
    save () {
      this.loading = true
      // handle login
      this.$validator.validateAll().then(res => {
        if (res) {
          request({
            url: "/Api/User/Add",
            method: "post",
            data: this.model
          }).then(({ data }) => {
            this.loading = false
            if (data.Result) {
              this.close()
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
    close () {
      this.dialog = false
      this.model = {}
    }
  }
}
</script>

<style lang="stylus" scoped></style>
