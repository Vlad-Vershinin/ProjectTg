using Client.Models;
using Client.Views;
using Client.Views.DialogViews;
using Client.ViewModels.DialogModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace Client.ViewModels;

public class ChatsViewModel : ReactiveObject
{
    private readonly MainViewModel mainViewModel_;
    private readonly MainWindow mainWindow_;

    public ObservableCollection<Chat> Chats { get; set; }


    public ReactiveCommand<Unit, Unit> CreateChatCommand { get; set; }

    public ChatsViewModel(MainViewModel mainVm, MainWindow mainWindow)
    {
        mainViewModel_ = mainVm;
        mainWindow_ = mainWindow;

        CreateChatCommand = ReactiveCommand.CreateFromTask(CreateChat);
    }

    private async Task CreateChat()
    {
        var dialogView = new CreateChatView();
        var viewModel = new CreateChatViewModel(this);
        dialogView.DataContext = viewModel;

        var result = await dialogView.ShowDialog<bool>(mainWindow_);
        if (result)
        {
            dialogView.Close();
            // Обновляем список чатов после создания
            //await LoadChats();
        }
    }
}
