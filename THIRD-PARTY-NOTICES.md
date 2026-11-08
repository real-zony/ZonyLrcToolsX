# Third-Party Notices

This project contains code derived from the following MIT-licensed works.
The MIT licence requires the copyright notice and permission notice to be
included in all copies or substantial portions of the software, so they are
reproduced in full below.

---

## kgg-dec — KuGou `.kgg` decryption reference implementation

Source: <https://github.com/lizipeng0013/kgg-dec.mirror>

Used by: `src/ZonyLrcTools.Common/MusicDecryption/Kugou/`
(`TcTea.cs`, `EkeyDecoder.cs`, `Qmc2Cipher.cs`, `KugouEkeyDatabase.cs`,
`KggMusicDecryptor.cs`)

The C# classes above are a port of the TC-TEA / ekey / QMC2 / KGMusicV3.db
algorithms from that project. They were rewritten for this codebase (different
structure, streaming I/O, cancellation support), but the cryptographic
constants and the database page-decryption scheme originate there.

```
MIT License

Copyright (c) 2024 LSR

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
