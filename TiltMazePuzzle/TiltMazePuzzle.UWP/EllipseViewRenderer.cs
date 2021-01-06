using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(Xamarin.FormsBook.Platform.EllipseView), 
                          typeof(Xamarin.FormsBook.Platform.WinRT.EllipseViewRenderer))]

namespace Xamarin.FormsBook.Platform.WinRT
{
    public class EllipseViewRenderer : ViewRenderer<EllipseView, Ellipse>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<EllipseView> args)
        {
            base.OnElementChanged(args);

            // TEST IT
            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    SetNativeControl(new Ellipse());
                }
                SetColor();
            }
        }

        protected override void OnElementPropertyChanged(object sender, 
                                                         PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == EllipseView.ColorProperty.PropertyName)
            {
                SetColor();
            }
        }

        void SetColor()
        {
            UWPSetColor(Element.Color);
        }

        void UWPSetColor(Xamarin.Forms.Color color)
        {
            if (Element.Color == Xamarin.Forms.Color.Default)
            {
                Control.Fill = null;
            }
            else
            {
                global::Windows.UI.Color winColor =
                    global::Windows.UI.Color.FromArgb((byte)(color.A * 255),
                                                      (byte)(color.R * 255),
                                                      (byte)(color.G * 255),
                                                      (byte)(color.B * 255));

                Control.Fill = new Windows.UI.Xaml.Media.SolidColorBrush(winColor);
            }
        }
    }
}