#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace A_.Platform
{
    public class WpfRuntime : IUIRuntime
    {
        private Window _window;
        private StackPanel _rootPanel;
        private Dictionary<string, UIElementHandle> _elements = new Dictionary<string, UIElementHandle>();
        private DispatcherTimer _bindingTimer;
        private ListBox _errorListBox;
        private TextBox _consoleOutput;
        private bool _consoleVisible;
        private bool _windowShown;
        public bool IsUIAvailable => true;
        public UIElementHandle RootPanel { get; private set; }
        public Dictionary<string, UIElementHandle> AllElements => _elements;

        public void EnsureWindow()
        {
            if (_window == null)
            {
                _window = new Window();
                _window.Title = "A+ UI";
                _window.Width = 800;
                _window.Height = 600;
                _window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                _window.ShowInTaskbar = true;
                _window.Topmost = false;

                var dock = new DockPanel();
                _consoleOutput = new TextBox
                {
                    IsReadOnly = true,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    MaxHeight = 150,
                    Background = Brushes.Black,
                    Foreground = Brushes.LightGreen,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 12,
                    Visibility = Visibility.Collapsed
                };
                DockPanel.SetDock(_consoleOutput, Dock.Bottom);
                dock.Children.Add(_consoleOutput);

                _rootPanel = new StackPanel();
                _rootPanel.Margin = new Thickness(10);
                var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
                scroll.Content = _rootPanel;
                dock.Children.Add(scroll);

                _window.Content = dock;
                RootPanel = new UIElementHandle(_rootPanel, "stackpanel") { VarName = "__root" };
                _elements["__root"] = RootPanel;
            }
        }

        public UIElementHandle CreateElement(string typeName)
        {
            object elem;
            switch (typeName.ToLower())
            {
                case "grid": elem = new Grid { Margin = new Thickness(2) }; break;
                case "stackpanel": elem = new StackPanel { Margin = new Thickness(2) }; break;
                case "wrappanel": elem = new WrapPanel { Margin = new Thickness(2) }; break;
                case "dockpanel": elem = new DockPanel { Margin = new Thickness(2) }; break;
                case "canvas": elem = new Canvas { Margin = new Thickness(2) }; break;
                case "uniformgrid": elem = new UniformGrid { Margin = new Thickness(2) }; break;
                case "viewbox": elem = new Viewbox { Margin = new Thickness(2) }; break;
                case "border": elem = new Border { Margin = new Thickness(2), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) }; break;
                case "scrollviewer": elem = new ScrollViewer { Margin = new Thickness(2) }; break;
                case "textblock": elem = new TextBlock { Margin = new Thickness(2), FontSize = 14 }; break;
                case "textbox": elem = new TextBox { Margin = new Thickness(2) }; break;
                case "richtextbox": elem = new RichTextBox { Margin = new Thickness(2) }; break;
                case "label": elem = new Label { Margin = new Thickness(2) }; break;
                case "button": elem = new Button { Margin = new Thickness(2), Padding = new Thickness(10, 5, 10, 5) }; break;
                case "repeatbutton": elem = new RepeatButton { Margin = new Thickness(2) }; break;
                case "togglebutton": elem = new ToggleButton { Margin = new Thickness(2) }; break;
                case "checkbox": elem = new CheckBox { Margin = new Thickness(2) }; break;
                case "radiobutton": elem = new RadioButton { Margin = new Thickness(2) }; break;
                case "listbox": elem = new ListBox { Margin = new Thickness(2) }; break;
                case "listview": elem = new ListView { Margin = new Thickness(2) }; break;
                case "combobox": elem = new ComboBox { Margin = new Thickness(2) }; break;
                case "treeview": elem = new TreeView { Margin = new Thickness(2) }; break;
                case "menu": elem = new Menu { Margin = new Thickness(2) }; break;
                case "menuitem": elem = new MenuItem { Margin = new Thickness(2) }; break;
                case "contextmenu": elem = new ContextMenu(); break;
                case "tabcontrol": elem = new TabControl { Margin = new Thickness(2) }; break;
                case "tabitem": elem = new TabItem { Margin = new Thickness(2) }; break;
                case "image": case "img": elem = new Image { Margin = new Thickness(2) }; break;
                case "video": case "sound": case "mediaelement": elem = new MediaElement { Margin = new Thickness(2), Width = 400, Height = 300 }; break;
                case "slider": elem = new Slider { Margin = new Thickness(2), Width = 150 }; break;
                case "progressbar": elem = new ProgressBar { Margin = new Thickness(2), Width = 150, Height = 20 }; break;
                case "datepicker": elem = new DatePicker { Margin = new Thickness(2) }; break;
                case "calendar": elem = new Calendar { Margin = new Thickness(2) }; break;
                case "passwordbox": elem = new PasswordBox { Margin = new Thickness(2) }; break;
                case "rectangle": elem = new Rectangle { Width = 50, Height = 50, Fill = Brushes.LightGray, Stroke = Brushes.Gray, StrokeThickness = 1 }; break;
                case "ellipse": elem = new Ellipse { Width = 50, Height = 50, Fill = Brushes.LightGray, Stroke = Brushes.Gray, StrokeThickness = 1 }; break;
                case "line": elem = new Line { X1 = 0, Y1 = 0, X2 = 50, Y2 = 50, Stroke = Brushes.Black, StrokeThickness = 1 }; break;
                case "polygon": elem = new Polygon { Stroke = Brushes.Black, StrokeThickness = 1, Fill = Brushes.LightGray }; break;
                case "polyline": elem = new Polyline { Stroke = Brushes.Black, StrokeThickness = 1 }; break;
                case "path": elem = new Path { Stroke = Brushes.Black, StrokeThickness = 1, Fill = Brushes.LightGray }; break;
                case "window": elem = new StackPanel { Margin = new Thickness(2) }; break;
                case "viewport3d": elem = new Viewport3D { Width = 400, Height = 300, ClipToBounds = true }; break;
                default: elem = new Button { Content = typeName, Margin = new Thickness(2) }; break;
            }
            return new UIElementHandle(elem, typeName.ToLower());
        }

        public void SetProperty(UIElementHandle element, string propName, object value)
        {
            var nativeObj = element.NativeElement;
            if (nativeObj == null) return;
            var fe = nativeObj as FrameworkElement;
            if (fe == null) return;

            // Handle Viewport3D special members (Camera, Light, Object)
            if (fe is Viewport3D vp3d)
            {
                string m = propName;
                if (string.Equals(m, "cam", StringComparison.OrdinalIgnoreCase)) m = "Camera";
                if (string.Equals(m, "ligt", StringComparison.OrdinalIgnoreCase) || string.Equals(m, "lig", StringComparison.OrdinalIgnoreCase)) m = "Light";
                if (string.Equals(m, "objet", StringComparison.OrdinalIgnoreCase)) m = "Object";

                if (m == "Camera" && value is Camera cam)
                {
                    vp3d.Camera = cam;
                    return;
                }
                if (m == "Light" && value is Light light)
                {
                    vp3d.Children.Clear();
                    vp3d.Children.Add(new ModelVisual3D { Content = light });
                    return;
                }
                if (m == "Object" && value is Model3D model)
                {
                    vp3d.Children.Clear();
                    vp3d.Children.Add(new ModelVisual3D { Content = model });
                    return;
                }
                if (m == "Camera" || m == "Light" || m == "Object")
                {
                    // value might be wrapped in UIElementHandle
                    if (value is UIElementHandle uih)
                        value = uih.NativeElement;
                    if (m == "Camera" && value is Camera cam2) { vp3d.Camera = cam2; return; }
                    if (m == "Light" && value is Light light2)
                    {
                        vp3d.Children.Clear();
                        vp3d.Children.Add(new ModelVisual3D { Content = light2 });
                        return;
                    }
                    if (m == "Object" && value is Model3D model2)
                    {
                        vp3d.Children.Clear();
                        vp3d.Children.Add(new ModelVisual3D { Content = model2 });
                        return;
                    }
                }
            }

            // Handle MediaElement zoom via ScaleTransform
            if (string.Equals(propName, "ZoomFactor", StringComparison.OrdinalIgnoreCase) && fe is System.Windows.Controls.MediaElement)
            {
                double factor = Convert.ToDouble(value);
                fe.RenderTransform = new ScaleTransform(factor, factor);
                fe.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                return;
            }

            // MediaElement Position as double seconds -> TimeSpan
            if (string.Equals(propName, "Position", StringComparison.OrdinalIgnoreCase) && fe is System.Windows.Controls.MediaElement mePos)
            {
                mePos.Position = TimeSpan.FromSeconds(Math.Max(0, Convert.ToDouble(value)));
                return;
            }

            var prop = fe.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanWrite) return;
            try
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(fe, value?.ToString() ?? "");
                else if (prop.PropertyType == typeof(double))
                    prop.SetValue(fe, Convert.ToDouble(value));
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(fe, Convert.ToInt32(value));
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(fe, Convert.ToDouble(value) != 0 || value?.ToString() == "true");
                else if (prop.PropertyType == typeof(Thickness))
                {
                    var parts = (value?.ToString() ?? "0").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1) prop.SetValue(fe, new Thickness(Convert.ToDouble(parts[0])));
                    else if (parts.Length == 2) prop.SetValue(fe, new Thickness(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]), Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1])));
                    else if (parts.Length >= 4) prop.SetValue(fe, new Thickness(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2]), Convert.ToDouble(parts[3])));
                }
                else if (prop.PropertyType == typeof(Brush))
                    prop.SetValue(fe, new SolidColorBrush((Color)ColorConverter.ConvertFromString(value?.ToString() ?? "Gray")));
                else if (prop.PropertyType == typeof(Uri))
                    prop.SetValue(fe, new Uri(value?.ToString() ?? "", UriKind.RelativeOrAbsolute));
                else if (prop.PropertyType == typeof(System.Windows.Media.ImageSource))
                {
                    string uriStr = value?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(uriStr))
                    {
                        var img = new System.Windows.Media.Imaging.BitmapImage();
                        img.BeginInit();
                        img.UriSource = new Uri(uriStr, UriKind.RelativeOrAbsolute);
                        img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        img.EndInit();
                        prop.SetValue(fe, img);
                    }
                }
                else if (prop.PropertyType.IsEnum)
                {
                    var enumNames = Enum.GetNames(prop.PropertyType);
                    string valStr = value?.ToString() ?? "";
                    var matched = enumNames.FirstOrDefault(n => string.Equals(n, valStr, StringComparison.OrdinalIgnoreCase));
                    if (matched != null) prop.SetValue(fe, Enum.Parse(prop.PropertyType, matched));
                }
                else if (prop.PropertyType.IsInstanceOfType(value))
                    prop.SetValue(fe, value);
                else if (prop.PropertyType == typeof(HorizontalAlignment))
                {
                    if (Enum.TryParse<HorizontalAlignment>(value?.ToString(), true, out var ha))
                        prop.SetValue(fe, ha);
                }
                else if (prop.PropertyType == typeof(VerticalAlignment))
                {
                    if (Enum.TryParse<VerticalAlignment>(value?.ToString(), true, out var va))
                        prop.SetValue(fe, va);
                }
                else
                    prop.SetValue(fe, Convert.ChangeType(value, prop.PropertyType));
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تعيين الخاصية '{prop.Name}': {ex.Message}");
            }
        }

        public object GetProperty(UIElementHandle element, string propName)
        {
            var nativeObj = element.NativeElement;
            if (nativeObj == null) return null;
            var fe = nativeObj as FrameworkElement;
            if (fe == null) return null;
            string m = propName;
            if (m.ToLower() == "play") m = "Play";
            if (m.ToLower() == "pause") m = "Pause";
            if (m.ToLower() == "stop") m = "Stop";
            if (fe is System.Windows.Controls.MediaElement me)
            {
                if (string.Equals(m, "position", StringComparison.OrdinalIgnoreCase) || string.Equals(m, "موضع", StringComparison.OrdinalIgnoreCase))
                {
                    try { return me.Position.TotalSeconds; }
                    catch { return 0.0; }
                }
                if (string.Equals(m, "duration", StringComparison.OrdinalIgnoreCase) || string.Equals(m, "مدة", StringComparison.OrdinalIgnoreCase))
                {
                    if (me.NaturalDuration.HasTimeSpan)
                        return me.NaturalDuration.TimeSpan.TotalSeconds;
                    return 0.0;
                }
            }
            var prop = fe.GetType().GetProperty(m, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null) return prop.GetValue(fe);
            var methodInfo = fe.GetType().GetMethod(m, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (methodInfo != null && methodInfo.GetParameters().Length == 0)
            {
                try { return methodInfo.Invoke(fe, null); }
                catch { return 0; }
            }
            return null;
        }

        public void BindEvent(UIElementHandle element, string eventName, Action callback)
        {
            var nativeObj = element.NativeElement;
            if (nativeObj == null) return;
            string member = eventName;
            var fe = nativeObj as FrameworkElement;
            switch (member.ToLower())
            {
                case "نقر": member = "Click"; break;
                case "تحميل": member = "Loaded"; break;
                case "دخول_فأرة": member = "MouseEnter"; break;
                case "خروج_فأرة": member = "MouseLeave"; break;
                case "ضغط_فأرة": member = "MouseDown"; break;
                case "رفع_فأرة": member = "MouseUp"; break;
                case "تحريك_فأرة": member = "MouseMove"; break;
                case "ضغط_مفتاح": member = "KeyDown"; break;
                case "رفع_مفتاح": member = "KeyUp"; break;
                case "اكتسب_تركيز": member = "GotFocus"; break;
                case "فقد_تركيز": member = "LostFocus"; break;
                case "تحديث_تخطيط": member = "LayoutUpdated"; break;
                case "تغير_نص": member = "TextChanged"; break;
                case "تغير_اختيار": member = "SelectionChanged"; break;
                case "تغير_قيمة": member = "ValueChanged"; break;
                case "تغير_حجم": member = "SizeChanged"; break;
                case "تم_تحديد": member = "Checked"; break;
                case "تم_إلغاء": member = "Unchecked"; break;
                case "غير_محدد": member = "Indeterminate"; break;
                case "إفلات": member = "Drop"; break;
                case "سحب_فوق": member = "DragOver"; break;
                case "انتهى_الوسائط": case "mediaended": member = "MediaEnded"; break;
                case "فشل_الوسائط": case "mediafailed": member = "MediaFailed"; break;
                case "فتح_الوسائط": case "mediaopened": member = "MediaOpened"; break;
            }
            var evt = nativeObj.GetType().GetEvent(member, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (evt == null) return;
            if (evt.EventHandlerType == typeof(RoutedEventHandler))
            {
                RoutedEventHandler h = (s, e) =>
                {
                    try { callback(); }
                    catch (Exception ex) { ShowError(ex.Message); }
                };
                evt.AddEventHandler(nativeObj, h);
            }
            else if (evt.EventHandlerType == typeof(EventHandler))
            {
                EventHandler h = (s, e) =>
                {
                    try { callback(); }
                    catch (Exception ex) { ShowError(ex.Message); }
                };
                evt.AddEventHandler(nativeObj, h);
            }
            else if (evt.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<double>))
            {
                RoutedPropertyChangedEventHandler<double> h = (s, e) =>
                {
                    try { callback(); }
                    catch (Exception ex) { ShowError(ex.Message); }
                };
                evt.AddEventHandler(nativeObj, h);
            }
            else if (evt.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<int>))
            {
                RoutedPropertyChangedEventHandler<int> h = (s, e) =>
                {
                    try { callback(); }
                    catch (Exception ex) { ShowError(ex.Message); }
                };
                evt.AddEventHandler(nativeObj, h);
            }
        }

        public void AddChild(UIElementHandle parent, UIElementHandle child)
        {
            var parentFE = parent.GetNative<FrameworkElement>();
            var childFE = child.GetNative<UIElement>();
            if (parentFE == null || childFE == null) return;

            DisconnectFromParent(childFE);

            if (parentFE is Panel panel)
                panel.Children.Add(childFE);
            else
            {
                var childProp = parentFE.GetType().GetProperty("Child");
                if (childProp != null && childProp.CanWrite)
                    childProp.SetValue(parentFE, childFE);
                else
                {
                    var contentProp = parentFE.GetType().GetProperty("Content");
                    if (contentProp != null && contentProp.CanWrite)
                        contentProp.SetValue(parentFE, childFE);
                }
            }
        }

        private void DisconnectFromParent(UIElement child)
        {
            if (child == null) return;
            var parent = LogicalTreeHelper.GetParent(child);
            if (parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(child);
                return;
            }
            if (parent is Decorator decorator && decorator.Child == child)
            {
                decorator.Child = null;
                return;
            }
            if (parent is ContentControl contentControl && contentControl.Content == child)
            {
                contentControl.Content = null;
                return;
            }
            if (parent is ItemsControl itemsControl && itemsControl.Items.Contains(child))
            {
                itemsControl.Items.Remove(child);
            }
        }

        public void RemoveChild(UIElementHandle parent, UIElementHandle child)
        {
            var parentFE = parent.GetNative<FrameworkElement>();
            var childFE = child.GetNative<UIElement>();
            if (parentFE == null || childFE == null) return;
            if (parentFE is Panel panel)
                panel.Children.Remove(childFE);
            else
            {
                var childProp = parentFE.GetType().GetProperty("Child");
                if (childProp != null && childProp.CanWrite)
                    childProp.SetValue(parentFE, null);
                else
                {
                    var contentProp = parentFE.GetType().GetProperty("Content");
                    if (contentProp != null && contentProp.CanWrite)
                        contentProp.SetValue(parentFE, null);
                }
            }
        }

        public bool HasParent(UIElementHandle element)
        {
            var fe = element.GetNative<FrameworkElement>();
            if (fe == null) return false;
            return LogicalTreeHelper.GetParent(fe) != null;
        }

        public void ShowError(string message)
        {
            if (_window != null)
                MessageBox.Show(message, "A+ UI");
            else
                Console.WriteLine($"[UI Error] {message}");
        }

        public void WriteOutput(string message)
        {
            if (_window == null || _consoleOutput == null) return;
            _consoleOutput.Visibility = Visibility.Visible;
            _consoleVisible = true;
            _consoleOutput.AppendText(message + "\n");
            _consoleOutput.ScrollToEnd();
        }

        public string ReadInput(string prompt)
        {
            if (_window == null) return "";
            WriteOutput(prompt);
            var dialog = new Window
            {
                Title = "A+ Input",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = _window,
                Topmost = true,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            };
            var stack = new StackPanel { Margin = new Thickness(10) };
            var label = new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 10) };
            var inputBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okBtn = new Button { Content = "OK", Width = 75, IsDefault = true, Margin = new Thickness(0, 0, 10, 0) };
            var cancelBtn = new Button { Content = "Cancel", Width = 75, IsCancel = true };
            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            stack.Children.Add(label);
            stack.Children.Add(inputBox);
            stack.Children.Add(btnPanel);
            dialog.Content = stack;
            string result = "";
            okBtn.Click += (s, e) => { result = inputBox.Text; dialog.DialogResult = true; };
            dialog.ShowDialog();
            return result;
        }

        public void AddErrorText(string error)
        {
            if (_window == null || _rootPanel == null) return;
            if (_errorListBox == null)
            {
                _errorListBox = new ListBox
                {
                    Background = new SolidColorBrush(Color.FromRgb(50, 0, 0)),
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    MaxHeight = 100,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                _rootPanel.Children.Insert(0, _errorListBox);
            }
            _errorListBox.Items.Add(error);
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
            _bindingTimer?.Stop();
            _bindingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
            _bindingTimer.Tick += (s, e) => callback();
            _bindingTimer.Start();
        }

        public void StopTimer()
        {
            _bindingTimer?.Stop();
            _bindingTimer = null;
        }

        public void CallMethod(UIElementHandle element, string methodName)
        {
            var nativeObj = element.NativeElement;
            if (nativeObj == null) return;
            var method = nativeObj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (method != null && method.GetParameters().Length == 0)
            {
                try { method.Invoke(nativeObj, null); }
                catch (Exception ex) { ShowError($"خطأ في {methodName}: {ex.InnerException?.Message ?? ex.Message}"); }
            }
        }

        public void ToggleFullscreen(UIElementHandle element)
        {
            if (_window == null) return;
            if (_window.WindowState == WindowState.Maximized && _window.WindowStyle == WindowStyle.None)
            {
                _window.WindowState = WindowState.Normal;
                _window.WindowStyle = WindowStyle.SingleBorderWindow;
                _window.Topmost = false;
            }
            else
            {
                _window.WindowStyle = WindowStyle.None;
                _window.WindowState = WindowState.Maximized;
                _window.Topmost = true;
            }
        }

        public bool HasShownWindow() => _windowShown;

        public void ShowWindow()
        {
            if (_windowShown) return;
            if (_window == null || _rootPanel == null) return;
            _windowShown = true;
            foreach (var kv in _elements)
            {
                if (kv.Value.GetNative<FrameworkElement>() is FrameworkElement fe
                    && fe.Parent == null && fe != _rootPanel)
                    _rootPanel.Children.Add(fe);
            }
            try { _rootPanel.UpdateLayout(); } catch { }
            try { _window.UpdateLayout(); } catch { }
            try { _window.ShowDialog(); } catch (Exception ex) { ShowError($"خطأ في إظهار النافذة: {ex.Message}"); return; }
            StopTimer();
            try { _window.Close(); } catch { }
            Cleanup();
        }

        public void CloseWindow()
        {
            _window?.Close();
            Cleanup();
        }

        public void UpdateLayout()
        {
            _rootPanel?.UpdateLayout();
            _window?.UpdateLayout();
        }

        private void Cleanup()
        {
            StopTimer();
            _window = null;
            _rootPanel = null;
            _elements.Clear();
            _errorListBox = null;
        }

        public UIElementHandle Create3DObject(string type, double arg1, double arg2, double arg3)
        {
            switch (type.ToLower())
            {
                case "camera":
                    var cam = new PerspectiveCamera { Position = new Point3D(arg1, arg2, arg3), LookDirection = new Vector3D(0, 0, -1), FieldOfView = 60 };
                    return new UIElementHandle(cam, "camera");
                case "light":
                    var light = new DirectionalLight(Color.FromRgb(255, 255, 255), new Vector3D(arg1, arg2, -1));
                    return new UIElementHandle(light, "light");
                case "object":
                    double x = arg1, y = arg2;
                    string path = arg3 > 0 ? "cube" : "";
                    var mesh = new MeshGeometry3D();
                    var material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                    if (path == "cube")
                    {
                        double sz = 0.5;
                        mesh.Positions.Add(new Point3D(x - sz, y - sz, -sz));
                        mesh.Positions.Add(new Point3D(x + sz, y - sz, -sz));
                        mesh.Positions.Add(new Point3D(x + sz, y + sz, -sz));
                        mesh.Positions.Add(new Point3D(x - sz, y + sz, -sz));
                        mesh.Positions.Add(new Point3D(x - sz, y - sz, sz));
                        mesh.Positions.Add(new Point3D(x + sz, y - sz, sz));
                        mesh.Positions.Add(new Point3D(x + sz, y + sz, sz));
                        mesh.Positions.Add(new Point3D(x - sz, y + sz, sz));
                        mesh.TriangleIndices = new Int32Collection {
                            0,1,2, 0,2,3, 1,5,6, 1,6,2, 5,4,7, 5,7,6,
                            4,0,3, 4,3,7, 3,2,6, 3,6,7, 4,5,1, 4,1,0 };
                    }
                    else
                    {
                        mesh.Positions.Add(new Point3D(x - 1, y - 1, 0));
                        mesh.Positions.Add(new Point3D(x + 1, y - 1, 0));
                        mesh.Positions.Add(new Point3D(x, y + 1, 0));
                        mesh.TriangleIndices = new Int32Collection { 0, 2, 1 };
                    }
                    var geoModel = new GeometryModel3D(mesh, material);
                    geoModel.BackMaterial = material;
                    // GeometryModel3D extends Model3D, which can be used in ModelVisual3D.Content
                    return new UIElementHandle(geoModel, "object");
                default:
                    return null;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
#endif
