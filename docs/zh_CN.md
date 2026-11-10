简体中文 | [English](./en_US.md)

# 免责声明

- 本工具仅作个人学习研究使用，可运行的二进制文件仅用于演示功能，不得将源码及其产物用于商业用途，否则由此造成的相关法律问题，[本人](https://github.com/real-zony) 不承担任何法律责任。
- 任何单位或个人因下载使用软件所产生的任何意外、疏忽、合约毁坏、诽谤、版权或知识产权侵犯及其造成的损失 (包括但不限于直接、间接、附带或衍生的损失等)，[本人](https://github.com/real-zony) 不承担任何法律责任。
- 用户明确并同意本声明条款列举的全部内容，对使用本工具可能存在的风险和相关后果将完全由用户自行承担，[本人](https://github.com/real-zony) 不承担任何法律责任。

# 简介

ZonyLrcToolX 4 是一个提供 .NET 10 CLI 和 Avalonia 桌面界面的跨平台歌词下载工具，**QQ 群:337656932**，群文件里面有详细的视频教程。

🚧 当前版本正在开发当中。
🚧 如果你想查看可以工作的代码，请切换到 dev 分支。

# 下载

如果你要获取最新版本，请访问 **[Release](https://github.com/real-zony/ZonyLrcToolsX/releases)** 页面进行下载。

## Arch Linux 用户仓库

本软件已打包到 [AUR](https://aur.archlinux.org/packages/zonylrctoolsx-bin)。Arch Linux 及其衍生发行版用户可用如下命令安装：

```bash
# yay 或其他 AUR Helper
yay -S zonylrctoolsx-bin

ZonyLrcTools.Cli --help
```

# 用法

软件的具体使用方法已经迁移到了全新的文档站点，请跳转到 [https://docs.myzony.com](https://docs.myzony.com)，里面包含完整的说明文档。

## 桌面端本地开发

需要 .NET SDK 10。在仓库根目录执行：

```bash
dotnet run --project src/ZonyLrcTools.Desktop -c Release
dotnet publish src/ZonyLrcTools.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

CI 分发 Windows x64 版本。本机发布请选择对应 RID，例如 Apple Silicon 使用 `osx-arm64`。指定 RID 时桌面端默认开启裁剪，使用 `-p:PublishTrimmed=false` 可以关闭。发布成功不等于目标系统上的功能验收通过。分发时保留整个发布目录，包括 SQLite/Avalonia 原生库、配置和 `THIRD-PARTY-NOTICES.md`。

配置优先保存在可写的程序目录，否则回退到用户应用数据目录。保存时保留未知 YAML 字段和注释；无法安全修改的锚点或多行值会明确报错，不覆盖原文件。`netease.session.json` 包含登录凭据，不要分享或提交到仓库。Unix 仅允许当前用户读写，Windows 继承目录权限。屏蔽词字典的相对路径以当前工作目录为基准。

KGG 解密需要包含对应文件密钥的 `KGMusicV3.db`。自动查找覆盖 Windows 酷狗目录，也可手动选择匹配的数据库。已有解密音频不会被覆盖，而是报告输出冲突。取消后不会发布不完整文件；不支持取消的歌词源、封面源及第三方解密器仍需等待当前调用返回。

合并前请手动检查：导航与语言/主题切换、本地歌词和封面下载、取消后重启任务、扫码登录/退出/重启、设置保存后重启，以及真实 KGG 配合匹配数据库的解密。合成解密测试不能代替所有客户端格式的兼容性验证。fork PR 的独立检出步骤见 [README](../README.md#run-a-contribution-locally)。


# 捐赠
[爱发电](https://afdian.net/a/zony-lrc-tools)

# Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=real-zony/ZonyLrcToolsX&type=Timeline)](https://star-history.dera.page/#real-zony/ZonyLrcToolsX&Timeline)
