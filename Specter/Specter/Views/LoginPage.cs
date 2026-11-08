using Specter.Services;

namespace Specter.Views;

public class LoginPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly BackupService _backup;
    private Entry _usernameEntry = null!;
    private Entry _passwordEntry = null!;
    private CheckBox _rememberCheckBox = null!;
    private Label _errorLabel = null!;
    private Button _btnTestDev = null!;
    private bool _isRestoring;

    public LoginPage(AuthService authService, BackupService backup)
    {
        _auth = authService;
        _backup = backup;
        Title = "Login";
        BackgroundColor = Color.FromArgb("#1A1A2E");
        BuildUI();
        ConfigureContextActions();
    }

    private void BuildUI()
    {
        _usernameEntry = new Entry
        {
            Placeholder = "Username",
            TextColor = Colors.White,
            PlaceholderColor = Colors.Gray
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Password",
            IsPassword = true,
            TextColor = Colors.White,
            PlaceholderColor = Colors.Gray
        };

        _rememberCheckBox = new CheckBox { Color = Colors.White };
        _errorLabel = new Label
        {
            TextColor = Colors.Red,
            IsVisible = false
        };

        var loginButton = new Button { Text = "Login" };
        loginButton.Clicked += OnLoginClicked;

        var registerButton = new Button { Text = "Register" };
        registerButton.Clicked += OnRegisterClicked;

        var stack = new VerticalStackLayout
        {
            Padding = new Thickness(30),
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Specter",
                    FontSize = 42,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                _usernameEntry,
                _passwordEntry,
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        _rememberCheckBox,
                        new Label
                        {
                            Text = "Remember me",
                            TextColor = Colors.White,
                            VerticalTextAlignment = TextAlignment.Center
                        }
                    }
                },
                _errorLabel,
                loginButton,
                registerButton
            }
        };

#if DEBUG
        _btnTestDev = new Button
        {
            Text = "⚡ Dev: Create Test User",
            BackgroundColor = Color.FromArgb("#424242"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 44,
            FontSize = 13,
            Margin = new Thickness(0, 8, 0, 0)
        };
        _btnTestDev.Clicked += async (_, _) => await OnTestDevClickedAsync();
        stack.Children.Add(_btnTestDev);
#endif

        Content = new ScrollView
        {
            Content = stack
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isRestoring)
            return;

        _isRestoring = true;
        try
        {
            var savedUser = await SecureStorage.GetAsync(AuthService.SessionUserKey);
            var savedPassword = await SecureStorage.GetAsync(AuthService.SessionPasswordKey);

            _usernameEntry.Text = savedUser ?? "";
            _passwordEntry.Text = savedPassword ?? "";
            _rememberCheckBox.IsChecked =
                !string.IsNullOrEmpty(savedUser) && !string.IsNullOrEmpty(savedPassword);

            if (await _auth.TryRestoreSessionAsync())
                await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ShowError($"Could not restore the saved session: {ex.Message}");
        }
        finally
        {
            _isRestoring = false;
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await RunAuthActionAsync(async () =>
        {
            if (!await _auth.LoginAsync(
                    _usernameEntry.Text ?? "",
                    _passwordEntry.Text ?? ""))
            {
                ShowError("Invalid username or password.");
                return false;
            }

            ApplyRememberPreference();
            return true;
        });
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await RunAuthActionAsync(async () =>
        {
            var username = _usernameEntry.Text ?? "";
            var password = _passwordEntry.Text ?? "";

            if (!await _auth.RegisterAsync(username, password))
            {
                ShowError(_auth.LastRegisterError);
                return false;
            }

            if (!await _auth.LoginAsync(username, password))
            {
                ShowError("Registration succeeded, but login failed.");
                return false;
            }

            ApplyRememberPreference();
            return true;
        });
    }

    private async Task RunAuthActionAsync(Func<Task<bool>> action)
    {
        _errorLabel.IsVisible = false;

        try
        {
            if (await action())
                await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ApplyRememberPreference()
    {
        if (_rememberCheckBox.IsChecked)
            return;

        SecureStorage.Remove(AuthService.SessionUserKey);
        SecureStorage.Remove(AuthService.SessionPasswordKey);
    }

    private void ShowError(string message)
    {
        _errorLabel.Text = message;
        _errorLabel.IsVisible = true;
    }

    private void ConfigureContextActions()
    {
#if WINDOWS
        HandlerChanged += (_, _) => ConfigureWindowsContextMenu();
#elif ANDROID
        HandlerChanged += (_, _) => ConfigureAndroidLongPress();
#endif
    }

#if WINDOWS
    private void ConfigureWindowsContextMenu()
    {
        if (Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement nativeView)
            return;

        var menu = new Microsoft.UI.Xaml.Controls.MenuFlyout();
        var openDataFolderItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
        {
            Text = "Open Data Folder"
        };
        openDataFolderItem.Click += (_, _) => OpenDataFolder();

        var manualBackupItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
        {
            Text = "Manual Backup"
        };
        manualBackupItem.Click += async (_, _) => await ManualBackupAsync();

        menu.Items.Add(openDataFolderItem);
        menu.Items.Add(manualBackupItem);
        nativeView.ContextFlyout = menu;
    }
#elif ANDROID
    private Android.Views.View? _androidLongPressView;

    private void ConfigureAndroidLongPress()
    {
        if (_androidLongPressView != null ||
            Handler?.PlatformView is not Android.Views.View nativeView)
        {
            return;
        }

        _androidLongPressView = nativeView;
        nativeView.LongClickable = true;
        nativeView.LongClick += OnAndroidLongClick;
    }

    private async void OnAndroidLongClick(object? sender, Android.Views.View.LongClickEventArgs e)
    {
        e.Handled = true;
        await ShowContextMenuAsync();
    }

    private async Task ShowContextMenuAsync()
    {
        string? action = await DisplayActionSheet(
            "Options",
            "Cancel",
            null,
            "Open Data Folder",
            "Manual Backup");

        switch (action)
        {
            case "Open Data Folder":
                OpenDataFolder();
                break;
            case "Manual Backup":
                await ManualBackupAsync();
                break;
        }
    }
#endif

    private void OpenDataFolder()
    {
#if WINDOWS
        try
        {
            string folder = FileSystem.AppDataDirectory;
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
#else
        DisplayAlert("Info", $"Data folder:\n{FileSystem.AppDataDirectory}", "OK");
#endif
    }

    private async Task ManualBackupAsync()
    {
        var result = await _backup.ManualBackupAsync();
        await DisplayAlert(
            result.success ? "Backup Complete" : "Backup Failed",
            result.message, "OK");
    }

#if DEBUG
    private async Task OnTestDevClickedAsync()
    {
        string suffix = Convert.ToHexString(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(2))
            .ToLowerInvariant();
        string username = $"test_{suffix}";
        string password = "test1234";

        bool registered = await _auth.RegisterAsync(username, password);
        if (!registered)
        {
            _errorLabel.Text = "DEV register failed: " + _auth.LastRegisterError;
            _errorLabel.IsVisible = true;
            return;
        }

        if (!await _auth.LoginAsync(username, password))
        {
            _errorLabel.Text = "DEV login failed after register";
            _errorLabel.IsVisible = true;
            return;
        }

        await Shell.Current.GoToAsync("//home");
    }
#endif
}
