using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace Armourbl_CalculatorEC
{
    public partial class App : Application
    {
        public static MainPage MainPageInstance { get; private set; }

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            MainPageInstance = MainPage as MainPage;
        }
        protected override void OnStart()
        {
            // Other app startup code...

            // Subscribe to the theme changed event
            App.Current.RequestedThemeChanged += Current_RequestedThemeChanged;
        }

        private void Current_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            MainPage mainPage = (MainPage)MainPage;
            mainPage.UpdateTheme(e.RequestedTheme);
        }

        protected override void OnSleep()
        {
            // If MainPageInstance is not null, save the current result to the clipboard and reset the calculator
            if (MainPageInstance != null)
            {
                var resultText = MainPageInstance.GetResultText();
                if (!string.IsNullOrEmpty(resultText))
                {
                    Clipboard.SetTextAsync(resultText);
                }

                MainPageInstance.ResetCalculator();
            }
        }

        protected override void OnResume()
        {
        }
    }
}