namespace Memtly.Core.Helpers.Notifications
{
    public interface INotificationHelper
    {
        Task<bool> Send(string title, string message, string? actionLink = null);
    }
}