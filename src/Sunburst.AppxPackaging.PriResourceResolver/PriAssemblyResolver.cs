using System;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;

namespace Sunburst.AppxPackaging
{
    public static class PriAssemblyResolver
    {
        static class KnownMrtFailureCodes
        {
            // The HRESULT thrown when MRT is used in a normal desktop app
            internal const int MrtNeedsAppXPackagedApp = -2147009760;

            // The HRESULT thrown when a file is missing from the package
            internal const int MissingDefaultOrNeutralResource = -2147009780;
        }

        public static bool IsSupported
        {
            get
            {
                try
                {
                    // If this property call succeeds, we are running in a package.
                    // (If we are not, it will throw an exception.)
                    var unused = Package.Current.Id;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Enable()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;
        }

        private static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName fullName = new AssemblyName(args.Name);
            string desiredFileName = fullName.Name;
            string desiredCulture = fullName.CultureName;

            try
            {
                ResourceContext context = ResourceContext.GetForViewIndependentUse();
                context.Languages = new[] { desiredCulture };

                ResourceManager manager = ResourceManager.Current;
                ResourceMap fileResources = manager.MainResourceMap.GetSubtree("Files");

                // This is the filename as it appeared on-disk when the package was created. At a minimum, you
                // need to add ".dll" on the end, but you might also need to prepend a folder name or do other
                // modifications to match your configuration.
                string targetFileName = $"{desiredFileName}.dll";

                NamedResource desiredResource = fileResources[targetFileName];
                ResourceCandidate bestCandidate = desiredResource.Resolve(context);
                string absoluteFileName = bestCandidate.ValueAsString;
                return LoadAssemblyFromPackageGraph(Package.Current, absoluteFileName);
            }
            catch (Exception ex) when (ex.HResult == KnownMrtFailureCodes.MrtNeedsAppXPackagedApp)
            {
                throw new InvalidOperationException("PRI-based assembly resolving only works in an AppX-packaged (Centennial) app.");
            }
            catch (Exception ex) when (ex.HResult == KnownMrtFailureCodes.MissingDefaultOrNeutralResource)
            {
                throw new FileNotFoundException("Missing AppX resource", desiredFileName, ex);
            }
        }

        private static Assembly LoadAssemblyFromPackageGraph(Package package, string absolutePath)
        {
            if (IsFilePathInDirectory(absolutePath, package.InstalledLocation.Path))
                return Assembly.UnsafeLoadFrom(absolutePath);

            foreach (var dep in package.Dependencies)
                if (IsFilePathInDirectory(absolutePath, dep.InstalledLocation.Path))
                    return Assembly.UnsafeLoadFrom(absolutePath);

            throw new UnauthorizedAccessException("Attempted to load assembly from outside package graph");
        }

        private static bool IsFilePathInDirectory(string absoluteFile, string absoluteDirectory)
        {
            var fileName = new FileInfo(absoluteFile).FullName;
            var directoryName = new DirectoryInfo(absoluteDirectory).FullName;

            if (!directoryName.EndsWith(Path.DirectorySeparatorChar.ToString()))
                directoryName += Path.DirectorySeparatorChar;

            int index = string.Compare(fileName, 0, directoryName, 0, directoryName.Length, StringComparison.OrdinalIgnoreCase);
            return index == 0;
        }
    }
}
