using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class AdoptionApplicationsPage : ContentPage
{
    private readonly IAdoptionApplicationRepository _applications;

    public ObservableCollection<AdoptionApplication> WidoczneWnioski { get; private set; } = new();

    public AdoptionApplicationsPage()
    {
        InitializeComponent();
        BindingContext = this;

        _applications = ServiceLocator.GetRequiredService<IAdoptionApplicationRepository>();

        PickerStatus.ItemsSource = new List<string>
        {
            "Wszystkie",
            "Nowy",
            "W weryfikacji",
            "Zatwierdzony",
            "Odrzucony"
        };
        PickerStatus.SelectedIndex = 0;

        ApplyFilters();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _applications.Reload();
        ApplyFilters();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void OnFilterChanged(object sender, EventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        var all = _applications.Applications
            .OrderByDescending(a => a.DataUtworzenia)
            .ToList();

        // 1) Filtr statusu
        var statusFilter = PickerStatus.SelectedItem as string ?? "Wszystkie";
        if (statusFilter != "Wszystkie")
        {
            all = all.Where(a => a.StatusOpis == statusFilter).ToList();
        }

        // 2) Filtr tekstowy
        var q = SearchBar.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(q))
        {
            all = all
                .Where(a => (a.ApplicantName ?? string.Empty).ToLowerInvariant().Contains(q)
                         || (a.AnimalOpis ?? string.Empty).ToLowerInvariant().Contains(q))
                .ToList();
        }

        WidoczneWnioski = new ObservableCollection<AdoptionApplication>(all);
        OnPropertyChanged(nameof(WidoczneWnioski));
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AdoptionApplication app)
        {
            await Navigation.PushAsync(new AdoptionApplicationDetailsPage(app));
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AddAdoptionApplicationPage());
}
