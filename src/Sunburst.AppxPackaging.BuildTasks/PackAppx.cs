using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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

        private static string GetFullPath(ITaskItem item)
        {
            string path = item.GetMetadata("FullPath");

            // MakePri.exe chokes if an argument ends with a backslash, followed immediately by
            // a double-quote. I must therefore remove any trailing backslashes from the paths.
            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                path = path.Substring(0, path.Length - 1);

            return path;
        }
    }
}
