[English](README.md) | [中文](docs/README_zh_CN.md)

---

# Introduction

DesktopImageFilter is a C# command-line tool that easily filters out images from the current folder that are suitable for use as your computer wallpaper and moves them to a new folder.

# Parameters
- `--height <value>`: Image height must be greater than this ratio of your screen height (default 0.9)
- `--width <value>`: Image width must be greater than this ratio of your screen width (default 0.9)
- `--ratio <value>`: Allowed difference between image and your screen aspect ratio (default 0.3)

All parameters are optional. Defaults are used if not specified.

# Usage 
```sh
# Use default parameters
./a.exe
# Specify some parameters
./a.exe --height 0.85 --width 0.7
# Specify all parameters
./a.exe --height 0.85 --width 0.7 --ratio 0.2
```

For example, with the command `./a.exe --height 0.85 --width 0.7 --ratio 0.2`, an image will only be considered suitable if it meets all of the following three conditions:

1. The image's height is at least `0.85` times the height of your current screen.
2. The image's width is at least `0.7` times the height of your current screen.
3. The absolute difference between the image's `width-to-height ratio` and your current screen's `width-to-height ratio` is less than `0.2`.


# Build

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