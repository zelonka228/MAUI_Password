namespace MAUI_PAssword
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell())
            {
                Title = "Паролі",
                Width = 560,
                Height = 800,
                MinimumWidth = 380,
                MinimumHeight = 520,
            };
        }
    }
}
