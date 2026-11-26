using Specter.Models;
using Specter.Services;

namespace Specter.Views;

public class ArchivedClientsPage : ContentPage
{
    private readonly ClientDatabaseService _clientDb;
    private readonly string _username;
    private readonly Entry _searchEntry;
    private readonly VerticalStackLayout _listLayout;
    private List<Client> _clients = new();

    public ArchivedClientsPage(ClientDatabaseService clientDb, string username)
    {
        _clientDb = clientDb;
        _username = username;

        Title = AppStrings.ArchivedClients;
        BackgroundColor = Color.FromArgb("#1A1A2E");

        _searchEntry = new Entry
        {
            Placeholder = AppStrings.SearchPlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
        };
        _searchEntry.TextChanged += OnSearchChanged;

        _listLayout = new VerticalStackLayout { Spacing = 10 };

        var contentGrid = new Grid
        {
            Padding = new Thickness(16),
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };
        contentGrid.Add(_searchEntry, 0, 0);
        contentGrid.Add(new ScrollView { Content = _listLayout }, 0, 1);
        Content = contentGrid;

        AppStrings.LanguageChanged += () =>
            MainThread.BeginInvokeOnMainThread(() => RenderList(_clients));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _clients = await _clientDb.GetArchivedClientsAsync(_username);
        RenderList(_clients);
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        string query = _searchEntry.Text?.Trim() ?? "";
        if (query.Length == 0)
        {
            RenderList(_clients);
            return;
        }

        RenderList(_clients.Where(client =>
            client.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            client.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList());
    }

    private void RenderList(List<Client> clients)
    {
        _listLayout.Children.Clear();

        foreach (var client in clients)
        {
            _listLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#2A2A3E"),
                CornerRadius = 8,
                Padding = new Thickness(12),
                HasShadow = false,
                Content = new VerticalStackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label
                        {
                            Text = $"{AppStrings.ArchivedOn} {client.ArchivedAt?.ToString("dd/MM/yyyy")}",
                            TextColor = Color.FromArgb("#888"),
                            FontSize = 12
                        },
                        new Label
                        {
                            Text = client.Title,
                            TextColor = Colors.White,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Label { Text = AppStrings.PrivacyMode ? AppStrings.Mask(client.Name) : client.Name, TextColor = Colors.White },
                        new Label { Text = AppStrings.PrivacyMode ? AppStrings.Mask(client.PhoneNumber) : client.PhoneNumber, TextColor = Color.FromArgb("#90CAF9") },
                        new Label { Text = client.Notes, TextColor = Color.FromArgb("#CCCCCC") }
                    }
                }
            });
        }
    }
}
