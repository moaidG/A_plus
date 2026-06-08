using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using A_.Platform;

namespace A_
{
    partial class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Set console output encoding to UTF-8 for proper Arabic support
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Handle command line arguments
            if (args.Length > 0)
            {
                // Process arguments
                string projectName = null;
                string projectLocation = null;
                string targetOS = null;
                bool createNew = false;
                bool mlMode = false;
                bool interactiveMode = false;
                bool debugMode = false;
                List<string> fileArgs = new List<string>();

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    string lowerArg = arg.ToLowerInvariant();
                    if (i == 0 && lowerArg == "create")
                    {
                        createNew = true;
                        continue;
                    }
                    if (i == 0 && lowerArg == "make" && i + 1 < args.Length && args[i + 1].Equals("A+", StringComparison.OrdinalIgnoreCase))
                    {
                        createNew = true;
                        targetOS = "all";
                        i++;
                        continue;
                    }
                    if (i == 0 && (lowerArg == "package" || lowerArg == "حزمة"))
                    {
                        if (i + 1 < args.Length)
                        {
                            string scriptPath = args[++i];
                            string outputName = Path.GetFileNameWithoutExtension(scriptPath);
                            string pkgTarget = "exe";
                            string pkgOutputDir = null;
                            for (int j = i + 1; j < args.Length; j++)
                            {
                                if (args[j].ToLowerInvariant() == "-os" && j + 1 < args.Length)
                                { pkgTarget = args[++j]; }
                                else if ((args[j].ToLowerInvariant() == "-o" || args[j].ToLowerInvariant() == "-output") && j + 1 < args.Length)
                                { pkgOutputDir = args[++j]; }
                                else if (!args[j].StartsWith("-"))
                                { outputName = args[j]; }
                            }
                            PackageScript(scriptPath, outputName, pkgTarget, pkgOutputDir);
                        }
                        else
                        {
                            Console.Error.WriteLine("Usage: package <script.a> [outputName] -os [exe|apk|ios|web|linux|macos|all] -o <outputDir>");
                            Console.Error.WriteLine("  -os exe      Windows .exe (self-contained)");
                            Console.Error.WriteLine("  -os apk      Android .apk");
                            Console.Error.WriteLine("  -os ios      iOS app");
                            Console.Error.WriteLine("  -os web      Web app (HTML/JS)");
                            Console.Error.WriteLine("  -os linux    Linux binary");
                            Console.Error.WriteLine("  -os macos    macOS binary");
                            Console.Error.WriteLine("  -os all      All platforms");
                            Console.Error.WriteLine("  -o <path>   Output directory (default: ./build)");
                        }
                        return;
                    }
                    if (arg == "-N" || lowerArg == "-name")
                    {
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            projectName = args[++i];
                        }
                        continue;
                    }
                    if (arg == "-L" || lowerArg == "-location")
                    {
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            projectLocation = args[++i];
                        }
                        continue;
                    }

                    switch (lowerArg)
                    {
                        case "--new":
                        case "-n":
                            createNew = true;
                            break;
                        case "-l":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                projectLocation = args[++i];
                            }
                            break;
                        case "-os":
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                targetOS = args[++i].ToLower();
                            }
                            break;
                        case "-ml":
                            mlMode = true;
                            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                            {
                                targetOS = args[++i].ToLower();
                            }
                            break;
                        case "-i":
                        case "--interactive":
                        case "-repl":
                            interactiveMode = true;
                            break;
                        case "-d":
                        case "--debug":
                            debugMode = true;
                            break;
                        default:
                            fileArgs.Add(arg);
                            break;
                    }
                }

                if (createNew)
                {
                    CreateNewProjectWithOptions(projectName, projectLocation, targetOS);
                    return;
                }

                if (interactiveMode)
                {
                    RunInteractive();
                    return;
                }

                if (debugMode)
                {
                    var debugger = new DapDebugger();
                    debugger.Run();
                    return;
                }

                // Handle file/directory arguments
                if (fileArgs.Count == 0)
                {
                    if (File.Exists("index.a"))
                        RunFile("index.a", mlMode);
                    else
                        HelpAndExit();
                }
                else if (fileArgs.Count == 1 && Directory.Exists(fileArgs[0]))
                {
                    string dir = fileArgs[0];
                    string indexPath = Path.Combine(dir, "index.a");
                    if (!File.Exists(indexPath))
                        indexPath = Path.Combine(dir, "index.a+");
                    if (File.Exists(indexPath))
                    {
                        string cwd = Directory.GetCurrentDirectory();
                        Directory.SetCurrentDirectory(dir);
                        RunFile(Path.GetFileName(indexPath), mlMode);
                        var files = Directory.GetFiles(".", "*.a").OrderBy(f => f)
                            .Concat(Directory.GetFiles(".", "*.a+").OrderBy(f => f));
                        foreach (string f in files)
                        {
                            string fn = Path.GetFileName(f);
                            if (fn != "index.a" && fn != "index.a+")
                                RunFile(fn, mlMode);
                        }
                        Directory.SetCurrentDirectory(cwd);
                    }
                    else
                    {
                        Console.Error.WriteLine($"[Error] index.a or index.a+ not found in '{dir}'");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    string cwd = Directory.GetCurrentDirectory();
                    foreach (string f in fileArgs)
                    {
                        if ((f.EndsWith(".a") || f.EndsWith(".a+")) && File.Exists(f))
                        {
                            string dir = Path.GetDirectoryName(Path.GetFullPath(f));
                            Directory.SetCurrentDirectory(dir);
                            RunFile(Path.GetFileName(f), mlMode);
                            Directory.SetCurrentDirectory(cwd);
                        }
                    }
                }
            }
            else
            {
                if (File.Exists("index.a"))
                    RunFile("index.a");
                else
                    HelpAndExit();
            }
            if (!Console.IsInputRedirected && !ConsoleRuntime.PauseHandled)
            {
                try { Console.WriteLine(); Console.WriteLine("Press any key to exit... / اضغط أي مفتاح للخروج..."); Console.ReadKey(true); }
                catch { /* ignore if stdin redirected */ }
            }
        }

        static void HelpAndExit()
        {
            Console.Error.WriteLine("A+ Language Runner v1.0");
            Console.Error.WriteLine("Usage:");
            Console.Error.WriteLine("  a+ <file.a>       Run a single file");
            Console.Error.WriteLine("  a+ <dir>          Run project (index.a + files)");
            Console.Error.WriteLine("  a+ (no args)      Run index.a in current dir");
            Console.Error.WriteLine("  a+ create -N MyApp -L .\\Projects -OS exe");
            Console.Error.WriteLine("  make A+           Create a full starter project in current directory");
            Console.Error.WriteLine("  a+ --new          Create new project in current directory");
            Console.Error.WriteLine("  a+ -n             Create new project in current directory");
            Console.Error.WriteLine("  a+ -N <name>      Project name for new project");
            Console.Error.WriteLine("  a+ -L <path>      Location for new project");
            Console.Error.WriteLine("  a+ -OS <os>       Target OS (exe, apk, ios, web, windows, linux, macos, all)");
            Console.Error.WriteLine("  a+ -ml            Multi-language mode (auto-detect Arabic/English)");
            Console.Error.WriteLine("  a+ -ml <target>   Multi-language with target (ios, apk, html, wis, exe)");
            Console.Error.WriteLine("  a+ -i             Interactive REPL mode");
            Console.Error.WriteLine("  a+ -d             Debug adapter protocol mode (VS Code)");
            Environment.Exit(1);
        }

        static void CreateNewProject()
        {
            CreateNewProjectWithOptions(null, null, null);
        }

        static void CreateNewProjectWithOptions(string projectName, string projectLocation, string targetOS)
        {
            targetOS = NormalizeTargetOS(targetOS);

            // Determine project directory
            string dir;
            if (!string.IsNullOrEmpty(projectLocation))
            {
                dir = projectLocation;
                if (!string.IsNullOrEmpty(projectName) && !Path.GetFileName(Path.TrimEndingDirectorySeparator(dir)).Equals(projectName, StringComparison.OrdinalIgnoreCase))
                    dir = Path.Combine(dir, projectName);
                // Ensure directory exists
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            else
            {
                dir = Directory.GetCurrentDirectory();
            }

            // Determine project name
            string appName;
            if (!string.IsNullOrEmpty(projectName))
            {
                appName = projectName;
            }
            else
            {
                appName = new DirectoryInfo(dir).Name;
            }

            string indexPath = Path.Combine(dir, "index.a");
            if (File.Exists(indexPath))
            {
                Console.Error.WriteLine($"[Error] index.a already exists in '{dir}'");
                return;
            }

            // Generate appropriate template based on target OS
            string template;
            if (targetOS == "android" || targetOS == "apk")
            {
                template = GenerateAndroidProjectTemplate(appName);
            }
            else if (targetOS == "ios")
            {
                template = GenerateBasicProjectTemplate(appName, "ios");
            }
            else if (targetOS == "web")
            {
                template = GenerateBasicProjectTemplate(appName, "web");
            }
            else
            {
                // Default Windows/WPF template
                template = GenerateBasicProjectTemplate(appName, targetOS ?? "exe");
            }

            File.WriteAllText(indexPath, template);
            Console.WriteLine($"Created project: {indexPath}");

            CreateProjectScaffold(dir, appName, targetOS);

            Console.WriteLine("Run: a+");
        }

        static string NormalizeTargetOS(string targetOS)
        {
            if (string.IsNullOrWhiteSpace(targetOS)) return "exe";
            targetOS = targetOS.Trim().ToLowerInvariant();
            return targetOS switch
            {
                "windows" or "win" => "exe",
                "android" => "apk",
                "all" or "*" => "all",
                _ => targetOS
            };
        }

        static string SafeIdentifier(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "APlusApp" : value.Trim();
            string safe = Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9_]", "_");
            safe = Regex.Replace(safe, @"_+", "_").Trim('_');
            if (string.IsNullOrEmpty(safe) || char.IsDigit(safe[0])) safe = "app_" + safe;
            return safe;
        }

        static string GenerateBasicProjectTemplate(string appName, string targetOS)
        {
            return "# " + appName + " - A+ Program\n"
                + "# index.a - entry point\n"
                + "\n"
                + "let appName = \"" + appName + "\"\n"
                + "let appVersion = \"1.0.0\"\n"
                + "let target = \"" + targetOS + "\"\n"
                + "let p = platform()\n"
                + "\n"
                + "func main() {\n"
                + "    print(\"==============\")\n"
                + "    print(appName + \" v\" + appVersion)\n"
                + "    print(\"Target: \" + target)\n"
                + "    print(\"Platform: \" + p)\n"
                + "    print(\"Welcome to A+!\")\n"
                + "}\n"
                + "\n"
                + "main()\n";
        }

        static string GenerateAndroidProjectTemplate(string appName)
        {
            return "# " + appName + " - A+ Android Application\n"
                + "# index.a - entry point\n"
                + "\n"
                + "let appName = \"" + appName + "\"\n"
                + "let appVersion = \"1.0.0\"\n"
                + "let p = platform()\n"
                + "\n"
                + "// Android-specific imports\n"
                + "extern func AndroidAppMain() from \"libandroid.so\"\n"
                + "\n"
                + "func main() {\n"
                + "    print(\"==============\")\n"
                + "    print(appName + \" v\" + appVersion)\n"
                + "    print(\"==============\")\n"
                + "    print(\"Platform: \" + p)\n"
                + "    if (p == \"android\") {\n"
                + "        print(\"Running on Android\")\n"
                + "        AndroidAppMain()\n"
                + "    } else {\n"
                + "        print(\"Welcome to A+!\")\n"
                + "    }\n"
                + "}\n"
                + "\n"
                + "main()\n";
        }

        static void CreateAndroidProjectFiles(string dir, string appName)
        {
            string packageName = "org.aplus." + SafeIdentifier(appName);
            // Create basic Android project structure
            string manifestPath = Path.Combine(dir, "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                string manifest = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                    + "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"\n"
                    + "    package=\"" + packageName + "\"\n"
                    + "    android:versionCode=\"1\"\n"
                    + "    android:versionName=\"1.0\">\n"
                    + "    <uses-sdk android:minSdkVersion=\"21\" android:targetSdkVersion=\"33\" />\n"
                    + "    <application\n"
                    + "        android:allowBackup=\"true\"\n"
                    + "        android:label=\"@string/app_name\"\n"
                    + "        android:icon=\"@mipmap/ic_launcher\">\n"
                    + "        <activity android:name=\".MainActivity\"\n"
                    + "            android:label=\"@string/app_name\">\n"
                    + "            <intent-filter>\n"
                    + "                <action android:name=\"android.intent.action.MAIN\" />\n"
                    + "                <category android:name=\"android.intent.category.LAUNCHER\" />\n"
                    + "            </intent-filter>\n"
                    + "        </activity>\n"
                    + "    </application>\n"
                    + "</manifest>";
                File.WriteAllText(manifestPath, manifest);
            }

            // Create MainActivity.java
            string javaDir = Path.Combine(dir, "java", "org", "aplus", SafeIdentifier(appName));
            Directory.CreateDirectory(javaDir);
            string mainActivityPath = Path.Combine(javaDir, "MainActivity.java");
            if (!File.Exists(mainActivityPath))
            {
                string mainActivity = "package " + packageName + ";\n"
                    + "\n"
                    + "import android.app.Activity;\n"
                    + "import android.os.Bundle;\n"
                    + "import android.widget.TextView;\n"
                    + "\n"
                    + "public class MainActivity extends Activity {\n"
                    + "    @Override\n"
                    + "    protected void onCreate(Bundle savedInstanceState) {\n"
                    + "        super.onCreate(savedInstanceState);\n"
                    + "\n"
                    + "        // Create a simple text view\n"
                    + "        TextView textView = new TextView(this);\n"
                    + "        textView.setText(\"Welcome to A+ on Android!\");\n"
                    + "        setContentView(textView);\n"
                    + "    }\n"
                    + "}";
                File.WriteAllText(mainActivityPath, mainActivity);
            }

            // Create build.gradle
            string buildGradlePath = Path.Combine(dir, "build.gradle");
            if (!File.Exists(buildGradlePath))
            {
                string buildGradle = "apply plugin: 'com.android.application'\n"
                    + "\n"
                    + "android {\n"
                    + "    compileSdkVersion 33\n"
                    + "    defaultConfig {\n"
                    + "        applicationId \"" + packageName + "\"\n"
                    + "        minSdkVersion 21\n"
                    + "        targetSdkVersion 33\n"
                    + "        versionCode 1\n"
                    + "        versionName \"1.0\"\n"
                    + "    }\n"
                    + "    buildTypes {\n"
                    + "        release {\n"
                    + "            minifyEnabled false\n"
                    + "            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'\n"
                    + "        }\n"
                    + "    }\n"
                    + "}\n"
                    + "\n"
                    + "dependencies {\n"
                    + "    implementation fileTree(dir: 'libs', include: ['*.jar'])\n"
                    + "    implementation 'androidx.appcompat:appcompat:1.4.1'\n"
                    + "    implementation 'com.google.android.material:material:1.5.0'\n"
                    + "    implementation 'androidx.constraintlayout:constraintlayout:2.1.3'\n"
                    + "}";
                File.WriteAllText(buildGradlePath, buildGradle);
            }

            // Create res/values/strings.xml
            string valuesDir = Path.Combine(dir, "res", "values");
            Directory.CreateDirectory(valuesDir);
            string stringsPath = Path.Combine(valuesDir, "strings.xml");
            if (!File.Exists(stringsPath))
            {
                string strings = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                    + "<resources>\n"
                    + "    <string name=\"app_name\">" + appName + "</string>\n"
                    + "</resources>";
                File.WriteAllText(stringsPath, strings);
            }
        }

        static void CreateProjectScaffold(string dir, string appName, string targetOS)
        {
            Directory.CreateDirectory(Path.Combine(dir, "src"));
            Directory.CreateDirectory(Path.Combine(dir, "runtime"));
            Directory.CreateDirectory(Path.Combine(dir, "libs", "c"));
            Directory.CreateDirectory(Path.Combine(dir, "libs", "cpp"));
            Directory.CreateDirectory(Path.Combine(dir, "libs", "csharp"));
            Directory.CreateDirectory(Path.Combine(dir, "examples"));
            Directory.CreateDirectory(Path.Combine(dir, "templates"));
            Directory.CreateDirectory(Path.Combine(dir, "build", "exe"));
            Directory.CreateDirectory(Path.Combine(dir, "ap.a_APK"));
            Directory.CreateDirectory(Path.Combine(dir, "as.ios"));
            Directory.CreateDirectory(Path.Combine(dir, "web"));
            Directory.CreateDirectory(Path.Combine(dir, ".aplus", "context"));

            CreateAndWriteFile(Path.Combine(dir, "README.md"),
                "# {0}" + Newl + Newl +
                "مشروع A+ مع دعم EXE, APK, iOS, و Web." + Newl + Newl +
                "## التشغيل" + Newl +
                "```" + Newl +
                "A_Plus_Console.exe main.a" + Newl +
                "```" + Newl + Newl +
                "## البناء" + Newl +
                "```" + Newl +
                "dotnet run -- package main.a {0} -os all" + Newl +
                "```", appName);
            CreateAndWriteFile(Path.Combine(dir, "src", "main.a"),
                "# {0} — المشروع الرئيسي" + Newl +
                "include \"ui_library\"" + Newl + Newl +
                "a+ xml app = <StackPanel Width=\"500\" Height=\"400\" Background=\"#0f0f23\">" + Newl +
                "    <TextBlock Text=\"مرحباً بك في {0}\" FontSize=\"24\" Color=\"#e0e0ff\"" + Newl +
                "               HorizontalAlignment=\"Center\" Margin=\"0,30,0,10\" />" + Newl +
                "    <TextBox Name=\"nameInput\" Width=\"300\" Height=\"40\" Placeholder=\"اكتب اسمك...\" />" + Newl +
                "    <Button Content=\"ابدأ\" Width=\"150\" Height=\"45\" Background=\"#7c3aed\" Foreground=\"white\"" + Newl +
                "            Click=\"{sayHello}\" Margin=\"0,10\" />" + Newl +
                "    <TextBlock Name=\"result\" FontSize=\"18\" Color=\"#10b981\" HorizontalAlignment=\"Center\" />" + Newl +
                "</StackPanel>" + Newl + Newl +
                "func sayHello(sender, args) {" + Newl +
                "    let name = app.nameInput.text" + Newl +
                "    if name == \"\" {" + Newl +
                "        app.result.text = \"الرجاء إدخال اسم\"" + Newl +
                "    } else {" + Newl +
                "        app.result.text = \"أهلاً \" + name + \"! في عالم A+\"" + Newl +
                "    }" + Newl +
                "}" + Newl + Newl +
                "show()", appName);
            CreateAndWriteFile(Path.Combine(dir, "src", "utils.a"),
                "# utils.a — دوال مساعدة" + Newl +
                "export func greet(name) {" + Newl +
                "    return \"مرحباً \" + name" + Newl +
                "}" + Newl + Newl +
                "export func add(a, b) {" + Newl +
                "    return a + b" + Newl +
                "}");
            CreateAndWriteFile(Path.Combine(dir, "runtime", "interop.a"),
                "# استدعاء دوال من مكتبات خارجية" + Newl +
                "# extern func NativeAdd(a, b) from \"native.dll\"");
            CreateAndWriteFile(Path.Combine(dir, "libs", "README.md"),
                "# ضع مكتبات A+ الخاصة هنا" + Newl +
                "# include \"مكتبتي\"");
            CreateAndWriteFile(Path.Combine(dir, "examples", "hello.axml"),
                "<StackPanel Width=\"400\" Height=\"300\" Background=\"#0f0f23\">" + Newl +
                "    <TextBlock Text=\"تطبيق AXML مستقل\" FontSize=\"22\" Color=\"#e0e0ff\" HorizontalAlignment=\"Center\" Margin=\"0,20\" />" + Newl +
                "    <Button Content=\"اضغطني\" Width=\"120\" Height=\"40\" Background=\"#7c3aed\" Foreground=\"white\" HorizontalAlignment=\"Center\" />" + Newl +
                "</StackPanel>" + Newl +
                "show()");
            CreateAndWriteFile(Path.Combine(dir, "templates", "oop.a"),
                "class App {" + Newl +
                "    let name" + Newl +
                "    func greet() {" + Newl +
                "        print(\"Hello \" + self.name)" + Newl +
                "    }" + Newl +
                "}" + Newl + Newl +
                "let myApp = new App()" + Newl +
                "myApp.name = \"{0}\"" + Newl +
                "myApp.greet()", appName);
            CreateAndWriteFile(Path.Combine(dir, "templates", "xml_ui.a"),
                "<StackPanel Width=\"400\" Height=\"300\" Background=\"#0f0f23\">" + Newl +
                "    <TextBlock Text=\"A+ XML UI\" FontSize=\"24\" Color=\"#e0e0ff\" HorizontalAlignment=\"Center\" Margin=\"0,20\" />" + Newl +
                "    <Button Content=\"تشغيل\" Width=\"150\" Height=\"45\" Background=\"#7c3aed\" Foreground=\"white\" HorizontalAlignment=\"Center\" />" + Newl +
                "</StackPanel>" + Newl +
                "show()");

            if (targetOS == "apk" || targetOS == "all") CreateAndroidProjectFiles(Path.Combine(dir, "ap.a_APK"), appName);
            if (targetOS == "ios" || targetOS == "all") CreateIosProjectFiles(Path.Combine(dir, "as.ios"), appName);
            if (targetOS == "web" || targetOS == "all") CreateWebProjectFiles(Path.Combine(dir, "web"), appName);
        }

        static void CreateIosProjectFiles(string dir, string appName)
        {
            Directory.CreateDirectory(dir);
            WriteFileIfMissing(Path.Combine(dir, "Info.plist"), "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<plist version=\"1.0\"><dict><key>CFBundleName</key><string>" + appName + "</string></dict></plist>\n");
            WriteFileIfMissing(Path.Combine(dir, "README.md"), "# iOS Target\n\nUse this folder for Swift/.NET iOS packaging assets generated from A+.\n");
        }

        static void CreateWebProjectFiles(string dir, string appName)
        {
            Directory.CreateDirectory(dir);
            WriteFileIfMissing(Path.Combine(dir, "index.html"), "<!doctype html>\n<html><head><meta charset=\"utf-8\"><title>" + appName + "</title></head><body><main id=\"app\">A+ Web Target</main><script src=\"main.js\"></script></body></html>\n");
            WriteFileIfMissing(Path.Combine(dir, "main.js"), "document.getElementById('app').textContent = 'Welcome to " + appName + " from A+';\n");
        }

        static readonly string Newl = Environment.NewLine;

        static void WriteFileIfMissing(string path, string content)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (!File.Exists(path)) File.WriteAllText(path, content, new System.Text.UTF8Encoding(false));
        }

        static void CreateAndWriteFile(string path, string template, params object[] args)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string content = args.Length > 0 ? string.Format(template, args) : template;
            if (!File.Exists(path)) File.WriteAllText(path, content, new System.Text.UTF8Encoding(false));
        }

        static void RunFile(string file, bool mlMode = false)
        {
            if (!File.Exists(file)) return;
            try
            {
                string code = File.ReadAllText(file);
                if (file.EndsWith(".axml", StringComparison.OrdinalIgnoreCase))
                    code = Shorthand.ConvertAxmlFile(file);
                else
                    code = Shorthand.Convert(code);
                var ast = new Parser(new Lexer(code).Tokenize()).Parse();
                var interp = new Interpreter();
                interp.Output = msg => Console.WriteLine(msg);
                interp.BaseDir = Path.GetDirectoryName(Path.GetFullPath(file)) ?? ".";
                interp.UIRuntime = CreateRuntime();
                TryLoadStdLib(interp);
                interp.Visit(ast);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Error] {file}: {ex}");
            }
        }

        static void RunInteractive()
        {
            Console.WriteLine("A+ Interactive REPL (أ+ وضع تفاعلي)");
            Console.WriteLine("اكتب 'exit' أو 'خروج' للخروج");
            var interp = new Interpreter();
            interp.Output = msg => Console.WriteLine(msg);
            interp.BaseDir = Directory.GetCurrentDirectory();
            interp.UIRuntime = CreateRuntime();
            TryLoadStdLib(interp);
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null || line.Trim().ToLower() == "exit" || line.Trim() == "خروج")
                    break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    string code = Shorthand.Convert(line);
                Console.Error.WriteLine("=== CONVERTED ===");
                Console.Error.WriteLine(code);
                Console.Error.WriteLine("=== END ===");
                var ast = new Parser(new Lexer(code).Tokenize()).Parse();
                    object result = interp.Visit(ast);
                    if (result is System.Threading.Tasks.Task task)
                    {
                        task.GetAwaiter().GetResult();
                        if (task is System.Threading.Tasks.Task<object> taskObj)
                            Console.WriteLine("=> " + FormatAplusValue(taskObj.Result));
                        else
                            Console.WriteLine("=> (async)");
                    }
                    else if (result != null && !(result is double && (double)result == 0))
                        Console.WriteLine("=> " + FormatAplusValue(result));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[خطأ] {ex.Message}");
                }
            }
        }

        static string FormatAplusValue(object value)
        {
            if (value is double d)
            {
                if (d == Math.Floor(d) && !double.IsInfinity(d))
                    return ((long)d).ToString();
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (value is string s) return "\"" + s + "\"";
            if (value is bool b) return b ? "true" : "false";
            if (value is List<object> list) return "[" + string.Join(", ", list.Select(FormatAplusValue)) + "]";
            if (value is Dictionary<string, object> dict)
            {
                if (dict.ContainsKey("__class")) return "<" + dict["__class"] + ">";
                return "{" + string.Join(", ", dict.Select(kv => kv.Key + ": " + FormatAplusValue(kv.Value))) + "}";
            }
            return value?.ToString() ?? "nil";
        }

        static void TryLoadStdLib(Interpreter interp)
        {
            string exeDir = AppContext.BaseDirectory;
            string[] candidates = {
                Path.GetFullPath(Path.Combine(exeDir, "stdlib")),
                Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", "stdlib"))
            };
            foreach (string path in candidates)
            {
                if (Directory.Exists(path))
                {
                    interp.StdLibPath = path;
                    interp.LoadStdLib();
                    return;
                }
            }
        }

        static string ResolveRunnerProjectDir()
        {
            string baseDir = AppContext.BaseDirectory;
            string[] candidates = {
                baseDir,
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..")),
                Path.GetFullPath(Path.Combine(baseDir, "runner")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "runner"))
            };
            foreach (string candidate in candidates)
            {
                if (File.Exists(Path.Combine(candidate, "Language", "Lexer.cs")))
                    return candidate;
            }
            // Last resort: search upward from baseDir
            string dir = baseDir;
            for (int i = 0; i < 10; i++)
            {
                if (File.Exists(Path.Combine(dir, "Language", "Lexer.cs")))
                    return dir;
                var parent = Directory.GetParent(dir);
                if (parent == null) break;
                dir = parent.FullName;
            }
            return baseDir;
        }

        static string ResolveStdLibDir()
        {
            string baseDir = AppContext.BaseDirectory;
            string[] candidates = {
                Path.GetFullPath(Path.Combine(baseDir, "stdlib")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "stdlib")),
                Path.GetFullPath(Path.Combine(ResolveRunnerProjectDir(), "stdlib"))
            };
            foreach (string candidate in candidates)
            {
                if (Directory.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        static IUIRuntime CreateRuntime()
        {
#if WINDOWS
            return new WpfRuntime();
#else
            return new ConsoleRuntime();
#endif
        }

        static void PackageScript(string scriptPath, string outputName, string target, string outputDir = null)
        {
            if (!File.Exists(scriptPath))
            {
                Console.Error.WriteLine($"[Error] Script not found: {scriptPath}");
                return;
            }
            string scriptCode = File.ReadAllText(scriptPath);
            target = target.ToLowerInvariant();
            string buildDir = outputDir ?? Path.Combine(Directory.GetCurrentDirectory(), "build");
            Directory.CreateDirectory(buildDir);

            switch (target)
            {
                case "exe":
                    PackageForExe(scriptCode, outputName, buildDir, "win-x64");
                    break;
                case "linux":
                    PackageForExe(scriptCode, outputName, buildDir, "linux-x64");
                    break;
                case "macos":
                    PackageForExe(scriptCode, outputName, buildDir, "osx-x64");
                    break;
                case "apk":
                    PackageForAndroid(scriptCode, outputName, buildDir);
                    break;
                case "ios":
                    PackageForiOS(scriptCode, outputName, buildDir);
                    break;
                case "web":
                    PackageForWeb(scriptCode, outputName, buildDir, scriptPath);
                    break;
                case "aab":
                    PackageForAab(scriptCode, outputName, buildDir);
                    break;
                case "appimage":
                    PackageForAppImage(scriptCode, outputName, buildDir);
                    break;
                case "macos_app":
                    PackageForMacApp(scriptCode, outputName, buildDir);
                    break;
                case "all":
                    Console.WriteLine("Packaging for all platforms...");
                    PackageForExe(scriptCode, outputName, buildDir, "win-x64");
                    PackageForExe(scriptCode, outputName, buildDir, "linux-x64");
                    PackageForExe(scriptCode, outputName, buildDir, "osx-x64");
                    PackageForAndroid(scriptCode, outputName, buildDir);
                    PackageForAab(scriptCode, outputName, buildDir);
                    PackageForiOS(scriptCode, outputName, buildDir);
                    PackageForWeb(scriptCode, outputName, buildDir, scriptPath);
                    break;
                default:
                    Console.Error.WriteLine($"[Error] Unknown target: {target}. Use: exe, apk, ios, web, linux, macos, aab, appimage, macos_app, all");
                    break;
            }
        }

        static void PackageForExe(string scriptCode, string outputName, string buildDir, string rid)
        {
            string targetDir = Path.Combine(buildDir, rid);
            Directory.CreateDirectory(targetDir);

            string projDir = Path.Combine(Path.GetTempPath(), "aplus_pkg_" + Guid.NewGuid().ToString("N"));
            System.Diagnostics.Process proc = null;
            try
            {
                Directory.CreateDirectory(projDir);
                string scriptEmbedPath = Path.Combine(projDir, "main.a");
                File.WriteAllText(scriptEmbedPath, scriptCode);

                string csproj = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <RuntimeIdentifier>" + rid + @"</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <AssemblyName>" + SafeIdentifier(outputName) + @"</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Content Include=""stdlib\**\*"" CopyToOutputDirectory=""PreserveNewest"" />
  </ItemGroup>
</Project>";

                File.WriteAllText(Path.Combine(projDir, "pkg.csproj"), csproj);

                string programCs = "using System;\n"
                    + "using System.IO;\n"
                    + "using System.Text.RegularExpressions;\n"
                    + "using System.Collections.Generic;\n"
                    + "using System.Linq;\n"
                    + "using System.Windows;\n"
                    + "using A_.Platform;\n"
                    + "\n"
                    + "namespace A_\n{\n    class Program\n    {\n"
                    + "        [STAThread]\n        static void Main(string[] args)\n        {\n"
                    + "            Interpreter interp = null;\n"
                    + "            try\n"
                    + "            {\n"
                    + "                string code = " + ToLiteral(scriptCode) + ";\n"
                    + "                code = Shorthand.Convert(code);\n"
                    + "                var lexer = new Lexer(code);\n"
                    + "                var tokens = lexer.Tokenize();\n"
                    + "                var parser = new Parser(tokens);\n"
                    + "                var ast = parser.Parse();\n"
                    + "                interp = new Interpreter();\n"
                    + "                interp.Output = msg => { try { interp?.UIRuntime?.WriteOutput(msg); } catch { } };\n"
                    + "                interp.BaseDir = Path.GetDirectoryName(Environment.ProcessPath) ?? \".\";\n"
                    + "                interp.UIRuntime = new WpfRuntime();\n"
                    + "                interp.StdLibPath = Path.Combine(AppContext.BaseDirectory, \"stdlib\");\n"
                    + "                interp.LoadStdLib();\n"
                    + "                interp.UIRuntime.EnsureWindow();\n"
                    + "                interp.Visit(ast);\n"
                    + "            }\n"
                    + "            catch (Exception ex)\n"
                    + "            {\n"
                    + "                try { System.Windows.MessageBox.Show(ex.ToString(), \"A+ Error\"); } catch { }\n"
                    + "            }\n"
                    + "            finally\n"
                    + "            {\n"
                    + "                try\n"
                    + "                {\n"
                    + "                    if (interp?.UIRuntime != null && !interp.UIRuntime.HasShownWindow())\n"
                    + "                    {\n"
                    + "                        interp.UIRuntime.EnsureWindow();\n"
                    + "                        interp.UIRuntime.ShowWindow();\n"
                    + "                    }\n"
                    + "                }\n"
                    + "                catch { }\n"
                    + "            }\n"
                    + "        }\n    }\n}\n";
                File.WriteAllText(Path.Combine(projDir, "Program.cs"), programCs);

                string libDir = Path.Combine(projDir, "stdlib");
                Directory.CreateDirectory(libDir);

                string runnerProjectDir = ResolveRunnerProjectDir();
                foreach (string f in Directory.GetFiles(runnerProjectDir, "*.cs", SearchOption.AllDirectories))
                {
                    string fn = Path.GetFileName(f);
                    string rel = Path.GetRelativePath(runnerProjectDir, f);
                    if (fn == "Program.cs" || fn.EndsWith(".g.cs") || fn.EndsWith(".i.cs")) continue;
                    if (rel.StartsWith("MobileTemplate", StringComparison.OrdinalIgnoreCase)) continue;
                    string dest = Path.Combine(projDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(f, dest, true);
                }

                string stdLibSrc = ResolveStdLibDir();
                if (Directory.Exists(stdLibSrc))
                {
                    foreach (string f in Directory.GetFiles(stdLibSrc, "*.a"))
                        File.Copy(f, Path.Combine(libDir, Path.GetFileName(f)), true);
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projDir}/pkg.csproj\" -o \"{targetDir}\" --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(120000);

                if (proc.ExitCode == 0)
                {
                    Console.WriteLine($"[{rid}] Packaged to: {targetDir}");
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    Console.Error.WriteLine($"[{rid}] Package failed: {errMsg}");
                    Console.WriteLine($"[{rid}] Temp project kept at: {projDir}");
                    Console.WriteLine($"[{rid}] To debug: dotnet build \"{projDir}/pkg.csproj\" --nologo");
                }
            }
            finally
            {
                if (proc != null)
                {
                    try { if (!proc.HasExited) proc.WaitForExit(5000); } catch { }
                    if (proc.HasExited && proc.ExitCode == 0)
                    {
                        try { Directory.Delete(projDir, true); } catch { }
                    }
                }
            }
        }

        static string MobileTemplateDir
        {
            get
            {
                string asmDir = AppContext.BaseDirectory;
                string[] candidates = {
                    Path.GetFullPath(Path.Combine(asmDir, "MobileTemplate")),
                    Path.GetFullPath(Path.Combine(asmDir, "..", "..", "..", "MobileTemplate")),
                    Path.GetFullPath(Path.Combine(ResolveRunnerProjectDir(), "MobileTemplate"))
                };
                foreach (string c in candidates)
                    if (Directory.Exists(c)) return c;
                return null;
            }
        }

        static void PackageForAndroid(string scriptCode, string outputName, string buildDir)
        {
            string targetDir = Path.Combine(buildDir, "apk");
            string templateDir = MobileTemplateDir;
            if (templateDir == null)
            {
                Console.Error.WriteLine("[apk] MobileTemplate directory not found. Ensure the template is present in the runner directory.");
                return;
            }

            string projDir = Path.Combine(Path.GetTempPath(), "aplus_maui_" + Guid.NewGuid().ToString("N"));
            try
            {
                CopyMobileTemplate(templateDir, projDir, outputName);
                File.WriteAllText(Path.Combine(projDir, "Script.g.cs"),
                    "namespace A_Mobile;\npublic static class ScriptSource\n{\n    public static string Code => " + ToLiteral(scriptCode) + ";\n}\n");

                string csprojPath = Path.Combine(projDir, "A_Mobile.csproj");
                string csproj = File.ReadAllText(csprojPath);
                csproj = csproj.Replace("APP_NAME", SafeIdentifier(outputName));
                csproj = csproj.Replace("APP_TITLE", outputName);
                csproj = csproj.Replace("APP_ID", SafeIdentifier(outputName));
                File.WriteAllText(csprojPath, csproj);

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-android -c Release -o \"{targetDir}\" --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(180000);

                if (proc.ExitCode == 0)
                {
                    Console.WriteLine($"[apk] Android APK packaged to: {targetDir}");
                    CleanupTempDir(projDir);
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    if (errMsg.Contains("NETSDK1147"))
                    {
                        Console.WriteLine("[apk] MAUI workload not installed. Attempting auto-install...");
                        var wi = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "workload install maui-android",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        });
                        string wiOut = wi.StandardOutput.ReadToEnd();
                        string wiErr = wi.StandardError.ReadToEnd();
                        wi.WaitForExit(120000);
                        if (wi.ExitCode == 0)
                        {
                            Console.WriteLine("[apk] MAUI workload installed. Retrying publish...");
                            var retry = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "dotnet",
                                Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-android -c Release -o \"{targetDir}\" --nologo -v q",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            });
                            string retryOut = retry.StandardOutput.ReadToEnd();
                            string retryErr = retry.StandardError.ReadToEnd();
                            retry.WaitForExit(180000);
                            if (retry.ExitCode == 0)
                            {
                                Console.WriteLine($"[apk] Android APK packaged to: {targetDir}");
                                CleanupTempDir(projDir);
                                return;
                            }
                            errMsg = string.IsNullOrEmpty(retryErr) ? retryOut : retryErr;
                        }
                        else
                            Console.Error.WriteLine($"[apk] Workload install failed: {wiErr}");
                    }
                    else if (errMsg.Contains("XA5300"))
                    {
                        Console.WriteLine("[apk] Android SDK not found. Attempting to accept licenses and install...");
                        Console.WriteLine("[apk] Checking for ANDROID_HOME/ANDROID_SDK_ROOT...");
                        string androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
                        string androidSdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
                        if (string.IsNullOrEmpty(androidHome) && string.IsNullOrEmpty(androidSdkRoot))
                        {
                            Console.WriteLine("[apk] ANDROID_HOME not set. Trying 'dotnet android' command...");
                            var ac = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "dotnet",
                                Arguments = "android",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            });
                            string acOut = ac.StandardOutput.ReadToEnd();
                            string acErr = ac.StandardError.ReadToEnd();
                            ac.WaitForExit(60000);
                            if (ac.ExitCode == 0)
                            {
                                Console.WriteLine("[apk] Android SDK installed via 'dotnet android'. Retrying...");
                                var retry2 = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "dotnet",
                                    Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-android -c Release -o \"{targetDir}\" --nologo -v q",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    CreateNoWindow = true
                                });
                                string r2Out = retry2.StandardOutput.ReadToEnd();
                                string r2Err = retry2.StandardError.ReadToEnd();
                                retry2.WaitForExit(180000);
                                if (retry2.ExitCode == 0)
                                {
                                    Console.WriteLine($"[apk] Android APK packaged to: {targetDir}");
                                    CleanupTempDir(projDir);
                                    return;
                                }
                                errMsg = string.IsNullOrEmpty(r2Err) ? r2Out : r2Err;
                            }
                            else
                                Console.Error.WriteLine($"[apk] 'dotnet android' failed: {acErr}");
                        }
                        Console.WriteLine("[apk] To install Android SDK manually:");
                        Console.WriteLine("[apk]   1. Install Android Studio from https://developer.android.com/studio");
                        Console.WriteLine("[apk]   2. Set environment variable: ANDROID_HOME = C:\\Users\\<user>\\AppData\\Local\\Android\\Sdk");
                        Console.WriteLine("[apk]   3. Or run: dotnet android");
                    }
                    Console.Error.WriteLine($"[apk] Package failed: {errMsg}");
                    Console.WriteLine($"[apk] Temp project kept at: {projDir}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[apk] Error: {ex.Message}");
                Console.WriteLine($"[apk] Temp project kept at: {projDir}");
            }
        }

        static void PackageForiOS(string scriptCode, string outputName, string buildDir)
        {
            string targetDir = Path.Combine(buildDir, "ios");
            string templateDir = MobileTemplateDir;
            if (templateDir == null)
            {
                Console.Error.WriteLine("[ios] MobileTemplate directory not found.");
                return;
            }

            string projDir = Path.Combine(Path.GetTempPath(), "aplus_maui_" + Guid.NewGuid().ToString("N"));
            try
            {
                CopyMobileTemplate(templateDir, projDir, outputName);
                File.WriteAllText(Path.Combine(projDir, "Script.g.cs"),
                    "namespace A_Mobile;\npublic static class ScriptSource\n{\n    public static string Code => " + ToLiteral(scriptCode) + ";\n}\n");

                string csprojPath = Path.Combine(projDir, "A_Mobile.csproj");
                string csproj = File.ReadAllText(csprojPath);
                csproj = csproj.Replace("APP_NAME", SafeIdentifier(outputName));
                csproj = csproj.Replace("APP_TITLE", outputName);
                csproj = csproj.Replace("APP_ID", SafeIdentifier(outputName));
                File.WriteAllText(csprojPath, csproj);

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-ios -c Release -o \"{targetDir}\" --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(180000);

                if (proc.ExitCode == 0)
                {
                    Console.WriteLine($"[ios] iOS app packaged to: {targetDir}");
                    CleanupTempDir(projDir);
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    if (errMsg.Contains("NETSDK1147"))
                    {
                        Console.WriteLine("[ios] MAUI workload not installed. Attempting auto-install...");
                        var wi = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "workload install maui-ios",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        });
                        string wiOut = wi.StandardOutput.ReadToEnd();
                        string wiErr = wi.StandardError.ReadToEnd();
                        wi.WaitForExit(120000);
                        if (wi.ExitCode == 0)
                        {
                            Console.WriteLine("[ios] MAUI workload installed. Retrying publish...");
                            var retry = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "dotnet",
                                Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-ios -c Release -o \"{targetDir}\" --nologo -v q",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            });
                            string retryOut = retry.StandardOutput.ReadToEnd();
                            string retryErr = retry.StandardError.ReadToEnd();
                            retry.WaitForExit(180000);
                            if (retry.ExitCode == 0)
                            {
                                Console.WriteLine($"[ios] iOS app packaged to: {targetDir}");
                                CleanupTempDir(projDir);
                                return;
                            }
                            errMsg = string.IsNullOrEmpty(retryErr) ? retryOut : retryErr;
                        }
                        else
                            Console.Error.WriteLine($"[ios] Workload install failed: {wiErr}");
                    }
                    Console.Error.WriteLine($"[ios] Package failed: {errMsg}");
                    Console.WriteLine($"[ios] Temp project kept at: {projDir}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ios] Error: {ex.Message}");
                Console.WriteLine($"[ios] Temp project kept at: {projDir}");
            }
        }

        static void PackageForAab(string scriptCode, string outputName, string buildDir)
        {
            string targetDir = Path.Combine(buildDir, "aab");
            string templateDir = MobileTemplateDir;
            if (templateDir == null)
            {
                Console.Error.WriteLine("[aab] MobileTemplate directory not found.");
                return;
            }

            string projDir = Path.Combine(Path.GetTempPath(), "aplus_maui_" + Guid.NewGuid().ToString("N"));
            try
            {
                CopyMobileTemplate(templateDir, projDir, outputName);
                File.WriteAllText(Path.Combine(projDir, "Script.g.cs"),
                    "namespace A_Mobile;\npublic static class ScriptSource\n{\n    public static string Code => " + ToLiteral(scriptCode) + ";\n}\n");

                string csprojPath = Path.Combine(projDir, "A_Mobile.csproj");
                string csproj = File.ReadAllText(csprojPath);
                csproj = csproj.Replace("APP_NAME", SafeIdentifier(outputName));
                csproj = csproj.Replace("APP_TITLE", outputName);
                csproj = csproj.Replace("APP_ID", SafeIdentifier(outputName));
                File.WriteAllText(csprojPath, csproj);

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-android -c Release -o \"{targetDir}\" -p:AndroidPackageFormat=aab --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(180000);

                if (proc.ExitCode == 0)
                {
                    Console.WriteLine($"[aab] Android AAB packaged to: {targetDir}");
                    CleanupTempDir(projDir);
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    if (errMsg.Contains("NETSDK1147"))
                    {
                        Console.WriteLine("[aab] MAUI workload not installed. Attempting auto-install...");
                        var wi = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "workload install maui-android",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        });
                        string wiOut = wi.StandardOutput.ReadToEnd();
                        string wiErr = wi.StandardError.ReadToEnd();
                        wi.WaitForExit(120000);
                        if (wi.ExitCode == 0)
                        {
                            Console.WriteLine("[aab] MAUI workload installed. Retrying publish...");
                            var retry = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "dotnet",
                                Arguments = $"publish \"{projDir}/A_Mobile.csproj\" -f net10.0-android -c Release -o \"{targetDir}\" -p:AndroidPackageFormat=aab --nologo -v q",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            });
                            string retryOut = retry.StandardOutput.ReadToEnd();
                            string retryErr = retry.StandardError.ReadToEnd();
                            retry.WaitForExit(180000);
                            if (retry.ExitCode == 0)
                            {
                                Console.WriteLine($"[aab] Android AAB packaged to: {targetDir}");
                                CleanupTempDir(projDir);
                                return;
                            }
                            errMsg = string.IsNullOrEmpty(retryErr) ? retryOut : retryErr;
                        }
                        else
                            Console.Error.WriteLine($"[aab] Workload install failed: {wiErr}");
                    }
                    Console.Error.WriteLine($"[aab] Package failed: {errMsg}");
                    Console.WriteLine($"[aab] Temp project kept at: {projDir}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[aab] Error: {ex.Message}");
                Console.WriteLine($"[aab] Temp project kept at: {projDir}");
            }
        }

        static void PackageForAppImage(string scriptCode, string outputName, string buildDir)
        {
            string targetDir = Path.Combine(buildDir, "appimage");
            Directory.CreateDirectory(targetDir);

            // Build linux-x64 binary using existing PackageForExe logic
            string tempDir = Path.Combine(Path.GetTempPath(), "aplus_appimage_" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(tempDir);
                string scriptEmbedPath = Path.Combine(tempDir, "main.a");
                File.WriteAllText(scriptEmbedPath, scriptCode);

                string csproj = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <AssemblyName>" + SafeIdentifier(outputName) + @"</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Content Include=""stdlib\**\*"" CopyToOutputDirectory=""PreserveNewest"" />
  </ItemGroup>
</Project>";

                File.WriteAllText(Path.Combine(tempDir, "pkg.csproj"), csproj);

                string programCs = "using System;\n"
                    + "using System.IO;\n"
                    + "using System.Text.RegularExpressions;\n"
                    + "using System.Collections.Generic;\n"
                    + "using System.Linq;\n"
                    + "using A_.Platform;\n"
                    + "\n"
                    + "namespace A_\n{\n    class Program\n    {\n"
                    + "        static void Main(string[] args)\n        {\n"
                    + "            Interpreter interp = null;\n"
                    + "            try\n"
                    + "            {\n"
                    + "                string code = " + ToLiteral(scriptCode) + ";\n"
                    + "                var lexer = new Lexer(code);\n"
                    + "                var tokens = lexer.Tokenize();\n"
                    + "                var parser = new Parser(tokens);\n"
                    + "                var ast = parser.Parse();\n"
                    + "                interp = new Interpreter();\n"
                    + "                interp.Output = msg => { try { Console.WriteLine(msg); } catch { } };\n"
                    + "                interp.BaseDir = Path.GetDirectoryName(Environment.ProcessPath) ?? \".\";\n"
                    + "                interp.UIRuntime = new ConsoleRuntime();\n"
                    + "                interp.StdLibPath = Path.Combine(AppContext.BaseDirectory, \"stdlib\");\n"
                    + "                interp.LoadStdLib();\n"
                    + "                interp.Visit(ast);\n"
                    + "            }\n"
                    + "            catch (Exception ex)\n"
                    + "            {\n"
                    + "                try { Console.Error.WriteLine(ex.ToString()); } catch { }\n"
                    + "            }\n"
                    + "        }\n    }\n}\n";
                File.WriteAllText(Path.Combine(tempDir, "Program.cs"), programCs);

                string libDir = Path.Combine(tempDir, "stdlib");
                Directory.CreateDirectory(libDir);

                string runnerProjectDir = ResolveRunnerProjectDir();
                foreach (string f in Directory.GetFiles(runnerProjectDir, "*.cs", SearchOption.AllDirectories))
                {
                    string fn = Path.GetFileName(f);
                    string rel = Path.GetRelativePath(runnerProjectDir, f);
                    if (fn == "Program.cs" || fn.EndsWith(".g.cs") || fn.EndsWith(".i.cs")) continue;
                    if (rel.StartsWith("MobileTemplate", StringComparison.OrdinalIgnoreCase)) continue;
                    string dest = Path.Combine(tempDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(f, dest, true);
                }

                string stdLibSrc = ResolveStdLibDir();
                if (Directory.Exists(stdLibSrc))
                {
                    foreach (string f in Directory.GetFiles(stdLibSrc, "*.a"))
                        File.Copy(f, Path.Combine(libDir, Path.GetFileName(f)), true);
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{tempDir}/pkg.csproj\" -o \"{targetDir}\" --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(120000);

                if (proc.ExitCode == 0)
                {
                    string binaryName = SafeIdentifier(outputName);
                    string binaryPath = Path.Combine(targetDir, binaryName);
                    string desktopPath = Path.Combine(targetDir, binaryName + ".desktop");
                    string appDirPath = Path.Combine(targetDir, binaryName + ".AppDir");

                    // Create AppDir structure
                    Directory.CreateDirectory(Path.Combine(appDirPath, "usr", "bin"));
                    Directory.CreateDirectory(Path.Combine(appDirPath, "usr", "share", "applications"));
                    Directory.CreateDirectory(Path.Combine(appDirPath, "usr", "share", "icons", "hicolor", "256x256", "apps"));

                    // Copy binary into AppDir
                    string binarySrc = Path.Combine(targetDir, binaryName);
                    if (File.Exists(binarySrc))
                        File.Copy(binarySrc, Path.Combine(appDirPath, "usr", "bin", binaryName), true);

                    // Copy stdlib
                    string stdlibAppDir = Path.Combine(appDirPath, "usr", "lib", binaryName, "stdlib");
                    Directory.CreateDirectory(stdlibAppDir);
                    if (Directory.Exists(libDir))
                    {
                        foreach (string f in Directory.GetFiles(libDir, "*.a"))
                            File.Copy(f, Path.Combine(stdlibAppDir, Path.GetFileName(f)), true);
                    }

                    // Create .desktop file
                    File.WriteAllText(desktopPath, "[Desktop Entry]\n"
                        + "Type=Application\n"
                        + "Name=" + outputName + "\n"
                        + "Comment=A+ Application\n"
                        + "Exec=" + binaryName + "\n"
                        + "Icon=" + binaryName + "\n"
                        + "Terminal=false\n"
                        + "Categories=Utility;\n");

                    Console.WriteLine($"[appimage] linux-x64 + AppDir created at: {targetDir}");
                    Console.WriteLine($"[appimage] To create final AppImage, run: appimagetool \"{appDirPath}\"");
                    Console.WriteLine($"[appimage] Or use the binary directly: {binaryPath}");
                    Directory.Delete(tempDir, true);
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    Console.Error.WriteLine($"[appimage] Build failed: {errMsg}");
                    Console.WriteLine($"[appimage] Temp project kept at: {tempDir}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[appimage] Error: {ex.Message}");
                Console.WriteLine($"[appimage] Temp project kept at: {tempDir}");
            }
        }

        static void PackageForMacApp(string scriptCode, string outputName, string buildDir)
        {
            string targetDir = Path.Combine(buildDir, "macos-app");
            Directory.CreateDirectory(targetDir);

            // Build osx-x64 binary using existing PackageForExe logic
            string tempDir = Path.Combine(Path.GetTempPath(), "aplus_mac_" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(tempDir);
                string scriptEmbedPath = Path.Combine(tempDir, "main.a");
                File.WriteAllText(scriptEmbedPath, scriptCode);

                string csproj = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>osx-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>false</PublishTrimmed>
    <AssemblyName>" + SafeIdentifier(outputName) + @"</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Content Include=""stdlib\**\*"" CopyToOutputDirectory=""PreserveNewest"" />
  </ItemGroup>
</Project>";

                File.WriteAllText(Path.Combine(tempDir, "pkg.csproj"), csproj);

                string programCs = "using System;\n"
                    + "using System.IO;\n"
                    + "using System.Text.RegularExpressions;\n"
                    + "using System.Collections.Generic;\n"
                    + "using System.Linq;\n"
                    + "using A_.Platform;\n"
                    + "\n"
                    + "namespace A_\n{\n    class Program\n    {\n"
                    + "        static void Main(string[] args)\n        {\n"
                    + "            Interpreter interp = null;\n"
                    + "            try\n"
                    + "            {\n"
                    + "                string code = " + ToLiteral(scriptCode) + ";\n"
                    + "                var lexer = new Lexer(code);\n"
                    + "                var tokens = lexer.Tokenize();\n"
                    + "                var parser = new Parser(tokens);\n"
                    + "                var ast = parser.Parse();\n"
                    + "                interp = new Interpreter();\n"
                    + "                interp.Output = msg => { try { Console.WriteLine(msg); } catch { } };\n"
                    + "                interp.BaseDir = Path.GetDirectoryName(Environment.ProcessPath) ?? \".\";\n"
                    + "                interp.UIRuntime = new ConsoleRuntime();\n"
                    + "                interp.StdLibPath = Path.Combine(AppContext.BaseDirectory, \"stdlib\");\n"
                    + "                interp.LoadStdLib();\n"
                    + "                interp.Visit(ast);\n"
                    + "            }\n"
                    + "            catch (Exception ex)\n"
                    + "            {\n"
                    + "                try { Console.Error.WriteLine(ex.ToString()); } catch { }\n"
                    + "            }\n"
                    + "        }\n    }\n}\n";
                File.WriteAllText(Path.Combine(tempDir, "Program.cs"), programCs);

                string libDir = Path.Combine(tempDir, "stdlib");
                Directory.CreateDirectory(libDir);

                string runnerProjectDir = ResolveRunnerProjectDir();
                foreach (string f in Directory.GetFiles(runnerProjectDir, "*.cs", SearchOption.AllDirectories))
                {
                    string fn = Path.GetFileName(f);
                    string rel = Path.GetRelativePath(runnerProjectDir, f);
                    if (fn == "Program.cs" || fn.EndsWith(".g.cs") || fn.EndsWith(".i.cs")) continue;
                    if (rel.StartsWith("MobileTemplate", StringComparison.OrdinalIgnoreCase)) continue;
                    string dest = Path.Combine(tempDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(f, dest, true);
                }

                string stdLibSrc = ResolveStdLibDir();
                if (Directory.Exists(stdLibSrc))
                {
                    foreach (string f in Directory.GetFiles(stdLibSrc, "*.a"))
                        File.Copy(f, Path.Combine(libDir, Path.GetFileName(f)), true);
                }

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{tempDir}/pkg.csproj\" -o \"{targetDir}/temp_bin\" --nologo -v q",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(120000);

                if (proc.ExitCode == 0)
                {
                    string binaryName = SafeIdentifier(outputName);
                    string binarySrc = Path.Combine(targetDir, "temp_bin", binaryName);

                    // Create .app bundle structure
                    string appBundle = Path.Combine(targetDir, outputName + ".app");
                    string macosDir = Path.Combine(appBundle, "Contents", "MacOS");
                    string resourcesDir = Path.Combine(appBundle, "Contents", "Resources");
                    Directory.CreateDirectory(macosDir);
                    Directory.CreateDirectory(resourcesDir);

                    // Copy binary
                    if (File.Exists(binarySrc))
                        File.Copy(binarySrc, Path.Combine(macosDir, binaryName), true);

                    // Copy stdlib
                    string stdlibDest = Path.Combine(resourcesDir, "stdlib");
                    Directory.CreateDirectory(stdlibDest);
                    if (Directory.Exists(libDir))
                    {
                        foreach (string f in Directory.GetFiles(libDir, "*.a"))
                            File.Copy(f, Path.Combine(stdlibDest, Path.GetFileName(f)), true);
                    }

                    // Create Info.plist
                    File.WriteAllText(Path.Combine(appBundle, "Contents", "Info.plist"),
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                        + "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n"
                        + "<plist version=\"1.0\">\n"
                        + "<dict>\n"
                        + "    <key>CFBundleExecutable</key>\n"
                        + "    <string>" + binaryName + "</string>\n"
                        + "    <key>CFBundleIdentifier</key>\n"
                        + "    <string>com.aplus." + SafeIdentifier(outputName) + "</string>\n"
                        + "    <key>CFBundleName</key>\n"
                        + "    <string>" + outputName + "</string>\n"
                        + "    <key>CFBundleVersion</key>\n"
                        + "    <string>1.0.0</string>\n"
                        + "    <key>CFBundlePackageType</key>\n"
                        + "    <string>APPL</string>\n"
                        + "    <key>LSMinimumSystemVersion</key>\n"
                        + "    <string>10.15</string>\n"
                        + "</dict>\n"
                        + "</plist>\n");

                    // Clean up temp bin
                    try { Directory.Delete(Path.Combine(targetDir, "temp_bin"), true); } catch { }

                    Console.WriteLine($"[macos_app] .app bundle created at: {appBundle}");
                    Directory.Delete(tempDir, true);
                }
                else
                {
                    string errMsg = string.IsNullOrEmpty(stderr) ? stdout : stderr;
                    Console.Error.WriteLine($"[macos_app] Build failed: {errMsg}");
                    Console.WriteLine($"[macos_app] Temp project kept at: {tempDir}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[macos_app] Error: {ex.Message}");
                Console.WriteLine($"[macos_app] Temp project kept at: {tempDir}");
            }
        }

        static void CopyMobileTemplate(string templateDir, string projDir, string outputName)
        {
            foreach (string file in Directory.GetFiles(templateDir, "*.*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(templateDir, file);
                string dest = Path.Combine(projDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(file, dest, true);
            }
        }

        static void CleanupTempDir(string dir)
        {
            try { Directory.Delete(dir, true); } catch { }
        }

        static string GetWebRuntimeJs()
        {
            string[] searchPaths = {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language", "Platform", "aplus_web_runtime.js"),
                Path.Combine(Environment.CurrentDirectory, "Language", "Platform", "aplus_web_runtime.js"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aplus_web_runtime.js"),
                Path.Combine(Environment.CurrentDirectory, "aplus_web_runtime.js"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "runner", "Language", "Platform", "aplus_web_runtime.js"),
            };
            foreach (var p in searchPaths)
            {
                var full = Path.GetFullPath(p);
                if (File.Exists(full)) return File.ReadAllText(full);
            }
            Console.Error.WriteLine("[web] Warning: aplus_web_runtime.js not found. Using limited runtime.");
            return "globalThis.aplusRun = function(code){ alert('A+ Runtime not found'); };";
        }

        static void PackageForWeb(string scriptCode, string outputName, string buildDir, string scriptPath = null)
        {
            string targetDir = Path.Combine(buildDir, "web");
            Directory.CreateDirectory(targetDir);

            // Resolve includes for web build (inline included files)
            string baseDir = scriptPath != null ? Path.GetDirectoryName(Path.GetFullPath(scriptPath)) : ".";
            string resolvedCode = Shorthand.ResolveIncludesForWeb(scriptCode, baseDir);
            // Apply shorthand conversion at build time
            string convertedCode = Shorthand.Convert(resolvedCode);

            try
            {
                // Parse and transpile
                var tokens = new Lexer(convertedCode).Tokenize();
                var ast = new Parser(tokens).Parse();
                var gen = new WebCodeGen();
                gen.Generate(ast, outputName, targetDir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[web] Warning: Transpilation failed, falling back to interpreter mode: {ex.Message}");
                // Fall back to old interpreter-based approach
                PackageForWebInterpreter(scriptCode, outputName, buildDir, scriptPath);
            }

            // Copy referenced assets (images, videos) to web output directory
            if (scriptPath != null)
                CopyWebAssets(scriptPath, targetDir);
        }

        static void CopyWebAssets(string scriptPath, string targetDir)
        {
            try
            {
                string scriptDir = Path.GetDirectoryName(Path.GetFullPath(scriptPath));
                string htmlPath = Path.Combine(targetDir, "index.html");
                if (!File.Exists(htmlPath)) return;

                string html = File.ReadAllText(htmlPath);
                // Match src="..." in img, video, source, audio, embed tags
                var regex = new Regex(@"(?:src|href)\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                int count = 0;
                foreach (Match m in regex.Matches(html))
                {
                    string src = m.Groups[1].Value;
                    // Skip URLs, absolute paths, data URIs
                    if (string.IsNullOrWhiteSpace(src)) continue;
                    if (src.StartsWith("http://") || src.StartsWith("https://") || src.StartsWith("//")) continue;
                    if (src.StartsWith("data:")) continue;
                    if (Path.IsPathRooted(src)) continue;

                    string sourceFile = Path.GetFullPath(Path.Combine(scriptDir, src));
                    string destFile = Path.Combine(targetDir, src);
                    if (File.Exists(sourceFile) && !File.Exists(destFile))
                    {
                        string destDir = Path.GetDirectoryName(destFile);
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                        File.Copy(sourceFile, destFile, overwrite: false);
                        count++;
                    }
                }
                if (count > 0)
                    Console.WriteLine($"[web] Copied {count} asset file(s) to web output");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[web] Warning: Could not copy assets: {ex.Message}");
            }
        }

        static void PackageForWebInterpreter(string scriptCode, string outputName, string buildDir, string scriptPath = null)
        {
            string targetDir = Path.Combine(buildDir, "web");
            Directory.CreateDirectory(targetDir);

            string baseDir = scriptPath != null ? Path.GetDirectoryName(Path.GetFullPath(scriptPath)) : ".";
            scriptCode = Shorthand.ResolveIncludesForWeb(scriptCode, baseDir);
            string runtimeJs = GetWebRuntimeJs();

            string escapedOutputName = outputName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            string html = "<!DOCTYPE html>\n<html lang=\"en\" dir=\"auto\">\n<head>\n"
                + "    <meta charset=\"UTF-8\">\n"
                + "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n"
                + "    <title>" + escapedOutputName + " - A+ Web App</title>\n"
                + "    <style>\n"
                + "        :root { --bg: #0f0f23; --card: #1a1a3e; --text: #e0e0ff; --accent: #7c3aed; --accent2: #06b6d4; --border: #2a2a5e; --success: #10b981; --error: #ef4444; }\n"
                + "        * { box-sizing: border-box; margin: 0; padding: 0; }\n"
                + "        body { background: var(--bg); color: var(--text); font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; min-height: 100vh; display: flex; flex-direction: column; align-items: center; padding: 1rem; }\n"
                + "        .container { max-width: 960px; width: 100%; }\n"
                + "        header { text-align: center; padding: 1.5rem 1rem; }\n"
                + "        header h1 { font-size: 1.5rem; font-weight: 700; background: linear-gradient(135deg, var(--accent), var(--accent2)); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; }\n"
                + "        header p { font-size: 0.85rem; color: #8888bb; margin-top: 0.25rem; }\n"
                + "        .tabs { display: flex; gap: 0; margin-bottom: 0; background: var(--card); border-radius: 12px 12px 0 0; border: 1px solid var(--border); border-bottom: none; overflow: hidden; }\n"
                + "        .tab { padding: 0.6rem 1.2rem; cursor: pointer; font-size: 0.85rem; font-weight: 500; color: #8888bb; background: transparent; border: none; border-bottom: 2px solid transparent; transition: all 0.2s; }\n"
                + "        .tab:hover { color: var(--text); background: rgba(255,255,255,0.05); }\n"
                + "        .tab.active { color: var(--accent2); border-bottom-color: var(--accent2); background: rgba(6,182,212,0.08); }\n"
                + "        .panel { display: none; background: var(--card); border: 1px solid var(--border); padding: 1rem; border-radius: 0 0 12px 12px; }\n"
                + "        .panel.active { display: block; }\n"
                + "        #source { font-family: 'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace; font-size: 0.82rem; line-height: 1.5; white-space: pre-wrap; overflow-x: auto; color: #c8d6e5; padding: 0.5rem; max-height: 300px; overflow-y: auto; }\n"
                + "        .kw { color: #7c3aed; } .str { color: #10b981; } .num { color: #f59e0b; } .cm { color: #6b7280; font-style: italic; } .fn { color: #06b6d4; } .op { color: #f472b6; }\n"
                + "        #output { font-family: 'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace; font-size: 0.82rem; line-height: 1.6; white-space: pre-wrap; overflow-x: auto; min-height: 200px; max-height: 500px; overflow-y: auto; padding: 0.5rem; }\n"
                + "        #output .error { color: var(--error); }\n"
                + "        #output .repl { color: #8888bb; }\n"
                + "        #console-input { display: flex; gap: 0.5rem; margin-top: 0.75rem; flex-wrap: wrap; }\n"
                + "        #console-input input { flex: 1; min-width: 150px; padding: 0.6rem 0.8rem; background: #0f0f23; border: 1px solid var(--border); border-radius: 8px; color: var(--text); font-family: 'JetBrains Mono', monospace; font-size: 0.82rem; outline: none; transition: border 0.2s; }\n"
                + "        #console-input input:focus { border-color: var(--accent); }\n"
                + "        #console-input button, .btn { padding: 0.6rem 1.2rem; background: linear-gradient(135deg, var(--accent), var(--accent2)); color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.82rem; transition: opacity 0.2s; }\n"
                + "        #console-input button:hover, .btn:hover { opacity: 0.9; }\n"
                + "        .btn-outline { background: transparent; border: 1px solid var(--border); color: var(--text); }\n"
                + "        .btn-outline:hover { border-color: var(--accent); }\n"
                + "        .stats { display: flex; gap: 1rem; margin-top: 0.75rem; font-size: 0.75rem; color: #6666aa; flex-wrap: wrap; }\n"
                + "        .stats span { display: flex; align-items: center; gap: 0.3rem; }\n"
                + "        #status { color: var(--success); }\n"
                + "        .repl-area { margin-top: 0.75rem; padding-top: 0.75rem; border-top: 1px solid var(--border); }\n"
                + "        .repl-area label { font-size: 0.75rem; color: #6666aa; display: block; margin-bottom: 0.4rem; }\n"
                + "        @media (max-width: 600px) { header h1 { font-size: 1.2rem; } .tab { padding: 0.4rem 0.8rem; font-size: 0.75rem; } }\n"
                + "        ::-webkit-scrollbar { width: 6px; height: 6px; }\n"
                + "        ::-webkit-scrollbar-track { background: transparent; }\n"
                + "        ::-webkit-scrollbar-thumb { background: var(--border); border-radius: 3px; }\n"
                + "    </style>\n</head>\n<body>\n"
                + "    <div class=\"container\">\n"
                + "        <header>\n"
                + "            <h1>⚡ " + escapedOutputName + "</h1>\n"
                + "            <p>A+ Language · Web Runtime · يعمل في المتصفح</p>\n"
                + "        </header>\n"
                + "        <div class=\"tabs\">\n"
                + "            <button class=\"tab active\" onclick=\"switchTab('output-tab')\" id=\"tab-output\">▶ Output / المخرجات</button>\n"
                + "            <button class=\"tab\" onclick=\"switchTab('source-tab')\" id=\"tab-source\">📄 Source / الكود</button>\n"
                + "        </div>\n"
                + "        <div class=\"panel active\" id=\"output-tab\">\n"
                + "            <div id=\"output\"><span id=\"status\">⟳ Loading A+ runtime...</span></div>\n"
                + "            <div class=\"repl-area\">\n"
                + "                <label>▶ REPL — Enter A+ code / أدخل كود A+</label>\n"
                + "                <div id=\"console-input\">\n"
                + "                    <input type=\"text\" id=\"input-field\" placeholder=\"print(&quot;Hello&quot;)\" spellcheck=\"false\" autofocus>\n"
                + "                    <button onclick=\"runCode()\">Run / تشغيل</button>\n"
                + "                    <button class=\"btn-outline\" onclick=\"clearOutput()\">Clear / مسح</button>\n"
                + "                </div>\n"
                + "                <div class=\"stats\">\n"
                + "                    <span id=\"status-display\">● <span id=\"status\">Ready</span></span>\n"
                + "                    <span id=\"stats-time\">⏱ —</span>\n"
                + "                </div>\n"
                + "            </div>\n"
                + "        </div>\n"
                + "        <div class=\"panel\" id=\"source-tab\">\n"
                + "            <div id=\"source\">" + escapedOutputName + "</div>\n"
                + "        </div>\n"
                + "    </div>\n"
                + "    <script src=\"runtime.js\"></script>\n"
                + "    <script src=\"app.js\"></script>\n"
                + "</body>\n</html>";
            File.WriteAllText(Path.Combine(targetDir, "index.html"), html);

            string jsonScriptCode = System.Text.Json.JsonSerializer.Serialize(scriptCode);
            string jsonAppName = System.Text.Json.JsonSerializer.Serialize(outputName);
            string appJs = "// " + outputName + " - A+ Web App\n"
                + "// Script embedded below\n"
                + "const scriptCode = " + jsonScriptCode + ";\n\n"
                + "const appName = " + jsonAppName + ";\n"
                + "const outputDiv = document.getElementById('output');\n"
                + "const sourceDiv = document.getElementById('source');\n\n"
                + "// Show source code\n"
                + "if (sourceDiv) sourceDiv.textContent = scriptCode;\n\n"
                + "// Switch tabs\n"
                + "function switchTab(id) {\n"
                + "    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));\n"
                + "    document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));\n"
                + "    document.getElementById(id).classList.add('active');\n"
                + "    document.querySelector('[onclick*=\"' + id + '\"]').classList.add('active');\n"
                + "}\n\n"
                + "function clearOutput() {\n"
                + "    outputDiv.innerHTML = '';\n"
                + "}\n\n"
                + "function updateStats(startTime) {\n"
                + "    const elapsed = Date.now() - startTime;\n"
                + "    const statsTime = document.getElementById('stats-time');\n"
                + "    if (statsTime) statsTime.textContent = '⏱ ' + elapsed + 'ms';\n"
                + "}\n\n"
                + "function print(msg) {\n"
                + "    const escaped = String(msg).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');\n"
                + "    outputDiv.innerHTML += escaped + '\\n';\n"
                + "}\n\n"
                + "function printError(msg) {\n"
                + "    outputDiv.innerHTML += '<span class=\"error\">[Error] ' + String(msg).replace(/&/g, '&amp;').replace(/</g, '&lt;') + '</span>\\n';\n"
                + "}\n\n"
                + "function runCode() {\n"
                + "    const input = document.getElementById('input-field');\n"
                + "    if (!input.value.trim()) return;\n"
                + "    const code = input.value.trim();\n"
                + "    outputDiv.innerHTML += '<span class=\"repl\">> ' + code.replace(/&/g, '&amp;').replace(/</g, '&lt;') + '</span>\\n';\n"
                + "    const startTime = Date.now();\n"
                + "    try {\n"
                + "        globalThis.aplusRun(code, { print: print });\n"
                + "    } catch (e) {\n"
                + "        printError(e.message);\n"
                + "    }\n"
                + "    updateStats(startTime);\n"
                + "    input.value = '';\n"
                + "}\n\n"
                + "document.getElementById('input-field').addEventListener('keydown', function(e) {\n"
                + "    if (e.key === 'Enter') runCode();\n"
                + "});\n\n"
                + "// Run the embedded script\n"
                + "print('=== ' + appName + ' ===');\n"
                + "const startTime = Date.now();\n\n"
                + "try {\n"
                + "    globalThis.aplusRun(scriptCode, { print: print });\n"
                + "} catch (e) {\n"
                + "    printError(e.message);\n"
                + "}\n"
                + "updateStats(startTime);\n";
            File.WriteAllText(Path.Combine(targetDir, "app.js"), appJs);

            File.WriteAllText(Path.Combine(targetDir, "runtime.js"), runtimeJs);

            // Copy referenced assets to web output directory
            if (scriptPath != null)
                CopyWebAssets(scriptPath, targetDir);
        }

        static string ToLiteral(string input)
        {
            return "@\"" + input.Replace("\"", "\"\"") + "\"";
        }
    }
}
