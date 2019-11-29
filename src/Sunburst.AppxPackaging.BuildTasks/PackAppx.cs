// Copyright (c) William Kent. All rights reserved.
// Licensed under the Apache License, version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#pragma warning disable CS8618 // Non-nullable field is uninitialized, but is checked by MSBuild, so will not actually cause an exception.

namespace Sunburst.AppxPackaging.BuildTasks
{
    public sealed class PackAppx : ToolTask
    {
        [Required]
        public ITaskItem ManifestFile { get; set; }

        [Required]
        public ITaskItem ResourceLayoutFile { get; set; }

        [Required]
        public ITaskItem OutputFile { get; set; }

        [Required]
        public string ToolsVersion { get; set; }

        protected override string ToolName => "makeappx.exe";

        protected override string GenerateFullPathToTool()
        {
            return $@"C:\Program Files (x86)\Windows Kits\10\bin\{ToolsVersion}\x86\makeappx.exe";
        }

        protected override string GenerateCommandLineCommands()
        {
            List<string> argv = new List<string>();

            argv.Add("pack");
            argv.Add("/h");
            argv.Add("SHA256");
            argv.Add("/m");
            argv.Add(ManifestFile.GetMetadata("FullPath"));
            argv.Add("/f");
            argv.Add(ResourceLayoutFile.GetMetadata("FullPath"));
            argv.Add("/p");
            argv.Add(OutputFile.GetMetadata("FullPath"));
            argv.Add("/o");

            return string.Join(" ", argv.Select(x => $"\"{x}\""));
        }
    }
}
