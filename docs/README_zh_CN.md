[English](../README.md) | [中文](README_zh_CN.md)

---

# 功能简介

DesktopImageFilter 是一个 C# 命令行工具，可以简单地筛选出当前文件夹下适合做你电脑壁纸的那些图片，并移动到新文件夹中。

# 使用方法
- `--height <值>`：图片高度需大于你的屏幕高度的倍数（默认0.9）
- `--width <值>`：图片宽度需大于你的屏幕宽度的倍数（默认0.9）
- `--ratio <值>`：图片宽高比与你的屏幕宽高比的容差（默认0.3）

参数可选，未指定时使用默认值。

```sh
# 使用默认参数
./a.exe
# 指定部分参数
./a.exe --height 0.85 --width 0.7
# 指定全部参数
./a.exe --height 0.85 --width 0.7 --ratio 0.2
```

以 `./a.exe --height 0.85 --width 0.7 --ratio 0.2` 为例，当这张照片同时满足以下三点，才会被认为是符合要求的。

1. 这张图片的高度至少是你当前屏幕高度的 `0.85` 倍
2. 这张图片的宽度至少是你当前屏幕高度的 `0.7` 倍
3. 这张图片的`宽与高的比值` 与 `你当前的屏幕的宽与高的比值` 之差的绝对值小于 `0.2`

# 编译

## Windows
```sh
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:EnableCompressionInSingleFile=true
```
## macOS
```sh
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:EnableCompressionInSingleFile=true
```
## Linux
```sh
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:EnableCompressionInSingleFile=true
```