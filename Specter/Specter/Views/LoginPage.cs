using Specter.Services;

namespace Specter.Views;

public class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly Entry _usernameEntry;
    private readonly Entry _passwordEntry;
    private readonly CheckBox _rememberCheckBox;
    private readonly Label _errorLabel;
    private bool _isRestoring;

    public LoginPage(AuthService authService)
    {
        _authService = authService;
        Title = "Login";
        BackgroundColor = Color.FromArgb("#1A1A2E");

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

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
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
            }
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

            if (await _authService.TryRestoreSessionAsync())
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
            if (!await _authService.LoginAsync(
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

            if (!await _authService.RegisterAsync(username, password))
            {
                ShowError(_authService.LastRegisterError);
                return false;
            }

            if (!await _authService.LoginAsync(username, password))
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
}
