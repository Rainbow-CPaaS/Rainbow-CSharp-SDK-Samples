using System.Windows;

namespace SDK.WpfApp
{
    public class Spacing
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


        public static double GetHorizontal(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalProperty);
        }

        public static double GetVertical(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalProperty);
        }

        private static void HorizontalChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var space = (double)e.NewValue;
            var obj = (DependencyObject)sender;

            MarginSetter.SetMargin(obj, new Thickness(0, 0, space, 0));
            MarginSetter.SetLastItemMargin(obj, new Thickness(0));
        }

        public static void SetHorizontal(DependencyObject obj, double space)
        {
            obj.SetValue(HorizontalProperty, space);
        }

        public static void SetVertical(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalProperty, value);
        }

        private static void VerticalChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var space = (double)e.NewValue;
            var obj = (DependencyObject)sender;
            MarginSetter.SetMargin(obj, new Thickness(0, 0, 0, space));
            MarginSetter.SetLastItemMargin(obj, new Thickness(0));
        }

        public static readonly DependencyProperty VerticalProperty =
            DependencyProperty.RegisterAttached("Vertical", typeof(double), typeof(Spacing),
                new UIPropertyMetadata(0d, VerticalChangedCallback));

        public static readonly DependencyProperty HorizontalProperty =
            DependencyProperty.RegisterAttached("Horizontal", typeof(double), typeof(Spacing),
                new UIPropertyMetadata(0d, HorizontalChangedCallback));
    }
}