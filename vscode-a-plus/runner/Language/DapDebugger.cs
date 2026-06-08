using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace A_
{
    public class DapDebugger
    {
        private int _seq;
        private bool _running = true;
        private int _threadId = 1;
        private string _scriptPath;
        private HashSet<int> _breakpointLines = new();
        private ManualResetEventSlim _resumeEvent = new(false);
        private int _pauseLine;
        private int _pauseColumn;
        private List<Dictionary<string, string>> _pauseLocals = new();

        public void Run()
        {
            var stdin = Console.OpenStandardInput();
            var buffer = new byte[65536];
            var sb = new StringBuilder();

            while (_running)
            {
                try
                {
                    int contentLength = -1;
                    string headers = "";
                    while (true)
                    {
                        int b = stdin.ReadByte();
                        if (b < 0) { _running = false; break; }
                        headers += (char)b;
                        if (headers.EndsWith("\r\n\r\n"))
                        {
                            foreach (var line in headers.Split('\n'))
                            {
                                if (line.StartsWith("Content-Length:"))
                                    contentLength = int.Parse(line.Substring("Content-Length:".Length).Trim());
                            }
                            break;
                        }
                    }
                    if (contentLength <= 0) continue;

                    int read = 0;
                    while (read < contentLength)
                    {
                        int n = stdin.Read(buffer, 0, contentLength - read);
                        if (n <= 0) { _running = false; break; }
                        read += n;
                    }
                    if (read < contentLength) break;

                    string raw = Encoding.UTF8.GetString(buffer, 0, read);
                    ProcessMessage(raw);
                }
                catch { _running = false; }
            }
        }

        void ProcessMessage(string raw)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "request")
                    HandleRequest(root);
            }
            catch { }
        }

        void HandleRequest(JsonElement req)
        {
            string command = req.GetProperty("command").GetString();
            int reqSeq = req.GetProperty("seq").GetInt32();
            var args = req.TryGetProperty("arguments", out var a) ? a : default;

            switch (command)
            {
                case "initialize":
                    SendResponse(reqSeq, command, new
                    {
                        supportsConfigurationDoneRequest = true,
                        supportsTerminateRequest = true
                    });
                    SendEvent("initialized", new { });
                    break;

                case "launch":
                    _scriptPath = args.TryGetProperty("program", out var p) ? p.GetString() : "";
                    SendResponse(reqSeq, command, new { });
                    StartScriptThread();
                    break;

                case "setBreakpoints":
                    HandleSetBreakpoints(reqSeq, command, args);
                    break;

                case "configurationDone":
                    SendResponse(reqSeq, command, new { });
                    break;

                case "threads":
                    SendResponse(reqSeq, command, new
                    {
                        threads = new[] { new { id = _threadId, name = "main" } }
                    });
                    break;

                case "stackTrace":
                    SendResponse(reqSeq, command, new
                    {
                        stackFrames = new[]
                        {
                            new
                            {
                                id = 1, name = _scriptPath ?? "script.a",
                                line = _pauseLine, column = _pauseColumn,
                                source = new { path = _scriptPath ?? "" }
                            }
                        },
                        totalFrames = 1
                    });
                    break;

                case "scopes":
                    SendResponse(reqSeq, command, new
                    {
                        scopes = new[]
                        {
                            new { name = "Locals", variablesReference = 1, expensive = false }
                        }
                    });
                    break;

                case "variables":
                    var vars = new List<object>();
                    foreach (var v in _pauseLocals)
                        vars.Add(new { name = v["name"], value = v["value"] ?? "nil", variablesReference = 0 });
                    SendResponse(reqSeq, command, new { variables = vars.ToArray() });
                    break;

                case "continue":
                case "next":
                case "stepIn":
                case "stepOut":
                    _resumeEvent.Set();
                    SendResponse(reqSeq, command, new { allThreadsContinued = true });
                    break;

                case "disconnect":
                    _running = false;
                    _resumeEvent.Set();
                    SendResponse(reqSeq, command, new { });
                    break;
            }
        }

        void HandleSetBreakpoints(int seq, string command, JsonElement args)
        {
            _breakpointLines.Clear();
            var bps = new List<object>();
            if (args.TryGetProperty("breakpoints", out var bpArr))
            {
                foreach (var bp in bpArr.EnumerateArray())
                {
                    int line = bp.GetProperty("line").GetInt32();
                    _breakpointLines.Add(line);
                    bps.Add(new { id = line, verified = true, line });
                }
            }
            SendResponse(seq, command, new { breakpoints = bps.ToArray() });
        }

        void StartScriptThread()
        {
            new Thread(() =>
            {
                try
                {
                    var interpreter = new Interpreter();
                    interpreter.Debugger = this;
                    interpreter.InterpretFile(_scriptPath);
                }
                catch (Exception ex)
                {
                    SendConsole($"DAP Error: {ex.Message}");
                }
                finally
                {
                    SendEvent("terminated", new { });
                }
            }).Start();
        }

        public bool ShouldBreak(int line)
        {
            return _breakpointLines.Contains(line);
        }

        public void PauseAtBreakpoint(int line, int col, List<Dictionary<string, string>> locals)
        {
            _pauseLine = line;
            _pauseColumn = col;
            _pauseLocals = locals;
            SendEvent("stopped", new { reason = "breakpoint", threadId = _threadId, allThreadsStopped = true });
            _resumeEvent.Reset();
            _resumeEvent.Wait();
        }

        void SendConsole(string msg)
        {
            SendEvent("output", new { category = "console", output = msg + "\n" });
        }

        void SendResponse(int reqSeq, string command, object body)
        {
            var msg = JsonSerializer.Serialize(new
            {
                type = "response",
                seq = ++_seq,
                request_seq = reqSeq,
                success = true,
                command,
                body
            });
            SendRaw(msg);
        }

        void SendEvent(string eventName, object body)
        {
            var msg = JsonSerializer.Serialize(new
            {
                type = "event",
                seq = ++_seq,
                @event = eventName,
                body
            });
            SendRaw(msg);
        }

        void SendRaw(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            Console.Write($"Content-Length: {bytes.Length}\r\n\r\n");
            Console.Write(json);
            Console.Out.Flush();
        }
    }
}
