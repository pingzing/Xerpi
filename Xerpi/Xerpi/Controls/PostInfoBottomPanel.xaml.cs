using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xerpi.Models.API;
using Xerpi.ViewModels;

namespace Xerpi.Controls
{
    public partial class PostInfoBottomPanel : ContentView
    {
        private PanelState _panelState = PanelState.Maximized;
        public const double PanelHeight = 200;
        public const double MinimizedHeight = 30;

        public static BindableProperty IsOpenProperty = BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(PostInfoBottomPanel),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnIsOpenChanged);
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        private static async void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var _this = (PostInfoBottomPanel)bindable;

            if (_this._panelState == PanelState.Toggling)
            {
                return; // In the middle of an animation, ignore anything external property fiddling any caller is trying to do
            }

            bool newIsOpen = (bool)newValue;
            PanelState newState = newIsOpen ? PanelState.Maximized : PanelState.Minimized;
            await _this.SetPanelState(newState);
        }

        public static BindableProperty TagsProperty = BindableProperty.Create(
            nameof(Tags),
            typeof(IEnumerable<ApiTag>),
            typeof(PostInfoBottomPanel));
        public IEnumerable<ApiTag> Tags
        {
            get => (IEnumerable<ApiTag>)GetValue(TagsProperty);
            set => SetValue(TagsProperty, value);
        }

        public static BindableProperty UpvotesProperty = BindableProperty.Create(
            nameof(Upvotes),
            typeof(int),
            typeof(PostInfoBottomPanel),
            defaultValue: 0);

        public int Upvotes
        {
            get => (int)GetValue(UpvotesProperty);
            set => SetValue(UpvotesProperty, value);
        }

        public static BindableProperty ScoreProperty = BindableProperty.Create(
            nameof(Score),
            typeof(int),
            typeof(PostInfoBottomPanel),
            defaultValue: 0);
        public int Score
        {
            get => (int)GetValue(ScoreProperty);
            set => SetValue(ScoreProperty, value);
        }

        public static BindableProperty DownvotesProperty = BindableProperty.Create(
            nameof(Downvotes),
            typeof(int),
            typeof(PostInfoBottomPanel),
            defaultValue: 0);
        public int Downvotes
        {
            get => (int)GetValue(DownvotesProperty);
            set => SetValue(DownvotesProperty, value);
        }

        public static BindableProperty CommentsProperty = BindableProperty.Create(
            nameof(Comments),
            typeof(IEnumerable<CommentViewModel>),
            typeof(PostInfoBottomPanel),
            propertyChanged: CommentsPropertyChanged);
        public IEnumerable<CommentViewModel> Comments
        {
            get => (IEnumerable<CommentViewModel>)GetValue(CommentsProperty);
            set => SetValue(CommentsProperty, value);
        }
        private static void CommentsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var _this = (PostInfoBottomPanel)bindable;
            var oldList = (IEnumerable<CommentViewModel>)oldValue;
            var newList = (IEnumerable<CommentViewModel>)newValue;

            // Null means we're loading.
            if (newList == null)
            {
                // The "Loaded" view will just be empty, so this is fine
                VisualStateManager.GoToState(_this.RootGrid, "Loaded");
                return;
            }
            if (!newList.Any())
            {
                VisualStateManager.GoToState(_this.RootGrid, "Empty");
                return;
            }

            VisualStateManager.GoToState(_this.RootGrid, "Loaded");
        }

        public PostInfoBottomPanel()
        {
            InitializeComponent();
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            // TODO: Implement paging for loading more comments
        }

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                return; // Don't do anythin if we're in the middle of animating.
            }

            var targetState = _panelState == PanelState.Maximized ? PanelState.Minimized : PanelState.Maximized;

            _ = SetPanelState(targetState);
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                // Ignore any user input if we're in the middle of a canned animation
                return;
            }
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    VisualStateManager.GoToState(TopBar, "Highlighted");
                    break;
                case GestureStatus.Running:
                    double targetTranslation = TranslationY + e.TotalY;

                    if (targetTranslation < 0)
                    {
                        return; // Don't allow opening higher than fully-maximized
                    }
                    if (targetTranslation > PanelHeight - MinimizedHeight)
                    {
                        return; // Don't allow hiding the top-bar.
                    }

                    TranslationY = targetTranslation;
                    break;
                case GestureStatus.Completed:
                    VisualStateManager.GoToState(TopBar, "Unhighlighted");
                    if (TranslationY > PanelHeight / 2) // The panel is closed or almost closed
                    {
                        _ = SetPanelState(PanelState.Minimized);
                    }
                    else
                    {
                        _ = SetPanelState(PanelState.Maximized);
                    }
                    break;
            }
        }

        private async Task SetPanelState(PanelState newPanelState)
        {
            if (newPanelState == PanelState.Toggling)
            {
                return; // Invalid. Ignore.
            }

            if (newPanelState == PanelState.Minimized)
            {
                _panelState = PanelState.Toggling;
                await this.TranslateTo(X, 170, 200, Easing.CubicIn);
                IsOpen = false;
                _panelState = PanelState.Minimized;
            }
            else if (newPanelState == PanelState.Maximized)
            {
                _panelState = PanelState.Toggling;
                await this.TranslateTo(X, 0, 200, Easing.CubicOut);
                IsOpen = true;
                _panelState = PanelState.Maximized;
            }
        }

        private void TopBar_Down(object sender, MR.Gestures.DownUpEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                // Ignore any user input if we're in the middle of a canned animation
                return;
            }
            VisualStateManager.GoToState(TopBar, "Highlighted");
        }

        private void TopBar_Up(object sender, MR.Gestures.DownUpEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                // Ignore any user input if we're in the middle of a canned animation
                return;
            }
            VisualStateManager.GoToState(TopBar, "Unhighlighted");
        }

        private void TopBar_Tapping(object sender, MR.Gestures.TapEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                return; // Don't do anythin if we're in the middle of animating.
            }

            var targetState = _panelState == PanelState.Maximized ? PanelState.Minimized : PanelState.Maximized;

            _ = SetPanelState(targetState);
        }

        private void TopBar_Panning(object sender, MR.Gestures.PanEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                // Ignore any user input if we're in the middle of a canned animation
                return;
            }

            double targetTranslation = TranslationY + e.TotalDistance.Y;

            if (targetTranslation < 0)
            {
                return; // Don't allow opening higher than fully-maximized
            }
            if (targetTranslation > PanelHeight - MinimizedHeight)
            {
                return; // Don't allow hiding the top-bar.
            }

            TranslationY = targetTranslation;
        }

        private void TopBar_Panned(object sender, MR.Gestures.PanEventArgs e)
        {
            if (_panelState == PanelState.Toggling)
            {
                // Ignore any user input if we're in the middle of a canned animation
                return;
            }

            if (TranslationY > PanelHeight / 2) // The panel is closed or almost closed
            {
                _ = SetPanelState(PanelState.Minimized);
            }
            else
            {
                _ = SetPanelState(PanelState.Maximized);
            }
        }
    }

    public enum PanelState
    {
        Minimized,
        Toggling,
        Maximized
    }
}