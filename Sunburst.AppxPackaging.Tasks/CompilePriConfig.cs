using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sunburst.AppxPackaging.Tasks
{
    public sealed class CompilePriConfig : ToolTask
    {
        [Required]
        public ITaskItem PackageLayout { get; set; }
        [Required]
        public ITaskItem ConfigFile { get; set; }
        [Required]
        public ITaskItem OutputFile { get; set; }

        protected override string ToolName => "makepri.exe";

        protected override string GenerateFullPathToTool()
        {
            return @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x86\makepri.exe";
        }

        protected override string GenerateCommandLineCommands()
        {
            List<string> argv = new List<string>();

            argv.Add("new");
            argv.Add("/pr");
            argv.Add(PackageLayout.GetMetadata("FullPath"));
            argv.Add("/cf");
            argv.Add(ConfigFile.GetMetadata("FullPath"));
            argv.Add("/of");
            argv.Add(OutputFile.GetMetadata("FullPath"));
            argv.Add("/mf");
            argv.Add("AppX");
            argv.Add("/o");

            return string.Join(" ", argv.Select(arg => $"\"{arg}\""));
        }
    }
}
