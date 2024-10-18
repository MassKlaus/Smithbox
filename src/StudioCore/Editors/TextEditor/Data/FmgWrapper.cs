﻿using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Data;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(FmgWrapper))]
[JsonSerializable(typeof(FMG))]
[JsonSerializable(typeof(FMG.Entry))]

public class FmgWrapper
{
    public string Name { get; set; }

    public FMG Fmg { get; set; }

    public bool Partial { get; set; }

    public FmgWrapper()
    {

    }
}