using System;
using System.Collections.Generic;

namespace A_
{
    partial class Interpreter
    {
        static readonly HashSet<string> _blockedDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kernel32", "kernel32.dll", "kernelbase", "kernelbase.dll",
            "ntdll", "ntdll.dll", "user32", "user32.dll", "gdi32", "gdi32.dll",
            "shell32", "shell32.dll", "advapi32", "advapi32.dll",
            "comctl32", "comctl32.dll", "comdlg32", "comdlg32.dll",
            "crypt32", "crypt32.dll", "cryptnet", "cryptnet.dll",
            "cryptui", "cryptui.dll", "cryptsvc", "cryptsvc.dll",
            "dnsapi", "dnsapi.dll", "iphlpapi", "iphlpapi.dll",
            "mpr", "mpr.dll", "msimg32", "msimg32.dll",
            "msvcrt", "msvcrt.dll", "netapi32", "netapi32.dll",
            "ole32", "ole32.dll", "oleaut32", "oleaut32.dll",
            "rpcrt4", "rpcrt4.dll", "secur32", "secur32.dll",
            "setupapi", "setupapi.dll", "shlwapi", "shlwapi.dll",
            "urlmon", "urlmon.dll", "wininet", "wininet.dll",
            "winhttp", "winhttp.dll", "wldap32", "wldap32.dll",
            "ws2_32", "ws2_32.dll", "wsock32", "wsock32.dll",
            "wtsapi32", "wtsapi32.dll", "samlib", "samlib.dll",
            "sspicli", "sspicli.dll", "credui", "credui.dll",
            "credprov", "credprov.dll", "certcli", "certcli.dll",
            "bcrypt", "bcrypt.dll", "ncrypt", "ncrypt.dll",
            "powrprof", "powrprof.dll", "psapi", "psapi.dll",
            "wbemuuid", "wbemuuid.dll", "wintrust", "wintrust.dll"
        };

        static readonly HashSet<string> _dangerousExternFuncs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "winexec", "createprocess", "createprocessa", "createprocessw",
            "shellexecute", "shellexecutea", "shellexecutew",
            "createfile", "createfilea", "createfilew",
            "deletefile", "deletefilea", "deletefilew",
            "writefile", "writefilea", "writefilew",
            "movefile", "movefilea", "movefilew",
            "copyfile", "copyfilea", "copyfilew",
            "removeDirectory", "removedirectorya", "removedirectoryw",
            "createdirectory", "createdirectorya", "createdirectoryw",
            "createremotethread", "createthread",
            "virtualalloc", "virtualprotect",
            "writeProcessMemory", "readProcessMemory",
            "opencryptokey", "decrypt", "encryptfilea", "encryptfilew",
            "regcreatekeyex", "regopenkeyex", "regsetvalueex", "regdeletekeyex",
            "regenumkeyex", "regenumvalueex", "regdeletevalueex",
            "loadlibrary", "loadlibrarya", "loadlibraryw",
            "getprocadress", "dllmain",
            "system", "popen", "_popen", "_wsystem",
            "getcommandlinea", "getcommandlinew"
        };

        static readonly string[] _systemRootPatterns =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "/etc/", "/sys/", "/proc/", "/dev/", "/boot/",
            "/var/log", "/var/run", "/usr/bin", "/usr/lib", "/bin/", "/sbin/"
        };

        void ValidateExternDll(string dllName, Node node)
        {
            if (dllName.Contains('\\') || dllName.Contains('/'))
                throw new Exception(Err(node, $"ممنوع: مسار DLL خارجي '{dllName}'. استخدم اسم DLL فقط بدون مسار"));
            string dllLower = dllName.ToLower();
            if (!dllLower.EndsWith(".dll")) dllLower += ".dll";
            if (_blockedDlls.Contains(dllLower))
                throw new Exception(Err(node, $"ممنوع: لا يمكن تحميل DLL النظام '{dllName}' لأسباب أمنية"));
        }

        void ValidateExternFunc(string funcName, Node node)
        {
            if (_dangerousExternFuncs.Contains(funcName))
                throw new Exception(Err(node, $"ممنوع: الدالة '{funcName}' خطيرة ولا يمكن استدعاؤها via extern"));
        }

        void ValidateUri(string url, Node node)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception(Err(node, "رابط فارغ"));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new Exception(Err(node, $"رابط غير صالح: {url}"));

            string scheme = uri.Scheme.ToLower();
            if (scheme != "http" && scheme != "https")
                throw new Exception(Err(node, $"ممنوع: بروتوكول '{uri.Scheme}' غير مسموح. فقط http/https"));

            if (uri.IsLoopback)
                throw new Exception(Err(node, $"ممنوع: لا يمكن الاتصال بالعناوين المحلية (loopback): {url}"));

            if (uri.HostNameType == UriHostNameType.IPv4)
            {
                var parts = uri.Host.Split('.');
                if (parts.Length == 4 && int.TryParse(parts[0], out var first))
                {
                    if (first == 10)
                        throw new Exception(Err(node, $"ممنوع: عنوان خاص (10.x.x.x): {url}"));
                    if (first == 172 && parts.Length > 1 && int.TryParse(parts[1], out var second) && second >= 16 && second <= 31)
                        throw new Exception(Err(node, $"ممنوع: عنوان خاص (172.16-31.x.x): {url}"));
                    if (first == 192 && parts.Length > 1 && parts[1] == "168")
                        throw new Exception(Err(node, $"ممنوع: عنوان خاص (192.168.x.x): {url}"));
                }
            }
        }

        void ValidateFilePath(string path, Node node)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception(Err(node, "مسار فارغ"));

            string fullPath;
            try { fullPath = System.IO.Path.GetFullPath(path); }
            catch { throw new Exception(Err(node, $"مسار غير صالح: {path}")); }

            if (fullPath.IndexOf("..") >= 0 || path.Contains(".."))
                throw new Exception(Err(node, $"ممنوع: استخدام '..' للوصول خارج المسار المسموح"));

            foreach (var root in _systemRootPatterns)
            {
                if (!string.IsNullOrEmpty(root) && fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new Exception(Err(node, $"ممنوع: الوصول إلى مسار النظام '{fullPath}'"));
            }
        }

        void ValidateDotNetAccess(string typeName, string memberName, Node node)
        {
            string lowerType = typeName.ToLower();
            string[] blockedNamespaces = {
                "system.diagnostics", "system.io", "microsoft.win32",
                "system.management", "system.reflection", "system.runtime.interopservices",
                "system.runtime.remoting", "system.security.cryptography",
                "system.directoryservices", "system.serviceprocess"
            };
            foreach (var ns in blockedNamespaces)
            {
                if (lowerType.StartsWith(ns))
                    throw new Exception(Err(node, $"ممنوع: الوصول إلى النوع '{typeName}' لأسباب أمنية"));
            }

            string[] blockedMethods = {
                "start", "execute", "create", "delete", "kill",
                "setvalu", "deletevalu", "openremote", "impersonate",
                "run", "shell", "filewrite", "fileappend"
            };
            string lowerMethod = memberName.ToLower();
            foreach (var bm in blockedMethods)
            {
                if (lowerMethod.StartsWith(bm))
                    throw new Exception(Err(node, $"ممنوع: استدعاء الدالة '{typeName}.{memberName}' لأسباب أمنية"));
            }
        }
    }
}
