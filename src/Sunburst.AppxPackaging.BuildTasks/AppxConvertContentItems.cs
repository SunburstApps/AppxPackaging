// Copyright (c) William Kent. All rights reserved.
// Licensed under the Apache License, version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Sunburst.AppxPackaging.BuildTasks
{
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required by MSBuild.")]
    public sealed class AppxConvertContentItems : Task
    {
        [Required]
        public string RootDestination { get; set; } = string.Empty;

        [Required]
        public ITaskItem[] InputItems { get; set; } = Array.Empty<ITaskItem>();

        [Output]
        public ITaskItem[] OutputItems { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            var outputs = new List<ITaskItem>();

            foreach (ITaskItem item in InputItems)
            {
                Log.LogMessage(MessageImportance.Normal, "Processing {0}", item.ItemSpec);
                string copyPolicy = item.GetMetadata("CopyToOutputDirectory") ?? string.Empty;
                if (!(copyPolicy == "Always" || copyPolicy == "PreserveNewest"))
                {
                    continue;
                }

                string destination;
                string link = item.GetMetadata("Link");
                if (!string.IsNullOrEmpty(link))
                {
                    destination = Path.Combine(RootDestination, Path.GetDirectoryName(link));
                }
                else if (!string.IsNullOrEmpty(Path.GetDirectoryName(item.ItemSpec)))
                {
                    destination = Path.Combine(RootDestination, Path.GetDirectoryName(item.ItemSpec));
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
