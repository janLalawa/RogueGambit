namespace RogueGambit.Debug;

public interface IDebugConsole
{
    public void RegisterCommand(string name, Func<string[], string> action);
}

public partial class DebugConsole : CanvasLayer, IDebugConsole
{
    private readonly List<string> _commandHistory = new();

    private readonly Dictionary<string, Func<string[], string>> _commands = new();
    private Control _consoleContainer;
    private int _historyIndex = -1;
    private int _historySize = 50;
    private LineEdit _inputField;
    private bool _isVisible;
    private RichTextLabel _outputText;

    public void RegisterCommand(string name, Func<string[], string> action)
    {
        _commands[name.ToLower()] = action;
    }

    public override void _Ready()
    {
        CreateConsoleUi();
        _consoleContainer.Visible = false;
        _inputField.TextSubmitted += ExecuteCommand;
        RegisterCommands();

        GD.Print("Debug console initialized");
    }

    private void CreateConsoleUi()
    {
        _consoleContainer = new Control();
        _consoleContainer.Name = "ConsoleContainer";

        _consoleContainer.AnchorLeft = 0.5f;
        _consoleContainer.AnchorTop = 0.0f;
        _consoleContainer.AnchorRight = 1.0f;
        _consoleContainer.AnchorBottom = 1.0f;

        _consoleContainer.OffsetLeft = 0;
        _consoleContainer.OffsetTop = 0;
        _consoleContainer.OffsetRight = 0;
        _consoleContainer.OffsetBottom = 0;

        AddChild(_consoleContainer);

        var vbox = new VBoxContainer();
        vbox.Name = "VBoxLayout";
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _consoleContainer.AddChild(vbox);

        var panel = new Panel();
        panel.Name = "Background";
        panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        panel.MouseFilter = Control.MouseFilterEnum.Ignore;

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        styleBox.BorderWidthLeft = 1;
        styleBox.BorderWidthTop = 1;
        styleBox.BorderWidthRight = 1;
        styleBox.BorderWidthBottom = 1;
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.3f);

        panel.AddThemeStyleboxOverride("panel", styleBox);

        _consoleContainer.AddChild(panel);

        var marginContainer = new MarginContainer();
        marginContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        marginContainer.AddThemeConstantOverride("margin_left", 10);
        marginContainer.AddThemeConstantOverride("margin_top", 10);
        marginContainer.AddThemeConstantOverride("margin_right", 10);
        marginContainer.AddThemeConstantOverride("margin_bottom", 10);
        _consoleContainer.AddChild(marginContainer);

        var innerVBox = new VBoxContainer();
        innerVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        innerVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        marginContainer.AddChild(innerVBox);

        var titleLabel = new Label();
        titleLabel.Text = "DEBUG CONSOLE";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;

        titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.2f));
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        innerVBox.AddChild(titleLabel);

        var separator = new HSeparator();
        separator.AddThemeConstantOverride("separation", 10);
        innerVBox.AddChild(separator);

        _outputText = new RichTextLabel();
        _outputText.Name = "OutputText";
        _outputText.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _outputText.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _outputText.BbcodeEnabled = true;
        _outputText.ScrollFollowing = true;

        _outputText.AddThemeColorOverride("default_color", new Color(0.9f, 0.9f, 0.9f));
        _outputText.AddThemeFontSizeOverride("normal_font_size", 14);

        var textStyleBox = new StyleBoxFlat();
        textStyleBox.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.7f);

        textStyleBox.BorderWidthLeft = 1;
        textStyleBox.BorderWidthTop = 1;
        textStyleBox.BorderWidthRight = 1;
        textStyleBox.BorderWidthBottom = 1;
        textStyleBox.BorderColor = new Color(0.3f, 0.3f, 0.3f);

        textStyleBox.ContentMarginLeft = 5;
        textStyleBox.ContentMarginTop = 5;
        textStyleBox.ContentMarginRight = 5;
        textStyleBox.ContentMarginBottom = 5;

        _outputText.AddThemeStyleboxOverride("normal", textStyleBox);

        innerVBox.AddChild(_outputText);

        var inputSeparator = new HSeparator();
        inputSeparator.AddThemeConstantOverride("separation", 5);
        innerVBox.AddChild(inputSeparator);

        _inputField = new LineEdit();
        _inputField.Name = "InputField";
        _inputField.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _inputField.PlaceholderText = "Type command here...";
        _inputField.CustomMinimumSize = new Vector2(0, 30);

        _inputField.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f));
        _inputField.AddThemeFontSizeOverride("font_size", 14);

        var inputStyleBox = new StyleBoxFlat();
        inputStyleBox.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        inputStyleBox.BorderWidthLeft = 1;
        inputStyleBox.BorderWidthTop = 1;
        inputStyleBox.BorderWidthRight = 1;
        inputStyleBox.BorderWidthBottom = 1;
        inputStyleBox.BorderColor = new Color(0.4f, 0.4f, 0.4f);

        inputStyleBox.ContentMarginLeft = 5;
        inputStyleBox.ContentMarginTop = 5;
        inputStyleBox.ContentMarginRight = 5;
        inputStyleBox.ContentMarginBottom = 5;

        _inputField.AddThemeStyleboxOverride("normal", inputStyleBox);

        innerVBox.AddChild(_inputField);

        var helpLabel = new Label();
        helpLabel.Text = "Press ESC to close, UP/DOWN for history";
        helpLabel.HorizontalAlignment = HorizontalAlignment.Center;
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        helpLabel.AddThemeFontSizeOverride("font_size", 12);
        innerVBox.AddChild(helpLabel);

        LogOutput("Debug Console Initialized", new Color(0.5f, 1.0f, 0.5f));
        LogOutput("Type 'help' for a list of commands");
    }

    public override void _Input(InputEvent @event)
    {
        // Toggle console with tilde/backtick key
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Quoteleft) // Tilde/backtick key
            {
                ToggleConsole();
                GetViewport().SetInputAsHandled();
            }

            // Command history navigation when console is visible
            if (_isVisible)
            {
                if (eventKey.Keycode == Key.Up)
                {
                    NavigateHistory(-1);
                    GetViewport().SetInputAsHandled();
                }
                else if (eventKey.Keycode == Key.Down)
                {
                    NavigateHistory(1);
                    GetViewport().SetInputAsHandled();
                }
                else if (eventKey.Keycode == Key.Escape)
                {
                    ToggleConsole(); // Close console with Escape key
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }

    private void ToggleConsole()
    {
        _isVisible = !_isVisible;
        _consoleContainer.Visible = _isVisible;

        if (!_isVisible) return;
        _inputField.GrabFocus();
        _historyIndex = -1;
    }

    private void ExecuteCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _commandHistory.Add(text);
        if (_commandHistory.Count > _historySize)
            _commandHistory.RemoveAt(0);
        _historyIndex = -1;

        LogOutput($"> {text}", new Color(0.8f, 0.8f, 1.0f));

        var parts = text.Split(' ');
        var command = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(command, out var action))
            try
            {
                var result = action(args);
                if (!string.IsNullOrEmpty(result))
                    LogOutput(result);
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}", Colors.Red);
            }
        else
            LogOutput($"Unknown command: {command}", Colors.Red);

        _inputField.Text = "";
        CallDeferred("_DelayedRefocusInput");
    }

    private void _DelayedRefocusInput()
    {
        _inputField.ReleaseFocus();

        CreateTween().SetTrans(Tween.TransitionType.Linear).TweenCallback(
            Callable.From(() => { _inputField.GrabFocus(); })
        ).SetDelay(0.01);
    }

    private void NavigateHistory(int direction)
    {
        if (_commandHistory.Count == 0)
            return;

        _historyIndex += direction;

        if (_historyIndex >= _commandHistory.Count)
            _historyIndex = _commandHistory.Count - 1;
        else if (_historyIndex < -1)
            _historyIndex = -1;

        if (_historyIndex == -1)
            _inputField.Text = "";
        else
            _inputField.Text = _commandHistory[_historyIndex];

        _inputField.CaretColumn = _inputField.Text.Length;
    }

    private void LogOutput(string message, Color? color = null)
    {
        if (_outputText == null)
            return;

        if (color.HasValue)
            _outputText.PushColor(color.Value);

        _outputText.AddText(message + "\n");

        if (color.HasValue)
            _outputText.Pop();
    }

    private void RegisterCommands()
    {
        // Help command
        _commands["help"] = args => { return "Available commands:\n" + string.Join("\n", _commands.Keys.OrderBy(k => k)); };

        // Clear console
        _commands["clear"] = args =>
        {
            _outputText.Clear();
            return null;
        };

        // Echo command
        _commands["echo"] = args => { return string.Join(" ", args); };

        // Print debug info
        _commands["debug_info"] = args =>
        {
            return $"Screen size: {GetViewport().GetVisibleRect().Size}\n" +
                   $"Console size: {_consoleContainer.Size}\n" +
                   $"Output text size: {_outputText.Size}";
        };

        // Game-specific commands
        _commands["spawn_piece"] = args =>
        {
            if (args.Length < 3) return "Usage: spawn_piece <type> <x> <y>";
            return $"NOT YET IMPLEMENTED Spawned {args[0]} at position ({args[1]}, {args[2]})";
        };

        // Toggle debug visualization
        _commands["debug_view"] = args =>
        {
            var enable = args.Length == 0 || args[0].ToLower() == "on" || args[0] == "1";
            // Call your debug visualization toggle here
            return $"Debug visualization {(enable ? "enabled" : "disabled")}";
        };

        // The Toast command. Yay!
        _commands["toast"] = args =>
        {
            GD.Print("Ran the toast command");
            return "Toasty!";
        };
    }
}