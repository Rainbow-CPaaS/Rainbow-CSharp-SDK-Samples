using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApplication.Helpers
{
    public class ColorInterpolator
    {
        // Based on this code: https://gist.github.com/peterk87/5453080

        delegate double ComponentSelector(Color color);
        static ComponentSelector _redSelector = color => color.R;
        static ComponentSelector _greenSelector = color => color.G;
        static ComponentSelector _blueSelector = color => color.B;
        static ComponentSelector _alphaSelector = color => color.A;

        public static Color InterpolateBetween(
            Color endPoint1,
            Color endPoint2,
            double lambda)
        {
            if (lambda < 0 || lambda > 1)
            {
                throw new ArgumentOutOfRangeException("lambda");
            }
            Color color = Color.FromRgba(
                InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _alphaSelector)
            );

            return color;
        }

        static double InterpolateComponent(
            Color endPoint1,
            Color endPoint2,
            double lambda,
            ComponentSelector selector)
        {
            return (double)(selector(endPoint1)
                + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }
}
