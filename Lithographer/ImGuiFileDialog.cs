using System;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace Lithographer
{
	public class ImGuiFileDialog
	{
		private string _curPath;

		private string _lastFullPath;

		private string _curFilename;
		private string _curDrive;

		private readonly DriveInfo[] _drives = DriveInfo.GetDrives();

		private readonly Action<string> _action;

		private string[] _directories;
		private string[] _files;

		private bool _pathError = false;

		public ImGuiFileDialog(string curPath, Action<string> action)
		{
			if (String.IsNullOrWhiteSpace(curPath))
			{
				_curPath = Directory.GetCurrentDirectory();
				_curFilename = "";
			}
			else
			{
				_curPath = Path.GetDirectoryName(Path.GetFullPath(curPath)) ?? "";
				_curFilename = Path.GetFileName(curPath);

				if (!Directory.Exists(_curPath))
				{
					_curPath = Directory.GetCurrentDirectory();
				}
			}

			UpdateListing();

			_curDrive = Directory.GetDirectoryRoot(_curPath);

			_action = action;
		}

		private void UpdateListing()
		{
			try
			{
				_directories = Directory.GetDirectories(_curPath);
				_files = Directory.GetFiles(_curPath);

				Array.Sort(_directories);
				Array.Sort(_files);
			}
			catch (Exception e)
			{
				// left empty
			}

			_lastFullPath = _curPath;
		}

		public void Update()
		{
			var stillOpen = true;
			if (!ImGui.BeginPopupModal("Open file...", ref stillOpen,
				    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.HorizontalScrollbar)) return;

			if (ImGui.BeginCombo("Drive", _curDrive))
			{
				foreach (var drive in _drives)
				{
					if (ImGui.Selectable(drive.Name, drive.Name == _curDrive))
					{
						_curPath = drive.Name;
						_curDrive = drive.Name;

						_pathError = false;

						UpdateListing();
					}
				}

				ImGui.EndCombo();
			}

			if (_pathError)
			{
				ImGui.TextColored(new Vector4(1, 0, 0, 1), "Invalid path!");
			}

			if (ImGui.InputText("Path", ref _curPath, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
			{
				_pathError = !Directory.Exists(_curPath);

				if (!_pathError)
				{
					UpdateListing();
				}
			}

			ImGui.SameLine();

			if (ImGui.Button("Up"))
			{
				_curPath = Path.GetFullPath(Path.Combine(_lastFullPath, "../"));
				_pathError = false;
				UpdateListing();
			}

			var dirty = false;
			if (ImGui.BeginChild("Listing##" + _lastFullPath, new Vector2(400, 200), true,
				    ImGuiWindowFlags.HorizontalScrollbar))
			{
				foreach (var dir in _directories)
				{
					if (ImGui.Selectable(dir + '/'))
					{
						_curPath = dir;
						_pathError = false;
						_curDrive = Directory.GetDirectoryRoot(_curPath);
						dirty = true;
					}
				}

				foreach (var file in _files)
				{
					var name = Path.GetFileName(file);
					if (ImGui.Selectable(name, name.Equals(_curFilename), ImGuiSelectableFlags.AllowDoubleClick))
					{
						_curFilename = name;

						if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
						{
							_action(Path.Combine(_curPath, _curFilename));
							ImGui.CloseCurrentPopup();
						}
					}
				}
			}

			ImGui.EndChild();

			if (dirty)
				UpdateListing();

			if (ImGui.InputText("Filename", ref _curFilename, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
			{
				_action(Path.Combine(_curPath, _curFilename));
				ImGui.CloseCurrentPopup();
			}

			ImGui.SameLine();

			if (ImGui.Button("Open"))
			{
				_action(Path.Combine(_curPath, _curFilename));
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
