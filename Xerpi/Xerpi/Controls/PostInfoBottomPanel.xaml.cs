using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xerpi.Models.API;
using Xerpi.ViewModels;

namespace Xerpi.Controls
{
    public partial class PostInfoBottomPanel : ContentView
    {
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

            // null means "not loaded yet"
            if (newList == null)
            {
                _this.CommentsLoadingPanel.IsVisible = true;
                _this.CommentsPanel.IsVisible = false;
                return;
            }

            // Non-null, but empty, means loaded, but no comments
            if (newList?.Any() != true)
            {
                // TODO: Have a third state that says "No comments"?
                _this.CommentsLoadingPanel.IsVisible = false;
                _this.CommentsPanel.IsVisible = false;
                return;
            }

            _this.CommentsLoadingPanel.IsVisible = false;
            _this.CommentsPanel.IsVisible = true;
        }

        public PostInfoBottomPanel()
        {
            InitializeComponent();
        }

        private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            Debug.WriteLine("ScrollY: " + e.ScrollY);
        }
    }
}