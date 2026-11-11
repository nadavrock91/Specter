using System.Globalization;
using Specter.Models;
using Specter.Services;

namespace Specter.Views;

public class CalendarPage : ContentPage
{
    private readonly ClientDatabaseService _clientDb;
    private readonly string _username;
    private readonly Label _monthLabel;
    private readonly Grid _calendarGrid;
    private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public CalendarPage(ClientDatabaseService clientDb, string username)
    {
        _clientDb = clientDb;
        _username = username;

        Title = "Calendar";
        BackgroundColor = Color.FromArgb("#1A1A2E");

        var previousButton = new Button
        {
            Text = "‹",
            FontSize = 24,
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            WidthRequest = 48
        };
        previousButton.Clicked += async (_, _) => await ChangeMonthAsync(-1);

        var nextButton = new Button
        {
            Text = "›",
            FontSize = 24,
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            WidthRequest = 48
        };
        nextButton.Clicked += async (_, _) => await ChangeMonthAsync(1);

        _monthLabel = new Label
        {
            TextColor = Colors.White,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        var topBar = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        topBar.Add(previousButton, 0, 0);
        topBar.Add(_monthLabel, 1, 0);
        topBar.Add(nextButton, 2, 0);

        _calendarGrid = new Grid
        {
            ColumnSpacing = 4,
            RowSpacing = 4
        };
        for (int column = 0; column < 7; column++)
            _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _calendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (int row = 0; row < 6; row++)
            _calendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        string[] dayHeaders = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        for (int column = 0; column < dayHeaders.Length; column++)
        {
            _calendarGrid.Add(new Label
            {
                Text = dayHeaders[column],
                TextColor = Colors.White,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 4)
            }, column, 0);
        }

        Content = new Grid
        {
            Padding = new Thickness(16),
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                topBar,
                new ScrollView
                {
                    Content = _calendarGrid,
                    Orientation = ScrollOrientation.Vertical
                }
            }
        };
        Grid.SetRow((View)((Grid)Content).Children[1], 1);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMonthAsync();
    }

    private async Task ChangeMonthAsync(int monthOffset)
    {
        _displayedMonth = _displayedMonth.AddMonths(monthOffset);
        await LoadMonthAsync();
    }

    private async Task LoadMonthAsync()
    {
        _monthLabel.Text = _displayedMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

        var conn = await _clientDb.GetConnectionAsync();
        var allScheduledClients = await conn.Table<Client>()
            .Where(c => c.Username == _username && c.ScheduledDate != null)
            .ToListAsync();
        var clients = allScheduledClients
            .Where(c => c.ScheduledDate!.Value.Year == _displayedMonth.Year &&
                        c.ScheduledDate.Value.Month == _displayedMonth.Month)
            .ToList();

        while (_calendarGrid.Children.Count > 7)
            _calendarGrid.Children.RemoveAt(_calendarGrid.Children.Count - 1);

        DateTime firstOfMonth = new(_displayedMonth.Year, _displayedMonth.Month, 1);
        DateTime firstVisibleDate = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);

        for (int index = 0; index < 42; index++)
        {
            DateTime date = firstVisibleDate.AddDays(index);
            var clientsForDate = clients
                .Where(c => c.ScheduledDate!.Value.Date == date.Date)
                .OrderBy(c => c.Name)
                .ToList();

            var cellContent = new VerticalStackLayout
            {
                Spacing = 3,
                Children =
                {
                    new Label
                    {
                        Text = date.Day.ToString(CultureInfo.InvariantCulture),
                        TextColor = date.Month == _displayedMonth.Month
                            ? Colors.White
                            : Color.FromArgb("#555555"),
                        FontSize = 12,
                        HorizontalTextAlignment = TextAlignment.Start
                    }
                }
            };

            foreach (var client in clientsForDate)
            {
                cellContent.Children.Add(new Frame
                {
                    BackgroundColor = Color.FromArgb("#1565C0"),
                    CornerRadius = 4,
                    Padding = new Thickness(2),
                    HasShadow = false,
                    Content = new Label
                    {
                        Text = client.Name,
                        TextColor = Colors.White,
                        FontSize = 11,
                        LineBreakMode = LineBreakMode.TailTruncation
                    }
                });
            }

            var dayCell = new Frame
            {
                BackgroundColor = Color.FromArgb("#24243A"),
                BorderColor = date.Date == DateTime.Today
                    ? Color.FromArgb("#4A90D9")
                    : Colors.Transparent,
                CornerRadius = 4,
                Padding = new Thickness(5),
                HasShadow = false,
                MinimumHeightRequest = 85,
                Content = cellContent
            };

            if (clientsForDate.Count > 0)
            {
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (_, _) =>
                    await Navigation.PushAsync(BuildClientsForDatePage(date, clientsForDate));
                dayCell.GestureRecognizers.Add(tapGesture);
            }

            int row = index / 7 + 1;
            int column = index % 7;
            _calendarGrid.Add(dayCell, column, row);
        }
    }

    private static ContentPage BuildClientsForDatePage(DateTime date, List<Client> clients)
    {
        var list = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 10
        };

        foreach (var client in clients)
        {
            list.Children.Add(new Frame
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
                            Text = client.Name,
                            TextColor = Colors.White,
                            FontSize = 16,
                            FontAttributes = FontAttributes.Bold
                        },
                        new Label { Text = client.PhoneNumber, TextColor = Color.FromArgb("#90CAF9") },
                        new Label { Text = client.ClientNumber, TextColor = Color.FromArgb("#CCCCCC") },
                        new Label { Text = client.Notes, TextColor = Color.FromArgb("#CCCCCC") }
                    }
                }
            });
        }

        return new ContentPage
        {
            Title = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            BackgroundColor = Color.FromArgb("#1A1A2E"),
            Content = new ScrollView { Content = list }
        };
    }
}
