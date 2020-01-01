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
        private double _maxScale = 1.5;

        private double _zoomStartScale;
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
                double targetScale;
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
                double rawScaleDecimal = (e.Scale - 1);
                double adjustedScaleDecimal = rawScaleDecimal;

                // Track the positiveness of the three most recent values. If we abruptly get something out of the ordinary,
                // force it into the same positiveness as what we've been seeing.
                // This is to account for what seem to be bogus values coming from the platform when zooming out slowly.
                if (_lastThreeScaleDecimals[0] != 0 && _lastThreeScaleDecimals[1] != 0 && _lastThreeScaleDecimals[2] != 0)
                {
                    int negativeCount = _lastThreeScaleDecimals.Count(x => x <= 0);
                    int positiveCount = _lastThreeScaleDecimals.Count(x => x > 0);
                    adjustedScaleDecimal = positiveCount > negativeCount ? Math.Abs(rawScaleDecimal) : Math.Abs(rawScaleDecimal) * -1;
                }
                _lastThreeScaleDecimals.RemoveAt(0);
                _lastThreeScaleDecimals.Add(rawScaleDecimal);

                double current = Scale + adjustedScaleDecimal * .5625; // Smooth out zooming motion a bit with the .5625
                double targetScale = Clamp(current, _minScale * (1 - Overshoot), _maxScale * (1 + Overshoot));

                string debugInfo = "";
                // Skip translating if we're banging against the min or max thresholds.
                if (!(targetScale <= _minScale || targetScale >= _maxScale))
                {
                    double relativeX = _currentX / Width; // ScaleOrigin comes in relative [0.0, 1.0] coordinates, so get current Translation in relative coords
                    double deltaWidth = Width / (Width * _zoomStartScale); // Multiplication factor that expresses the difference between true width, and currently-scaled width.                    
                    // The X-coordinate of the pinch's anchor point, in terms of the image's current, scaled width.
                    double pinchX = (e.ScaleOrigin.X + relativeX) * Width * targetScale;

                    // See above.
                    double relativeY = _currentY / Height;
                    double deltaHeight = Height / (Height * _zoomStartScale);
                    double pinchY = (e.ScaleOrigin.Y + relativeY) * Height * targetScale;

                    // We multiple by '1 / e.Scale' instead of just 'e.Scale' because as the image INCREASES in size, the anchor point
                    // coordinates need to DECREASE (and vice-versa)

                    // TODO: These aren't large enough. I think we're calculating "change in image size from new scale" incorrectly.
                    double targetX = TranslationX + (pinchX * 1 / e.Scale - pinchX);
                    double targetY = TranslationY + (pinchY * 1 / e.Scale - pinchY);
                    // Note: Translation is relative to the top-left of the image: 0,0.
                    // Translation coordinates use container (i.e. DIP) coordinates, NOT real pixel coordinates.
                    UpdateTranslation(targetX, targetY);
                    //debugInfo = $"PinchTranslate to: X: {targetX}, Y: {targetY}. Pinch coords: X: {pinchX}, Y: {pinchY}. DeltaHeight: {deltaHeight}, DeltaWidth: {deltaWidth}, StartScale: {_zoomStartScale}";
                }

                Debug.WriteLine($"Scaling to: {targetScale}. {debugInfo}");
                Scale = targetScale;

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
            double viewportWidth = (Parent as View)!.Width;
            double viewportHeight = (Parent as View)!.Height;
            TranslationX = Clamp(x, (-Width * Scale) + viewportWidth / 2, (Width * _minScale / 2));
            TranslationY = Clamp(y, (-Height * Scale) + viewportHeight / 2, (Height * _minScale / 2));
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
