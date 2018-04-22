using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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
            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                path = path.Substring(0, path.Length - 1);

            return path;
        }
    }
}
