using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HikariNekoparaPatcher.Controls
{
    [TemplatePart(Name = "PART_Decoration", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_Content", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_IconButton", Type = typeof(Button))]
    public class BlurredDecorationWindow : Window
    {
        #region DependencyProperties

        public static readonly DependencyProperty BackgroundImageProperty = DependencyProperty.Register(
            "BackgroundImage", typeof(ImageSource), typeof(BlurredDecorationWindow), new PropertyMetadata(default(ImageSource)));

        public ImageSource BackgroundImage
        {
            get { return (ImageSource)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        public static readonly DependencyProperty DecorationHeightProperty = DependencyProperty.Register(
            "DecorationHeight", typeof(double), typeof(BlurredDecorationWindow), new PropertyMetadata(20.0));

        public double DecorationHeight
        {
            get { return (double)GetValue(DecorationHeightProperty); }
            set { SetValue(DecorationHeightProperty, value); }
        }

        public static readonly DependencyProperty IconToolTipProperty = DependencyProperty.Register(
            "IconToolTip", typeof(string), typeof(BlurredDecorationWindow), new PropertyMetadata(default(string)));

        public string IconToolTip
        {
            get { return (string)GetValue(IconToolTipProperty); }
            set { SetValue(IconToolTipProperty, value); }
        }

        public static readonly DependencyProperty IconCommandProperty = DependencyProperty.Register(
            "IconCommand", typeof(ICommand), typeof(BlurredDecorationWindow), new PropertyMetadata(default(ICommand)));

        public ICommand IconCommand
        {
            get { return (ICommand)GetValue(IconCommandProperty); }
            set { SetValue(IconCommandProperty, value); }
        }

        #endregion

        #region Constructors

        static BlurredDecorationWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BlurredDecorationWindow), new FrameworkPropertyMetadata(typeof(BlurredDecorationWindow)));
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElement decoration = GetTemplateChild("PART_Decoration") as FrameworkElement;
            if (decoration != null)
                decoration.MouseLeftButtonDown += (sender, args) => DragMove();

            Button closeButton = GetTemplateChild("PART_CloseButton") as Button;
            if (closeButton != null)
                closeButton.Click += (sender, args) => Application.Current.Shutdown();

            Button minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.Click += (sender, args) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

            Button iconButton = GetTemplateChild("PART_IconButton") as Button;
            if (iconButton != null)
                iconButton.Click += (sender, args) => IconCommand?.Execute(null);
        }

        #endregion
    }
}
