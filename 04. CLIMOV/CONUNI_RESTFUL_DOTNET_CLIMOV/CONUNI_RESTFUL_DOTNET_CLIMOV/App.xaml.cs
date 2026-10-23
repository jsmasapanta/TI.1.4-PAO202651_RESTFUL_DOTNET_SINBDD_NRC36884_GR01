using ConversorUnidades.Movil.Vista;

namespace ConversorUnidades.Movil;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new LoginPage());
	}
}
