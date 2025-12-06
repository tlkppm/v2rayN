# v2rayN Maintained Fork

> **This is a maintained fork of [2dust/v2rayN](https://github.com/2dust/v2rayN)**

A GUI client for Windows, Linux and macOS, support [Xray](https://github.com/XTLS/Xray-core)
and [sing-box](https://github.com/SagerNet/sing-box)
and [others](https://github.com/2dust/v2rayN/wiki/List-of-supported-cores)

[![GitHub Releases](https://img.shields.io/github/downloads/tlkppm/v2rayN/latest/total?logo=github)](https://github.com/tlkppm/v2rayN/releases)

## Maintenance Changes (v7.16.5-tlkppm.1)

- **Security**: Fixed SQL injection vulnerability in ProfileItems query
- **Architecture**: Split ConfigHandler.cs into 6 partial classes for better maintainability
- **Reliability**: Added Geo rule source fallback mechanism (auto-switch on download failure)
- **Bug Fix**: Fixed "Bypass Mainland (Whitelist)" routing missing default proxy rule
- **Dependencies**: Updated ReactiveUI to 22.3.1, ZXing to 0.16.22
- **Testing**: Added unit test project with 33 tests
- **Code Quality**: Extracted Repository data access layer

## How to use

Read the [Wiki](https://github.com/2dust/v2rayN/wiki) for details.

## Credits

- Original project: [2dust/v2rayN](https://github.com/2dust/v2rayN)
- Xray-core: [XTLS/Xray-core](https://github.com/XTLS/Xray-core)
- sing-box: [SagerNet/sing-box](https://github.com/SagerNet/sing-box)
