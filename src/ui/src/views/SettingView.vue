<template>

  <v-form>
    <v-list subheader flat two-line>
      <v-subheader>网络代理</v-subheader>

      <v-list-item>
        <v-switch label="启用代理" @change="proxyEnabledChange" v-model="isProxyEnabled"/>
      </v-list-item>
      <v-list-item>
        <v-text-field :disabled="proxyEnabled" v-model="proxyAddress" label="代理地址"
                      placeholder="请输入代理服务器的地址，例如 127.0.0.1:1080。"/>
      </v-list-item>
      <v-list-item>
        <v-text-field :disabled="proxyEnabled" v-model="updateAddress" label="更新检查地址"
                      placeholder="请输入更新检查地址，默认情况下无需变更。"/>
      </v-list-item>

      <v-divider></v-divider>
      <v-subheader>全局下载设置</v-subheader>
      <v-list-item>
        <v-switch label="是否启用翻译歌词"/>
      </v-list-item>
      <v-list-item>
        <v-switch label="仅输出翻译歌词"/>
      </v-list-item>
      <v-list-item>
        <v-switch label="跳过歌词文件已存在的歌曲"/>
      </v-list-item>
      <v-list-item>
        <v-switch label="双语歌词合并展示" v-model="mergeLrc"/>
      </v-list-item>
      <v-list-item>
        <!--文件编码-->
        <v-select v-model="lrcEncoding" :items="lrcEncodings" label="歌词文件编码" placeholder="请选择歌词文件编码"/>
      </v-list-item>
      <v-list-item>
        <!--换行符下拉选择-->
        <v-row>
          <v-col>
            <v-select label="换行符" v-model="lineBreak" :items="lineBreakOptions" item-text="label"
                      item-value="value"/>
          </v-col>
        </v-row>
      </v-list-item>

      <v-divider></v-divider>
      <v-list-item>
        <v-row>
          <v-col cols="6">
            <v-subheader>歌词下载器设置</v-subheader>
            <v-list-item>
              <v-data-table hide-default-footer :headers="lyricDownloaderHeaders" :items="lyricDownloader"
                            class="elevation-1" disable-sort>
                <template v-slot:item.enabled="{ item }">
                  <v-simple-checkbox v-model="item.enabled" color="primary"/>
                </template>
              </v-data-table>
            </v-list-item>
          </v-col>

          <v-col cols="6">
            <v-subheader>标签解析器设置</v-subheader>
            <v-list-item>
              <v-data-table hide-default-footer :headers="tagParserHeaders" :items="tagParser" class="elevation-1"
                            disable-sort>
                <template v-slot:item.enabled="{ item }">
                  <v-simple-checkbox v-model="item.enabled" color="primary"></v-simple-checkbox>
                </template>
              </v-data-table>
            </v-list-item>
          </v-col>
        </v-row>
      </v-list-item>

      <v-divider class="mt-5"></v-divider>
      <v-subheader>屏蔽字典</v-subheader>
      <v-list-item>
        <v-switch label="启用屏蔽字典" v-model="isBlockDictEnabled" @change="blockDictEnabledChange"/>
      </v-list-item>
      <v-list-item>
        <v-text-field :disabled="blockDictEnabled" v-model="blockDictPath" label="屏蔽字典路径"
                      placeholder="请选择屏蔽字典的文件路径。"/>
        <v-btn :disabled="blockDictEnabled" icon @click="openBlockDict">
          <v-icon>mdi-folder</v-icon>
        </v-btn>
      </v-list-item>

      <v-divider></v-divider>
      <v-subheader>扫描设置</v-subheader>
      <v-list-item>
        <v-text-field v-model="fileSearchSuffix" label="文件搜索后缀"
                      placeholder="请输入文件搜索后缀，多个后缀以英文逗号分隔。"/>
      </v-list-item>

    </v-list>
  </v-form>

</template>

<script>
export default {
  name: "Setting",
  data: () => {
    return {
      isProxyEnabled: true,
      proxyEnabled: false,
      isBlockDictEnabled: true,
      blockDictEnabled: false,
      blockDictPath: "",
      proxyAddress: "",
      updateAddress: "",
      mergeLrc: false,
      lineBreak: "lf",
      lineBreakOptions: [
        {label: "LF", value: "lf"},
        {label: "CRLF", value: "crlf"},
      ],
      lrcEncoding: "utf-8",
      lyricDownloaderHeaders: [
        {text: "启用", value: "enabled"},
        {text: "歌词源", value: "name"},
        {text: "搜索深度", value: "searchDepth"},
        {text: "优先级", value: "priority"},
      ],
      lyricDownloader: [
        {enabled: true, name: "网易云音乐", searchDepth: 10, priority: 1},
      ],
      tagParserHeaders: [
        {text: "启用", value: "enabled", sortable: false, width: "10%"},
        {text: "标签解析器", value: "name", sortable: false, width: "90%"},
      ],
      tagParser: [
        {name: '正则表达式解析器', enabled: true},
      ],
    }
  },
  computed: {},
  methods: {
    proxyEnabledChange() {
      this.proxyEnabled = !this.proxyEnabled;
    },
    blockDictEnabledChange() {
      this.blockDictEnabled = !this.blockDictEnabled;
    }
  },
};
</script>