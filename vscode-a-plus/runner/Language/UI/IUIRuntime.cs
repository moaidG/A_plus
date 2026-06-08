using System;
using System.Collections.Generic;

namespace A_.Platform
{
    public class UIElementHandle
    {
        public object NativeElement { get; }
        public string TypeName { get; }
        public string VarName { get; set; }

        public UIElementHandle(object nativeElement, string typeName)
        {
            NativeElement = nativeElement;
            TypeName = typeName;
        }

        public T GetNative<T>() where T : class => NativeElement as T;

        public Type NativeType => NativeElement?.GetType();
    }

    public interface IUIRuntime : IDisposable
    {
        bool IsUIAvailable { get; }
        void EnsureWindow();
        void ShowWindow();
        void CloseWindow();
        void UpdateLayout();
        UIElementHandle CreateElement(string typeName);
        void SetProperty(UIElementHandle element, string propName, object value);
        object GetProperty(UIElementHandle element, string propName);
        void BindEvent(UIElementHandle element, string eventName, Action callback);
        void AddChild(UIElementHandle parent, UIElementHandle child);
        void RemoveChild(UIElementHandle parent, UIElementHandle child);
        bool HasParent(UIElementHandle element);
        void WriteOutput(string message);
        string ReadInput(string prompt);
        void ShowError(string message);
        void AddErrorText(string error);
        UIElementHandle RootPanel { get; }
        void RegisterElement(string varName, UIElementHandle element);
        UIElementHandle GetElement(string varName);
        Dictionary<string, UIElementHandle> AllElements { get; }
        void StartTimer(int intervalMs, Action callback);
        void StopTimer();
        UIElementHandle Create3DObject(string type, double arg1, double arg2, double arg3);
        void CallMethod(UIElementHandle element, string methodName);
        void ToggleFullscreen(UIElementHandle element);
        bool HasShownWindow();
    }
}
