namespace ShelterManager;

public partial class LoginPage : ContentPage
{
    // Publiczna właściwość statyczna dostępna w całej aplikacji
    public static string RolaUzytkownika { get; private set; } = "Gosc";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        // PROSTA AUTORYZACJA
        if (PasswordEntry.Text == "admin123")
        {
            RolaUzytkownika = "Admin";
            Application.Current!.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Błąd", "Niepoprawne hasło administratora! Podpowiedź: admin123", "OK");
            PasswordEntry.Text = string.Empty;
        }
    }

    private void OnUserClicked(object sender, EventArgs e)
    {
        // Pracownik wchodzi bez hasła
        RolaUzytkownika = "Pracownik";
        Application.Current!.MainPage = new AppShell();
    }
}