#!/bin/bash

set -e

msbuild /p:Configuration=Release /p:Platform=x64

mkdir -p out/{windows,linux}

cp -r Lithographer/bin/x64/Release/* out/windows
rm -rf out/windows/{lib64,{cimgui,FAudio,FNA3D,libtheorafile,SDL2}.dll,ffmpeg,ffmpeg.exe}

cp -r out/windows/* out/linux

# Windows
cp -r natives/x64/* out/windows
cp ffmpeg/ffmpeg.exe out/windows

cd out/windows
zip -r ../LithographerWin.zip *

cd ../..

# Linux
cp -r natives/lib64 out/linux
cp ffmpeg/ffmpeg out/linux
cp /usr/lib/mono/4.5/Facades/System.Runtime.dll out/linux
cp -r ../MonoKickstart/precompiled/* out/linux
mv out/linux/kick.bin.x86_64 out/linux/Lithographer.bin.x86_64
rm out/linux/kick.bin*

cd out/linux
tar -cJvf ../LithographerLinux.tar.xz *
