using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using ReactiveUI;

namespace v2rayN.Views;

public partial class CheckUpdateWindow : Window
{
    public CheckUpdateViewModel ViewModel { get; private set; }
    private DispatcherTimer _progressTimer;
    private int _animatedProgress = 0;

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
            StartProgressAnimation();
            await ViewModel.CheckUpdateAllCmd.Execute();
            StopProgressAnimation();
            btnCheckUpdateAll.IsEnabled = true;
        };

        btnClose.Click += (s, e) => Close();

        ViewModel.WhenAnyValue(x => x.CurrentProgress)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(progress =>
            {
                if (progress > _animatedProgress)
                {
                    _animatedProgress = progress;
                }
                UpdateProgressUI();
            });

        ViewModel.WhenAnyValue(x => x.ProgressStatus)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(status =>
            {
                txtProgressStatus.Text = status ?? "";
            });

        ViewModel.WhenAnyValue(x => x.IsUpdating)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(isUpdating =>
            {
                if (isUpdating)
                {
                    StartProgressAnimation();
                }
                else
                {
                    StopProgressAnimation();
                }
            });
    }

    private void StartProgressAnimation()
    {
        _animatedProgress = 0;
        if (_progressTimer == null)
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
            _progressTimer.Tick += (s, e) =>
            {
                if (_animatedProgress < 95 && ViewModel.IsUpdating)
                {
                    _animatedProgress += 1;
                    UpdateProgressUI();
                }
            };
        }
        _progressTimer.Start();
        windowTaskbar.ProgressState = TaskbarItemProgressState.Normal;
    }

    private void StopProgressAnimation()
    {
        _progressTimer?.Stop();
        _animatedProgress = 100;
        UpdateProgressUI();
        windowTaskbar.ProgressState = TaskbarItemProgressState.None;
    }

    private void UpdateProgressUI()
    {
        progressBar.Value = _animatedProgress;
        windowTaskbar.ProgressValue = _animatedProgress / 100.0;
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
