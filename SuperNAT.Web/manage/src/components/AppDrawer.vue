<template>
  <v-navigation-drawer class="app--drawer"
                       :mini-variant.sync="mini"
                       app
                       v-model="showDrawer"
                       :width="drawWidth">
    <v-toolbar color="primary darken-1"
               dark>
      <img :src="computeLogo"
           height="36"
           alt="SuperNAT" />
      <v-toolbar-title class="ml-0 pl-3">
        <span class="hidden-sm-and-down">SuperNAT</span>
      </v-toolbar-title>
    </v-toolbar>
    <vue-perfect-scrollbar class="drawer-menu--scroll"
                           :settings="scrollSettings">
      <v-list expand
              dense>
        <template v-for="route in routes">
          <!-- 有子级菜单 -->
          <template v-if="route.children">
            <!-- 子级菜单等于1个，并且设置总是展开，直接把子级菜单当作一级菜单 -->
            <template v-if="route.children.length == 1 && !route.always_show">
              <v-list-tile ripple
                           :to="{ name: route.children[0].name }"
                           :key="route.children[0].name"
                           rel="noopener">
                <v-list-tile-action>
                  <!-- 用顶级的图标 -->
                  <v-icon>{{ route.meta && route.meta.icon }}</v-icon>
                </v-list-tile-action>
                <v-list-tile-content>
                  <v-list-tile-title>{{ route.children[0].meta.title }}</v-list-tile-title>
                </v-list-tile-content>
              </v-list-tile>
            </template>
            <!-- 子级菜单大于1个 -->
            <template v-else>
              <v-list-group :prepend-icon="route.meta && route.meta.icon"
                            :key="route.name"
                            no-action>
                <v-list-tile slot="activator"
                             ripple>
                  <v-list-tile-content>
                    <v-list-tile-title>{{ route.meta.title }}</v-list-tile-title>
                  </v-list-tile-content>
                </v-list-tile>
                <template v-for="cRoute in route.children">
                  <v-list-tile ripple
                               v-if="!(cRoute.hidden || false)"
                               :to="{ name: cRoute.name }"
                               :key="cRoute.name">
                    <v-list-tile-content>
                      <v-list-tile-title>{{ cRoute.meta.title }}</v-list-tile-title>
                    </v-list-tile-content>
                  </v-list-tile>
                </template>
              </v-list-group>
            </template>
          </template>
          <!-- 顶级菜单 -->
          <v-list-tile v-else
                       ripple
                       :to="{ name: route.name }"
                       :key="route.name"
                       rel="noopener">
            <v-list-tile-action>
              <v-icon>{{ route.meta && route.meta.icon }}</v-icon>
            </v-list-tile-action>
            <v-list-tile-content>
              <v-list-tile-title>{{ route.meta.title }}</v-list-tile-title>
            </v-list-tile-content>
          </v-list-tile>
        </template>
      </v-list>
    </vue-perfect-scrollbar>
  </v-navigation-drawer>
</template>
<script>
import VuePerfectScrollbar from "vue-perfect-scrollbar"
import { DefaultLayout } from "@/components/layouts"
export default {
  name: "AppDrawer",
  components: {
    VuePerfectScrollbar
  },
  props: {
    expanded: {
      type: Boolean,
      default: true
    },
    drawWidth: {
      type: [Number, String],
      default: "260"
    },
    showDrawer: Boolean
  },
  data () {
    return {
      mini: false,
      scrollSettings: {
        maxScrollbarLength: 160
      }
    }
  },
  computed: {
    computeGroupActive () {
      return true
    },
    computeLogo () {
      return "/static/m.png"
    },
    sideToolbarColor () {
      return this.$vuetify.options.extra.sideNav
    },
    routes () {
      // 构造树形菜单
      var menuArr = this.$store.getters.user.menu_list || []
      var parentArr = menuArr.filter(c => !c.pid)
      var routers = parentArr.map(v => {
        var children = menuArr.filter(c => c.pid === v.menu_id).map(m => {
          return {
            path: m.path,
            component: () => import(m.component),
            name: m.name,
            hidden: m.hidden,
            meta: {
              title: m.title,
              icon: m.icon
            }
          }
        })
        var redirect = v.path
        if (children.length > 0) {
          redirect += '/' + children[0].path
        }
        return {
          path: v.path,
          component: DefaultLayout,
          redirect: redirect,
          name: v.name,
          meta: {
            title: v.title,
            icon: v.icon
          },
          always_show: v.always_show,
          hidden: v.hidden,
          children: children
        }
      })
      console.log(routers)
      return routers
    }
  },
  created () { },
  methods: {
    showChildren (children) {
      return children.some(c => !c.hidden)
    }
  }
}
</script>

<style lang="stylus" scoped>
.app--drawer
  overflow hidden
  .drawer-menu--scroll
    height calc(100vh - 48px)
    overflow auto
</style>
