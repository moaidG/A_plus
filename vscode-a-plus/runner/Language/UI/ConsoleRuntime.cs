using System;
using System.Collections.Generic;
using System.Threading;

namespace A_.Platform
{
    public class ConsoleRuntime : IUIRuntime
    {
        public static bool PauseHandled { get; set; } = false;

        private Dictionary<string, UIElementHandle> _elements = new Dictionary<string, UIElementHandle>();
        private List<string> _errorList = new List<string>();
        private Timer _timer;
        private bool _shown = false;

        public bool IsUIAvailable => false;

        public UIElementHandle RootPanel
        {
            get
            {
                EnsureWindow();
                return _elements.ContainsKey("__root") ? _elements["__root"] : null;
            }
        }

        public Dictionary<string, UIElementHandle> AllElements => _elements;

        public void EnsureWindow()
        {
            if (!_elements.ContainsKey("__root"))
            {
                var root = new UIElementHandle(new ConsolePanel("root"), "stackpanel") { VarName = "__root" };
                _elements["__root"] = root;
            }
        }

        public UIElementHandle CreateElement(string typeName)
        {
            string tn = typeName.ToLower();
            var consoleElem = new ConsoleElement(tn);
            return new UIElementHandle(consoleElem, tn);
        }

        public void SetProperty(UIElementHandle element, string propName, object value)
        {
            var ce = element.GetNative<ConsoleElement>();
            if (ce != null)
                ce.Properties[propName.ToLower()] = value?.ToString() ?? "";
        }

        public object GetProperty(UIElementHandle element, string propName)
        {
            var ce = element.GetNative<ConsoleElement>();
            if (ce != null && ce.Properties.TryGetValue(propName.ToLower(), out var val))
                return val;
            return null;
        }

        public void BindEvent(UIElementHandle element, string eventName, Action callback)
        {
            var ce = element.GetNative<ConsoleElement>();
            if (ce != null)
                ce.EventHandlers[eventName.ToLower()] = callback;
        }

        public void AddChild(UIElementHandle parent, UIElementHandle child)
        {
            var parentCe = parent.GetNative<ConsolePanel>() ?? parent.GetNative<ConsoleElement>();
            var childCe = child.GetNative<ConsoleElement>();
            if (parentCe is ConsolePanel panel && childCe != null)
            {
                childCe.Parent = panel;
                panel.Children.Add(childCe);
            }
            else if (parentCe != null && childCe != null)
            {
                childCe.Parent = parentCe;
                parentCe.Children.Add(childCe);
            }
        }

        public void RemoveChild(UIElementHandle parent, UIElementHandle child)
        {
            var parentCe = parent.GetNative<ConsolePanel>() ?? parent.GetNative<ConsoleElement>();
            var childCe = child.GetNative<ConsoleElement>();
            if (parentCe != null && childCe != null)
                parentCe.Children.Remove(childCe);
        }

        public bool HasParent(UIElementHandle element)
        {
            return false; // Console mode doesn't track parentage
        }

        public void ShowError(string message)
        {
            Console.WriteLine($"[خطأ] {message}");
        }

        public void AddErrorText(string error)
        {
            _errorList.Add(error);
            Console.WriteLine($"[خطأ UI] {error}");
        }

        public void WriteOutput(string message)
        {
            Console.WriteLine(message);
        }

        public string ReadInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? "";
        }

        public void RegisterElement(string varName, UIElementHandle element)
        {
            element.VarName = varName;
            _elements[varName] = element;
        }

        public UIElementHandle GetElement(string varName)
        {
            _elements.TryGetValue(varName, out var elem);
            return elem;
        }

        public void StartTimer(int intervalMs, Action callback)
        {
            _timer?.Dispose();
            _timer = new Timer(_ => callback(), null, intervalMs, intervalMs);
        }

