using Specter.Models;
using Specter.Services;

namespace Specter.Views;

public class ClientsPage : ContentPage
{
    private readonly AuthService _auth;
    private readonly ClientDatabaseService _clientDb;
    private VerticalStackLayout _listLayout = null!;
    private HorizontalStackLayout _filterRow = null!;
    private Entry _searchEntry = null!;
    private List<Client> _clients = new();
    private List<ClientCategory> _categories = new();
    private int? _activeCategoryFilter = null; // null means "All"
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

        _filterRow = new HorizontalStackLayout { Spacing = 8 };

        var filterScroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = _filterRow,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
        };

        var listScroll = new ScrollView
        {
            Content = _listLayout,
            VerticalOptions = LayoutOptions.Fill
        };

        Content = new Grid
        {
            Padding = new Thickness(16),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            Children =
            {
                topRow,
                filterScroll,
                listScroll
            }
        };

        Grid.SetRow(topRow, 0);
        Grid.SetRow(filterScroll, 1);
        Grid.SetRow(listScroll, 2);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        _clients = await _clientDb.GetClientsAsync(_auth.CurrentUsername);
        _categories = await _clientDb.GetCategoriesAsync(_auth.CurrentUsername);
        RebuildFilterChips();
        ApplyFilter();
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        string q = _searchEntry.Text?.Trim() ?? "";
        var filtered = q.Length == 0
            ? _clients
            : _clients.Where(c =>
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.ClientNumber.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Notes.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (_activeCategoryFilter.HasValue)
            filtered = filtered.Where(c => c.CategoryId == _activeCategoryFilter).ToList();

        RenderList(filtered);
    }

    private void RebuildFilterChips()
    {
        _filterRow.Children.Clear();

        // "All" chip
        var allChip = MakeFilterChip(AppStrings.AllCategories, null);
        _filterRow.Children.Add(allChip);

        foreach (var cat in _categories)
            _filterRow.Children.Add(MakeFilterChip(cat.Name, cat.Id));
    }

    private Button MakeFilterChip(string label, int? categoryId)
    {
        bool isActive = _activeCategoryFilter == categoryId;
        var chip = new Button
        {
            Text = label,
            FontSize = 12,
            HeightRequest = 30,
            Padding = new Thickness(10, 0),
            CornerRadius = 15,
            BackgroundColor = isActive ? Color.FromArgb("#1565C0") : Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White
        };
        chip.Clicked += (_, _) =>
        {
            _activeCategoryFilter = categoryId;
            RebuildFilterChips();
            ApplyFilter();
        };
        return chip;
    }

    private void ApplyFilter()
    {
        var filtered = _activeCategoryFilter == null
            ? _clients
            : _clients.Where(c => c.CategoryId == _activeCategoryFilter).ToList();
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
                    Text = AppStrings.PrivacyMode ? AppStrings.Mask(client.Name) : client.Name,
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

        if (client.CategoryId.HasValue)
        {
            var cat = _categories.FirstOrDefault(c => c.Id == client.CategoryId.Value);
            if (cat != null)
            {
                details.Children.Add(new Label
                {
                    Text = $" {cat.Name}",
                    TextColor = Color.FromArgb("#FFD54F"),
                    FontSize = 12
                });
            }
        }

        if (!string.IsNullOrEmpty(client.PhoneNumber))
        {
            details.Children.Add(new Label
            {
                Text = AppStrings.PrivacyMode ? AppStrings.Mask(client.PhoneNumber) : client.PhoneNumber,
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

        var archiveBtn = new Button
        {
            Text = AppStrings.Archive,
            FontSize = 11,
            HeightRequest = 28,
            BackgroundColor = Color.FromArgb("#424242"),
            TextColor = Color.FromArgb("#AAAAAA"),
            CornerRadius = 6,
            HorizontalOptions = LayoutOptions.End
        };
        archiveBtn.Clicked += async (_, _) =>
        {
            await _clientDb.ArchiveClientAsync(client.Id, client.Username);
            await LoadClientsAsync();
        };
        details.Children.Add(archiveBtn);
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

        var categories = await _clientDb.GetCategoriesAsync(_auth.CurrentUsername);

        var titleEntry = new Entry
        {
            Text = client.Title ?? "",
            Placeholder = AppStrings.TitlePlaceholder,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
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

        // Category label
        var categoryLabel = new Label
        {
            Text = AppStrings.CategoryLabel,
            TextColor = Color.FromArgb("#AAAAAA"),
            FontSize = 13
        };

        // Picker — first item is always "— No Category —"
        var categoryPicker = new Picker
        {
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            TitleColor = Color.FromArgb("#888")
        };
        categoryPicker.Items.Add(AppStrings.NoCategoryOption);
        foreach (var cat in categories)
            categoryPicker.Items.Add(cat.Name);
        // Select current category if editing
        if (client.CategoryId.HasValue)
        {
            var idx = categories.FindIndex(c => c.Id == client.CategoryId.Value);
            categoryPicker.SelectedIndex = idx >= 0 ? idx + 1 : 0;
        }
        else
        {
            categoryPicker.SelectedIndex = 0;
        }

        // "Add category" button
        var addCategoryBtn = new Button
        {
            Text = AppStrings.AddCategory,
            FontSize = 11,
            HeightRequest = 30,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Color.FromArgb("#90CAF9"),
            HorizontalOptions = LayoutOptions.Start
        };
        addCategoryBtn.Clicked += async (_, _) =>
        {
            string? newName = await DisplayPromptAsync(AppStrings.AddCategory, AppStrings.CategoryName);
            if (string.IsNullOrWhiteSpace(newName)) return;
            var created = await _clientDb.AddCategoryAsync(_auth.CurrentUsername, newName);
            categories.Add(created);
            categoryPicker.Items.Add(created.Name);
            categoryPicker.SelectedIndex = categoryPicker.Items.Count - 1;
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

        var copyTitleToNotesBtn = new Button
        {
            Text = AppStrings.CopyTitleToNotes,
            FontSize = 12,
            HeightRequest = 30,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Color.FromArgb("#90CAF9"),
            HorizontalOptions = LayoutOptions.Fill
        };
        copyTitleToNotesBtn.Clicked += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(titleEntry.Text))
                notesEditor.Text = titleEntry.Text;
        };

        var closingLabel = new Label
        {
            Text = AppStrings.ClosingChanceLabel,
            TextColor = Color.FromArgb("#AAAAAA"),
            FontSize = 13
        };
        var closingEntry = new Entry
        {
            Text = client.ClosingChance.ToString(),
            Placeholder = "0-100",
            Keyboard = Keyboard.Numeric,
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888")
        };

        var scheduledDateLabel = new Label
        {
            Text = AppStrings.ScheduledDate,
            TextColor = Color.FromArgb("#AAAAAA"),
            FontSize = 13
        };

        var scheduledDatePicker = new DatePicker
        {
            Format = "dd/MM/yyyy",
            BackgroundColor = Color.FromArgb("#2A2A3E"),
            TextColor = Colors.White,
            MinimumDate = DateTime.Today
        };
        if (client.ScheduledDate.HasValue)
            scheduledDatePicker.Date = client.ScheduledDate.Value;
        else
            scheduledDatePicker.Date = DateTime.Today;

        var dateOffsetRow = new HorizontalStackLayout { Spacing = 8 };
        var offsets = new[] { ("Today", 0), ("+1", 1), ("+2", 2), ("+3", 3), ("+7", 7), ("+14", 14), ("+30", 30) };
        foreach (var (label, days) in offsets)
        {
            var btn = new Button
            {
                Text = label,
                FontSize = 12,
                HeightRequest = 32,
                Padding = new Thickness(8, 0),
                BackgroundColor = Color.FromArgb("#2A2A3E"),
                TextColor = Colors.White,
                CornerRadius = 6,
                BorderColor = Color.FromArgb("#555"),
                BorderWidth = 1
            };
            int d = days;
            btn.Clicked += (_, _) => scheduledDatePicker.Date = DateTime.Today.AddDays(d);
            dateOffsetRow.Children.Add(btn);
        }

        var dateOffsetScroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = dateOffsetRow,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
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
                titleEntry,
                nameEntry,
                clientNumEntry,
                phoneEntry,
                categoryLabel,
                categoryPicker,
                addCategoryBtn,
                copyTitleToNotesBtn,
                notesEditor,
                closingLabel,
                closingEntry,
                scheduledDateLabel,
                dateOffsetScroll,
                scheduledDatePicker,
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

            if (string.IsNullOrWhiteSpace(notesEditor.Text))
            {
                await DisplayAlert(AppStrings.Error, AppStrings.NotesRequired, AppStrings.Ok);
                return;
            }

            bool hasPhone = !string.IsNullOrWhiteSpace(client.PhoneNumber);
            bool hasClientNum = !string.IsNullOrWhiteSpace(client.ClientNumber);

            if (hasPhone && client.PhoneNumber.Length != 10)
            {
                await DisplayAlert(AppStrings.Error, AppStrings.PhoneLengthError, AppStrings.Ok);
                return;
            }
            if (isNew)
            {
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
            }

            client.ScheduledDate = scheduledDatePicker.Date;
            client.Title = titleEntry.Text?.Trim() ?? "";
            if (int.TryParse(closingEntry.Text?.Trim(), out int chance) && chance >= 0 && chance <= 100)
                client.ClosingChance = chance;
            else if (!string.IsNullOrWhiteSpace(closingEntry.Text))
            {
                await DisplayAlert(AppStrings.Error, AppStrings.ClosingChanceError, AppStrings.Ok);
                return;
            }
            int pickerIdx = categoryPicker.SelectedIndex;
            client.CategoryId = (pickerIdx > 0 && pickerIdx - 1 < categories.Count)
                ? categories[pickerIdx - 1].Id
                : (int?)null;
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
        MainThread.BeginInvokeOnMainThread(async () => await LoadClientsAsync());
    }
}
