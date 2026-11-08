using Specter.Services;

namespace Specter.Views;

public class HomePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly BackupService _backup;
    private readonly ClientDatabaseService _clientDb;
    private readonly Label _welcomeLabel;

    public HomePage(
        AuthService authService,
        BackupService backup,
        ClientDatabaseService clientDb)
    {
        _authService = authService;
        _backup = backup;
        _clientDb = clientDb;
        Title = "Home";

        _welcomeLabel = new Label
        {
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var logoutButton = new Button { Text = "Logout" };
        logoutButton.Clicked += OnLogoutClicked;

        var clientsBtn = new Button
        {
            Text = " Clients",
            BackgroundColor = Color.FromArgb("#1565C0"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 48,
            Margin = new Thickness(0, 0, 0, 8)
        };
        clientsBtn.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new ClientsPage(_authService, _clientDb));

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
                clientsBtn,
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