        public void StopTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void CallMethod(UIElementHandle element, string methodName)
        {
            var ce = element.GetNative<ConsoleElement>();
            if (ce == null) return;
            string tn = element.TypeName.ToLower();
            if (tn == "mediaelement" || tn == "video" || tn == "sound")
            {
                if (methodName.ToLower() == "play")
                {
                    string src = ce.Properties.TryGetValue("source", out var s) ? s?.ToString() ?? "" : "";
                    Console.WriteLine($"[وسائط] تشغيل: {src}");
                }
                else if (methodName.ToLower() == "pause")
                    Console.WriteLine("[وسائط] تم الإيقاف المؤقت");
                else if (methodName.ToLower() == "stop")
                    Console.WriteLine("[وسائط] تم الإيقاف");
            }
        }

        public void ToggleFullscreen(UIElementHandle element)
        {
            Console.WriteLine("[وسائط] شاشة كاملة (غير متاح في وضع النص)");
        }

        public void ShowWindow()
        {
            if (_shown) return;
            _shown = true;
            // Attach orphans to root
            if (_elements.TryGetValue("__root", out var rootHandle))
            {
                var rootCe = rootHandle.GetNative<ConsoleElement>();
                if (rootCe != null)
                {
                    foreach (var kv in _elements)
                    {
                        if (kv.Value == rootHandle) continue;
                        var ce = kv.Value.GetNative<ConsoleElement>();
                        if (ce != null && ce.Parent == null)
                        {
                            ce.Parent = rootCe;
                            rootCe.Children.Add(ce);
                        }
                    }
                }
            }
            Console.WriteLine("═══════════════════════════════════");
            Console.WriteLine(" A+ Console UI (وضع النص)");
            Console.WriteLine("═══════════════════════════════════");
            RenderElement(_elements.ContainsKey("__root") ? _elements["__root"] : null, 0);
            Console.WriteLine("═══════════════════════════════════");
            if (_errorList.Count > 0)
            {
                Console.WriteLine("الأخطاء:");
                foreach (var e in _errorList)
                    Console.WriteLine($"  • {e}");
            }
            try
            {
                Console.WriteLine("اضغط أي مفتاح للخروج...");
                Console.ReadKey(true);
                PauseHandled = true;
            }
            catch { /* ignore if stdin redirected */ }
            Cleanup();
        }

        public bool HasShownWindow() => _shown;

        public void CloseWindow()
        {
            Cleanup();
        }

        public void UpdateLayout()
        {
            // No-op in console mode
        }

        private void RenderElement(UIElementHandle elem, int indent)
        {
            if (elem == null) return;
            var ce = elem.GetNative<ConsoleElement>();
            if (ce == null) return;
            string prefix = new string(' ', indent * 2);
            string content = ce.Properties.ContainsKey("content") ? $": {ce.Properties["content"]}" :
                            ce.Properties.ContainsKey("text") ? $": {ce.Properties["text"]}" : "";
            Console.WriteLine($"{prefix}[{ce.TypeName}]{content}");
            foreach (var child in ce.Children)
                RenderElement(new UIElementHandle(child, child.TypeName), indent + 1);
        }

        private void Cleanup()
        {
            StopTimer();
            _elements.Clear();
            _errorList.Clear();
            _shown = false;
        }

        public void Dispose()
        {
            Cleanup();
        }

        public UIElementHandle Create3DObject(string type, double arg1, double arg2, double arg3)
        {
            var stub = new ConsoleElement(type);
            stub.Properties["x"] = arg1.ToString();
            stub.Properties["y"] = arg2.ToString();
            stub.Properties["z"] = arg3.ToString();
            return new UIElementHandle(stub, type);
        }
    }

    public class ConsoleElement
    {
        public string TypeName { get; }
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
        public Dictionary<string, Action> EventHandlers { get; } = new Dictionary<string, Action>();
        public List<ConsoleElement> Children { get; } = new List<ConsoleElement>();
        public ConsoleElement Parent { get; set; }
        public ConsoleElement(string typeName) { TypeName = typeName; }
    }

    public class ConsolePanel : ConsoleElement
    {
        public ConsolePanel(string typeName) : base(typeName) { }
    }
}
