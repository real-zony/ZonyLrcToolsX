简体中文 | [English](./docs/en_US.md)

## 免责声明
- 本工具仅作个人学习研究使用，可运行的二进制文件仅用于演示功能，不得将源码及其产物用于商业用途，否则由此造成的相关法律问题，[本人](https://github.com/real-zony) 不承担任何法律责任。
- 任何单位或个人因下载使用软件所产生的任何意外、疏忽、合约毁坏、诽谤、版权或知识产权侵犯及其造成的损失 (包括但不限于直接、间接、附带或衍生的损失等)，[本人](https://github.com/real-zony) 不承担任何法律责任。
- 用户明确并同意本声明条款列举的全部内容，对使用本工具可能存在的风险和相关后果将完全由用户自行承担，[本人](https://github.com/real-zony) 不承担任何法律责任。

## 简介

ZonyLrcToolX 4 是一个基于 CEF 的跨平台歌词下载工具。

🚧 当前版本正在开发当中。  
🚧 如果你想查看可以工作的代码，请切换到 dev 分支。

## 下载

工具会执行每日构建动作，请访问 **[Release](https://github.com/real-zony/ZonyLrcToolsX/releases)** 页面进行下载。

## 用法

Windows 用户请在软件目录当中，按住 Shift + 右键呼出菜单，然后选择 PowerShell/命令提示符/Windows 终端，根据下述说明执行命令即可。

macOS 和 Linux 用户请打开终端，切换到软件目录，一样执行命令即可。

### 子命令

#### 歌曲下载

子命令为 `download`，可用于下载歌词数据和专辑图像，支持多个下载器进行下载。

```shell
./ZonyLrcTools.Cli.exe download -d|dir <WAIT_SCAN_DIRECTORY> [-l|--lyric] [-a|--album] [-n|--number]

./ZonyLrcTools.Cli.exe download -h|--help
```

**例子**

```shell
# 下载歌词
./ZonyLrcTools.Cli.exe download -d "C:\歌曲目录" -l -n 2
# 下载专辑封面
./ZonyLrcTools.Cli.exe download -d "C:\歌曲目录" -a -n 2
```

#### 加密格式转换

子命令为 `util`，可用于转换部分加密歌曲，**仅供个人研究学习使用，思路与源码都来自于网络**。

目前软件支持 NCM、QCM(开发中...🚧) 格式的音乐文件转换，命令如下。

```shell
./ZonyLrcTools.Cli.exe util -t=Ncm D:\CloudMusic
```

### 配置文件

程序的所有的配置信息，都在 `config.yaml` 进行更改，下面标注了各个配置的说明。

其中是否开启的可选项为 `true` 或者 `false`，等同于中文的是和否。

```yaml
globalOption:
  # 允许扫描的歌曲文件后缀名。
  supportFileExtensions:
    - '*.mp3'
    - '*.flac'
    - '*.wav'
  # 网络代理服务设置，仅支持 HTTP 代理。
  networkOptions:
    isEnable: false # 是否启用代理。
    ip: 127.0.0.1   # 代理服务 IP 地址。
    port: 4780      # 代理服务端口号。
  
  # 下载器的相关参数配置。
  provider:
    # 标签扫描器的相关参数配置。
    tag:
      # 支持的标签扫描器。
      plugin:
        - name: Taglib    # 基于 Taglib 库的标签扫描器。
          priority: 1     # 优先级，升序排列。
        - name: FileName  # 基于文件名的标签扫描器。
          priority: 2
          # 基于文件名扫描器的扩展参数。
          extensions:
            # 正则表达式，用于匹配文件名中的作者信息和歌曲信息，可根据
            # 自己的需求进行调整。
            regularExpressions: "(?'artist'.+)\\s-\\s(?'name'.+)"
      # 歌曲标签屏蔽字典替换功能。
      blockWord:
        isEnable: false             # 是否启用屏蔽字典。
        filePath: 'BlockWords.json' # 屏蔽字典的路径。
    # 歌词下载器的相关参数配置。
    lyric:
      # 支持的歌词下载器。
      plugin:
        - name: NetEase   # 基于网易云音乐的歌词下载器。
          priority: 1     # 优先级，升序排列，改为 -1 时禁用。
          depth: 30       # 搜索深度，值越大搜索结果越多，但搜索时间越长。
        - name: QQ        # 基于 QQ 音乐的歌词下载器。
          priority: 2
          # depth: 10       # 暂时不支持。
        - name: KuGou     # 基于酷狗音乐的歌词下载器。
          priority: 3
          depth: 10
      # 歌词下载的一些共有配置参数。
      config:
        isOneLine: true                 # 双语歌词是否合并为一行展示。
        lineBreak: "\n"                 # 换行符的类型，记得使用双引号指定。
        isEnableTranslation: true       # 是否启用翻译歌词。
        isOnlyOutputTranslation: false  # 是否只输出翻译歌词。
        isSkipExistLyricFiles: false    # 如果歌词文件已经存在，是否跳过这些文件。
        fileEncoding: 'utf-8'           # 歌词文件的编码格式。
```

#### 支持的编码格式

详细信息请参考: [MSDN Encoding 列表](https://learn.microsoft.com/en-us/dotnet/api/System.Text.Encoding.GetEncodings?view=net-6.0#examples)，使用 `identifier and name` 作为参数值填入 `config.yaml` 文件当中的 `fileEncoding`。

### 屏蔽字典

屏蔽字典适用于网易云音乐歌词下载，针对某些单词，网易云音乐使用了 * 号进行屏蔽，这个时候可以使用屏蔽字典，设置歌曲名的关键词替换。例如有一首歌曲叫做 *Fucking ABC* ，这个时候网易云实际的名字是 *Fu****ing* ，用户只需要在屏蔽字典加入替换逻辑即可，例如:

```json
{
    "Fuckking": "Fu****ing"
}
```

屏蔽字典默认路径为程序所在目录的 *BlockWords.json* 文件，用户可以在 *appsettings.json* 文件中配置其他路径。

## 捐赠

<img src="./docs/img/alipay.jpg" width="200"/><img src="./docs/img/wechat.jpg" width="200"/>

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=real-zony/ZonyLrcToolsX&type=Timeline)](https://star-history.com/#real-zony/ZonyLrcToolsX&Timeline)

## 路线图

- [x] 支持跨平台的 CLI 工具。
- [ ] 基于 Web GUI 的操作站点。
- [ ] 支持插件系统(Lua 引擎)。
