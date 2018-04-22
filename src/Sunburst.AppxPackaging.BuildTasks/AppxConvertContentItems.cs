using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sunburst.AppxPackaging.BuildTasks
{
    public sealed class AppxConvertContentItems : Task
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
                Log.LogMessage(MessageImportance.Normal, "Processing {0}", item.ItemSpec);
                string copyPolicy = item.GetMetadata("CopyToOutputDirectory") ?? "";
                if (!(copyPolicy == "Always" || copyPolicy == "PreserveNewest")) continue;

                string destination;
                string link = item.GetMetadata("Link");
                if (!string.IsNullOrEmpty(link))
                {
                    destination = Path.Combine(RootDestination, Path.GetDirectoryName(link));
                }
                else
                {
                    destination = RootDestination;
                }

                Log.LogMessage(MessageImportance.Normal, "-> {0}", destination);
                ITaskItem newItem = new TaskItem(item);
                newItem.SetMetadata("Destination", destination);
                outputs.Add(newItem);
            }

            OutputItems = outputs.ToArray();
            return true;
        }
    }
}
