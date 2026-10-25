using Microsoft.Win32;
using System.IO;
using System.Printing;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NastaliqStudio;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, string> _mappings = new(StringComparer.Ordinal);
    private bool _isUrdu = true;
    private string _chosenTextColor = "#25233A";
    private string _chosenHighlight = "#FFF1A8";

    private string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NastaliqStudio", "keyboard.json");

    public MainWindow()
    {
        InitializeComponent();
        LoadMappings();
        BuildKeyboard();
        BuildSymbols();
        BuildPalettes();
        Editor.Document = CreateDocument();
        Editor.Focus();
    }

    private static FlowDocument CreateDocument()
    {
        return new FlowDocument
        {
            PageWidth = 793.7,
            PageHeight = 1122.5,
            PagePadding = new Thickness(54),
            ColumnWidth = 685,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 20,
            FlowDirection = FlowDirection.RightToLeft,
            TextAlignment = TextAlignment.Right,
            Language = System.Windows.Markup.XmlLanguage.GetLanguage("ur-PK")
        };
    }

    private void LoadMappings()
    {
        foreach (var entry in new Dictionary<string, string>
        {
            ["a"] = "ا", ["b"] = "ب", ["p"] = "پ", ["t"] = "ت", ["T"] = "ٹ",
            ["j"] = "ج", ["c"] = "چ", ["h"] = "ہ", ["x"] = "خ", ["d"] = "د",
            ["r"] = "ر", ["z"] = "ز", ["s"] = "س", ["S"] = "ش", ["f"] = "ف",
            ["q"] = "ق", ["k"] = "ک", ["g"] = "گ", ["l"] = "ل", ["m"] = "م",
            ["n"] = "ن", ["w"] = "و", ["i"] = "ی", ["y"] = "ے", ["e"] = "ع",
            ["u"] = "ء", ["o"] = "و", ["v"] = "ظ", ["H"] = "ح", ["D"] = "ڈ"
        })
            _mappings[entry.Key] = entry.Value;

        if (File.Exists(SettingsPath))
        {
            try
            {
                var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SettingsPath));
                if (saved is not null)
                    foreach (var pair in saved)
                        _mappings[pair.Key] = pair.Value;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                MessageBox.Show($"Saved keyboard mappings could not be read.\n\n{ex.Message}",
                    "Keyboard mappings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void BuildKeyboard()
    {
        foreach (var character in new[]
        {
            "ا", "ب", "پ", "ت", "ٹ", "ج", "چ", "ح", "خ", "د", "ڈ", "ر",
            "ڑ", "ز", "س", "ش", "ص", "ض", "ط", "ظ", "ع", "غ", "ف", "ق",
            "ک", "گ", "ل", "م", "ن", "و", "ہ", "ء", "ی", "ے", "،"
        })
        {
            var button = new Button
            {
                Content = character,
                FontSize = 19,
                Height = 36,
                Margin = new Thickness(2),
                Background = new SolidColorBrush(Color.FromRgb(247, 246, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(231, 229, 239)),
                Cursor = Cursors.Hand
            };
            button.Click += (_, _) => InsertAtCaret(character);
            KeyboardGrid.Children.Add(button);
        }
    }

    private void BuildSymbols()
    {
        foreach (var symbol in new[] { "﷽", "ﷺ", "۞", "☪", "ٱ", "ﷲ", "۩", "۝", "ؒ" })
        {
            var button = new Button
            {
                Content = symbol,
                FontSize = 20,
                Height = 40,
                Margin = new Thickness(2),
                Background = new SolidColorBrush(Color.FromRgb(247, 246, 250)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(231, 229, 239)),
                Cursor = Cursors.Hand
            };
            button.Click += (_, _) => InsertAtCaret(symbol);
            SymbolsGrid.Children.Add(button);
        }
    }

    private void BuildPalettes()
    {
        foreach (var color in new[] { "#25233A", "#000000", "#7655D9", "#D84A67", "#E68A35", "#168B72", "#3284C4", "#B28A20" })
            AddColorSwatch(ColorPalette, color, 29, () =>
            {
                _chosenTextColor = color;
                ApplySelection(TextElement.ForegroundProperty, MakeBrush(color));
            });

        foreach (var color in new[] { "#FFF1A8", "#D7F5DF", "#DDEEFF", "#F9DDE4", "#EAE0FF", "#FFFFFF" })
            AddColorSwatch(HighlightPalette, color, 31, () =>
            {
                _chosenHighlight = color;
                ApplySelection(TextElement.BackgroundProperty,
                    color == "#FFFFFF" ? Brushes.Transparent : MakeBrush(color));
            });
    }

    private static void AddColorSwatch(Panel panel, string color, double size, Action apply)
    {
        var button = new Button
        {
            Width = size,
            Height = size,
            Margin = new Thickness(3),
            Background = MakeBrush(color),
            BorderBrush = new SolidColorBrush(Color.FromRgb(220, 218, 228)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            ToolTip = color
        };
        button.Click += (_, _) => apply();
        panel.Children.Add(button);
    }

    private static SolidColorBrush MakeBrush(string value) =>
        new((Color)ColorConverter.ConvertFromString(value));

    private void InsertAtCaret(string text)
    {
        Editor.Focus();
        var insertion = Editor.Selection.Start.GetInsertionPosition(LogicalDirection.Forward);
        var range = new TextRange(insertion, Editor.Selection.End);
        range.Text = text;
        Editor.CaretPosition = range.End;
        Editor.Selection.Select(Editor.CaretPosition, Editor.CaretPosition);
    }

    private void Editor_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!_isUrdu || e.Text.Length != 1 || !_mappings.TryGetValue(e.Text, out var mapped))
            return;
        InsertAtCaret(mapped);
        e.Handled = true;
    }

    private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && _isUrdu && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
        {
            InsertAtCaret(" ");
            e.Handled = true;
        }
    }

    private void ApplySelection(DependencyProperty property, object value)
    {
        if (Editor is null)
            return;

        var selection = Editor.Selection;
        if (selection.IsEmpty)
        {
            Editor.Focus();
            selection.ApplyPropertyValue(property, value);
        }
        else
            selection.ApplyPropertyValue(property, value);
        Editor.Focus();
    }

    private void ApplyFontSize()
    {
        if (FontSizeBox.SelectedItem is not ComboBoxItem item ||
            !double.TryParse(item.Content?.ToString(), out var size))
            return;
        ApplySelection(TextElement.FontSizeProperty, size);
    }

    private void FontSize_Changed(object sender, SelectionChangedEventArgs e) => ApplyFontSize();

    private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (FontFamilyBox.SelectedItem is not ComboBoxItem item || item.Content is not string family)
            return;
        ApplySelection(TextElement.FontFamilyProperty, new FontFamily(family));
    }

    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        var current = Editor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
        ApplySelection(TextElement.FontWeightProperty,
            current is FontWeight weight && weight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
    }

    private void Italic_Click(object sender, RoutedEventArgs e)
    {
        var current = Editor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
        ApplySelection(TextElement.FontStyleProperty,
            current is FontStyle style && style == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
    }

    private void Underline_Click(object sender, RoutedEventArgs e)
    {
        var current = Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        ApplySelection(Inline.TextDecorationsProperty,
            current == DependencyProperty.UnsetValue || current is not TextDecorationCollection
                ? TextDecorations.Underline
                : new TextDecorationCollection());
    }

    private void Highlight_Click(object sender, RoutedEventArgs e) =>
        ApplySelection(TextElement.BackgroundProperty, MakeBrush(_chosenHighlight));

    private void TextColor_Click(object sender, RoutedEventArgs e) =>
        ApplySelection(TextElement.ForegroundProperty, MakeBrush(_chosenTextColor));

    private void AlignRight_Click(object sender, RoutedEventArgs e) =>
        ApplyParagraph(TextAlignment.Right, FlowDirection.RightToLeft);

    private void AlignCenter_Click(object sender, RoutedEventArgs e) =>
        ApplyParagraph(TextAlignment.Center, FlowDirection.RightToLeft);

    private void Direction_Click(object sender, RoutedEventArgs e)
    {
        var rtl = Editor.CaretPosition.Paragraph?.FlowDirection != FlowDirection.RightToLeft;
        ApplyParagraph(rtl ? TextAlignment.Right : TextAlignment.Left,
            rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
    }

    private void ApplyParagraph(TextAlignment alignment, FlowDirection direction)
    {
        var paragraph = Editor.CaretPosition.Paragraph;
        if (paragraph is not null)
        {
            paragraph.TextAlignment = alignment;
            paragraph.FlowDirection = direction;
        }
        Editor.Focus();
    }

    private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (Editor.Selection.IsEmpty)
        {
            StatusText.Text = "Ready · A4 document";
            return;
        }
        var length = Editor.Selection.Text.Length;
        StatusText.Text = $"{length} character{(length == 1 ? "" : "s")} selected";
    }

    private void Save_Click(object sender, RoutedEventArgs e) => SaveDocument();
    private void Open_Click(object sender, RoutedEventArgs e) => OpenDocument();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Window_StateChanged(object? sender, EventArgs e) =>
        MaximizeButton.Content = WindowState == WindowState.Maximized ? "Restore" : "Maximize";

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Start a new document? Unsaved changes will be discarded.", "New document",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        Editor.Document = CreateDocument();
        Editor.Focus();
        StatusText.Text = "New A4 document";
    }

    private void SaveDocument()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save Urdu document",
            Filter = "Nastaliq Studio document (*.nastaliq)|*.nastaliq",
            DefaultExt = ".nastaliq",
            FileName = "Urdu document.nastaliq"
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            using var stream = File.Create(dialog.FileName);
            new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd)
                .Save(stream, DataFormats.XamlPackage);
            StatusText.Text = $"Saved · {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            MessageBox.Show($"The document could not be saved.\n\n{ex.Message}", "Save document",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenDocument()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open Urdu document",
            Filter = "Nastaliq Studio document (*.nastaliq)|*.nastaliq|All files|*.*"
        };
        if (dialog.ShowDialog(this) != true)
            return;
        try
        {
            var document = CreateDocument();
            using var stream = File.OpenRead(dialog.FileName);
            new TextRange(document.ContentStart, document.ContentEnd).Load(stream, DataFormats.XamlPackage);
            Editor.Document = document;
            Editor.Focus();
            StatusText.Text = $"Opened · {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            MessageBox.Show($"The document could not be opened.\n\n{ex.Message}", "Open document",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CreateExam_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Create exam sheet",
            Width = 410,
            Height = 500,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ShowInTaskbar = false,
            Background = Brushes.White
        };
        var form = new StackPanel { Margin = new Thickness(20) };
        dialog.Content = form;
        form.Children.Add(new TextBlock
        {
            Text = "Step 1: Set up your exam",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        });
        form.Children.Add(new TextBlock
        {
            Text = "Choose the exam details below. Press Do it when you are ready to create the editable exam sheet.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Color.FromRgb(105, 101, 122)),
            Margin = new Thickness(0, 0, 0, 8)
        });

        TextBox AddField(string label, string initialValue)
        {
            form.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 4, 0, 3) });
            var field = new TextBox
            {
                Text = initialValue,
                Height = 30,
                Padding = new Thickness(6, 3, 6, 3),
                FlowDirection = FlowDirection.RightToLeft
            };
            form.Children.Add(field);
            return field;
        }

        var title = AddField("Exam title", "امتحانی پرچہ");
        var subject = AddField("Subject", "مضمون");
        var grade = AddField("Class / grade", "جماعت");
        var duration = AddField("Time allowed", "دو گھنٹے");
        var marks = AddField("Total marks", "50");
        form.Children.Add(new TextBlock { Text = "Number of questions", Margin = new Thickness(0, 5, 0, 3) });
        var count = new ComboBox { Height = 30, SelectedIndex = 2 };
        foreach (var number in new[] { "3", "4", "5", "6", "8" })
            count.Items.Add(number);
        form.Children.Add(count);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0)
        };
        var cancel = new Button { Content = "Cancel", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(4) };
        cancel.Click += (_, _) => dialog.DialogResult = false;
        var create = new Button
        {
            Content = "Do it — Create exam",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(4),
            Background = MakeBrush("#7655D9"),
            Foreground = Brushes.White,
            FontWeight = FontWeights.SemiBold,
            MinWidth = 150
        };
        create.Click += (_, _) =>
        {
            if (new[] { title.Text, subject.Text, grade.Text, duration.Text, marks.Text }
                .Any(string.IsNullOrWhiteSpace))
            {
                MessageBox.Show(dialog, "Fill in each exam detail before creating the sheet.",
                    "Exam details", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            dialog.DialogResult = true;
        };
        buttons.Children.Add(cancel);
        buttons.Children.Add(create);
        form.Children.Add(buttons);

        if (dialog.ShowDialog() != true)
            return;

        var document = CreateDocument();
        AddParagraph(document, title.Text, 26, bold: true, centered: true, marginBottom: 13);
        AddParagraph(document, $"مضمون: {subject.Text}       جماعت: {grade.Text}", 18, marginBottom: 8);
        AddParagraph(document, "نام: ____________________       رول نمبر: ____________________       تاریخ: ______________", 17, marginBottom: 8);
        AddParagraph(document, $"وقت: {duration.Text}       کل نمبر: {marks.Text}", 17, marginBottom: 9);
        AddParagraph(document, "ہدایات: تمام سوالات حل کریں۔ صاف اور واضح لکھیں۔", 17, bold: true, marginBottom: 14);

        var questionCount = int.Parse(count.SelectedItem!.ToString()!);
        for (var index = 0; index < questionCount; index++)
        {
            var number = ToUrduDigits(index + 1);
            AddParagraph(document, $"سوال {number}: ____________________________________________________", 18, bold: true, marginBottom: 5);
            for (var line = 0; line < 3; line++)
                AddParagraph(document, "__________________________________________________________________", 15, marginBottom: 7);
        }
        Editor.Document = document;
        Editor.Focus();
        StatusText.Text = $"Exam submitted and created · {questionCount} questions · edit or export as PDF";
    }

    private static void AddParagraph(FlowDocument document, string text, double size,
        bool bold = false, bool centered = false, double marginBottom = 0)
    {
        var paragraph = new Paragraph(new Run(text))
        {
            FontSize = size,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = centered ? TextAlignment.Center : TextAlignment.Right,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Thickness(0, 0, 0, marginBottom)
        };
        document.Blocks.Add(paragraph);
    }

    private static string ToUrduDigits(int value)
    {
        const string digits = "۰۱۲۳۴۵۶۷۸۹";
        return string.Concat(value.ToString().Select(character => digits[character - '0']));
    }

    private void SaveMapping_Click(object sender, RoutedEventArgs e)
    {
        var key = MappingKeyBox.Text.Trim();
        var value = MappingValueBox.Text.Trim();
        if (key.Length != 1 || value.Length == 0)
        {
            MessageBox.Show("Enter one keyboard key and the Urdu letter or word it should produce.",
                "Customize keyboard", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        _mappings[key] = value;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_mappings, new JsonSerializerOptions { WriteIndented = true }));
            MappingKeyBox.Clear();
            MappingValueBox.Clear();
            StatusText.Text = $"Keyboard mapping saved · {key} → {value}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"The mapping could not be saved.\n\n{ex.Message}", "Customize keyboard",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UrduMode_Changed(object sender, RoutedEventArgs e)
    {
        _isUrdu = UrduModeToggle.IsChecked == true;
        UrduModeToggle.Content = _isUrdu ? "Urdu typing: ON" : "Urdu typing: OFF";
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (Editor.CanUndo)
            Editor.Undo();
        Editor.Focus();
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        if (Editor.CanRedo)
            Editor.Redo();
        Editor.Focus();
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var pdfPrinter = new LocalPrintServer()
                .GetPrintQueues()
                .FirstOrDefault(queue => queue.Name.Contains("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase));
            if (pdfPrinter is null)
            {
                MessageBox.Show("Microsoft Print to PDF is not available. Enable it in Windows Features, then try again.",
                    "Export PDF", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var document = Editor.Document;
            document.PageWidth = 793.7;
            document.PageHeight = 1122.5;
            document.PagePadding = new Thickness(54);
            document.ColumnWidth = document.PageWidth - document.PagePadding.Left - document.PagePadding.Right;
            var printDialog = new PrintDialog
            {
                PrintQueue = pdfPrinter,
                PrintTicket = pdfPrinter.DefaultPrintTicket
            };
            printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Nastaliq Studio Urdu document");
            StatusText.Text = "PDF exported · A4 pages · choose a file name in the Windows dialog";
        }
        catch (Exception ex) when (ex is PrintSystemException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"The PDF could not be exported.\n\n{ex.Message}", "Export PDF",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var control = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        if (control && e.Key == Key.S) { SaveDocument(); e.Handled = true; }
        else if (control && e.Key == Key.O) { OpenDocument(); e.Handled = true; }
        else if (control && e.Key == Key.N) { New_Click(this, new RoutedEventArgs()); e.Handled = true; }
        else if (control && e.Key == Key.P) { ExportPdf_Click(this, new RoutedEventArgs()); e.Handled = true; }
        else if (control && e.Key == Key.B) { Bold_Click(this, new RoutedEventArgs()); e.Handled = true; }
        else if (control && e.Key == Key.I) { Italic_Click(this, new RoutedEventArgs()); e.Handled = true; }
        else if (control && e.Key == Key.U) { Underline_Click(this, new RoutedEventArgs()); e.Handled = true; }
        else if (control && e.Key == Key.Z && Editor.CanUndo) { Editor.Undo(); e.Handled = true; }
        else if (control && e.Key == Key.Y && Editor.CanRedo) { Editor.Redo(); e.Handled = true; }
    }
}
