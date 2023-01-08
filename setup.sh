#!/bin/bash

# Exit immediately on error
set -e

mkdir -p natives
cd natives

echo -e "\e[32mDownloading fnalibs...\e[m"
curl -O https://fna.flibitijibibo.com/archive/fnalibs.tar.bz2

echo -e "\e[32mExtracting fnalibs...\e[m"
tar -xf fnalibs.tar.bz2

cd ..

echo -e "\e[32mCopying ImGui.NET natives...\e[m"
cp lib/ImGui.NET/deps/cimgui/win-x64/cimgui.dll natives/x64/
cp lib/ImGui.NET/deps/cimgui/linux-x64/cimgui.so natives/lib64/

cd ffmpeg

echo -e "\e[32mExtracting ffmpeg...\e[m"
tar -xf ffmpeg.tar.xz

echo -e "\e[32mDone!\e[m"
