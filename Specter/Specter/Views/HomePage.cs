using Specter.Services;

namespace Specter.Views;

public class HomePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly BackupService _backup;
    private readonly Label _welcomeLabel;

    public HomePage(AuthService authService, BackupService backup)
    {
        _authService = authService;
        _backup = backup;
        Title = "Home";

        _welcomeLabel = new Label
        {
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var logoutButton = new Button { Text = "Logout" };
        logoutButton.Clicked += OnLogoutClicked;

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(30),
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _welcomeLabel,
                new Label
                {
                    Text = "Sales tools coming soon",
                    HorizontalTextAlignment = TextAlignment.Center
                },
                logoutButton
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _welcomeLabel.Text = $"Welcome, {_authService.CurrentUsername}";
        await _backup.AutoBackupAsync("login");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await _backup.AutoBackupAsync("logout");
        await _authService.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
