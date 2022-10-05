<template>
  <v-app id="inspire">

    <v-navigation-drawer
        v-model="drawer"
        app
    >
      <v-sheet
          color="grey lighten-4"
          class="pa-4"
      >
        <div>ZonyLrcTools-X</div>
      </v-sheet>

      <v-divider></v-divider>

      <v-list>
        <v-list-item
            v-for="[icon, text, to] in links"
            :key="icon"
            :to="to"
            link
        >
          <v-list-item-icon>
            <v-icon>{{ icon }}</v-icon>
          </v-list-item-icon>

          <v-list-item-content>
            <v-list-item-title>{{ text }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar app>
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>

      <v-toolbar-title>ZonyLrcToolsX</v-toolbar-title>
    </v-app-bar>

    <v-main>
      <v-container
          class="py-8 px-6"
          fluid
      >
        <router-view></router-view>
      </v-container>
    </v-main>
  </v-app>
</template>

<script>
export default {
  data: () => ({
    drawer: null,
    links: [
      ['mdi-play-circle', '开始', '/'],
      ['mdi-multiplication-box', '设置', '/setting'],
      ['mdi-information', '关于', '/about'],
    ],
  }),
}
</script>