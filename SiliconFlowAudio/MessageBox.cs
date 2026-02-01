using System.Windows;

namespace SiliconFlowAudio
{
    internal static class MessageBox
    {
        public static MessageBoxResult Show(string message)
            => Show(message, string.Empty, MessageBoxButton.OK);

        public static MessageBoxResult Show(string message, string caption)
            => Show(message, caption, MessageBoxButton.OK);

        public static MessageBoxResult Show(string message, string caption, MessageBoxButton button)
        {
            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                        ?? Application.Current.MainWindow;

            var dialog = new ModernMessageBox(message, caption, button)
            {
                Owner = owner
            };

            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
