using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace Xerpi.Controls
{
    public class PanZoomImage : CachedImage
    {
        private const double Overshoot = 0.15;

        private double _minScale;
        private double _maxScale = 1.5;

        private DateTime _tapHeardTime;
        private DateTime _lastPinchHeard = DateTime.MinValue;
        private DateTime _lastPanStartTime;
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

            AnchorX = 0;
            AnchorY = 0;

            this.PropertyChanged += PanZoomImage_PropertyChanged;
        }

        bool _initialMeasurePerformed = false;
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            ResetSize(widthConstraint, heightConstraint);
            _initialMeasurePerformed = true;
            return base.OnMeasure(widthConstraint, heightConstraint);
        }

        private void PanZoomImage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.IsVisible) && IsVisible)
            {
                // ForceLayout does nothing unless the element is actually visible.
                (this.Parent as Layout)!.ForceLayout();
            }

            // Order matters here. Properties change in the order they're declared...
            if (!(e.PropertyName == nameof(this.Source)))
            {
                return;
            }

            TranslationX = 0;
            TranslationY = 0;

            // ...so we should find a way to wait for HeightRequest, WidthRequest and Source to all change without 
            // forcing consumers to order their properties correctly.
            if (_initialMeasurePerformed)
            {
                ResetSize((this.Parent as View)!.Width, (this.Parent as View)!.Height);
            }
        }

        private void ResetSize(double widthConstraint, double heightConstraint)
        {
            double targetScale;
            if (WidthRequest - widthConstraint > HeightRequest - heightConstraint)
            {
                // Greater diff in needed vs given WIDTH
                targetScale = widthConstraint / WidthRequest;
            }
            else
            {
                // Greater diff in needed vs given HEIGHT
                targetScale = heightConstraint / HeightRequest;
            }

            // If LayoutBounds are never reset, the image takes the dimensions of the first image loaded, then never changes.
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(0, 0, WidthRequest, HeightRequest));
            targetScale = Math.Min(targetScale, 1.0); // If the image's full size is so small we'd have to scale it up, just leave it as-is.
            Scale = targetScale;
            _minScale = targetScale;
        }

        private async void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
            {
                double rawScaleDecimal = Clamp((e.Scale - 1), -0.15, 1.15);
                Debug.WriteLine($"Raw scale decimal: {rawScaleDecimal}");
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

                double current = Scale + adjustedScaleDecimal * .75; // Smooth out zooming motion a bit with the .75
                double targetScale = Clamp(current, _minScale * (1 - Overshoot), _maxScale * (1 + Overshoot));

                // Skip translating if we're banging against the min or max thresholds.
                if (!(targetScale <= _minScale || targetScale >= _maxScale))
                {
                    double actualWidth = Width * Scale;
                    double projectedNextWidth = Width * targetScale;
                    double changeInWidth = -(projectedNextWidth - actualWidth);

                    double actualHeight = Height * Scale;
                    double projectedNextHeight = Height * targetScale;
                    double changeInHeight = -(projectedNextHeight - actualHeight);

                    double targetX = TranslationX + (changeInWidth * e.ScaleOrigin.X);
                    double targetY = TranslationY + (changeInHeight * e.ScaleOrigin.Y);
                    // Note: Translation is relative to the top-left of the image: 0,0.
                    // Translation coordinates use container (i.e. DIP) coordinates, NOT real pixel coordinates.
                    UpdateTranslation(targetX, targetY);
                }

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
                _lastPanStartTime = DateTime.Now;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                UpdateTranslation(TranslationX + e.TotalX * Scale, TranslationY + e.TotalY * Scale);
            }
        }

        private void UpdateTranslation(double x, double y)
        {
            double viewportWidth = (Parent as View)!.Width;
            double viewportHeight = (Parent as View)!.Height;
            TranslationX = Clamp(x, (-Width * Scale) + viewportWidth / 2, (Width * _minScale / 2));
            TranslationY = Clamp(y, (-Height * Scale) + viewportHeight / 2, (Height * _minScale / 2));
        }

        private void OnTapped(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (now - _tapHeardTime <= TimeSpan.FromMilliseconds(200)
                && now - _lastPanStartTime > TimeSpan.FromMilliseconds(200))
            {
                // Commmenting this out for now because it seems like the Android pinch handler actually implements quickzoom.
                // TODO: What about the other platforms?

                // This is a double-tap.
                //if (Scale <= _minScale)
                //{
                //    this.ScaleTo(1, 250, Easing.CubicOut);
                //    // Argh, why no tap location x.x. Cannot translate to smart place.
                //    this.TranslateTo(0, 0, 250, Easing.CubicOut);
                //}
                //else
                //{
                //    this.ScaleTo(_minScale, 250, Easing.CubicOut);
                //    // Argh, why no tap location x.x. Cannot translate to smart place.
                //    this.TranslateTo(0, 0, 250, Easing.CubicOut);
                //}
            }
            else
            {
                // Just a single-tap. 
            }

            _tapHeardTime = DateTime.Now;
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
