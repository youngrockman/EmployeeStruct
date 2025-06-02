using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EmployeeStruct;

public class SimpleMessageBox : Window
{
    public SimpleMessageBox(string message)
    {
        InitializeComponent();
        this.FindControl<TextBlock>("MessageText").Text = message;
        this.FindControl<Button>("OkButton").Click += (_, _) => Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}