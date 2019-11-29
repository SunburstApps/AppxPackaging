// Copyright (c) William Kent. All rights reserved.
// Licensed under the Apache License, version 2.0. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#pragma warning disable CS8618 // Non-nullable field is uninitialized, but is checked by MSBuild, so will not actually cause an exception.

namespace Sunburst.AppxPackaging.BuildTasks
{
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required by MSBuild.")]
    public sealed class CreateReswFiles : Task
    {
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Required]
        public ITaskItem OutputDirectory { get; set; }

        [Required]
        public string DefaultLanguage { get; set; }

        [Output]
        public string[] AppxPriLanguages { get; set; }

        public override bool Execute()
        {
            string outputPath = OutputDirectory.GetMetadata("FullPath");
            HashSet<string> locales = new HashSet<string>();
            locales.Add(DefaultLanguage);

            foreach (ITaskItem input in InputFiles)
            {
                Log.LogMessage(MessageImportance.Normal, "Processing {0}", input.GetMetadata("FullPath"));
                string[] fileContents = File.ReadAllLines(input.GetMetadata("FullPath"));
                if (fileContents == null)
                {
                    throw new InvalidOperationException("fileContents is null, this shouldn't happen");
                }

                XDocument doc = new XDocument();
                XElement root = new XElement(XName.Get("root", string.Empty));
                doc.Add(root);

                XName name_data = XName.Get("data", string.Empty);
                XName name_value = XName.Get("value", string.Empty);
                XName name_attr = XName.Get("name", string.Empty);

                foreach (string line in fileContents)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.Length == 0 || trimmedLine.StartsWith(";", StringComparison.Ordinal) || trimmedLine.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string[] parts = line.Split('=');
                    string key = parts.First().Trim();
                    string content = string.Join("=", parts.Skip(1)).Trim();
                    Log.LogMessage(MessageImportance.Normal, "-> Adding {0} = {1}", key, content);

                    XElement valueElem = new XElement(name_value, content);
                    XElement dataElem = new XElement(name_data, valueElem);
                    dataElem.Add(new XAttribute(name_attr, key));
                    root.Add(dataElem);
                }

                string outputFileName = Path.GetFileNameWithoutExtension(input.GetMetadata("FullPath"));
                string localeName = DefaultLanguage;
                if (outputFileName.Split('.').Length > 1)
                {
                    List<string> parts = outputFileName.Split('.').ToList();
                    localeName = parts.Last();
                    parts.RemoveAt(parts.Count - 1);
                    outputFileName = string.Join(".", parts);
                    locales.Add(localeName);
                }

                string outputFile = Path.Combine(outputPath, "Strings", localeName, outputFileName + ".resw");
                Log.LogMessage(MessageImportance.Normal, "-> Writing to {0}", outputFile);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                doc.Save(outputFile);
            }

            AppxPriLanguages = locales.ToArray();
            return true;
        }
    }
}
