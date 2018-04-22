using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sunburst.AppxPackaging.BuildTasks
{
    public sealed class AppxConvertTargetPaths : Task
    {
        [Required]
        public string RootDestination { get; set; }

        [Required]
        public ITaskItem[] InputItems { get; set; }

        [Output]
        public ITaskItem[] OutputItems { get; set; }

        public override bool Execute()
        {
            List<ITaskItem> outputs = new List<ITaskItem>();

            foreach (ITaskItem item in InputItems)
            {
                string directory = Path.GetDirectoryName(item.GetMetadata("TargetPath"));
                bool hasDirectory = directory.Length > 0;
                string destination = (hasDirectory ? Path.Combine(RootDestination, directory) : RootDestination);

                ITaskItem newItem = new TaskItem(item.ItemSpec);
                newItem.SetMetadata("Destination", destination);
                outputs.Add(newItem);
            }

            OutputItems = outputs.ToArray();
            return true;
        }
    }
}
