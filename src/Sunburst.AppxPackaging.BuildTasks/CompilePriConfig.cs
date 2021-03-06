﻿// Copyright (c) William Kent. All rights reserved.
// Licensed under the Apache License, version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#pragma warning disable CS8618 // Non-nullable field is uninitialized, but is checked by MSBuild, so will not actually cause an exception.

namespace Sunburst.AppxPackaging.BuildTasks
{
    public sealed class CompilePriConfig : ToolTask
    {
        [Required]
        public ITaskItem PackageLayout { get; set; }

        [Required]
        public ITaskItem ConfigFile { get; set; }

        [Required]
        public ITaskItem OutputFile { get; set; }

        [Required]
        public string ToolsVersion { get; set; }

        protected override string ToolName => "makepri.exe";

        protected override Encoding StandardOutputEncoding => Encoding.Unicode;

        protected override Encoding StandardErrorEncoding => Encoding.Unicode;

        protected override string GenerateFullPathToTool()
        {
            return $@"C:\Program Files (x86)\Windows Kits\10\bin\{ToolsVersion}\x86\makepri.exe";
        }

        protected override string GenerateCommandLineCommands()
        {
            List<string> argv = new List<string>();

            argv.Add("new");
            argv.Add("/pr");
            argv.Add(GetFullPath(PackageLayout));
            argv.Add("/cf");
            argv.Add(GetFullPath(ConfigFile));
            argv.Add("/of");
            argv.Add(GetFullPath(OutputFile));
            argv.Add("/mf");
            argv.Add("AppX");
            argv.Add("/o");

            return string.Join(" ", argv.Select(arg => $"\"{arg}\""));
        }

        private static string GetFullPath(ITaskItem item)
        {
            string path = item.GetMetadata("FullPath");

            // MakePri.exe chokes if an argument ends with a backslash, followed immediately by
            // a double-quote. I must therefore remove any trailing backslashes from the paths.
            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }
    }
}
