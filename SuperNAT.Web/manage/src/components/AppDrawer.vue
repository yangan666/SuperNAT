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
           alt="Vue Material Admin Template" />
      <v-toolbar-title class="ml-0 pl-3">
        <span class="hidden-sm-and-down">SuperNAT</span>
      </v-toolbar-title>
    </v-toolbar>
    <vue-perfect-scrollbar class="drawer-menu--scroll"
                           :settings="scrollSettings">
      <v-list expand
              dense>
        <template v-for="(route, index) in routes">
          <template v-if="showChildren(route.children)">
            <v-list-group v-if="!(route.hidden || false)"
                          value="true"
                          :prepend-icon="route.meta && route.meta.icon"
                          :key="index"
                          no-action>
              <v-list-tile slot="activator"
                           ripple>
                <v-list-tile-content>
                  <v-list-tile-title>{{ route.meta.title }}</v-list-tile-title>
                </v-list-tile-content>
              </v-list-tile>
              <v-list-tile ripple
                           v-for="(cRoute, idx) in route.children"
                           :to="{ path: cRoute.path }"
                           :key="idx">
                <!-- <v-list-tile-action>
                  <v-icon>{{ cRoute.meta && cRoute.meta.icon }}</v-icon>
                </v-list-tile-action> -->
                <v-list-tile-content v-if="!(cRoute.hidden || false)">
                  <v-list-tile-title>{{ cRoute.meta.title }}</v-list-tile-title>
                </v-list-tile-content>
              </v-list-tile>
            </v-list-group>
          </template>
          <template v-else>
            <v-list-tile v-if="!(route.hidden || false)"
                         ripple
                         :to="{ path: route.path }"
                         :key="index">
              <v-list-tile-action>
                <v-icon>{{ route.meta && route.meta.icon }}</v-icon>
              </v-list-tile-action>
              <v-list-tile-content>
                <v-list-tile-title>{{ route.meta.title }}</v-list-tile-title>
              </v-list-tile-content>
            </v-list-tile>
          </template>
        </template>
      </v-list>
    </vue-perfect-scrollbar>
  </v-navigation-drawer>
</template>
<script>
import {
  protectedRoute
} from "@/router/config"
import VuePerfectScrollbar from "vue-perfect-scrollbar"
console.log(protectedRoute)
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
      routes: protectedRoute,
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
    }
  },
  created () { },

  methods: {
    showChildren (children) {
      return children.some(c => !c.hidden)
    },
    genChildTarget (item, subItem) {
      if (subItem.href) return
      if (subItem.component) {
        return {
          name: subItem.component
        }
      }
      return { name: `${item.group}/${subItem.name}` }
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
