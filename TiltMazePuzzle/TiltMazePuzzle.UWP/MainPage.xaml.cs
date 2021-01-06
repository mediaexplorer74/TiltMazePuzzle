using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TiltMazePuzzle.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Настроечки авторотации экрана выставлены на режим для планшета Surface Pro : Landscape (гориз.) 
            // DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape; // for Desktop
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait; // for Mobile

            LoadApplication(new TiltMazePuzzle.App());
        }
    }
}
