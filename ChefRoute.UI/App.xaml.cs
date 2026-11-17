namespace ChefRoute.UI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new MainPage())
        {
            Title = "ChefRoute - Coordinación de recetas",
            Width = 1260,
            Height = 800,
            MinimumWidth = 1000,
            MinimumHeight = 700
        };
    }
}
