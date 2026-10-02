# Keystria

Keystria is a lightweight Windows utility that lets you control the mouse using your keyboard.

## Features

- Global keyboard control
- WASD mouse movement
- Arrow-key mouse movement
- Keyboard-controlled left and right clicking
- Customizable keybinds
- Keybind conflict detection
- Mouse speed control from 1 to 30
- Restore Defaults
- Lightweight single-file Windows release
- Settings interface
- Subtle Settings UI animations
- Custom application icon

## Default Controls

- Ctrl + X - Enable / Disable
- W - Move Up
- A - Move Left
- S - Move Down
- D - Move Right
- Up Arrow - Move Up
- Left Arrow - Move Left
- Down Arrow - Move Down
- Right Arrow - Move Right
- Q - Left Click
- E - Right Click

## Download

Download `Keystria-Portable.zip` from the Releases section.

The ZIP contains the portable `Keystria.exe` application and a requirements README.

## Requirements

- Windows x64
- .NET 8 Windows Desktop Runtime

Keystria is currently framework-dependent. The .NET runtime is therefore not bundled with the application.

## Installation

1. Download `Keystria-Portable.zip`.
2. Extract the archive.
3. Run `Keystria.exe`.

## Development

Keystria is built with:

- C#
- .NET 8
- WPF
- Windows x64

### Build

`dotnet build`

### Publish

`dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true`

## Project Structure

- Assets
- Commands
- Core
- Input
- Interaction
- Services
- MainWindow.xaml
- MainWindow.xaml.cs
- KeyboardControl.csproj
- README.md

## Status

Keystria is an active personal project focused on keyboard-driven mouse control for Windows.

## License

No license has been specified yet.
