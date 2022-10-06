<template>
  <div class="container">
    <div class="button-group">
      <v-btn @click="openDir">
        扫描文件
      </v-btn>
      <input type="file" class="d-none">

      <v-btn class="ml-5" disabled>开始下载</v-btn>
    </div>
    <div class="file-list mt-5">
      <v-data-table
          :headers="headers"
          :items="items"
          class="elevation-1"
      >
        <template v-slot:item.status="{ item }">
          <v-icon v-if="item.status === 'success'" color="success">mdi-check</v-icon>
          <v-icon v-else-if="item.status === 'error'" color="error">mdi-close</v-icon>
          <v-icon v-else>mdi-minus</v-icon>
        </template>
      </v-data-table>
      <div class="output mt-8">
        <v-textarea disabled outlined label="日志信息" :value='outputText'/>
      </div>
    </div>
  </div>
</template>

<script>
import Socket from '@/communication/socket'
import eventBus from "@/communication/eventbus";

export default {
  name: 'Home',
  data() {
    return {
      headers: [
        {
          text: '文件名',
          align: 'start',
          sortable: false,
          value: 'name',
        },
        {text: '大小', value: 'size'},
        {text: '状态', value: 'status'},
        {text: '操作', value: 'action'},
      ],
      items: [
        {
          name: 'Frozen Yogurt',
          size: 159,
          status: 'success',
          action: 4.0,
        }
      ],
      outputText: ''
    }
  },
  methods: {
    openDir() {
      Socket.send({
        type: 'openDir',
        data: {},
      })
    }
  },
  computed: {},
  mounted() {
    // eventBus.$on('getFile', (msgData) => {
    //   console.log(msgData);
    // });
    eventBus.$on('getFileInfo', (msgData) => {
      this.items = [...this.items, ...msgData.data.map(item => {
        return {
          name: item.name,
          size: item.size,
          status: 'success',
          action: 4.0,
        }
      })]
    });

    eventBus.$on('output', (msgData) => {
      this.outputText = this.outputText.concat(msgData.data.text).concat('\n');
    });
  }
}
</script>
