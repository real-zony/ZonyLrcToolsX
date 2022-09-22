New Features: 

- [[#114](https://github.com/real-zony/ZonyLrcToolsX/issues/114)] 下载双语歌词的时候，可以控制是否仅输出翻译歌词。
- [[6d7ee04](https://github.com/real-zony/ZonyLrcToolsX/commit/6d7ee04b741191b5526075cdd3c3fc2c5737880f)] 提供深度搜索选项，以便更加准确地匹配歌曲歌词，值越大结果越准确，但是搜索速度越慢。

Breaking Changes: None

Enhancement: 

- 关于网易云歌词下载器在搜索歌曲名带括号的歌曲时，会搜索不到的情况，现在默认会移除掉歌曲名中的括号进行搜索以增强准确性。
- [[#100](https://github.com/real-zony/ZonyLrcToolsX/issues/100)] 优化歌曲查找逻辑，优先匹配长度符合实际歌曲长度的歌词。

Fixed Bugs: 

- [[#106](https://github.com/real-zony/ZonyLrcToolsX/issues/106)] 跳过已存在歌词的选项存在逻辑问题，会导致下载专辑图像的时候也会跳过带歌词的歌曲文件。
- [[85f325b](https://github.com/real-zony/ZonyLrcToolsX/commit/85f325b300dd4e6f54c783b2f9065ab4c566e3ad)] 修复某些极端情况下网易云歌词下载器报错的问题。
