New Features: None
Breaking Changes: None  
Enhancement:  : 

- 网易云歌单下载歌词时，支持使用 `;` 传递多个歌单 ID。(`15445646;468435123;4131357`) - [`41cba02`](https://github.com/real-zony/ZonyLrcToolsX/commit/41cba028333d0cf65f7dc3ee466660cc20e558fc)
- 优化了结果提示信息，更加明确。- [`1e5c418`](https://github.com/real-zony/ZonyLrcToolsX/commit/1e5c41852f70df4b824d2795f175e57c82a0d104)
- 优化了程序的启动时间，不会每次启动都去检测更新了。- [`b240564`](https://github.com/real-zony/ZonyLrcToolsX/commit/b240564cf7ac3432715dd54c280d73f9f985c4fc)

Fixed Bugs:  

- 更新了依赖的 NuGet 程序包版本。- [`1f312c7`](https://github.com/real-zony/ZonyLrcToolsX/commit/1f312c749d8e7784e7670ec3821db0b4b430ce2e)
- 过滤了包含特殊符号的文件路径，避免报错。 - [`62d08df`](https://github.com/real-zony/ZonyLrcToolsX/commit/62d08df7353343711741b23a6ecb9c3908754075)
- 修复了从网易云歌单下载歌曲时，将专辑名称当作歌手名称进行取值。 - [`98c935e`](https://github.com/real-zony/ZonyLrcToolsX/commit/98c935ed93f9278e8606c056868824ea8335ee5d)