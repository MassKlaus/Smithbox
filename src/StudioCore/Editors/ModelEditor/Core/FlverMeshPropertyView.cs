using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Actions.Mesh;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class FlverMeshPropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverMeshPropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var index = Selection._selectedMesh;

        if (index == -1)
            return;

        if (Screen.ResManager.GetCurrentFLVER().Meshes.Count < index)
            return;

        if (Selection.MeshMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Meshes are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Mesh");
        ImGui.Separator();

        var entry = Screen.ResManager.GetCurrentFLVER().Meshes[index];

        var useBoneWeights = entry.UseBoneWeights;
        int dynamic = entry.Dynamic;
        var materialIndex = entry.MaterialIndex;
        var nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Bone Weights");
        UIHelper.ShowHoverTooltip("Determines how the mesh is skinned.\nIf it is true the mesh is assumed to be in bind pose and is skinned using the Bone Indices and Bone Weights of the vertices.\n\nIf it is false each vertex specifies a single node to bind to using its NormalW\n\nThe mesh is assumed to not be in bind pose and the transform of the bound node is applied to each vertex.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Material Index");
        UIHelper.ShowHoverTooltip("Index of the material used by all triangles in this mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        UIHelper.ShowHoverTooltip("Index of the node representing this mesh in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##UseBoneWeights", ref useBoneWeights);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.UseBoneWeights != useBoneWeights)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_UseBoneWeights(entry, entry.UseBoneWeights, useBoneWeights));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##MaterialIndex", ref materialIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.MaterialIndex != materialIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_MaterialIndex(entry, entry.MaterialIndex, materialIndex));
        }

        Decorator.MaterialIndexDecorator(materialIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NodeIndex != nodeIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_NodeIndex(entry, entry.NodeIndex, nodeIndex));
        }

        Decorator.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);

        if (ImGui.CollapsingHeader("Bounding Box"))
        {
            if (entry != null && entry.BoundingBox != null)
            {
                DisplayBoundingBoxProperties(entry.BoundingBox);
            }
        }

        if (ImGui.CollapsingHeader("Face Sets", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < entry.FaceSets.Count; i++)
            {
                if (entry != null && entry.FaceSets[i] != null)
                {
                    DisplayFaceSetProperties(entry.FaceSets[i], i);
                }
            }
        }

        if (ImGui.CollapsingHeader("Vertex Buffers"))
        {
            for (int i = 0; i < entry.VertexBuffers.Count; i++)
            {
                if (entry != null && entry.VertexBuffers[i] != null)
                {
                    DisplayVertexBufferProperties(entry.VertexBuffers[i], i);
                }
            }
        }

        if (ImGui.CollapsingHeader("UVs"))
        {
            DisplayUvProperties(entry.Vertices);
        }

        DisplayMeshAdjustments(entry);
    }

    private Vector3 StoredTranslationInput = new Vector3();
    private Vector3 StoredScaleInput = new Vector3();

    private float StoredRotationInput_X = 0.0f;
    private float StoredRotationInput_Y = 0.0f;
    private float StoredRotationInput_Z = 0.0f;

    private void DisplayMeshAdjustments(FLVER2.Mesh entry)
    {
        ImGui.Separator();
        UIHelper.WrappedText("Mesh Adjustments");
        ImGui.Separator();

        var curFlver = Screen.ResManager.GetCurrentFLVER();

        if (ImGui.BeginTable($"meshAdjustmentsTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Button", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button("Translate", new Vector2(150, 24)))
            {
                var action = new TranslateMesh(curFlver, entry, StoredTranslationInput);
                Screen.EditorActionManager.ExecuteAction(action);
            }
            UIHelper.ShowHoverTooltip("Translate the selected mesh by the specified vector.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputFloat3("##translateInput", ref StoredTranslationInput);

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button("Scale", new Vector2(150, 24)))
            {
                var action = new ScaleMesh(curFlver, entry, StoredScaleInput);
                Screen.EditorActionManager.ExecuteAction(action);
            }
            UIHelper.ShowHoverTooltip("Scale the selected mesh by the specified vector.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputFloat3("##scaleInput", ref StoredScaleInput);

            // Row 3
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button("Rotate X##rotateXbutton", new Vector2(150, 24)))
            {
                var action = new RotateMesh(curFlver, entry, StoredRotationInput_X, RotationAxis.X);
                Screen.EditorActionManager.ExecuteAction(action);
            }
            UIHelper.ShowHoverTooltip("Rotate the selected mesh on the X-axis by the specified angle.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputFloat("##rotateInputX", ref StoredRotationInput_X);

            // Y rotation doesn't work currently
            // Row 4
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button("Rotate Y##rotateYbutton", new Vector2(150, 24)))
            {
                var action = new RotateMesh(curFlver, entry, StoredRotationInput_Y, RotationAxis.Y);
                Screen.EditorActionManager.ExecuteAction(action);
            }
            UIHelper.ShowHoverTooltip("Rotate the selected mesh on the Y-axis by the specified angle.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputFloat("##rotateInputY", ref StoredRotationInput_Y);

            // Row 5
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.Button("Rotate Z##rotateZbutton", new Vector2(150, 24)))
            {
                var action = new RotateMesh(curFlver, entry, StoredRotationInput_Z, RotationAxis.Z);
                Screen.EditorActionManager.ExecuteAction(action);
            }
            UIHelper.ShowHoverTooltip("Rotate the selected mesh on the Z-axis by the specified angle.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            ImGui.InputFloat("##rotateInputZ", ref StoredRotationInput_Z);

            ImGui.EndTable();
        }

        // Draw a rectangle spanning two columns
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.GetContentRegionAvail();
        var color = ImGui.GetColorU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
        drawList.AddRectFilled(pos, new Vector2(pos.X + size.X, pos.Y + 200), color);
    }

    private void DisplayFaceSetProperties(FLVER2.FaceSet faceset, int index)
    {
        ImGui.Separator();
        if (ImGui.Selectable($"Face Set {index}##FaceSet{index}", Selection._subSelectedFaceSetRow == index))
        {
            Selection._subSelectedFaceSetRow = index;
        }

        if (Selection._subSelectedFaceSetRow == index)
        {
            ContextMenu.FaceSetHeaderContextMenu(index);
        }

        ImGui.Separator();

        var flags = (int)faceset.Flags;
        var triangleStrip = faceset.TriangleStrip;
        var cullBackfaces = faceset.CullBackfaces;
        int unk06 = faceset.Unk06;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        UIHelper.ShowHoverTooltip("Flags on a faceset, mostly just used to determine lod level.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Triangle Strip");
        UIHelper.ShowHoverTooltip("Whether vertices are defined as a triangle strip or individual triangles.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Cull Backfaces");
        UIHelper.ShowHoverTooltip("Whether triangles can be seen through from behind.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk06");
        UIHelper.ShowHoverTooltip("");

        ImGui.NextColumn();

        // TODO: handle the flags properly
        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags_faceset{index}", ref flags);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)faceset.Flags != flags)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_Flags(faceset, (int)faceset.Flags, flags));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##TriangleStrip_faceset{index}", ref triangleStrip);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (faceset.TriangleStrip != triangleStrip)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_TriangleStrip(faceset, faceset.TriangleStrip, triangleStrip));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##CullBackfaces_faceset{index}", ref cullBackfaces);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (faceset.CullBackfaces != cullBackfaces)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_CullBackfaces(faceset, faceset.CullBackfaces, cullBackfaces));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Unk06_faceset{index}", ref unk06);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (faceset.Unk06 != unk06)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_Unk06(faceset, faceset.Unk06, unk06));
        }

        ImGui.Columns(1);

        faceset.Flags = (FLVER2.FaceSet.FSFlags)flags;
        faceset.TriangleStrip = triangleStrip;
        faceset.CullBackfaces = cullBackfaces;

        if (unk06 > short.MaxValue)
            unk06 = short.MaxValue;

        faceset.Unk06 = (short)unk06;
    }

    private void DisplayVertexBufferProperties(FLVER2.VertexBuffer vertexBuffer, int index)
    {
        var layoutIndex = vertexBuffer.LayoutIndex;

        ImGui.Separator();
        if (ImGui.Selectable($"Vertex Buffer {index}##VertexBuffer{index}", Selection._subSelectedVertexBufferRow == index))
        {
            Selection._subSelectedVertexBufferRow = index;
        }

        if (Selection._subSelectedVertexBufferRow == index)
        {
            ContextMenu.VertexBufferHeaderContextMenu(index);
        }

        ImGui.Separator();

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Layout Index");
        UIHelper.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##LayoutIndex{index}", ref layoutIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (vertexBuffer.LayoutIndex != layoutIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_VertexBuffer_VertexBuffer(vertexBuffer, vertexBuffer.LayoutIndex, layoutIndex));
        }

        Decorator.LayoutIndexDecorator(layoutIndex);

        ImGui.Columns(1);

        vertexBuffer.LayoutIndex = layoutIndex;
    }

    // TODO: Use the editor action manager...
    // TODO: Cleanup (Pear)

    private int _selectedUvChannel = -1;

    private void MirrorUVs(IEnumerable<FLVER.Vertex> vertices, bool mirrorX)
    {
        int index = _selectedUvChannel;
        foreach (FLVER.Vertex vertex in vertices)
        {
            Vector3 v = vertex.UVs[index];
            Vector3 flippedVector = mirrorX
                ? v with { X = -v.X }
                : v with { Y = -v.Y };
            vertex.UVs[index] = flippedVector;
        }
    }

