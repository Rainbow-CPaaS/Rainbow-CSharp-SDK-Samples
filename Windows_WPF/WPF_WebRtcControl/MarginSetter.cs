using System.Windows;
using System.Windows.Controls;

namespace SDK.WpfApp
{
    public class MarginSetter
    {
        /*
         CODE BASED FROM HERE:
          - https://stackoverflow.com/questions/932510/how-do-i-space-out-the-child-elements-of-a-stackpanel
          - https://gist.github.com/angularsen/90040fb174f71c5ab3ad

         EXAMPLE:
            <StackPanel Orientation="Horizontal" foo:Spacing.Horizontal="5">
                <Button>Button 1</Button>
                <Button>Button 2</Button>
            </StackPanel>

            <StackPanel Orientation="Vertical" foo:Spacing.Vertical="5">
                <Button>Button 1</Button>
                <Button>Button 2</Button>
            </StackPanel>

            <StackPanel Orientation="Vertical" foo:MarginSetter.Margin="5" foo:MarginSetter.LastItemMargin="5 0">
                <Button>Button 1</Button>
                <Button>Button 2</Button>
            </StackPanel>
         */

        private static Thickness GetLastItemMargin(Panel obj)
        {
            return (Thickness)obj.GetValue(LastItemMarginProperty);
        }

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        private static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Make sure this is put on a panel
            var panel = sender as Panel;
            if (panel == null) return;

            // Avoid duplicate registrations
            panel.Loaded -= OnPanelLoaded;
            panel.Loaded += OnPanelLoaded;

            if (panel.IsLoaded)
            {
                OnPanelLoaded(panel, null);
            }
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            var panel = (Panel)sender;

            // Go over the children and set margin for them:
            for (var i = 0; i < panel.Children.Count; i++)
            {
                UIElement child = panel.Children[i];
                var fe = child as FrameworkElement;
                if (fe == null) continue;

                bool isLastItem = i == panel.Children.Count - 1;
                fe.Margin = isLastItem ? GetLastItemMargin(panel) : GetMargin(panel);
            }
        }

        public static void SetLastItemMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(LastItemMarginProperty, value);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter),
                new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        public static readonly DependencyProperty LastItemMarginProperty =
            DependencyProperty.RegisterAttached("LastItemMargin", typeof(Thickness), typeof(MarginSetter),
                new UIPropertyMetadata(new Thickness(), MarginChangedCallback));
    }
}
