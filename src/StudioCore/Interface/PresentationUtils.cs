﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface
{
    public static class PresentationUtils
    {
        public static string GetTagListString(List<string> refTagList)
        {
            var tagListStr = "";

            if (refTagList.Count > 0)
            {
                var tagStr = refTagList[0];
                foreach (var entry in refTagList.Skip(1))
                {
                    tagStr = $"{tagStr},{entry}";
                }
                tagListStr = tagStr;
            }
            else
            {
                tagListStr = "";
            }

            return tagListStr;
        }

        public static List<String> GetTagList(string tags)
        {
            var list = new List<string>();

            if(tags.Contains(","))
            {
                list = tags.Split(',').ToList();
            }
            else
            {
                list.Add(tags);
            }

            return list;
        }
    }
}