private void DisplayUvProperties(IReadOnlyList<FLVER.Vertex> vertices)
{
    List<string> uvChannelNames = new();
    for (int i = 0; i < vertices[0].UVs.Count; i++)
    {
        uvChannelNames.Add($"UV{i}");
    }

    // Render UV Channel Selection UI
    ImGui.Columns(2);
    ImGui.AlignTextToFramePadding();
    ImGui.Text("UV Channel");
    UIHelper.ShowHoverTooltip("Select which UV channel to edit.");
    ImGui.NextColumn();
    
    if (ImGui.BeginCombo($"##UVChannelSelector", _selectedUvChannel >= 0 ? uvChannelNames[_selectedUvChannel] : "None"))
    {
        foreach ((string uvName, int index) in uvChannelNames.Select((name, idx) => (name, idx)))
        {
            bool isSelected = (_selectedUvChannel == index);
            if (index != -1 && ImGui.Selectable(uvName, isSelected))
            {
                _selectedUvChannel = index;
            }
        }
        ImGui.EndCombo();
    }

    // Render UV Actions (Mirror Buttons)
    if (_selectedUvChannel >= 0)
    {
        if (ImGui.Button($"Mirror {uvChannelNames[_selectedUvChannel]} on X"))
        {
            MirrorUVs(vertices, true);
        }
        ImGui.SameLine();
        if (ImGui.Button($"Mirror {uvChannelNames[_selectedUvChannel]} on Y"))
        {
            MirrorUVs(vertices, false);
        }

        // For testing purposes: Show first vertex UV
        ImGui.NewLine();
        ImGui.Text($"First Vertex UV: {vertices[0].UVs[_selectedUvChannel]}");
    }

    // Render the Palette Grid (using CGRam data)
    var PalZoom = 4; // scaleable
    var CGRam = new uint[256]; // data (assuming it’s set somewhere)
    var drawList = ImGui.GetWindowDrawList();

    for (int r = 0; r < 16; r++)
    {
        for (int c = 0; c < 16; c++)
        {
            // Get position of the current grid cell
            var p = ImGui.GetCursorScreenPos();
            var x = p.X + (c * PalZoom);
            var y = p.Y + (r * PalZoom);

            // Draw the colored rectangle for this grid cell
            drawList.AddRectFilled(
                new Vector2(x, y), // rect start position
                new Vector2(x + PalZoom, y + PalZoom), // rect end position
                CGRam[(r * 16) + c]); // index into data directly
        }
    }

    // Dummy element to provide spacing for the grid
    ImGui.Dummy(new Vector2(PalZoom * 16, PalZoom * 16f));

    ImGui.Columns(1); // Reset columns back to default


        // Draw a rectangle spanning two columns
        var pos = ImGui.GetCursorScreenPos();
        var size = ImGui.GetContentRegionAvail();
        float UVHeight = size.X;
        drawList.AddRectFilled(pos, new Vector2(pos.X + size.X, pos.Y + UVHeight), ImGui.GetColorU32(ImGuiCol.Button));
        drawList.AddRect(pos, new Vector2(pos.X + size.X, pos.Y + UVHeight), ImGui.GetColorU32(ImGuiCol.Border));

        foreach (var point in vertices.SelectMany(x => x.UVs))
        {
            drawList.AddCircleFilled(new Vector2(pos.X + size.X * point.X, pos.Y + UVHeight * point.Y), 2.0f, 0xFF00FF00); // Green points
        }

        // Extend the scroll area by 200 pixels
        ImGui.Dummy(new Vector2(0, UVHeight));
}

private void DisplayBoundingBoxProperties(FLVER2.Mesh.BoundingBoxes boundingBox)
    {
        var bbMin = boundingBox.Min;
        var bbMax = boundingBox.Max;
        var bbUnk = boundingBox.Unk;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Minimum");
        UIHelper.ShowHoverTooltip("Minimum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Maximum");
        UIHelper.ShowHoverTooltip("Maximum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Unknown");
        UIHelper.ShowHoverTooltip("Unknown; only present in Sekiro.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Min", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Min != bbMin)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Min(boundingBox, boundingBox.Min, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Max", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Max != bbMax)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Max(boundingBox, boundingBox.Max, bbMax));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Unk", ref bbUnk);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Unk != bbUnk)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Unk(boundingBox, boundingBox.Unk, bbUnk));
        }

        ImGui.Columns(1);

        // Changes
        boundingBox.Min = bbMin;
        boundingBox.Max = bbMax;
        boundingBox.Unk = bbUnk;
    }
}

