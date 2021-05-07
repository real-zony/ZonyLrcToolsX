## 简介
ZonyLrcToolX 2.0 是一个基于 CEF 的跨平台歌词下载工具。

🚧 当前版本正在开发当中。  
🚧 如果你想查看可以工作的代码，请切换到 1.0 分支。
## 用法

### 命令

#### 文件扫描

子命令为 `scan`，可用于扫描指定文件夹下的音乐文件数量(好像没什么卵用)，下面我以 Windows 的可执行程序为例。

```shell
./ZonyLrcTools.Cli.exe scan -d|dir <WAIT_SCAN_DIRECTORY>

./ZonyLrcTools.cli.exe -h|--help
```

#### 歌曲下载

子命令为 `download`，可用于下载歌词数据[^1]和专辑图像[^2]，支持多个下载器[^3]进行下载。

```shell
./ZonyLrcTools.Cli.exe download -d|dir <WAIT_SCAN_DIRECTORY> [-l|--lyric] [-a|--album] [-n|--number]

./ZonyLrcTools.Cli.exe download -h|--help
```

### 配置文件

程序的部分配置信息需要在 `appsettings.json` 进行更改，下面标注了各个配置的说明。

| 属性                                              | 说明                                                         | 示例值                          |
| ------------------------------------------------- | ------------------------------------------------------------ | ------------------------------- |
| ToolOption.SupportFileExtensions                  | 允许扫描的歌曲文件后缀名，以 `;` 号隔开多个后缀。            | `*.mp3;*.flac`                  |
| ToolOption.NetworkOptions.Enable                  | 是否启用 HTTP 网络代理服务，true 表示启用，false 表示禁用。  | false                           |
| ToolOption.NetworkOptions.ProxyIp                 | HTTP 网络代理服务的 IP，在 `Enable` 为 false 时会忽略该属性值。 | 127.0.0.1                       |
| ToolOption.NetworkOptions.ProxyPort               | HTTP 网络代理服务的 端口，在 `Enable` 为 false 时会忽略该属性值。 | 8080                            |
| TagInfoProviderOptions.FileNameRegularExpressions | 文件名 Tag 标签信息读取器使用，使用正则表达式匹配歌曲名和歌手，请使用命名分组编写正则表达式。 | (?'artist'.+)\\s-\\s(?'name'.+) |

## 捐赠

## 路线图

- [ ] 支持跨平台的 CLI 工具。
- [ ] 基于 Web GUI 的操作站点。
- [ ] 支持插件系统(Lua 引擎)。

[^1 ]: 哎是