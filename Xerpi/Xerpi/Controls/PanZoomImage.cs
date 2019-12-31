using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private double _maxScale = 2.0;

        private double _zoomStartScale;
        private double _lastTargetScale;
        private double _currentX;
        private double _currentY;

        private DateTime _tapHeardTime;
        private DateTime _lastPinchHeard = DateTime.MinValue;
        private List<double> _lastThreeScaleDecimals = new List<double>(3) { 0, 0, 0 };

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
            }
            else if (e.Status == GestureStatus.Running)
            {
                _lastTargetScale = e.Scale;
                double scaleDecimal = (e.Scale - 1) * .75; // .75 to smooth out zooming a bit

                // Track the positiveness of the three most recent values. If we abruptly get something out of the ordinary,
                // force it into the same positiveness as what we've been seeing.
                // This is to account for what seem to be bogus values coming from the platform when zooming out slowly.
                if (_lastThreeScaleDecimals[0] != 0 && _lastThreeScaleDecimals[1] != 0 && _lastThreeScaleDecimals[2] != 0)
                {
                    int negativeCount = _lastThreeScaleDecimals.Count(x => x <= 0);
                    int positiveCount = _lastThreeScaleDecimals.Count(x => x > 0);
                    scaleDecimal = positiveCount > negativeCount ? Math.Abs(scaleDecimal) : Math.Abs(scaleDecimal) * -1;
                }
                _lastThreeScaleDecimals.RemoveAt(0);
                _lastThreeScaleDecimals.Add(scaleDecimal);

                double current = Scale + scaleDecimal * .75;
                double targetScale = Clamp(current, _minScale * (1 - Overshoot), _maxScale * (1 + Overshoot));

                double relativeX = _currentX / Width;
                // Delta width is multiplication factor that expresses the difference between true width, and currently-scaled width.
                double deltaWidth = Width / (Width * _zoomStartScale);
                double originX = (e.ScaleOrigin.X + relativeX) * deltaWidth;

                double relativeY = _currentY / Height;
                double deltaHeight = Height / (Height * _zoomStartScale);
                double originY = (e.ScaleOrigin.Y + relativeY) * deltaHeight;

                //double targetX = _currentX - (originX * Width) * (targetScale - _zoomStartScale);
                //double targetY = _currentY - (originY * Height) * (targetScale - _zoomStartScale);
                Scale = targetScale;
                // Note: Translation is relative to the top-left of the image: 0,0.
                // Translation coordinates use container (i.e. DIP) coordinates, NOT real pixel coordinates.
                UpdateTranslation(originX, originY);

                Debug.WriteLine($"'Current': {current}, Target: {targetScale}, Scale Decimal: {scaleDecimal}, Raw Scale: {e.Scale}, X: {originX}, Y:{originY}");
                _lastPinchHeard = DateTime.Now;
            }
            else
            {
                _lastPinchHeard = DateTime.Now;
                for (int i = 0; i < 3; i++)
                {
                    _lastThreeScaleDecimals[i] = 0;
                }
                if (Scale > _maxScale)
                {
                    await this.ScaleTo(_maxScale, 250, Easing.SpringOut);
                }
                else if (Scale < _minScale)
                {
                    await this.ScaleTo(_minScale, 250, Easing.SpringOut);
                }

                //Debug.WriteLine($"New scale: {Scale}, TransX: {TranslationX}, TransY: {TranslationY}");
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            var now = DateTime.Now;
            if (now - _lastPinchHeard < TimeSpan.FromMilliseconds(200))
            {
                return; // Ignore panning too soon after a pinch, so we don't teleport the image around after pinching
            }

            if (e.StatusType == GestureStatus.Started)
            {
                _currentX = TranslationX;
                _currentY = TranslationY;
                Debug.WriteLine($"Pan started at X: {TranslationX}, Y: {TranslationY}");
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                if (now - _tapHeardTime <= TimeSpan.FromMilliseconds(200))
                {
                    // Quickzoom mode
                }
                else
                {
                    UpdateTranslation(_currentX + e.TotalX * Scale, _currentY + e.TotalY * Scale);
                    Debug.WriteLine($"Panned to: X: {TranslationX}, Y: {TranslationY}");
                }
            }
        }

        private void UpdateTranslation(double x, double y)
        {
            TranslationX = Clamp(x, (-Width / 2) * Scale, (Width / 2) * Scale);
            TranslationY = Clamp(y, (-Height / 2) * Scale, (Height / 2) * Scale);
            _currentX = TranslationX;
            _currentY = TranslationY;
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
