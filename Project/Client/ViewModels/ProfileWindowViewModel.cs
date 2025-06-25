using Avalonia.Controls.ApplicationLifetimes;
using Client.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using Avalonia;
using System.Net.Http;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using System.IO;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace Client.ViewModels;

public class ProfileWindowViewModel : ReactiveObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly string _apiUrl = "https://localhost:7068/api";
    private readonly HttpClient _httpClient = new();

    private readonly Guid userId;
    [Reactive] public User? User { get; set; }
    [Reactive] public Bitmap UserAvatar { get; set; }
    [Reactive] public string UserName { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public string Email { get; set; }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LoadImageCommand { get; set; }

    public ProfileWindowViewModel(Guid id)
    {
        userId = id;
        LoadUser();

        CloseCommand = ReactiveCommand.CreateFromTask(Close);
        LoadImageCommand = ReactiveCommand.CreateFromTask(LoadImage);
        SaveCommand = ReactiveCommand.CreateFromTask(SaveUser);
    }

    private async Task LoadUser()
    {
        var response = await _httpClient.GetAsync($"{_apiUrl}/users/finduser?Id={userId}");
        var userResponse = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(userResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        User = user;
        UserName = user.UserName;
        Description = user.Description ?? string.Empty;
        Email = user.Email;
        UserAvatar = new Bitmap("D:\\code\\projects\\cs\\Project\\Project\\Client\\Assets\\profile.png");
    }

    private async Task Close()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Ищем открытое диалоговое окно
            foreach (var window in desktop.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    return;
                }
            }
        }
    }

    private async Task LoadImage()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            var topLevel = TopLevel.GetTopLevel(mainWindow);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выбрать изображение",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                User.ImagePath = files[0].Path.AbsolutePath;
                UserAvatar = new Bitmap(User.ImagePath);
                this.RaisePropertyChanged(nameof(User));
            }
        }
    }

    private async Task SaveUser()
    {
        try
        {
            var content = new MultipartFormDataContent();

            // Добавляем текстовые данные
            content.Add(new StringContent(userId.ToString()), "Id");
            content.Add(new StringContent(UserName ?? string.Empty), "UserName");
            content.Add(new StringContent(Description ?? string.Empty), "Description");
            content.Add(new StringContent(Email ?? string.Empty), "Email");

            // Обрабатываем изображение, только если оно было выбрано
            if (!string.IsNullOrEmpty(User?.ImagePath) && File.Exists(User.ImagePath))
            {
                var fileStream = File.OpenRead(User.ImagePath);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/*");
                content.Add(streamContent, "Image", Path.GetFileName(User.ImagePath));
            }

            var response = await _httpClient.PostAsync($"{_apiUrl}/users/saveuser", content);

            if (response.IsSuccessStatusCode)
            {
                await LoadUser();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Ошибка сохранения: {response.StatusCode}\n{errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Исключение при сохранении: {ex.Message}");
        }
    }
}