﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Tools
{

    public static class AllSkeletonBoneGroups
    {
        public static string ExportBasePath = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\All Skeleton Groups\\";

        public static List<string> AllSkeletonGroupFiles = new List<string>();

        public static bool RefreshAllSkeletonGroupList = true;

        public static string _selectedAllSkeletonGroup = "";

        public static AllSkeletonList SelectedAllSkeletonList;

        public static void DisplaySubMenu(ModelEditorScreen screen)
        {
            UpdateAllSkeletonGroupList();

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Replace"))
            {
                foreach (var entry in AllSkeletonGroupFiles)
                {
                    ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedAllSkeletonGroup = entry;
                        SelectedAllSkeletonList = ReadAllSkeletonGroup(entry);

                        var action = new ReplaceAllSkeletonBoneList(screen, SelectedAllSkeletonList.List);
                        screen.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Append"))
            {
                foreach (var entry in AllSkeletonGroupFiles)
                {
                    ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
                    if (ImGui.MenuItem($"{entry}##menuItem{entry}"))
                    {
                        _selectedAllSkeletonGroup = entry;
                        SelectedAllSkeletonList = ReadAllSkeletonGroup(entry);

                        var action = new AppendAllSkeletonBoneList(screen, SelectedAllSkeletonList.List);
                        screen.EditorActionManager.ExecuteAction(action);
                    }
                }

                ImGui.EndMenu();
            }
        }

        public static void UpdateAllSkeletonGroupList()
        {
            if (RefreshAllSkeletonGroupList)
            {
                RefreshAllSkeletonGroupList = false;

                AllSkeletonGroupFiles = new List<string>();

                if (Directory.Exists(ExportBasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(ExportBasePath, "*.json"))
                    {
                        var fileName = Path.GetFileName(file);
                        AllSkeletonGroupFiles.Add(fileName.Replace(".json", ""));
                    }
                }
            }
        }

        public static void DisplayConfiguration(ModelEditorScreen screen)
        {
            var sectionWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();
            var defaultButtonSize = new Vector2(sectionWidth, 32);

            ImguiUtils.WrappedText("Create a stored All Skeleton Bone Group from your current selection with the All Skeleton Bone list.");
            ImguiUtils.WrappedText("A stored group can then be used to replace the existing All Skeleton Bone list, or appended to the end.");
            ImguiUtils.WrappedText("");

            UpdateAllSkeletonGroupList();

            if (ImGui.Button("Create All Skeleton Bone Group", defaultButtonSize))
            {
                if (screen.ModelHierarchy._selectedAllSkeletonBone != -1 ||
                    screen.ModelHierarchy.AllSkeletonMultiselect.StoredIndices.Count > 0)
                {
                    ImGui.OpenPopup($"##AllSkeletonGroupCreation");
                }
            }
            if (ImGui.BeginPopup("##AllSkeletonGroupCreation"))
            {
                DisplayCreationModal(screen);

                ImGui.EndPopup();
            }

            ImGui.Columns(2);

            ImGui.BeginChild("##AllSkeletonGroupSelection");

            foreach (var entry in AllSkeletonGroupFiles)
            {
                if (ImGui.Selectable($"{entry}##AllSkeletonGroup{entry}", entry == _selectedAllSkeletonGroup))
                {
                    _selectedAllSkeletonGroup = entry;
                    SelectedAllSkeletonList = ReadAllSkeletonGroup(entry);
                }
                if (_selectedAllSkeletonGroup == entry)
                {
                    if (ImGui.BeginPopupContextItem($"##AllSkeletonSelectionPopup{entry}"))
                    {
                        if (ImGui.Selectable("Delete"))
                        {
                            DeleteAllSkeletonGroup(entry);
                        }
                        ImguiUtils.ShowHoverTooltip("Delete this All Skeleton Bone group.");

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("##AllSkeletonGroupActions");

            var width = ImGui.GetWindowWidth();
            var buttonWidth = width;

            if (_selectedAllSkeletonGroup != "" && SelectedAllSkeletonList != null)
            {
                if (ImGui.CollapsingHeader("All Skeleton Bones in Group"))
                {
                    for (int i = 0; i < SelectedAllSkeletonList.List.Count; i++)
                    {
                        var nodeIndex = SelectedAllSkeletonList.List[i].NodeIndex;
                        var alias = "";
                        if (nodeIndex < screen.ResourceHandler.CurrentFLVER.Nodes.Count && nodeIndex > -1)
                            alias = screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                        if (ImGui.Selectable($"All Skeleton Bone {i} - {alias}"))
                        {

                        }
                    }
                }

                if (ImGui.Button("Replace", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new ReplaceAllSkeletonBoneList(screen, SelectedAllSkeletonList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Replace the existing All Skeleton Bones with the All Skeleton Bones within this All Skeleton group.");
                ImGui.SameLine();
                if (ImGui.Button("Append", new Vector2(buttonWidth / 2, 32)))
                {
                    var action = new AppendAllSkeletonBoneList(screen, SelectedAllSkeletonList.List);
                    screen.EditorActionManager.ExecuteAction(action);
                }
                ImguiUtils.ShowHoverTooltip("Append All Skeleton Bones within this All Skeleton Bone group to the existing All Skeletons Bones list.");
            }

            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private static string _createAllSkeletonGroupName = "";

        private static void DisplayCreationModal(ModelEditorScreen screen)
        {
            var width = ImGui.GetWindowWidth();
            var buttonWidth = width / 100 * 95;

            ImGui.InputText("Name##AllSkeletonGroupName", ref _createAllSkeletonGroupName, 255);
            ImguiUtils.ShowHoverTooltip("The name of the All Skeleton group.");

            if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
            {
                CreateAllSkeletonGroup(screen, _createAllSkeletonGroupName);
                ImGui.CloseCurrentPopup();
            }
        }

        public static void CreateAllSkeletonGroup(ModelEditorScreen screen, string filename)
        {
            AllSkeletonList newAllSkeletonList = new AllSkeletonList();

            for (int i = 0; i < screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons.Count; i++)
            {
                var curAllSkeleton = screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[i];

                if (screen.ModelHierarchy.AllSkeletonMultiselect.StoredIndices.Count > 0)
                {
                    for (int j = 0; j < screen.ModelHierarchy.AllSkeletonMultiselect.StoredIndices.Count; j++)
                    {
                        if (j == i)
                        {
                            newAllSkeletonList.List.Add(curAllSkeleton);
                        }
                    }
                }
                else
                {
                    if (i == screen.ModelHierarchy._selectedAllSkeletonBone)
                    {
                        newAllSkeletonList.List.Add(curAllSkeleton);
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(newAllSkeletonList, FLVERAllSkeletonListContext.Default.AllSkeletonList);

            WriteAllSkeletonGroup($"{filename}.json", jsonString);
        }

        public static AllSkeletonList ReadAllSkeletonGroup(string entry)
        {
            var newAllSkeletonList = new AllSkeletonList();
            var readPath = $"{ExportBasePath}\\{entry}.json";

            try
            {
                var jsonString = File.ReadAllText(readPath);
                newAllSkeletonList = JsonSerializer.Deserialize<AllSkeletonList>(jsonString, FLVERAllSkeletonListContext.Default.AllSkeletonList);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }

            return newAllSkeletonList;
        }

        public static void WriteAllSkeletonGroup(string filename, string jsonString)
        {
            var writePath = Path.Combine(ExportBasePath, $"{filename}");

            if (!Directory.Exists(ExportBasePath))
            {
                Directory.CreateDirectory(ExportBasePath);
            }

            var proceed = true;

            if (File.Exists(writePath))
            {
                var result = PlatformUtils.Instance.MessageBox($"{filename} already exists as a All Skeleton Bone Group. Are you sure you want to overwrite it?", "Warning", MessageBoxButtons.OKCancel);

                if (result is DialogResult.Cancel)
                {
                    proceed = false;
                }
            }

            if (proceed)
            {
                try
                {
                    var fs = new FileStream(writePath, System.IO.FileMode.Create);
                    var data = Encoding.ASCII.GetBytes(jsonString);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Dispose();

                    TaskLogs.AddLog($"Saved All Skeleton Bone Group: {writePath}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }

                RefreshAllSkeletonGroupList = true;
            }
        }

        public static void DeleteAllSkeletonGroup(string name)
        {
            var filepath = Path.Combine(ExportBasePath, $"{name}.json");

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            RefreshAllSkeletonGroupList = true;
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
    [JsonSerializable(typeof(AllSkeletonList))]
    [JsonSerializable(typeof(FLVER2.SkeletonSet.Bone))]
    internal partial class FLVERAllSkeletonListContext : JsonSerializerContext
    {
    }
    public class AllSkeletonList
    {
        public List<FLVER2.SkeletonSet.Bone> List { get; set; }

        public AllSkeletonList()
        {
            List = new List<FLVER2.SkeletonSet.Bone>();
        }
    }
}
