<template>
  <v-app id="inspire" class="app dashboard">
    <app-drawer class="app--drawer" :show-drawer="showDrawer"></app-drawer>
    <app-toolbar class="app--toolbar" @side-icon-click="handleDrawerVisiable"></app-toolbar>
    <v-content>
      <!-- Page Header -->
      <page-header></page-header>
      <div class="page-wrapper">
        <router-view></router-view>
      </div>
      <!-- App Footer -->
      <v-footer height="auto" class="white pa-3 app--footer my-footer">
        <div class="flex-box">
          <span
            class="caption"
          >©{{new Date().getFullYear()}}&nbsp;SuperNAT&nbsp;&nbsp;粤ICP备19095189号-1&nbsp;</span>
        </div>
      </v-footer>
    </v-content>
    <!-- Go to top -->
    <app-fab></app-fab>
  </v-app>
</template>

<script>
import AppDrawer from "@/components/AppDrawer"
import AppToolbar from "@/components/AppToolbar"
import AppFab from "@/components/AppFab"
import PageHeader from "@/components/PageHeader"

export default {
  components: {
    AppDrawer,
    AppToolbar,
    AppFab,
    PageHeader
  },

  data() {
    return {
      showDrawer: true
    }
  },
  methods: {
    handleDrawerVisiable() {
      this.showDrawer = !this.showDrawer
    }
  },
  created() {}
}
</script>

<style scoped>
.page-wrapper {
  min-height: calc(100vh - 64px - 50px - 81px);
}
.my-footer {
  width: 100%;
  position: fixed;
  bottom: 0;
}
.flex-box {
  display: flex;
  width: 100%;
  justify-content: center;
  align-content: center;
}
</style>
