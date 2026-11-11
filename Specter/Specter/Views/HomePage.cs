using Specter.Services;

namespace Specter.Views;

public class HomePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly BackupService _backup;
    private readonly ClientDatabaseService _clientDb;
    private readonly Label _welcomeLabel;
    private readonly Label _comingSoonLabel;
    private readonly Button _logoutButton;
    private readonly Button _clientsBtn;
    private readonly Button _langToggleBtn;
    private Button _calendarBtn = null!;

    public HomePage(
        AuthService authService,
        BackupService backup,
        ClientDatabaseService clientDb)
    {
        _authService = authService;
        _backup = backup;
        _clientDb = clientDb;

        _welcomeLabel = new Label
        {
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _comingSoonLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center
        };

        _logoutButton = new Button();
        _logoutButton.Clicked += OnLogoutClicked;

        _clientsBtn = new Button
        {
            BackgroundColor = Color.FromArgb("#1565C0"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 48,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _clientsBtn.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new ClientsPage(_authService, _clientDb));

        _calendarBtn = new Button
        {
            BackgroundColor = Color.FromArgb("#1565C0"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 48,
            Margin = new Thickness(0, 0, 0, 8)
        };
        _calendarBtn.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new CalendarPage(_clientDb, _authService.CurrentUsername));

        _langToggleBtn = new Button
        {
            BackgroundColor = Color.FromArgb("#424242"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 40,
            Margin = new Thickness(0, 8, 0, 0)
        };
        _langToggleBtn.Clicked += (_, _) =>
        {
            AppStrings.Toggle();
        };

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(30),
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _welcomeLabel,
                _comingSoonLabel,
                _clientsBtn,
                _calendarBtn,
                _logoutButton,
                _langToggleBtn
            }
        };

        AppStrings.LanguageChanged += ApplyStrings;
        ApplyStrings();
    }

    private void ApplyStrings()
    {
        Title = AppStrings.HomeTitle;
        _welcomeLabel.Text = AppStrings.Welcome(_authService.CurrentUsername ?? "");
        _comingSoonLabel.Text = AppStrings.ComingSoon;
        _logoutButton.Text = AppStrings.Logout;
        _clientsBtn.Text = AppStrings.ClientsBtn;
        _calendarBtn.Text = AppStrings.CalendarBtn;
        _langToggleBtn.Text = AppStrings.ToggleLang;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _welcomeLabel.Text = AppStrings.Welcome(_authService.CurrentUsername ?? "");
        await _backup.AutoBackupAsync("login");
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        await _backup.AutoBackupAsync("logout");
        await _authService.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
