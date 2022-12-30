# Lithographer

Tool that adds an image file to a music file, using ffmpeg.

### Requirements

- [FNA](https://github.com/FNA-XNA/FNA) and [ImGui.NET](https://github.com/mellinoe/ImGui.NET) cloned **next to** the project folder (not in)
- (Windows) Git Bash
- (Windows) .NET Framework 4.6.2
- (Linux) Mono (preferrably from [the upstream mono-project repos](https://www.mono-project.com/download/stable/))
- (Linux) A distro that isn't Arch Linux, Arch Linux has terrible Mono packages (recommended distro: Fedora)

### Building

1. `./setup.sh` to download natives (run in Git Bash on Windows and your preferred shell on Linux)
2. `msbuild` (or the build button in your IDE)

### Running

On Linux: Remember to set `LD_LIBRARY_PATH` to `(project exe directory)/lib64`

### Distribution

Run `./package.sh` to produce Windows and Linux distributions.
