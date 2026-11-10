New Features:
- [#172] Complete the Avalonia desktop workflow and add KGG decryption using a matching KuGou database.

Breaking Changes:
- Existing decrypted audio is preserved and reported as an output conflict instead of being overwritten or reported as newly decrypted.

Enhancement:
- Align new C# code with Chinese XML documentation, UTC naming and cancellation-token conventions.
- Preserve user YAML fields/comments and restrict Unix login-session permissions.
- Include third-party notices in distributions and update Tmds.DBus.Protocol to the security-fixed 0.21.3 backport.

Fixed Bugs:  

- [#167] Handle macOS code signing issues. (处理 macOS 代码签名问题。) 
- [#172] Correct KGG/KGM detection, QMC2 map decoding, unpadded ekey decoding and concurrent output handling.
- [#172] Keep decrypted key databases in memory; prevent canceled downloads from publishing partial files.
- [#172] Fix QR login races, stale scan results, desktop task threading and trimmed session serialization.
- Fix release authentication by using the built-in GitHub token with `contents: write` permission scoped to the release job.