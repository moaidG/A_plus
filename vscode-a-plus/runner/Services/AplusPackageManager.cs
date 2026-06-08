using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace A_
{
    static class AplusPackageManager
    {
        static string RegistryPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aplus", "registry.json");
        static string PackageDir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aplus", "packages");

        public static void EnsureInitialized()
        {
            if (!Directory.Exists(PackageDir))
                Directory.CreateDirectory(PackageDir);
        }

        public static List<LibraryInfo> GetRegistry()
        {
            try
            {
                if (!File.Exists(RegistryPath))
                    return GetDefaultRegistry();
                var json = File.ReadAllText(RegistryPath);
                return JsonSerializer.Deserialize<List<LibraryInfo>>(json) ?? GetDefaultRegistry();
            }
            catch { return GetDefaultRegistry(); }
        }

        static List<LibraryInfo> GetDefaultRegistry()
        {
            return new List<LibraryInfo>
            {
                new LibraryInfo { Name = "math", Version = "1.0.0", Description = "Advanced math: abs, sqrt, sin, cos, tan, floor, ceil, round, max, min, clamp, lerp, pi", Main = "math.a", Author = "A+" },
                new LibraryInfo { Name = "string", Version = "1.0.0", Description = "String utilities: split, join, reverse, trim, startsWith, endsWith, count, repeat, pad", Main = "string.a", Author = "A+" },
                new LibraryInfo { Name = "list", Version = "1.0.0", Description = "List operations: first, last, take, skip, range, flatten, chunk", Main = "list.a", Author = "A+" },
                new LibraryInfo { Name = "datetime", Version = "1.0.0", Description = "Date/time: now, addDays, addHours, addMinutes", Main = "datetime.a", Author = "A+" },
                new LibraryInfo { Name = "lib_console", Version = "1.0.0", Description = "Console utilities: printLine, readNumber, confirm, wait", Main = "lib_console.a", Author = "A+" },
                new LibraryInfo { Name = "ui_library", Version = "1.0.0", Description = "UI helpers: createWindow, createButton, createImage", Main = "ui_library.a", Author = "A+" },
            };
        }

        public static void Install(string libName)
        {
            EnsureInitialized();
            var registry = GetRegistry();
            var lib = registry.FirstOrDefault(l =>
                l.Name.Equals(libName, StringComparison.OrdinalIgnoreCase));
            if (lib == null)
                throw new Exception($"Library '{libName}' not found in registry");

            var destDir = Path.Combine(PackageDir, lib.Name);
            if (Directory.Exists(destDir))
                throw new Exception($"Library '{libName}' is already installed");

            Directory.CreateDirectory(destDir);
            var stdlibDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stdlib");
            var srcFile = Path.Combine(stdlibDir, lib.Main);
            if (File.Exists(srcFile))
            {
                File.Copy(srcFile, Path.Combine(destDir, lib.Main));
            }

            var metaPath = Path.Combine(destDir, "package.json");
            var meta = new { lib.Name, lib.Version, lib.Description, lib.Main, lib.Author };
            File.WriteAllText(metaPath, JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Uninstall(string libName)
        {
            var dir = Path.Combine(PackageDir, libName);
            if (!Directory.Exists(dir))
                throw new Exception($"Library '{libName}' is not installed");
            Directory.Delete(dir, true);
        }

        public static List<InstalledLibrary> ListInstalled()
        {
            EnsureInitialized();
            var result = new List<InstalledLibrary>();
            if (!Directory.Exists(PackageDir)) return result;
            foreach (var dir in Directory.GetDirectories(PackageDir))
            {
                var name = Path.GetFileName(dir);
                var metaPath = Path.Combine(dir, "package.json");
                string version = "?", description = "";
                if (File.Exists(metaPath))
                {
                    try
                    {
                        var meta = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(metaPath));
                        if (meta != null)
                        {
                            meta.TryGetValue("Version", out version);
                            meta.TryGetValue("Description", out description);
                        }
                    }
                    catch { }
                }
                result.Add(new InstalledLibrary { Name = name, Version = version ?? "?", Description = description ?? "" });
            }
            return result;
        }

        public static string ResolvePath(string libName, string mainFile)
        {
            return Path.Combine(PackageDir, libName, mainFile);
        }
    }

    class LibraryInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Main { get; set; }
        public string Author { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
    }

    class InstalledLibrary
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
