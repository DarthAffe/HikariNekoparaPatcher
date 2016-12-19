using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HikariNekoparaPatcher.Controls
{
    public class BlurredText : Control
    {
        #region Properties & Fields
        // ReSharper disable InconsistentNaming

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(BlurredText), new PropertyMetadata(default(string)));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register(
            "TextBrush", typeof(Brush), typeof(BlurredText), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            set { SetValue(TextBrushProperty, value); }
        }

        public static readonly DependencyProperty BlurBrushProperty = DependencyProperty.Register(
            "BlurBrush", typeof(Brush), typeof(BlurredText), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
        public Brush BlurBrush
        {
            get { return (Brush)GetValue(BlurBrushProperty); }
            set { SetValue(BlurBrushProperty, value); }
        }

        // ReSharper restore InconsistentNaming
        #endregion
    }
}
