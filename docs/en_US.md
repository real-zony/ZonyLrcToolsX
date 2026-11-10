English | [简体中文](./zh_CN.md)

# Disclaimer

- This tool is for personal learning and research purposes only. The executable binary files are for demonstration purposes only and the source code and its products must not be used for commercial purposes. Otherwise, I [here](https://github.com/real-zony) will not be responsible for any related legal issues.
- Any unit or individual that downloads and uses the software resulting in any accidents, negligence, contract breaches, defamation, copyright or intellectual property infringement, and their resulting losses (including but not limited to direct, indirect, incidental or derivative losses), I [here](https://github.com/real-zony) will not bear any legal responsibility.
- Users clearly agree to all the terms listed in this statement and fully assume the risks and consequences of using this tool. I [here](https://github.com/real-zony) will not bear any legal responsibility.

# Introduction

ZonyLrcToolX 4 is a cross-platform lyrics download tool with a .NET 10 CLI and an Avalonia desktop application. **QQ Group: 337656932**. Detailed video tutorials are available in the group files.

🚧 The current version is under development.
🚧 If you want to see working code, please switch to the dev branch.

# Download

To get the latest version, please visit the **[Release](https://github.com/real-zony/ZonyLrcToolsX/releases)** page for download.

## Arch Linux User Repository

This software has been packaged into [AUR](https://aur.archlinux.org/packages/zonylrctoolsx-bin). Arch Linux and its derivative distribution users can install it with the following command:

```bash
# yay or other AUR Helpers
yay -S zonylrctoolsx-bin

ZonyLrcTools.Cli --help
```

# Usage
Detailed usage instructions have been moved to a new documentation site. Please visit [https://docs.myzony.com](https://docs.myzony.com) for complete documentation.

## Desktop development

Requires .NET SDK 10. Run from the repository root:

```bash
dotnet run --project src/ZonyLrcTools.Desktop -c Release
dotnet publish src/ZonyLrcTools.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

CI distributes Windows x64. For local builds, choose the appropriate RID, such as `osx-arm64`. RID-specific desktop builds enable trimming by default; pass `-p:PublishTrimmed=false` to disable it. Publish success is not a substitute for testing the target OS. Keep the entire publish directory, including native SQLite/Avalonia libraries, configuration and `THIRD-PARTY-NOTICES.md`.

Settings use the writable executable directory, falling back to the user's application data directory. Saving preserves unknown YAML fields and comments; unsupported anchored or multiline values fail without overwriting the file. `netease.session.json` contains login credentials: never share or commit it. Unix restricts it to the current user; Windows inherits directory permissions. Relative block-word paths resolve against the current working directory.

KGG decryption requires a matching `KGMusicV3.db` containing that file's key. Automatic database discovery covers Windows KuGou locations; a matching database can also be selected manually. Existing decrypted audio is not overwritten and is reported as an output conflict. Cancellation prevents publishing partial output, but providers and third-party decryptors without cancellation support must finish their in-flight call first.

Before merging, manually check navigation, language/theme changes, local lyrics and cover downloads, cancel/restart, QR login/logout/restart, settings save/restart, and KGG with a real matching database. Synthetic decryption tests do not establish compatibility with every client format.


# Donation
[爱发电](https://afdian.net/a/zony-lrc-tools)

# Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=real-zony/ZonyLrcToolsX&type=Timeline)](https://star-history.dera.page/#real-zony/ZonyLrcToolsX&Timeline)
