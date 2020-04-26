using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xerpi.Extensions;

namespace Xerpi.Controls
{

    public partial class ImageViewer : ContentView
    {
        private double _maxX = 0;
        private double _maxY = 0;

        private double _minScale;
        private double _maxScale = 1.5;

        private DateTime _tapHeardTime;
        private DateTime _lastPinchHeard = DateTime.MinValue;
        private DateTime _lastPanStartTime;
        private List<double> _lastThreeScaleDecimals = new List<double>(3) { 0, 0, 0 };

        public static BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source),
            typeof(ImageSource),
            typeof(ImageViewer),
            propertyChanged: SourceChanged);

        [Xamarin.Forms.TypeConverter(typeof(FFImageLoading.Forms.ImageSourceConverter))]
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        private static void SourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ImageViewer _this = (ImageViewer)bindable;
            ImageSource newSource = (ImageSource)newValue;
            _this.CachedImage.Cancel();
            _this.CachedImage.Source = null;

            // If we're not visible, don't load an image. The IsVisible changed handler
            // down below will handle setting the source once the viewer is reopened.
            if (_this.IsVisible)
            {
                _this.CachedImage.Source = newSource;
                _this.ResetTranslationAndScale();
            }
        }

        public static BindableProperty CacheKeyFactoryProperty = BindableProperty.Create(
            nameof(CacheKeyFactory),
            typeof(ICacheKeyFactory),
            typeof(ImageViewer),
            defaultValue: null,
            propertyChanged: CacheKeyFactoryChanged);
        public ICacheKeyFactory CacheKeyFactory
        {
            get => (ICacheKeyFactory)GetValue(CacheKeyFactoryProperty);
            set => SetValue(CacheKeyFactoryProperty, value);
        }
        private static void CacheKeyFactoryChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ImageViewer _this = (ImageViewer)bindable;
            if (newValue == null)
            {
                _this.CachedImage.CacheKeyFactory = null;
                return;
            }

            _this.CachedImage.CacheKeyFactory = (ICacheKeyFactory)newValue;
        }

        public static BindableProperty ImageHeightProperty = BindableProperty.Create(
            nameof(ImageHeight),
            typeof(double),
            typeof(ImageViewer));
        public double ImageHeight
        {
            get => (double)GetValue(ImageHeightProperty);
            set => SetValue(ImageHeightProperty, value);
        }

        public static BindableProperty ImageWidthProperty = BindableProperty.Create(
            nameof(ImageWidth),
            typeof(double),
            typeof(ImageViewer));
        public double ImageWidth
        {
            get => (double)GetValue(ImageWidthProperty);
            set => SetValue(ImageWidthProperty, value);
        }

        public ImageViewer()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(IsVisible))
            {
                if (IsVisible)
                {
                    CachedImage.Source = Source;
                    ResetTranslationAndScale();
                }
                else
                {
                    CachedImage.Cancel();
                    CachedImage.Source = null;
                }
            }
            base.OnPropertyChanged(propertyName);
        }

        // Recenter and scale image to fit screen (or set to 1.0 scale, whichever is smaller)
        private void ResetTranslationAndScale()
        {
            // Note: Properties should change in the order they're declared. 
            // Hopefully, this means ImageHeight and ImageWidth have already been updated.            
            (double widthConstraint, double heightConstraint) = GetDimensionConstraints();

            double targetScale;
            if (ImageWidth - widthConstraint > ImageHeight - heightConstraint)
            {
                // Greater diff in needed vs given Width
                targetScale = widthConstraint / ImageWidth;
            }
            else
            {
                // Greater need in needed vs given Height
                targetScale = heightConstraint / ImageHeight;
            }

            targetScale = Math.Min(targetScale, 1.0);

            CachedImage.Scale = targetScale;
            _minScale = targetScale;

            var (newX, newY) = CenterImage();
            CachedImage.TranslationX = newX;
            CachedImage.TranslationY = newY;
        }

        // The very first time we open this control, Height and Width won't have been calculated yet.
        // So we need to approximate based on the Parent's dimensions, minus our own margins.
        // TODO: Deal with padding?
        private (double widthConstraint, double heightConstraint) GetDimensionConstraints()
        {
            if (Width != -1 && Height != -1)
            {
                // Great, we can do the normal thing
                return (Width, Height);
            }
            else
            {
                double parentHeight = (Parent as View)!.Height;
                double height = parentHeight - Margin.Top - Margin.Bottom;

                double parentWidth = (Parent as View)!.Width;
                double width = parentWidth - Margin.Left - Margin.Right;

                return (width, height);
            }
        }

        private (double newX, double newY) CenterImage()
        {
            double widthConstaint = Width;
            double heightConstraint = Height;

            double targetX = 0;
            double scaledWidth = ImageWidth * CachedImage.Scale;
            if (scaledWidth < widthConstaint)
            {
                double extraWidth = widthConstaint - scaledWidth;
                targetX = extraWidth / 2;
            }

            double targetY = 0;
            double scaledHeight = ImageHeight * CachedImage.Scale;
            if (scaledHeight < heightConstraint)
            {
                double extraHeight = heightConstraint - scaledHeight;
                targetY = extraHeight / 2;
            }

            _maxX = targetX;
            _maxY = targetY;
            return (_maxX, _maxY);
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
            {
                double rawScaleDecimal = Clamp((e.Scale - 1), -0.15, 1.15);
                Debug.WriteLine($"Raw scale decimal: {rawScaleDecimal}");
                double adjustedScaleDecimal = rawScaleDecimal;

                // Track the positiveness of the three most recent values. If we abruptly get something out of the ordinary,
                // force it into the same positiveness as what we've been seeing.
                // This is to account for what seem to be bogus values coming from the platform when zooming out slowly.
                // TODO: Rework this--we still get really wild values, especially at the start of the gesture.
                if (_lastThreeScaleDecimals[0] != 0 && _lastThreeScaleDecimals[1] != 0 && _lastThreeScaleDecimals[2] != 0)
                {
                    int negativeCount = _lastThreeScaleDecimals.Count(x => x <= 0);
                    int positiveCount = _lastThreeScaleDecimals.Count(x => x > 0);
                    adjustedScaleDecimal = positiveCount > negativeCount ? Math.Abs(rawScaleDecimal) : Math.Abs(rawScaleDecimal) * -1;
                }
                _lastThreeScaleDecimals.RemoveAt(0);
                _lastThreeScaleDecimals.Add(rawScaleDecimal);

                double current = CachedImage.Scale + adjustedScaleDecimal * .75; // Smooth out zooming motion a bit with the .75
                double targetScale = Clamp(current, _minScale, _maxScale);

                // TODO: If it's worth the perf gain, don't Translate if we were at the min or max scale last frame
                // Calculate change in translation based on zoom change
                double actualWidth = CachedImage.Width * CachedImage.Scale;
                double projectedNextWidth = CachedImage.Width * targetScale;
                double changeInWidth = -(projectedNextWidth - actualWidth);

                double actualHeight = CachedImage.Height * CachedImage.Scale;
                double projectedNextHeight = CachedImage.Height * targetScale;
                double changeInHeight = -(projectedNextHeight - actualHeight);

                double targetX = CachedImage.TranslationX + (changeInWidth * e.ScaleOrigin.X);
                double targetY = CachedImage.TranslationY + (changeInHeight * e.ScaleOrigin.Y);
                // Note: Translation is relative to the top-left of the image: 0,0.
                // Translation coordinates use container (i.e. DIP) coordinates, NOT real pixel coordinates.
                UpdateTranslation(targetX, targetY);

                CachedImage.Scale = targetScale;

                _lastPinchHeard = DateTime.Now;
            }
            else
            {
                _lastPinchHeard = DateTime.Now;
                for (int i = 0; i < 3; i++)
                {
                    _lastThreeScaleDecimals[i] = 0;
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
                UpdateTranslation(CachedImage.TranslationX + e.TotalX * CachedImage.Scale,
                    CachedImage.TranslationY + e.TotalY * CachedImage.Scale);
            }
        }

        private void UpdateTranslation(double x, double y)
        {
            double viewportWidth = Width;
            double viewportHeight = Height;

            double overdrawX = Math.Max((ImageWidth * CachedImage.Scale) - viewportWidth, 0);
            double overdrawY = Math.Max((ImageHeight * CachedImage.Scale) - viewportHeight, 0);

            if (overdrawX <= 0)
            {
                var (newX, _) = CenterImage();
                x = newX;
            }
            if (overdrawY <= 0)
            {
                var (_, newY) = CenterImage();
                y = newY;
            }

            double minX = -overdrawX;
            double minY = -overdrawY;

            CachedImage.TranslationX = Clamp(x, minX, _maxX);
            CachedImage.TranslationY = Clamp(y, minY, _maxY);
            Debug.WriteLine($"Translating to: X: {CachedImage.TranslationX}, Y: {CachedImage.TranslationY}. Overdraw - X: {overdrawX}, y: {overdrawY}");
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