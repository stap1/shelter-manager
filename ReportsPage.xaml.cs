using ShelterManager.Infrastructure;
using ShelterManager.Services;

namespace ShelterManager;

public partial class ReportsPage : ContentPage
{
    private readonly ReportService _reportService;

    public ReportsPage()
    {
        InitializeComponent();

        _reportService = ServiceLocator.GetRequiredService<ReportService>();

        // Domyślny okres: ostatnie 30 dni (włącznie).
        var end = DateTime.Today;
        var start = end.AddDays(-30);

        StartDatePicker.Date = start;
        EndDatePicker.Date = end;

        RefreshReport();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Odśwież po powrocie na zakładkę (np. po dodaniu zwierzęcia, transakcji, adopcji).
        RefreshReport();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        RefreshReport();
    }

    private void OnDateChanged(object sender, DateChangedEventArgs e)
    {
        // Nie liczymy logiki w UI, ale odświeżenie jest reakcją na zmianę zakresu.
        RefreshReport();
    }

    private void RefreshReport()
    {
        var dto = _reportService.BuildReport(StartDatePicker.Date, EndDatePicker.Date);
        BindingContext = dto;
    }
}
