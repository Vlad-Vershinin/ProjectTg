using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;
using Client.Models;
using System.Diagnostics;

namespace Client.Convertors;

public class LastMessageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PrivateChat chat)
        {
            if (chat.Messages == null || !chat.Messages.Any())
                return "Нет сообщений";

            var lastMessage = chat.Messages.OrderByDescending(m => m.SentAt).First();
            var senderName = lastMessage.SenderId == chat.User1Id ? chat.User2?.UserName : chat.User1?.UserName;
            var currentUserPrefix = lastMessage.SenderId == chat.CurrentUserId ? "Вы: " : $"{senderName}: ";
            var time = lastMessage.SentAt.ToLocalTime().ToString("HH:mm");

            return $"{currentUserPrefix}{lastMessage.Text} · {time}";
        }
        return "Нет сообщений";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}