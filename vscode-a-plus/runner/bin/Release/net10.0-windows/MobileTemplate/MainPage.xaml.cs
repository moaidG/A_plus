using System.Text;
using A_;
using A_.Platform;

namespace A_Mobile;

public partial class MainPage : ContentPage
{
    private readonly StringBuilder _output = new();
    private TaskCompletionSource<string> _inputTcs;

    public MainPage()
    {
        InitializeComponent();
        RunScript();
    }

    private async void RunScript()
    {
        try
        {
            string code = ScriptSource.Code;
            code = Shorthand.Convert(code);
            var lexer = new Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            var interp = new Interpreter();
            interp.Output = msg => AppendOutput(msg);
            interp.UIRuntime = new MauiRuntime(this);
            interp.BaseDir = FileSystem.AppDataDirectory;
            try { interp.LoadStdLib(); } catch { }
            AppendOutput("[A+ Mobile] جارٍ التشغيل...");
            await Task.Run(() => interp.Visit(ast));
            AppendOutput("[تم]");
        }
        catch (Exception ex)
        {
            AppendOutput($"[خطأ] {ex.Message}");
        }
    }

    public void AppendOutput(string msg)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _output.AppendLine(msg);
            OutputLabel.Text = _output.ToString();
            OutputScroll.ScrollToAsync(0, OutputScroll.ContentSize.Height, true);
        });
    }

    public Task<string> GetInputAsync()
    {
        _inputTcs = new TaskCompletionSource<string>();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            InputEntry.IsVisible = true;
            InputEntry.Focus();
            InputEntry.Completed -= OnInputCompleted;
            InputEntry.Completed += OnInputCompleted;
        });
        return _inputTcs.Task;
    }

    private void OnInputCompleted(object sender, EventArgs e)
    {
        string text = InputEntry.Text ?? "";
        InputEntry.Text = "";
        InputEntry.IsVisible = false;
        _inputTcs?.TrySetResult(text);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_output.ToString().Contains("جارٍ التشغيل"))
            RunScript();
    }
}
