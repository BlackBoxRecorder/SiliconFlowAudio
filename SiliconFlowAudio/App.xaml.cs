using System.Windows;
using MessageBox = SiliconFlowAudio.MessageBox;

namespace AudioTranscription
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. UI线程（Dispatcher）未处理异常
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            // 2. 非UI线程（后台线程）未处理异常
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            // 3. Task中未观察到的异常（.NET Framework 4.0+ / .NET Core 2.0+）
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
            base.OnStartup(e);
        }

        // UI线程异常处理
        private void OnDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e
        )
        {
            LogAndReport(e.Exception, "UI线程异常");
            // 谨慎设置：仅对明确可恢复的异常设为true，避免隐藏严重错误
            e.Handled = true;
        }

        // 后台线程异常处理（无法阻止崩溃，仅用于日志）
        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogAndReport(ex, "后台线程异常 - 应用即将终止");
            // 注意：此处无法设置Handled，应用仍会终止
        }

        // Task未观察异常处理
        private void OnTaskSchedulerUnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs e
        )
        {
            LogAndReport(e.Exception, "Task未观察异常");
            e.SetObserved(); // 避免GC时再次抛出（.NET 4.5+ 默认不终止，但仍建议调用）
        }

        // 日志与用户反馈（示例）
        private void LogAndReport(Exception ex, string context)
        {
            if (Current?.Dispatcher?.CheckAccess() == true) // 仅在UI线程安全调用
            {
                MessageBox.Show($"发生错误：{ex.Message}", "系统提示", MessageBoxButton.OK);
            }
        }
    }
}
