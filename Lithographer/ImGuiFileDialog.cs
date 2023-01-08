#region License

/* Copyright (c) 2023 darkerbit
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

#endregion

#region Using statements

using System;
using System.IO;
using System.Numerics;

using ImGuiNET;

#endregion

namespace Lithographer
{
	public class ImGuiFileDialog
	{
		private string curPath;

		private string lastFullPath;

		private string curFilename;
		private string curDrive;

		private readonly DriveInfo[] drives = DriveInfo.GetDrives();

		private readonly Action<string> action;

		private string[] directories;
		private string[] files;

		private bool pathError;

		public ImGuiFileDialog(string curPath, Action<string> action)
		{
			if (String.IsNullOrWhiteSpace(curPath))
			{
				this.curPath = Directory.GetCurrentDirectory();
				curFilename = "";
			}
			else
			{
				this.curPath = Path.GetDirectoryName(Path.GetFullPath(curPath)) ?? "";
				curFilename = Path.GetFileName(curPath);

				if (!Directory.Exists(this.curPath))
				{
					this.curPath = Directory.GetCurrentDirectory();
				}
			}

			UpdateListing();

			curDrive = Directory.GetDirectoryRoot(this.curPath);

			this.action = action;
		}

		private void UpdateListing()
		{
			try
			{
				directories = Directory.GetDirectories(curPath);
				files = Directory.GetFiles(curPath);

				Array.Sort(directories);
				Array.Sort(files);
			}
			catch (Exception)
			{
				// left empty
			}

			lastFullPath = curPath;
		}

		public void Update()
		{
			bool stillOpen = true;
			if (!ImGui.BeginPopupModal("Open file...", ref stillOpen,
				ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.HorizontalScrollbar))
			{
				return;
			}

			if (ImGui.BeginCombo("Drive", curDrive))
			{
				foreach (DriveInfo drive in drives)
				{
					if (ImGui.Selectable(drive.Name, drive.Name == curDrive))
					{
						curPath = drive.Name;
						curDrive = drive.Name;

						pathError = false;

						UpdateListing();
					}
				}

				ImGui.EndCombo();
			}

			if (pathError)
			{
				ImGui.TextColored(new Vector4(1, 0, 0, 1), "Invalid path!");
			}

			if (ImGui.InputText("Path", ref curPath, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
			{
				pathError = !Directory.Exists(curPath);

				if (!pathError)
				{
					UpdateListing();
				}
			}

			ImGui.SameLine();

			if (ImGui.Button("Up"))
			{
				curPath = Path.GetFullPath(Path.Combine(lastFullPath, "../"));
				pathError = false;
				UpdateListing();
			}

			bool dirty = false;
			if (ImGui.BeginChild("Listing##" + lastFullPath, new Vector2(400, 200), true,
				ImGuiWindowFlags.HorizontalScrollbar))
			{
				foreach (string dir in directories)
				{
					if (ImGui.Selectable(Path.GetFileName(dir) + '/'))
					{
						curPath = dir;
						pathError = false;
						curDrive = Directory.GetDirectoryRoot(curPath);
						dirty = true;
					}
				}

				foreach (string file in files)
				{
					string name = Path.GetFileName(file);
					if (ImGui.Selectable(name, name.Equals(curFilename), ImGuiSelectableFlags.AllowDoubleClick))
					{
						curFilename = name;

						if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
						{
							action(Path.Combine(curPath, curFilename));
							ImGui.CloseCurrentPopup();
						}
					}
				}
			}

			ImGui.EndChild();

			if (dirty)
			{
				UpdateListing();
			}

			if (ImGui.InputText("Filename", ref curFilename, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
			{
				action(Path.Combine(curPath, curFilename));
				ImGui.CloseCurrentPopup();
			}

			ImGui.SameLine();

			if (ImGui.Button("Open"))
			{
				action(Path.Combine(curPath, curFilename));
				ImGui.CloseCurrentPopup();
			}

			ImGui.SameLine();

			if (ImGui.Button("Cancel"))
			{
				ImGui.CloseCurrentPopup();
			}

			ImGui.EndPopup();
		}
	}
}
