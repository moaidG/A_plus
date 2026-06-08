using System;
using System.Collections.Generic;
using System.Threading;
using A_.Platform;

namespace A_Mobile;

public class MauiRuntime : IUIRuntime
{
    private readonly MainPage _page;
    private readonly Dictionary<string, UIElementHandle> _elements = new();
    private readonly Dictionary<string, object> _properties = new();
    private readonly Dictionary<string, Action> _handlers = new();
    private Timer _timer;
    private bool _disposed;

    public MauiRuntime(MainPage page)
    {
        _page = page;
    }

    public bool IsUIAvailable => true;

    public UIElementHandle RootPanel
    {
        get
        {
            if (!_elements.ContainsKey("__root"))
            {
                var root = new UIElementHandle("__MAUI_ROOT__", "stackpanel") { VarName = "__root" };
                _elements["__root"] = root;
            }
            return _elements["__root"];
        }
    }

    public Dictionary<string, UIElementHandle> AllElements => _elements;

    public void EnsureWindow() { }
    public void ShowWindow() { }
    public void CloseWindow() { }
    public void UpdateLayout() { }

    public UIElementHandle CreateElement(string typeName)
    {
        string tn = typeName.ToLower();
        _properties[tn + "_count"] = (_properties.ContainsKey(tn + "_count") ? (int)_properties[tn + "_count"] : 0) + 1;
        return new UIElementHandle(null, tn);
    }

    public void SetProperty(UIElementHandle element, string propName, object value)
    {
        string key = (element.VarName ?? "?") + "." + propName;
        _properties[key] = value;
    }

    public object GetProperty(UIElementHandle element, string propName)
    {
        string key = (element.VarName ?? "?") + "." + propName;
        _properties.TryGetValue(key, out var val);
        return val;
    }

    public void BindEvent(UIElementHandle element, string eventName, Action callback)
    {
        string key = (element.VarName ?? "?") + "." + eventName;
        _handlers[key] = callback;
    }

    public void AddChild(UIElementHandle parent, UIElementHandle child)
    {
        _page.AppendOutput($"[UI] أضف {child.TypeName} داخل {parent.TypeName}");
    }

    public void RemoveChild(UIElementHandle parent, UIElementHandle child) { }

    public bool HasParent(UIElementHandle element) => false;

    public void ShowError(string message)
    {
        _page.AppendOutput($"[خطأ] {message}");
    }

    public void AddErrorText(string error)
    {
        _page.AppendOutput($"[خطأ UI] {error}");
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

    public UIElementHandle Create3DObject(string type, double arg1, double arg2, double arg3) => null;

    public void CallMethod(UIElementHandle element, string methodName)
    {
        if (element?.TypeName?.ToLower() == "mediaelement")
        {
            string src = _properties.TryGetValue((element.VarName ?? "?") + ".Source", out var s) ? s?.ToString() ?? "" : "";
            _page.AppendOutput($"[وسائط] {methodName}: {src}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopTimer();
        _elements.Clear();
    }
}
