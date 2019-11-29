// Copyright (c) William Kent. All rights reserved.
// Licensed under the Apache License, version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sunburst.AppxPackaging.BuildTasks
{
    public sealed class CreatePriConfig : ToolTask
    {
        [Required]
        public string[] LanguageQualifiers { get; set; }
        [Required]
        public ITaskItem ConfigFilePath { get; set; }
        [Required]
        public string ToolsVersion { get; set; }

        protected override string ToolName => "makepri.exe";

        protected override string GenerateFullPathToTool()
        {
            return $@"C:\Program Files (x86)\Windows Kits\10\bin\{ToolsVersion}\x86\makepri.exe";
        }

        protected override string GenerateCommandLineCommands()
        {
            StringBuilder qualifierString = new StringBuilder();
            qualifierString.Append(string.Join("_", LanguageQualifiers.Where(x => !string.IsNullOrWhiteSpace(x))));

            List<string> argv = new List<string>();
            argv.Add("createconfig");
            argv.Add("/cf");
            argv.Add(ConfigFilePath.GetMetadata("FullPath"));
            argv.Add("/dq");
            argv.Add(qualifierString.ToString());
            argv.Add("/pv");
            argv.Add("10.0");
            argv.Add("/o");

            return string.Join(" ", argv.Select(arg => $"\"{arg}\""));
        }

        protected override Encoding StandardOutputEncoding => Encoding.Unicode;
        protected override Encoding StandardErrorEncoding => Encoding.Unicode;
    }
}
