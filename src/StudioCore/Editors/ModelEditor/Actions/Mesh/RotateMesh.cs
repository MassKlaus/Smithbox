﻿using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class RotateMesh : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private List<FLVER.Vertex> OriginalVertices;
    private FLVER2.Mesh CurrentMesh;
    private float ChangeAngle;
    private RotationAxis ChangeAxis;

    public RotateMesh(FLVER2 curFlver, FLVER2.Mesh curMesh, float angle, RotationAxis axis)
    {
        CurrentFLVER = curFlver;

        OriginalVertices = new List<FLVER.Vertex>();
        foreach (var entry in curMesh.Vertices)
        {
            OriginalVertices.Add(entry.Clone());
        }

        CurrentMesh = curMesh;
        ChangeAngle = angle;
        ChangeAxis = axis;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        VertexUtils.RotateMesh(CurrentMesh, ChangeAngle, ChangeAxis);

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.Vertices = OriginalVertices;

        Smithbox.EditorHandler.ModelEditor.ViewportManager.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}