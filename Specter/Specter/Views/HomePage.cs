using Specter.Services;

namespace Specter.Views;

public class HomePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly Label _welcomeLabel;

    public HomePage(AuthService authService)
    {
        _authService = authService;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _welcomeLabel.Text = $"Welcome, {_authService.CurrentUsername}";
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _authService.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
