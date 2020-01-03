using System.Collections.Generic;

using Xamarin.Forms;
using Xerpi.Models.API;

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
            typeof(IEnumerable<ApiComment>),
            typeof(PostInfoBottomPanel));
        public IEnumerable<ApiComment> Comments
        {
            get => (IEnumerable<ApiComment>)GetValue(CommentsProperty);
            set => SetValue(CommentsProperty, value);
        }

        public PostInfoBottomPanel()
        {
            InitializeComponent();
        }
    }
}