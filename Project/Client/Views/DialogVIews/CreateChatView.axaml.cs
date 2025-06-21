using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Client.Views.DialogViews;

public partial class CreateChatView : Window
{
    public CreateChatView()
    {
        InitializeComponent();
        ChatTypeComboBox.SelectionChanged += OnChatTypeChanged;
    }

    private void OnChatTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        var isPrivate = ChatTypeComboBox.SelectedIndex == 0;
        PrivateChatForm.IsVisible = isPrivate;
        GroupChatForm.IsVisible = !isPrivate;
    }
}