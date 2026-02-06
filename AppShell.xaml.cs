namespace ShelterManager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        // Jeśli użytkownik wybrał zakładkę wylogowania
        if (args.Target.Location.OriginalString.Contains("LogoutPage"))
        {
            // Anulujemy standardowe przejście do zakładki
            args.Cancel();

            // Pytamy o potwierdzenie
            bool czyWylogowac = await DisplayAlert("Wylogowanie", "Czy na pewno chcesz wrócić do ekranu logowania?", "Tak", "Nie");

            if (czyWylogowac)
            {
                // Resetujemy całą aplikację do ekranu logowania
                // Używamy operatora '!' aby uniknąć ostrzeżeń o nullach
                Application.Current!.MainPage = new LoginPage();
            }
        }
    }
}