using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using ShelterManager.Services;

namespace ShelterManager;

public partial class AdoptionApplicationDetailsPage : ContentPage
{
    private readonly AdoptionApplication _application;
    private readonly IAdoptionApplicationRepository _applications;
    private readonly AdoptionWorkflowService _workflow;

    private readonly bool _canManage = LoginPage.RolaUzytkownika is ("Admin" or "Pracownik");

    public AdoptionApplicationDetailsPage(AdoptionApplication application)
    {
        InitializeComponent();

        _application = application;
        BindingContext = _application;

        _applications = ServiceLocator.GetRequiredService<IAdoptionApplicationRepository>();
        _workflow = ServiceLocator.GetRequiredService<AdoptionWorkflowService>();

        // Gość może tylko podglądać.
        BtnInReview.IsVisible = _canManage;
        BtnApprove.IsVisible = _canManage;
        BtnReject.IsVisible = _canManage;
        NotesEditor.IsReadOnly = !_canManage;

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        // Zakończone wnioski blokujemy.
        bool finished = _application.Status is AdoptionApplicationStatus.Approved or AdoptionApplicationStatus.Rejected;

        BtnInReview.IsEnabled = _canManage && !finished && _application.Status == AdoptionApplicationStatus.New;
        BtnApprove.IsEnabled = _canManage && !finished && _application.Status != AdoptionApplicationStatus.Rejected;
        BtnReject.IsEnabled = _canManage && !finished;
    }

    private async void OnSaveNotesClicked(object sender, EventArgs e)
    {
        if (!_canManage)
        {
            await DisplayAlert("Brak uprawnień", "Tylko Administrator lub Pracownik może edytować notatki.", "OK");
            return;
        }

        _applications.SaveChanges();
        await DisplayAlert("OK", "Zapisano notatki.", "OK");
    }

    private async void OnInReviewClicked(object sender, EventArgs e)
    {
        var result = _workflow.SetStatus(_application, AdoptionApplicationStatus.InReview);
        if (!result.Success)
        {
            await DisplayAlert("Błąd", result.Error ?? "Nie udało się zmienić statusu.", "OK");
            return;
        }

        UpdateButtonStates();
        await DisplayAlert("OK", "Wniosek ustawiono na: W weryfikacji.", "OK");
    }

    private async void OnApproveClicked(object sender, EventArgs e)
    {
        if (!_canManage)
            return;

        bool confirm = await DisplayAlert(
            "Zatwierdzenie adopcji",
            "Zatwierdzenie ustawi zwierzę jako Adopted, zdejmie je z boksu i dopisze wpis do historii. Kontynuować?",
            "Tak",
            "Nie");

        if (!confirm)
            return;

        var result = _workflow.SetStatus(_application, AdoptionApplicationStatus.Approved);
        if (!result.Success)
        {
            await DisplayAlert("Błąd", result.Error ?? "Nie udało się zatwierdzić.", "OK");
            return;
        }

        UpdateButtonStates();
        await DisplayAlert("OK", "Wniosek zatwierdzony. Zwierzę oznaczone jako Adopted.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnRejectClicked(object sender, EventArgs e)
    {
        if (!_canManage)
            return;

        bool confirm = await DisplayAlert("Odrzucenie", "Czy na pewno odrzucić wniosek?", "Tak", "Nie");
        if (!confirm)
            return;

        var result = _workflow.SetStatus(_application, AdoptionApplicationStatus.Rejected);
        if (!result.Success)
        {
            await DisplayAlert("Błąd", result.Error ?? "Nie udało się odrzucić.", "OK");
            return;
        }

        UpdateButtonStates();
        await DisplayAlert("OK", "Wniosek odrzucony.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
