﻿using StudioCore.Banks.FormatBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Banks.TextureBlockBank;
using StudioCore.Editor;
using StudioCore.Localization;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Banks.TextureBlockBank
{
    public class TextureBlockBank
    {
        public TextureBlockResource TextureBlocks { get; set; }

        private string AliasDirectory = "";

        private string AliasFileName = "";


        public TextureBlockBank()
        {
            AliasDirectory = "Mappings";
            AliasFileName = "BlockedTextures";
        }

        public void LoadBank()
        {
            try
            {
                TextureBlocks = BankUtils.LoadTextureBlockJSON(AliasDirectory, AliasFileName);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog(
                    $"{LOC.Get("TEXTURE_BLOCK_BANK__FAILED_TO_LOAD")}" +
                    $"{e.Message}");
            }

            TaskLogs.AddLog($"{LOC.Get("TEXTURE_BLOCK_BANK__SUCCESSFUL_LOAD")}");
        }

        public bool IsBlockedTexture(string virtualPath)
        {
            if (IsBankValid())
            {
                foreach (var entry in TextureBlocks.list)
                {
                    if (virtualPath.Contains(entry))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsBankValid()
        {
            if (TextureBlocks.list == null)
            {
                return false;
            }

            if (TextureBlocks == null)
            {
                return false;
            }

            return true;
        }
    }
}
