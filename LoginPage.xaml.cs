using System.Security.Cryptography;
using System.Text;

namespace ShelterManager;

public partial class LoginPage : ContentPage
{
    // Rola bieżącej sesji. Statyczna, bo aplikacja startuje od jednego ekranu logowania.
    public static string RolaUzytkownika { get; private set; } = "Gosc";

    // SHA-256 hasła administratora dla wersji demo. Plaintext NIE jest trzymany w kodzie,
    // a aplikacja go nie ujawnia. To lokalne demo - realne wdrożenie użyłoby prawdziwego
    // dostawcy tożsamości; tu chodzi o brak plaintextu i brak podpowiedzi w UI.
    private const string AdminPasswordHash =
        "240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        if (IsAdminPassword(PasswordEntry.Text))
        {
            RolaUzytkownika = "Admin";
            Application.Current!.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Błąd", "Niepoprawne hasło administratora.", "OK");
            PasswordEntry.Text = string.Empty;
        }
    }

    private void OnUserClicked(object sender, EventArgs e)
    {
        // Pracownik wchodzi bez hasła (rola z ograniczeniami - bez usuwania rekordów).
        RolaUzytkownika = "Pracownik";
        Application.Current!.MainPage = new AppShell();
    }

    private static bool IsAdminPassword(string? candidate)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(candidate ?? string.Empty));
        string hash = Convert.ToHexString(bytes);
        return string.Equals(hash, AdminPasswordHash, StringComparison.OrdinalIgnoreCase);
    }
}
