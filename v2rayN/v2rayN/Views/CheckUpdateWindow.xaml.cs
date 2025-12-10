using System.Windows;
using System.Windows.Shell;
using ReactiveUI;

namespace v2rayN.Views;

public partial class CheckUpdateWindow : Window
{
    public CheckUpdateViewModel ViewModel { get; private set; }

    public CheckUpdateWindow()
    {
        InitializeComponent();

        ViewModel = new CheckUpdateViewModel(UpdateViewHandler);

        lstCheckUpdates.ItemsSource = ViewModel.CheckUpdateModels;
        togEnableAutoRestart.IsChecked = ViewModel.EnableAutoRestart;

        togEnableAutoRestart.Checked += (s, e) => ViewModel.EnableAutoRestart = true;
        togEnableAutoRestart.Unchecked += (s, e) => ViewModel.EnableAutoRestart = false;

        btnCheckUpdateAll.Click += async (s, e) =>
        {
            btnCheckUpdateAll.IsEnabled = false;
            await ViewModel.CheckUpdateAllCmd.Execute();
            btnCheckUpdateAll.IsEnabled = true;
        };

        btnClose.Click += (s, e) => Close();

        ViewModel.WhenAnyValue(x => x.CurrentProgress)
            .Subscribe(progress =>
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = progress;
                    if (progress > 0 && progress < 100)
                    {
                        windowTaskbar.ProgressState = TaskbarItemProgressState.Normal;
                        windowTaskbar.ProgressValue = progress / 100.0;
                    }
                    else
                    {
                        windowTaskbar.ProgressState = TaskbarItemProgressState.None;
                    }
                });
            });

        ViewModel.WhenAnyValue(x => x.ProgressStatus)
            .Subscribe(status =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtProgressStatus.Text = status;
                });
            });
    }

    public void StartAutoUpdate()
    {
        ViewModel.SelectAll();
        ViewModel.CheckUpdateAllCmd.Execute().Subscribe();
    }

    private async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        return await Task.FromResult(true);
    }
}
