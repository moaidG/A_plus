const vscode = require('vscode');
const path = require('path');
const cp = require('child_process');
const { checkCode, arabicTags, arabicAttrs, knownUiTypes, uiAttributes } = require('../checker');

const keywords = [
    'let', 'if', 'else', 'while', 'for', 'func', 'function', 'return',
    'print', 'class', 'new', 'self', 'this', 'true', 'false',
    'and', 'or', 'not', 'public', 'private', 'static', 'abstract',
    'interface', 'include', 'from', 'throw', 'try', 'catch', 'finally',
    'show', 'extern', 'dotnet', 'platform', 'typeof', 'help',
    'break', 'continue', 'switch', 'case', 'default', 'in', 'extends',
    'video', 'audio', 'viewport3d', 'MediaElement', 'Viewport3D',
    'nil', 'export', 'import', 'as', 'async', 'await', 'go', 'spawn',
    'عدم', 'صدر', 'استورد', 'كـ', 'غيرمتزامن', 'انتظر', 'انطلق', 'أنشئ',
    'متغير', 'م', 'إذا', 'لو', 'وإلا', 'طالما', 'بينما', 'ت', 'لكل',
    'دالة', 'اطبع', 'ط', 'صنف', 'فئة', 'جديد', 'نفس', 'هذا', 'صحيح', 'خاطئ',
    'و', 'أو', 'ليس', 'عام', 'خاص', 'ثابت', 'مجرد', 'واجهة',
    'استدعاء', 'ضم', 'من', 'ارم', 'حاول', 'التقط', 'اعرض', 'خارجي',
    'نت', 'منصة', 'نوع', 'مساعدة', 'توقف', 'استمر', 'كسر',
    'اختيار', 'حالة', 'افتراضي', 'في', 'يرث',
    'أخيرا', 'وأخيراً', 'عود',
    'منظور3d', 'وسائط', 'وسائط_فيديو', 'وسائط_صوت',
    // Terminal commands
    'input', 'consoleClear', 'consoleSetCursor', 'consoleColor', 'trace', 'debugger',
    'اقرأ', 'مسحشاشة', 'حددموقعمؤشر', 'لوننص', 'تتبع', 'مصحح',
];

const librarySuggestions = [
    { label: 'include "runtime/interop.a"', detail: 'Native C/C++/C# interop declarations' },
    { label: 'extern func NativeCall(arg) from "native.dll"', detail: 'Declare C/C++ DLL function' },
    { label: 'dotnet("System.Math", "Sqrt", 16)', detail: 'Call a static .NET/C# member' },
    { label: 'platform()', detail: 'Detect current runtime platform' },
    { label: 'show()', detail: 'Show generated XML/WPF UI' },
    { label: 'input("prompt")', detail: 'Read a line from user / اقرأ من المستخدم' },
    { label: 'consoleClear()', detail: 'Clear console screen / مسح الشاشة' },
    { label: 'consoleSetCursor(x, y)', detail: 'Set cursor position / حدد موقع المؤشر' },
    { label: 'consoleColor("red")', detail: 'Set text color (16 colors) / لون النص' },
    { label: 'trace("msg")', detail: 'Log trace message / تتبع' },
    { label: 'debugger', detail: 'Pause for debugging / مصحح' },
];

const uiMethods = [
    'add', 'remove', 'clear', 'insert', 'set', 'get', 'find', 'contains',
    'show', 'hide', 'focus', 'blur', 'updateLayout', 'invalidate',
    'measure', 'arrange', 'beginAnimation', 'stopAnimation',
];

const eventNames = [
    'Click', 'Loaded', 'MouseEnter', 'MouseLeave', 'MouseDown', 'MouseUp',
    'MouseMove', 'KeyDown', 'KeyUp', 'TextChanged', 'SelectionChanged',
    'Checked', 'Unchecked', 'Indeterminate', 'ValueChanged',
    'SizeChanged', 'LayoutUpdated', 'Drop', 'DragOver',
    'GotFocus', 'LostFocus',
];

let workspaceSymbols = { variables: [], functions: [], classes: [] };

