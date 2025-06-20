using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public Control Control { get; set; }

    public MainViewModel()
    {

    }
}
