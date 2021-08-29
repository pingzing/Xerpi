using FFImageLoading.Forms;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

namespace Xerpi.Controls
{

    public partial class ImageViewer : ContentView
    {
        private double _maxX = 0;
        private double _maxY = 0;

        double _lastPanX = 0;
        double _lastPanY = 0;

        private double _minScale; // Based on image dimensions, maxes out at 1.0.
        private double _maxScale = 1.5;

        private DateTime _tapHeardTime;
        private DateTime _lastPinchHeard = DateTime.MinValue;
        private DateTime _lastPanStartTime;

        public static BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source),
            typeof(ImageSource),
            typeof(ImageViewer),
            propertyChanged: SourceChanged);

        [TypeConverter(typeof(FFImageLoading.Forms.ImageSourceConverter))]
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

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
            (double widthConstraint, double heightConstraint) = GetDimensionConstraints();

            double targetScale;
            if (ImageWidth - widthConstraint > ImageHeight - heightConstraint)
            {
                // Greater diff in needed vs given Width
                targetScale = widthConstraint / ImageWidth;
            }
            else
            {
                // Greater diff in needed vs given Height
                targetScale = heightConstraint / ImageHeight;
            }

            targetScale = Math.Min(targetScale, 1.0);

            CachedImage.Scale = targetScale;
            _minScale = targetScale;

            (double newX, double newY) = GetMaxTranslations();
            _maxX = newX;
            _maxY = newY;

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

        private (double newX, double newY) GetMaxTranslations()
        {
            (double widthConstraint, double heightConstraint) = GetDimensionConstraints();

            double targetX = 0;
            double scaledWidth = ImageWidth * CachedImage.Scale;
            if (scaledWidth < widthConstraint)
            {
                double extraWidth = widthConstraint - scaledWidth;
                targetX = extraWidth / 2;
            }

            double targetY = 0;
            double scaledHeight = ImageHeight * CachedImage.Scale;
            if (scaledHeight < heightConstraint)
            {
                double extraHeight = heightConstraint - scaledHeight;
                targetY = extraHeight / 2;
            }

            return (targetX, targetY);
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
            {
                double scaleDecimal = Clamp((e.Scale - 1), -0.10, 0.10); // Anything more than 10% per frame is a little much. Even 10% is a little much...
                scaleDecimal = scaleDecimal * CachedImage.Scale; // Attempt to normalize scaling slightly
                double current = CachedImage.Scale + scaleDecimal;
                double targetScale = Clamp(current, _minScale, _maxScale);
                Debug.WriteLine($"scaleDecimal: {scaleDecimal}, Target scale: {targetScale}");

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
                UpdateTranslation(targetX, targetY);

                CachedImage.Scale = targetScale;

                _lastPinchHeard = DateTime.Now;
            }
            else
            {
                _lastPinchHeard = DateTime.Now;
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
                double diffX = e.TotalX - _lastPanX;
                double diffY = e.TotalY - _lastPanY;

                _lastPanX = e.TotalX;
                _lastPanY = e.TotalY;

                UpdateTranslation(CachedImage.TranslationX + diffX,
                    CachedImage.TranslationY + diffY);
            }
            else if (e.StatusType == GestureStatus.Canceled || e.StatusType == GestureStatus.Completed)
            {
                _lastPanX = 0;
                _lastPanY = 0;
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
                var (newX, _) = GetMaxTranslations();
                x = newX;
            }
            if (overdrawY <= 0)
            {
                var (_, newY) = GetMaxTranslations();
                y = newY;
            }

            double minX = -overdrawX;
            double minY = -overdrawY;

            CachedImage.TranslationX = Clamp(x, minX, _maxX);
            CachedImage.TranslationY = Clamp(y, minY, _maxY);
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