function scanFileText(text) {
    const vars = [], funcs = [], classes = [];
    for (const line of text.split('\n')) {
        let m;
        if (m = line.match(/(?:let|متغير|م)\s+([\w\u0600-\u06FF]+)\s*=/)) vars.push(m[1]);
        if (m = line.match(/a\+\s+new\s+\w+\s+([\w\u0600-\u06FF]+)/)) vars.push(m[1]);
        if (m = line.match(/(?:func|function|دالة)\s+([\w\u0600-\u06FF]+)\s*\(/)) funcs.push(m[1]);
        if (m = line.match(/(?:class|صنف)\s+([\w\u0600-\u06FF]+)/)) classes.push(m[1]);
        if (m = line.match(/(?:interface|واجهة)\s+([\w\u0600-\u06FF]+)/)) classes.push(m[1]);
    }
    return { variables: [...new Set(vars)], functions: [...new Set(funcs)], classes: [...new Set(classes)] };
}

async function updateWorkspaceSymbols() {
    workspaceSymbols = { variables: [], functions: [], classes: [] };
    const folders = vscode.workspace.workspaceFolders;
    if (!folders) return;
    const files = await vscode.workspace.findFiles('**/*.{a,axml,a+,aplus,r.a}', '**/{node_modules,bin,obj,dist,build}/**');
    for (const uri of files) {
        try {
            const content = Buffer.from(await vscode.workspace.fs.readFile(uri)).toString();
            const s = scanFileText(content);
            workspaceSymbols.variables = [...new Set([...workspaceSymbols.variables, ...s.variables])];
            workspaceSymbols.functions = [...new Set([...workspaceSymbols.functions, ...s.functions])];
            workspaceSymbols.classes = [...new Set([...workspaceSymbols.classes, ...s.classes])];
        } catch (e) {}
    }
}

function activate(context) {
       // أنواع الأشياء التي نريد تلوينها
    // أنواع التلوين
    const legend = new vscode.SemanticTokensLegend([
        'variable',   // 0
        'function',   // 1
        'class',      // 2
        'interface',  // 3
    ]);

    const provider = {
    provideDocumentSemanticTokens(document) {
        const tokens = new vscode.SemanticTokensBuilder(legend);
        const lines = document.getText().split('\n');

        const variables = new Set();
        const functions = new Set();
        const classes = new Set();
        const interfaces = new Set();

        //--------------------------------
        // المرحلة 1: جمع الأسماء
        //--------------------------------
        for (let line of lines) {

            // متغير
            let m = line.match(/\b(let|متغير|م)\s+([\p{L}_][\p{L}\p{N}_]*)/u);
            if (m) variables.add(m[2]);

            // دالة
            m = line.match(/\b(func|function|دالة)\s+([\p{L}_][\p{L}\p{N}_]*)/u);
            if (m) functions.add(m[2]);

            // class
            m = line.match(/\b(class|صنف|فئة)\s+([\p{L}_][\p{L}\p{N}_]*)/u);
            if (m) classes.add(m[2]);

            // interface
            m = line.match(/\b(interface|واجهة)\s+([\p{L}_][\p{L}\p{N}_]*)/u);
            if (m) interfaces.add(m[2]);
        }

        //--------------------------------
        // المرحلة 2: تلوين كل ظهور
        //--------------------------------
        for (let lineNum = 0; lineNum < lines.length; lineNum++) {
    const line = lines[lineNum];

    const regex = /[\p{L}_][\p{L}\p{N}_]*/gu;
    let match;

    while ((match = regex.exec(line)) !== null) {
        const word = match[0];
        const start = match.index;

        // هل قبلها نقطة؟ => property
        const prevChar = line[start - 1];
        if (prevChar === '.') continue;

        if (variables.has(word)) {
            tokens.push(lineNum, start, word.length, 0, 0);
        }

        if (functions.has(word)) {
            tokens.push(lineNum, start, word.length, 1, 0);
        }

        if (classes.has(word)) {
            tokens.push(lineNum, start, word.length, 2, 0);
        }

        if (interfaces.has(word)) {
            tokens.push(lineNum, start, word.length, 3, 0);
        }
    }
}

        return tokens.build();
    }
};
    context.subscriptions.push(
        vscode.languages.registerDocumentSemanticTokensProvider(
            { language: 'a-plus' },
            provider,
            legend
        )
    );

    const diagnostics = vscode.languages.createDiagnosticCollection('a-plus');
    const output = vscode.window.createOutputChannel('A+');
    context.subscriptions.push(diagnostics, output);

    const runnerDir = path.join(context.extensionPath, 'runner');
    let runnerPath = null;
    try {
        const csprojFiles = require('fs').readdirSync(runnerDir).filter(f => f.endsWith('.csproj'));
        runnerPath = csprojFiles.length > 0 ? path.join(runnerDir, csprojFiles[0]) : null;
    } catch (e) {
        vscode.window.showErrorMessage('Runner folder not found: ' + runnerDir);
    }
    if (!runnerPath) vscode.window.showErrorMessage('No .csproj found in runner folder');
    updateWorkspaceSymbols();

    function isAPlus(document) {
        return document && document.languageId === 'a-plus';
    }

    function isAPlusPath(fileName) {
        return /\.(a|axml|a\+|aplus)$/i.test(fileName) || /\.r\.a$/i.test(fileName);
    }

    function getAPlusOutputName(filePath) {
        return path.basename(filePath).replace(/(\.r\.a|\.a\+|\.aplus|\.a)$/i, '');
    }

    function hasArabic(text) {
        for (const ch of text) {
            const c = ch.charCodeAt(0);
            if ((c >= 0x0600 && c <= 0x06FF) || (c >= 0xFE70 && c <= 0xFEFF) || (c >= 0xFB50 && c <= 0xFDFF)) return true;
        }
        return false;
    }

    function updateTextDirection(document) {
        if (!isAPlus(document)) return;
        const dir = hasArabic(document.getText()) ? 'rtl' : 'ltr';
        for (const editor of vscode.window.visibleTextEditors) {
            if (editor.document.uri.toString() === document.uri.toString()) {
                vscode.commands.executeCommand(dir === 'rtl' ? 'editor.action.setTextDirectionRTL' : 'editor.action.setTextDirectionLTR');
            }
        }
    }

    function updateDiagnostics(document) {
        if (!isAPlus(document)) return;
        const errors = checkCode(document.getText());
        diagnostics.set(document.uri, errors.map(error => {
            const line = error.line != null ? Math.max(0, error.line - 1) : 0;
            const col = error.column != null ? Math.max(0, error.column - 1) : 0;
            const endCol = Math.max(col + 1, error.endCol != null ? error.endCol - 1 : col + 1);
            const range = new vscode.Range(line, col, line, endCol);
            return new vscode.Diagnostic(range, error.message, vscode.DiagnosticSeverity.Error);
        }));
        updateTextDirection(document);
    }

    function runAProgram(filePath, channel = output) {
        if (!runnerPath) {
            channel.appendLine('[Error] No A+ runner project found');
            return Promise.resolve(-1);
        }
        return new Promise(resolve => {
            const runnerDir = path.dirname(runnerPath);
            const tfm = process.platform === 'win32' ? 'net10.0-windows' : 'net10.0';
            channel.appendLine(`[A+] Running: ${path.basename(filePath)} (${tfm})`);
            const proc = cp.spawn('dotnet', ['run', '-f', tfm, '-c', 'Release', '--project', runnerPath, '--', filePath], {
                cwd: runnerDir,
                windowsHide: true
            });
            const allData = [];
            proc.stdout.on('data', data => { allData.push(data.toString()); });
            proc.stderr.on('data', data => { allData.push(data.toString()); });
            proc.on('close', code => {
                const text = allData.join('');
                const clean = text.split(/\r?\n/)
                    .filter(line => line.trim() && !/MSBuild|Determining|Restored|Time Elapsed| -> .* -> |^\s*$/.test(line));
                if (clean.length) channel.appendLine(clean.join('\n'));
                channel.appendLine(`[A+] Exit code: ${code}`);
                resolve(code);
            });
            proc.on('error', err => {
                channel.appendLine(`[Error] ${err.message}`);
                resolve(-1);
            });
        });
    }

    function scanDocumentSymbols(document) {
        const doc = scanFileText(document.getText());
        return {
            variables: [...new Set([...doc.variables, ...workspaceSymbols.variables])],
            functions: [...new Set([...doc.functions, ...workspaceSymbols.functions])],
            classes: [...new Set([...doc.classes, ...workspaceSymbols.classes])]
        };
    }

    function getContext(prefix, current) {
        if (/<\s*$/.test(prefix)) return 'after-open-tag';
        if (/new\s+$/i.test(prefix)) return 'after-new';
        if (/\.\w*$/.test(prefix)) return 'after-dot';
        if (/[(,]\s*$/.test(prefix)) return 'after-comma';
        return 'default';
    }

    context.subscriptions.push(vscode.languages.registerCompletionItemProvider('a-plus', {
        provideCompletionItems(document, position) {
            const prefix = document.lineAt(position).text.substring(0, position.character);
            const current = (prefix.match(/([\w\u0600-\u06FF>."()]+)$/) || [])[1] || '';
            const lower = current.toLowerCase();
            const symbols = scanDocumentSymbols(document);
            const ctx = getContext(prefix, current);
            const items = [];

            for (const word of keywords) {
                if (!current || word.toLowerCase().startsWith(lower) || word.startsWith(current)) {
                    items.push(new vscode.CompletionItem(word, vscode.CompletionItemKind.Keyword));
                }
            }

            for (const v of symbols.variables) {
                if (!current || v.toLowerCase().startsWith(lower))
                    items.push(new vscode.CompletionItem(v, vscode.CompletionItemKind.Variable));
            }
            for (const f of symbols.functions) {
                if (!current || f.toLowerCase().startsWith(lower))
                    items.push(new vscode.CompletionItem(f, vscode.CompletionItemKind.Function));
            }
            for (const c of symbols.classes) {
                if (!current || c.toLowerCase().startsWith(lower))
                    items.push(new vscode.CompletionItem(c, vscode.CompletionItemKind.Class));
            }

            if (ctx === 'after-open-tag') {
                for (const type of knownUiTypes)
                    items.push(new vscode.CompletionItem(type, vscode.CompletionItemKind.Class));
                for (const ar of Object.keys(arabicTags))
                    items.push(new vscode.CompletionItem(ar, vscode.CompletionItemKind.Class));
                for (const f of symbols.functions)
                    items.push(new vscode.CompletionItem(f, vscode.CompletionItemKind.Function));
                for (const c of symbols.classes)
                    items.push(new vscode.CompletionItem(c, vscode.CompletionItemKind.Class));
            }

            if (ctx === 'after-new') {
                for (const type of knownUiTypes)
                    items.push(new vscode.CompletionItem(type, vscode.CompletionItemKind.Class));
                for (const ar of Object.keys(arabicTags))
                    items.push(new vscode.CompletionItem(ar, vscode.CompletionItemKind.Class));
                for (const c of symbols.classes)
                    items.push(new vscode.CompletionItem(c, vscode.CompletionItemKind.Class));
            }

            if (ctx === 'after-dot') {
                for (const attr of uiAttributes)
                    items.push(new vscode.CompletionItem(attr, vscode.CompletionItemKind.Property));
                for (const ar of Object.keys(arabicAttrs))
                    items.push(new vscode.CompletionItem(ar, vscode.CompletionItemKind.Property));
                for (const ev of eventNames)
                    items.push(new vscode.CompletionItem(ev, vscode.CompletionItemKind.Event));
                for (const method of uiMethods) {
                    const item = new vscode.CompletionItem(method + '()', vscode.CompletionItemKind.Method);
                    item.insertText = new vscode.SnippetString(method + '($1)');
                    items.push(item);
                }
            }

            for (const s of librarySuggestions) {
                if (!current || s.label.toLowerCase().startsWith(lower)) {
                    const item = new vscode.CompletionItem(s.label, vscode.CompletionItemKind.Snippet);
                    item.detail = s.detail;
                    items.push(item);
                }
            }

            return items;
        }
    }, '.', '>', ' ', '"', '<'));

    context.subscriptions.push(vscode.languages.registerHoverProvider('a-plus', {
        provideHover(document, position) {
            const range = document.getWordRangeAtPosition(position, /[A-Za-z_\u0600-\u06FF][A-Za-z0-9_\u0600-\u06FF]*/);
            if (!range) return null;
            const word = document.getText(range);
            const docs = {};
            const regex = /\/\/\/\s*([^\n]*)\s*\n\s*(?:func|function|دالة|class|صنف)\s+([A-Za-z_\u0600-\u06FF][A-Za-z0-9_\u0600-\u06FF]*)/g;
            let match;
            while ((match = regex.exec(document.getText())) !== null) docs[match[2]] = match[1].trim();
            if (!docs[word]) return null;
            const markdown = new vscode.MarkdownString();
            markdown.appendCodeblock(word, 'a-plus');
            markdown.appendMarkdown(docs[word]);
            return new vscode.Hover(markdown, range);
        }
    }));

    let debounceTimer = null;
    let rtlTimer = null;
    context.subscriptions.push(vscode.workspace.onDidSaveTextDocument(document => {
        if (isAPlusPath(document.fileName)) updateWorkspaceSymbols();
    }));
    context.subscriptions.push(vscode.workspace.onDidChangeTextDocument(event => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => updateDiagnostics(event.document), 350);
        clearTimeout(rtlTimer);
        rtlTimer = setTimeout(() => updateTextDirection(event.document), 50);
    }));
    context.subscriptions.push(vscode.workspace.onDidOpenTextDocument(updateDiagnostics));
    context.subscriptions.push(vscode.window.onDidChangeVisibleTextEditors(editors => {
        editors.forEach(editor => updateTextDirection(editor.document));
    }));
    vscode.workspace.textDocuments.forEach(doc => {
        updateDiagnostics(doc);
        updateTextDirection(doc);
    });
    if (vscode.window.activeTextEditor) {
        updateDiagnostics(vscode.window.activeTextEditor.document);
        updateTextDirection(vscode.window.activeTextEditor.document);
    }

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.run', async uri => {
        const editor = vscode.window.activeTextEditor;
        const filePath = uri?.fsPath || editor?.document.fileName;
        if (!filePath) return vscode.window.showErrorMessage('No A+ file selected');
        if (editor && editor.document.fileName === filePath) await editor.document.save();
        output.clear();
        await vscode.window.withProgress({ location: vscode.ProgressLocation.Notification, title: 'Running A+...' }, () => runAProgram(filePath, output));
        output.show(true);
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.watch', async () => {
        const editor = vscode.window.activeTextEditor;
        if (!editor || !isAPlus(editor.document)) return vscode.window.showErrorMessage('No active A+ editor');
        await editor.document.save();
        const channel = vscode.window.createOutputChannel('A+ Watch');
        channel.clear();
        channel.appendLine('[A+ Watch] Watching for changes...');
        channel.show(true);
        await runAProgram(editor.document.fileName, channel);

        let lastRun = 0;
        const watcher = vscode.workspace.createFileSystemWatcher(new vscode.RelativePattern(path.dirname(editor.document.fileName), path.basename(editor.document.fileName)));
        watcher.onDidChange(async () => {
            const now = Date.now();
            if (now - lastRun < 500) return;
            lastRun = now;
            channel.appendLine('--- Change detected, re-running ---');
            await runAProgram(editor.document.fileName, channel);
        });
        context.subscriptions.push(watcher, channel);
        vscode.window.showInformationMessage('A+ Watch mode active. Save to re-run.');
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.newProject', async () => {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        const workspace = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath || process.cwd();
        const appName = await vscode.window.showInputBox({ prompt: 'Project name', placeHolder: 'MyApp', value: 'MyApp' });
        if (!appName) return;
        const targetOS = await vscode.window.showQuickPick(['exe', 'apk', 'ios', 'web', 'all'], { placeHolder: 'Select target OS' });
        const args = ['run', '--project', runnerPath, '--', 'create', '-N', appName, '-L', workspace];
        if (targetOS) args.push('-os', targetOS);
        await vscode.window.withProgress({ location: vscode.ProgressLocation.Notification, title: 'Creating A+ project...' }, () => new Promise(resolve => {
            const proc = cp.spawn('dotnet', args, { cwd: path.dirname(runnerPath), windowsHide: true });
            proc.on('close', code => {
                if (code === 0) vscode.window.showInformationMessage(`A+ project '${appName}' created`);
                else vscode.window.showErrorMessage('Failed to create A+ project');
                resolve();
            });
            proc.on('error', err => {
                vscode.window.showErrorMessage(err.message);
                resolve();
            });
        }));
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.openTerminal', () => {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        const terminal = vscode.window.createTerminal('A+ Terminal');
        terminal.show();
        terminal.sendText('dotnet run -c Release --project "' + runnerPath + '" --');
    }));

    async function packageForTarget(target, customOutputDir = null) {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        const editor = vscode.window.activeTextEditor;
        if (!editor || !isAPlus(editor.document)) return vscode.window.showErrorMessage('No active A+ file');
        const filePath = editor.document.fileName;
        const outputName = getAPlusOutputName(filePath);
        await editor.document.save();
        const runnerDir = path.dirname(runnerPath);
        const channel = vscode.window.createOutputChannel('A+ Package');
        channel.clear();
        channel.appendLine(`[A+] Packaging '${outputName}' for target: ${target}`);
        if (customOutputDir) channel.appendLine(`[A+] Output: ${customOutputDir}`);
        channel.show(true);
        const args = ['run', '-f', 'net10.0', '-c', 'Release', '--project', runnerPath, '--',
            'package', filePath, outputName, '-os', target];
        if (customOutputDir) args.push('-o', customOutputDir);
        const proc = cp.spawn('dotnet', args, { cwd: runnerDir, windowsHide: true });
        proc.stdout.on('data', data => channel.append(data.toString()));
        proc.stderr.on('data', data => channel.append(data.toString()));
        proc.on('close', code => {
            channel.appendLine(`\n[A+] Exit code: ${code}`);
            if (code === 0) {
                vscode.window.showInformationMessage(`A+ packaged for ${target}!`);
                openBuildOutput(target, runnerDir, customOutputDir);
            } else {
                vscode.window.showErrorMessage(`Package failed for ${target}`);
            }
        });
        proc.on('error', err => {
            channel.appendLine(`[Error] ${err.message}`);
            vscode.window.showErrorMessage(`Package error: ${err.message}`);
        });
    }

    async function buildFileForTarget(filePath, target, customOutputDir) {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        const outputName = getAPlusOutputName(filePath);
        const runnerDir = path.dirname(runnerPath);
        const channel = vscode.window.createOutputChannel('A+ Build');
        channel.clear();
        channel.appendLine(`[A+] Building '${outputName}' for target: ${target}`);
        channel.appendLine(`[A+] File: ${filePath}`);
        if (customOutputDir) channel.appendLine(`[A+] Output: ${customOutputDir}`);
        channel.show(true);
        const args = ['run', '-f', 'net10.0', '-c', 'Release', '--project', runnerPath, '--',
            'package', filePath, outputName, '-os', target];
        if (customOutputDir) args.push('-o', customOutputDir);
        const proc = cp.spawn('dotnet', args, { cwd: runnerDir, windowsHide: true });
        proc.stdout.on('data', data => channel.append(data.toString()));
        proc.stderr.on('data', data => channel.append(data.toString()));
        proc.on('close', code => {
            channel.appendLine(`\n[A+] Exit code: ${code}`);
            if (code === 0) {
                vscode.window.showInformationMessage(`A+ built for ${target}!`);
                openBuildOutput(target, runnerDir, customOutputDir);
            } else {
                vscode.window.showErrorMessage(`Build failed for ${target}`);
            }
        });
        proc.on('error', err => {
            channel.appendLine(`[Error] ${err.message}`);
            vscode.window.showErrorMessage(`Build error: ${err.message}`);
        });
    }

    function openBuildOutput(target, runnerDir, customOutputDir = null) {
        if (customOutputDir) {
            setTimeout(() => {
                const dir = customOutputDir;
                if (target === 'web') {
                    const indexPath = path.join(dir, 'index.html');
                    if (require('fs').existsSync(indexPath)) {
                        vscode.env.openExternal(vscode.Uri.file(indexPath));
                        return;
                    }
                }
                if (process.platform === 'win32') {
                    cp.exec(`start "" "${dir}"`);
                } else if (process.platform === 'darwin') {
                    cp.exec(`open "${dir}"`);
                } else {
                    cp.exec(`xdg-open "${dir}"`);
                }
            }, 2000);
            return;
        }
        const buildDir = path.join(runnerDir, 'build');
        setTimeout(() => {
            let openPath = null;
            const targetDirs = {
                web: path.join(buildDir, 'web'),
                exe: path.join(buildDir, 'win-x64'),
                linux: path.join(buildDir, 'linux-x64'),
                macos: path.join(buildDir, 'osx-x64'),
                macos_app: path.join(buildDir, 'macos-app'),
                appimage: path.join(buildDir, 'appimage'),
                apk: path.join(buildDir, 'apk'),
                aab: path.join(buildDir, 'aab'),
                ios: path.join(buildDir, 'ios'),
                all: buildDir,
                extension: path.join(runnerDir, '..')
            };
            openPath = targetDirs[target];
            if (!openPath || !require('fs').existsSync(openPath)) openPath = buildDir;
            if (target === 'web') {
                const indexPath = path.join(openPath, 'index.html');
                if (require('fs').existsSync(indexPath)) {
                    vscode.env.openExternal(vscode.Uri.file(indexPath));
                    return;
                }
            }
            if (process.platform === 'win32') {
                cp.exec(`start "" "${openPath}"`);
            } else if (process.platform === 'darwin') {
                cp.exec(`open "${openPath}"`);
            } else {
                cp.exec(`xdg-open "${openPath}"`);
            }
        }, 2000);
    }

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.package', async () => {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        const editor = vscode.window.activeTextEditor;
        if (!editor || !isAPlus(editor.document)) return vscode.window.showErrorMessage('No active A+ file');
        const pick = await vscode.window.showQuickPick([
            { label: '$(globe) Web',          target: 'web',       description: 'HTML/JS web app with REPL' },
            { label: '$(package) Windows EXE', target: 'exe',      description: 'Standalone .exe for Windows' },
            { label: '$(terminal) Linux',      target: 'linux',    description: 'Linux binary' },
            { label: '$(chevron-right) macOS', target: 'macos',    description: 'macOS binary' },
            { label: '$(device-mobile) Android (APK)', target: 'apk', description: 'Android app package' },
            { label: '$(device-mobile) iOS',   target: 'ios',      description: 'iOS app package' },
            { label: '$(list-selection) All Platforms', target: 'all', description: 'All platforms at once' },
            { label: '$(extensions) VS Code Extension (.vsix)', target: 'extension', description: 'Package A+ VS Code extension' },
        ], { placeHolder: 'Select package target' });
        if (!pick) return;
        if (pick.target === 'extension') {
            vscode.commands.executeCommand('a-plus.packageExtension');
        } else {
            packageForTarget(pick.target);
        }
    }));

    // Register per-target package commands
    const packageTargets = [
        { cmd: 'a-plus.packageWeb',     target: 'web' },
        { cmd: 'a-plus.packageExe',     target: 'exe' },
        { cmd: 'a-plus.packageLinux',   target: 'linux' },
        { cmd: 'a-plus.packageMacos',   target: 'macos' },
        { cmd: 'a-plus.packageApk',     target: 'apk' },
        { cmd: 'a-plus.packageIos',     target: 'ios' },
        { cmd: 'a-plus.packageAll',     target: 'all' },
    ];
    for (const pt of packageTargets) {
        context.subscriptions.push(
            vscode.commands.registerCommand(pt.cmd, () => packageForTarget(pt.target))
        );
    }

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.buildWithPicker', async (uri) => {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        // Step 1: Pick A+ file
        const defaultUri = uri?.fsPath ? vscode.Uri.file(uri.fsPath) : undefined;
        const fileResult = await vscode.window.showOpenDialog({
            canSelectFiles: true,
            canSelectFolders: false,
            canSelectMany: false,
            filters: { 'A+ Files': ['a', 'a+', 'aplus'] },
            defaultUri: defaultUri || (vscode.window.activeTextEditor ? vscode.Uri.file(path.dirname(vscode.window.activeTextEditor.document.fileName)) : undefined),
            title: 'Select A+ file to build'
        });
        if (!fileResult || fileResult.length === 0) return;
        const filePath = fileResult[0].fsPath;
        // Step 2: Pick target platform
        const pick = await vscode.window.showQuickPick([
            { label: '$(globe) Web (HTML5)',           target: 'web',        description: 'HTML/JS web app' },
            { label: '$(package) Windows (.exe)',      target: 'exe',        description: 'Standalone executable' },
            { label: '$(terminal) Linux (.AppImage)',  target: 'appimage',   description: 'Linux AppImage format' },
            { label: '$(chevron-right) macOS (.app)',  target: 'macos_app',  description: 'macOS .app bundle' },
            { label: '$(device-mobile) Android (.apk)',target: 'apk',        description: 'Android APK package' },
            { label: '$(device-mobile) Android (.aab)',target: 'aab',        description: 'Android App Bundle' },
            { label: '$(device-mobile) iOS (.ipa)',    target: 'ios',        description: 'iOS app package' },
        ], { placeHolder: 'Select target platform' });
        if (!pick) return;
        // Step 3: Pick output folder
        const outputName = getAPlusOutputName(filePath);
        const defaultOutput = path.join(path.dirname(filePath), 'build', pick.target);
        const folderResult = await vscode.window.showOpenDialog({
            canSelectFiles: false,
            canSelectFolders: true,
            canSelectMany: false,
            title: 'Select output folder (or Cancel for default: ' + defaultOutput + ')'
        });
        const outputDir = folderResult && folderResult.length > 0 ? folderResult[0].fsPath : null;
        // Step 4: Build with progress
        await vscode.window.withProgress(
            { location: vscode.ProgressLocation.Notification, title: `Building for ${pick.label}...` },
            () => new Promise(resolve => {
                buildFileForTarget(filePath, pick.target, outputDir);
                // Wait a bit then resolve
                setTimeout(resolve, 2000);
            })
        );
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.packageExtension', async () => {
        const extDir = context.extensionPath;
        const outDir = path.join(extDir, '..');
        const vsixPath = path.join(outDir, 'a-plus-language.vsix');
        const channel = vscode.window.createOutputChannel('A+ Package');
        channel.clear();
        channel.appendLine('[A+] Packaging VS Code Extension...');
        channel.show(true);
        const proc = cp.spawn('npx', ['vsce', 'package', '--out', vsixPath], { cwd: extDir, windowsHide: true });
        proc.stdout.on('data', d => channel.append(d.toString()));
        proc.stderr.on('data', d => channel.append(d.toString()));
        proc.on('close', code => {
            channel.appendLine(`\n[A+] Exit code: ${code}`);
            if (code === 0) {
                vscode.window.showInformationMessage('A+ Extension packaged!');
                openBuildOutput('extension', path.dirname(runnerPath));
            } else {
                vscode.window.showErrorMessage('Failed to package extension. Ensure vsce is installed: npm i -g @vscode/vsce');
            }
        });
        proc.on('error', err => {
            channel.appendLine(`[Error] ${err.message}`);
            vscode.window.showErrorMessage(`vsce error: ${err.message}`);
        });
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.libraryManager', async () => {
        const choice = await vscode.window.showQuickPick([
            { label: '$(cloud-download) Install Library', detail: 'Download a library from A+ registry' },
            { label: '$(library) Browse Libraries', detail: 'Browse available A+ libraries' },
            { label: '$(new-file) Create AXML File', detail: 'Create a new .axml UI layout file' },
            { label: '$(folder-opened) Installed Libraries', detail: 'Manage installed A+ packages' },
        ], { placeHolder: 'A+ Library Manager' });
        if (!choice) return;
        const cmd = choice.label.match(/\(([^)]+)\)/)?.[1] || '';
        if (cmd === 'cloud-download') {
            vscode.commands.executeCommand('a-plus.install');
        } else if (cmd === 'library') {
            browseLibraries();
        } else if (cmd === 'new-file') {
            createAxmlFile();
        } else if (cmd === 'folder-opened') {
            showInstalledLibraries();
        }
    }));

    async function createAxmlFile() {
        const name = await vscode.window.showInputBox({
            prompt: 'AXML file name',
            placeHolder: 'MyLayout',
            value: 'MyLayout'
        });
        if (!name) return;
        const workspace = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath || '';
        const filePath = path.join(workspace, name.endsWith('.axml') ? name : name + '.axml');
        const template = `<StackPanel Width="400" Height="300">\n    <TextBlock Text="مرحباً A+" FontSize="24" />\n    <Button Content="اضغطني" Width="150" />\n</StackPanel>\nshow()`;
        try {
            require('fs').writeFileSync(filePath, template, 'utf8');
            const doc = await vscode.workspace.openTextDocument(filePath);
            vscode.window.showTextDocument(doc);
            vscode.window.showInformationMessage(`Created ${name}.axml`);
        } catch (e) {
            vscode.window.showErrorMessage(`Error: ${e.message}`);
        }
    }

    async function browseLibraries() {
        const libs = getLibraryRegistry();
        const pick = await vscode.window.showQuickPick(
            libs.map(l => ({
                label: l.name + '  v' + l.version,
                detail: l.description,
                description: l.author || '',
                lib: l
            })),
            { placeHolder: 'Select a library to view details' }
        );
        if (!pick) return;
        const action = await vscode.window.showInformationMessage(
            `${pick.lib.name} v${pick.lib.version}: ${pick.lib.description}`,
            { modal: true, detail: `Author: ${pick.lib.author || 'A+'}\nMain: ${pick.lib.main || 'index.a'}\nDependencies: ${(pick.lib.dependencies || []).join(', ') || 'none'}` },
            'Install', 'Cancel'
        );
        if (action === 'Install') {
            installLibrary(pick.lib);
        }
    }

    function getLibraryRegistry() {
        return [
            { name: 'math', version: '1.0.0', description: 'Advanced math: abs, sqrt, sin, cos, tan, floor, ceil, round, max, min, clamp, lerp, pi', main: 'math.a', author: 'A+' },
            { name: 'string', version: '1.0.0', description: 'String utilities: split, join, reverse, trim, startsWith, endsWith, count, repeat, pad', main: 'string.a', author: 'A+' },
            { name: 'list', version: '1.0.0', description: 'List operations: first, last, take, skip, range, flatten, chunk', main: 'list.a', author: 'A+' },
            { name: 'datetime', version: '1.0.0', description: 'Date/time: now, addDays, addHours, addMinutes', main: 'datetime.a', author: 'A+' },
            { name: 'lib_console', version: '1.0.0', description: 'Console utilities: printLine, readNumber, confirm, wait', main: 'lib_console.a', author: 'A+' },
            { name: 'ui_library', version: '1.0.0', description: 'UI helpers: createWindow, createButton, createImage', main: 'ui_library.a', author: 'A+' },
        ];
    }

    async function installLibrary(lib) {
        const workspace = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
        if (!workspace) return vscode.window.showErrorMessage('Open a workspace first');
        const libDir = path.join(workspace, 'lib', lib.name);
        try {
            require('fs').mkdirSync(libDir, { recursive: true });
            const stdlibDir = path.join(context.extensionPath, 'runner', 'stdlib');
            const srcFile = path.join(stdlibDir, lib.main);
            const destFile = path.join(libDir, lib.main);
            if (require('fs').existsSync(srcFile)) {
                require('fs').copyFileSync(srcFile, destFile);
                vscode.window.showInformationMessage(`Installed ${lib.name} v${lib.version} to lib/${lib.name}/`);
            } else {
                vscode.window.showWarningMessage(`Library source not found: ${lib.name}`);
            }
        } catch (e) {
            vscode.window.showErrorMessage(`Install failed: ${e.message}`);
        }
    }

    async function showInstalledLibraries() {
        const workspace = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
        if (!workspace) return vscode.window.showErrorMessage('Open a workspace first');
        const libDir = path.join(workspace, 'lib');
        let installed = [];
        try {
            if (require('fs').existsSync(libDir)) {
                installed = require('fs').readdirSync(libDir).filter(f => {
                    const stat = require('fs').statSync(path.join(libDir, f));
                    return stat.isDirectory();
                });
            }
        } catch (e) {}
        if (installed.length === 0) {
            vscode.window.showInformationMessage('No libraries installed. Use "Install Library" to add one.');
            return;
        }
        const pick = await vscode.window.showQuickPick(installed, { placeHolder: 'Select a library to uninstall' });
        if (!pick) return;
        const confirm = await vscode.window.showWarningMessage(`Uninstall ${pick}?`, 'Yes', 'No');
        if (confirm === 'Yes') {
            try {
                require('fs').rmSync(path.join(libDir, pick), { recursive: true, force: true });
                vscode.window.showInformationMessage(`Uninstalled ${pick}`);
            } catch (e) {
                vscode.window.showErrorMessage(`Failed: ${e.message}`);
            }
        }
    }

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.install', async () => {
        const terminal = vscode.window.createTerminal('A+ Install');
        terminal.show();
        terminal.sendText('dotnet tool install --global dotnet-ef 2>$null; dotnet --version');
        vscode.window.showInformationMessage('A+: Checking .NET SDK...');
    }));

    context.subscriptions.push(vscode.commands.registerCommand('a-plus.check', async () => {
        if (!runnerPath) return vscode.window.showErrorMessage('No A+ runner project found');
        output.clear();
        output.appendLine('=== A+ Environment Check ===');
        output.appendLine('Extension: A+ Language v1.0.0');
        output.appendLine('Runner: ' + runnerPath);
        const proc = cp.spawn('dotnet', ['--version'], { windowsHide: true });
        proc.stdout.on('data', data => output.appendLine('.NET SDK: ' + data.toString().trim()));
        proc.stderr.on('data', data => output.appendLine('.NET SDK error: ' + data.toString().trim()));
        proc.on('close', code => {
            if (code !== 0) output.appendLine('.NET SDK: Not found - please install from https://dotnet.microsoft.com/download');
            output.appendLine('=== Check Complete ===');
            output.show(true);
        });
        proc.on('error', () => {
            output.appendLine('.NET SDK: Not found - please install from https://dotnet.microsoft.com/download');
            output.appendLine('=== Check Complete ===');
            output.show(true);
        });
    }));

    function runTerminalExample(context, exampleFile) {
        const runnerDir = path.dirname(runnerPath);
        const examplesDir = path.join(context.extensionPath, '..', 'A+_Examples', '12_Terminal');
        const filePath = path.join(examplesDir, exampleFile);
        const terminal = vscode.window.createTerminal('A+ Terminal');
        terminal.show();
        terminal.sendText(`dotnet run -c Release --project "${runnerPath}" -- "${filePath}"`);
    }

    const terminalCommands = [
        { cmd: 'a-plus.term.print',    title: 'A+: Terminal - Print Demo',         file: '01_print_demo.a' },
        { cmd: 'a-plus.term.input',    title: 'A+: Terminal - Input Demo',         file: '02_input_demo.a' },
        { cmd: 'a-plus.term.clear',    title: 'A+: Terminal - Console Clear',      file: '03_console_clear.a' },
        { cmd: 'a-plus.term.color',    title: 'A+: Terminal - Colors Demo',        file: '04_console_color.a' },
        { cmd: 'a-plus.term.cursor',   title: 'A+: Terminal - Cursor Demo',        file: '05_console_cursor.a' },
        { cmd: 'a-plus.term.platform', title: 'A+: Terminal - Platform Demo',      file: '06_platform.a' },
        { cmd: 'a-plus.term.debug',    title: 'A+: Terminal - Trace & Debugger',   file: '07_trace_debugger.a' },
        { cmd: 'a-plus.term.game',     title: 'A+: Terminal - Number Guessing',    file: '08_terminal_game.a' },
        { cmd: 'a-plus.term.progress', title: 'A+: Terminal - Progress Bar',       file: '09_progress_bar.a' },
    ];

    for (const t of terminalCommands) {
        context.subscriptions.push(
            vscode.commands.registerCommand(t.cmd, () => runTerminalExample(context, t.file))
        );
    }
}

function deactivate() {}

module.exports = { activate, deactivate };
