﻿using System.Collections.Generic;

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

        public PostInfoBottomPanel()
        {
            InitializeComponent();
        }
    }
}