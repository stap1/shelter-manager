namespace ShelterManager;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // ZMIANA: Startujemy od logowania, a nie od AppShell!
        MainPage = new LoginPage();
    }
}