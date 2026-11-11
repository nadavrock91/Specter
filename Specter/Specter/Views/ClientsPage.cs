using Specter.Models;
using Specter.Services;

namespace Specter.Views;

public class ClientsPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ClientDatabaseService _clientDb;
    private VerticalStackLayout _listLayout = null!;
    private Entry _searchEntry = null!;
    private List<Client> _clients = new();
    private bool _isOpeningForm;
    private bool _isLoggingAttempt;

    public ClientsPage(AuthService auth, ClientDatabaseService clientDb)
    {
        _auth = auth;
        _clientDb = clientDb;
        Title = AppStrings.ClientsTitle;
        BackgroundColor = Color.FromArgb("#1A1A2E");
        BuildUI();
        AppStrings.LanguageChanged += ApplyStrings;
    }

    private void BuildUI()
    {
        _searchEntry = new Entry
        {
            Placeholder = AppStrings.SearchPlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888"),
            HorizontalOptions = LayoutOptions.FillAndExpand
        };
        _searchEntry.TextChanged += OnSearchChanged;

        var addButton = new Button
        {
            Text = AppStrings.AddBtn,
            BackgroundColor = Color.FromArgb("#1565C0"),
            TextColor = Colors.White,
            WidthRequest = 80,
            HeightRequest = 48
        };
        addButton.Clicked += async (_, _) => await OnAddClientAsync();

        var topRow = new HorizontalStackLayout
        {
            Spacing = 12,
            Children =
            {
                _searchEntry,
                addButton
            }
        };

        _listLayout = new VerticalStackLayout { Spacing = 8 };

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 12,
            Children =
            {
                topRow,
                new ScrollView
                {
                    Content = _listLayout,
                    VerticalOptions = LayoutOptions.Fill
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        _clients = await _clientDb.GetClientsAsync(_auth.CurrentUsername);
        RenderList(_clients);
    }

    private async void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        string q = _searchEntry.Text?.Trim() ?? "";
        if (q.Length == 0)
        {
            RenderList(_clients);
            return;
        }

        var filtered = await _clientDb.SearchClientsAsync(
            _auth.CurrentUsername, q);
        RenderList(filtered);
    }

    private void RenderList(List<Client> clients)
    {
        _listLayout.Children.Clear();
        if (clients.Count == 0)
        {
            _listLayout.Children.Add(new Label
            {
                Text = AppStrings.NoClients,
                TextColor = Color.FromArgb("#888"),
                HorizontalOptions = LayoutOptions.Center
            });
            return;
        }

        foreach (var client in clients)
            _listLayout.Children.Add(BuildClientCard(client));
    }

    private View BuildClientCard(Client client)
    {
        var details = new VerticalStackLayout { Spacing = 6 };
        details.Children.Add(new Label
        {
            Text = AppStrings.CallAttempts(client.CallAttemptCount),
            TextColor = Color.FromArgb("#90CAF9"),
            FontSize = 13
        });
        details.Children.Add(new HorizontalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = client.Name,
                    TextColor = Colors.White,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                },
                new Label
                {
                    Text = $"#{client.ClientNumber}",
                    TextColor = Color.FromArgb("#888"),
                    FontSize = 12
                }
            }
        });

        if (!string.IsNullOrEmpty(client.PhoneNumber))
        {
            details.Children.Add(new Label
            {
                Text = client.PhoneNumber,
                TextColor = Color.FromArgb("#90CAF9"),
                FontSize = 13
            });
        }

        if (!string.IsNullOrEmpty(client.Notes))
        {
            details.Children.Add(new Label
            {
                Text = client.Notes,
                TextColor = Color.FromArgb("#CCC"),
                FontSize = 12,
                MaxLines = 2,
                LineBreakMode = LineBreakMode.TailTruncation
            });
        }

        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            CornerRadius = 10,
            Padding = new Thickness(12),
            Content = details
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (_, _) => await OnEditClientAsync(client);
        frame.GestureRecognizers.Add(tapGesture);
        return frame;
    }

    private async Task OnAddClientAsync()
    {
        await ShowClientFormAsync(null);
    }

    private async Task OnEditClientAsync(Client client)
    {
        await ShowClientFormAsync(client);
    }

    private async Task ShowClientFormAsync(Client? existing)
    {
        if (_isOpeningForm) return;
        _isOpeningForm = true;
        try
        {
        bool isNew = existing == null;
        var client = existing ?? new Client
        {
            Username = _auth.CurrentUsername
        };

        var nameEntry = new Entry
        {
            Text = client.Name,
            Placeholder = AppStrings.NamePlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
        };

        var phoneEntry = new Entry
        {
            Text = client.PhoneNumber,
            Placeholder = AppStrings.PhonePlaceholder,
            Keyboard = Keyboard.Telephone,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
        };

        var clientNumEntry = new Entry
        {
            Text = client.ClientNumber,
            Placeholder = AppStrings.ClientNumPlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
        };

        var notesEditor = new Editor
        {
            Text = client.Notes,
            Placeholder = AppStrings.NotesPlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888"),
            HeightRequest = 100,
            AutoSize = EditorAutoSizeOption.Disabled
        };

        var saveBtn = new Button
        {
            Text = isNew ? AppStrings.AddClientTitle : AppStrings.Save,
            BackgroundColor = Color.FromArgb("#1565C0"),
            TextColor = Colors.White,
            CornerRadius = 8
        };

        var layout = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(0),
            Children =
            {
                nameEntry,
                phoneEntry,
                clientNumEntry,
                notesEditor,
                saveBtn
            }
        };

        if (!isNew)
        {
            var attemptLabel = new Label
            {
                Text = AppStrings.CallAttempts(client.CallAttemptCount),
                TextColor = Color.FromArgb("#90CAF9"),
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };
            var logAttemptBtn = new Button
            {
                Text = AppStrings.LogCallBtn,
                BackgroundColor = Color.FromArgb("#1565C0"),
                TextColor = Colors.White,
                CornerRadius = 8
            };
            layout.Children.Insert(0, attemptLabel);
            layout.Children.Insert(1, logAttemptBtn);
            logAttemptBtn.Clicked += async (_, _) =>
            {
                if (_isLoggingAttempt) return;
                _isLoggingAttempt = true;
                logAttemptBtn.IsEnabled = false;
                saveBtn.IsEnabled = false;
                try
                {
                    client.CallAttemptCount = await _clientDb.LogCallAttemptAsync(client.Id, client.Username);
                    attemptLabel.Text = AppStrings.CallAttempts(client.CallAttemptCount);
                    await LoadClientsAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert(AppStrings.Error, $"Could not log the call attempt: {ex.Message}", AppStrings.Ok);
                }
                finally
                {
                    logAttemptBtn.IsEnabled = true;
                    saveBtn.IsEnabled = true;
                    _isLoggingAttempt = false;
                }
            };

            var deleteBtn = new Button
            {
                Text = AppStrings.DeleteClient,
                BackgroundColor = Color.FromArgb("#B71C1C"),
                TextColor = Colors.White,
                CornerRadius = 8
            };
            layout.Children.Add(deleteBtn);
            deleteBtn.Clicked += async (_, _) =>
            {
                bool confirm = await DisplayAlert(
                    AppStrings.Delete,
                    AppStrings.DeleteMsg(client.Name),
                    AppStrings.Delete,
                    AppStrings.Cancel);
                if (!confirm) return;

                await _clientDb.DeleteClientAsync(client.Id);
                await Navigation.PopAsync();
                await LoadClientsAsync();
            };
        }

        var formPage = new ContentPage
        {
            Title = isNew ? AppStrings.AddClientTitle : AppStrings.EditClientTitle,
            BackgroundColor = Color.FromArgb("#1A1A2E"),
            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Padding = new Thickness(16),
                    Spacing = 12,
                    Children = { layout }
                }
            }
        };

        saveBtn.Clicked += async (_, _) =>
        {
            client.Name = nameEntry.Text?.Trim() ?? "";
            client.PhoneNumber = phoneEntry.Text?.Trim() ?? "";
            client.ClientNumber = clientNumEntry.Text?.Trim() ?? "";
            client.Notes = notesEditor.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(client.Name))
            {
                await DisplayAlert(AppStrings.Error, AppStrings.NameRequired, AppStrings.Ok);
                return;
            }

            bool hasPhone = !string.IsNullOrWhiteSpace(client.PhoneNumber);
            bool hasClientNum = !string.IsNullOrWhiteSpace(client.ClientNumber);

            // Only verify if exactly one is filled
            if (hasPhone && !hasClientNum)
            {
                string confirm = await DisplayPromptAsync(AppStrings.ConfirmPhone,
                    AppStrings.ConfirmPhoneMsg, AppStrings.Ok, AppStrings.Cancel,
                    keyboard: Keyboard.Telephone);
                if (confirm == null) return;
                if ((confirm?.Trim() ?? "") != client.PhoneNumber)
                {
                    await DisplayAlert(AppStrings.Mismatch, AppStrings.PhoneMismatchMsg, AppStrings.Ok);
                    return;
                }
            }

            if (hasClientNum && !hasPhone)
            {
                string confirmNum = await DisplayPromptAsync(AppStrings.ConfirmClientNum,
                    AppStrings.ConfirmClientNumMsg, AppStrings.Ok, AppStrings.Cancel,
                    keyboard: Keyboard.Numeric);
                if (confirmNum == null) return;
                if ((confirmNum?.Trim() ?? "") != client.ClientNumber)
                {
                    await DisplayAlert(AppStrings.Mismatch, AppStrings.ClientNumMismatchMsg, AppStrings.Ok);
                    return;
                }
            }

            await _clientDb.SaveClientAsync(client);
            await Navigation.PopAsync();
            await LoadClientsAsync();
        };

        await Navigation.PushAsync(formPage);
        }
        finally
        {
            _isOpeningForm = false;
        }
    }
    private void ApplyStrings()
    {
        Title = AppStrings.ClientsTitle;
    }
}
