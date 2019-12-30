using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace Xerpi.Controls
{
    public class PanZoomImage : CachedImage
    {
        private const double Overshoot = 0.15;

        private bool _initialScaleSet = false;

        private double _minScale;
        private double _maxScale = 4.0;

        private double _zoomStartScale;
        private double _lastTargetScale;
        private double _lastX;
        private double _lastY;

        private DateTime _tapHeardTime;

        public PanZoomImage()
        {
            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += OnPinchUpdated;
            GestureRecognizers.Add(pinch);

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(pan);

            var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tap.Tapped += OnTapped;
            GestureRecognizers.Add(tap);

            Scale = 1.0;
            TranslationX = 0;
            TranslationY = 0;
            AnchorX = 0;
            AnchorY = 0;
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (!_initialScaleSet)
            {
                double targetScale = 1.0;
                if (Math.Abs(widthConstraint - WidthRequest) > Math.Abs(heightConstraint - HeightRequest))
                {
                    // Greater diff in needed vs given WIDTH
                    targetScale = widthConstraint / WidthRequest;
                }
                else
                {
                    // Greater diff in needed vs given HEIGHT
                    targetScale = heightConstraint / HeightRequest;
                }
                targetScale = Math.Min(targetScale, 1.0); // If the image's full size is so small we'd have to scale it up, just leave it as-is.
                Scale = targetScale;
                _minScale = targetScale;
                _initialScaleSet = true;
            }

            return base.OnMeasure(widthConstraint, heightConstraint);
        }

        private async void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                _zoomStartScale = Scale;
                _lastTargetScale = e.Scale;
                AnchorX = e.ScaleOrigin.X * Scale;
                AnchorY = e.ScaleOrigin.Y * Scale;
            }
            else if (e.Status == GestureStatus.Running)
            {
                if (e.Scale < 0 || Math.Abs(_lastTargetScale - e.Scale) > (_lastTargetScale * 1.3) - _lastTargetScale)
                {
                    // If new scale value varies too wildly, throw it away
                    return;
                }
                _lastTargetScale = e.Scale;
                double current = Scale + (e.Scale - 1) * _zoomStartScale;
                double targetScale = Clamp(current, _minScale * (1 - Overshoot), _maxScale * (1 + Overshoot));
                Debug.WriteLine($"'Current': {current}, Target: {targetScale}, e.Scale - 1: {e.Scale - 1}");
                Scale = targetScale;
            }
            else
            {
                if (Scale > _maxScale)
                {
                    await this.ScaleTo(_maxScale, 250, Easing.SpringOut);
                }
                else if (Scale < _minScale)
                {
                    await this.ScaleTo(_minScale, 250, Easing.SpringOut);
                }

                Debug.WriteLine($"New scale: {Scale}, TransX: {TranslationX}, TransY: {TranslationY}");
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Started)
            {

            }
            else if (e.StatusType == GestureStatus.Running)
            {
                var now = DateTime.Now;
                if (now - _tapHeardTime <= TimeSpan.FromMilliseconds(200))
                {
                    // Quickzoom mode
                }
                else
                {
                    double currXAnchor = _lastX + (e.TotalX * Scale / WidthRequest);
                    double currYAnchor = _lastY + (e.TotalY * Scale / HeightRequest);
                    _lastX = AnchorX = currXAnchor;
                    _lastY = AnchorY = currYAnchor;
                    Debug.WriteLine($"New Anchor - X: {AnchorX}, Y: {AnchorY}");
                }
            }
        }

        private void OnTapped(object sender, EventArgs e)
        {
            _tapHeardTime = DateTime.Now;
            // Possible TODO: Listen to see if a) This is the second tap within 
            // ~200ms AND we haven't heard a PanStart within that timeout.
            // If so, scale us back to our _minScale.
        }

        private T Clamp<T>(T value, T minimum, T maximum) where T : IComparable
        {
            if (value.CompareTo(minimum) < 0)
            {
                return minimum;
            }
            else if (value.CompareTo(maximum) > 0)
            {
                return maximum;
            }
            else
            {
                return value;
            }
        }
    }
}
