using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xerpi.Models;
using Xerpi.ViewModels.Popups;

namespace Xerpi.Views.Popups
{
    public class SortFilterPopupViewModel : BasePopupViewModel<SearchSortOptions>
    {
        public List<SortOptionViewModel> SortOptions { get; set; } = new List<SortOptionViewModel>
        {
            new SortOptionViewModel(SortProperties.UploadDate, "Upload date"),
            new SortOptionViewModel(SortProperties.ModifiedDate, "Last modified date"),
            new SortOptionViewModel(SortProperties.InitialPostDate, "Initial post date"),
            new SortOptionViewModel(SortProperties.AspectRatio, "Aspect ratio"),
            new SortOptionViewModel(SortProperties.FaveCount, "Fave count"),
            new SortOptionViewModel(SortProperties.Upvotes, "Upvotes"),
            new SortOptionViewModel(SortProperties.Downvotes, "Downvotes"),
            new SortOptionViewModel(SortProperties.Score, "Score"),
            new SortOptionViewModel(SortProperties.WilsonScore, "Wilson score"),
            new SortOptionViewModel(SortProperties.Relevance, "Relevance"),
            new SortOptionViewModel(SortProperties.Width, "Width"),
            new SortOptionViewModel(SortProperties.Height, "Height"),
            new SortOptionViewModel(SortProperties.Comments, "Comment count"),
            new SortOptionViewModel(SortProperties.TagCount, "Tag count"),
        };
        public List<SortOrderKindViewModel> OrderOptions { get; set; } = new List<SortOrderKindViewModel>
        {
            new SortOrderKindViewModel(SortOrderKind.Descending, "Descending"),
            new SortOrderKindViewModel(SortOrderKind.Ascending, "Ascending"),
        };

        private SortOptionViewModel? _selectedSortOption;
        public SortOptionViewModel? SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                Set(ref _selectedSortOption, value);
            }
        }

        private SortOrderKindViewModel _selectedSortOrder = null!;
        public SortOrderKindViewModel SelectedSortOrder
        {
            get => _selectedSortOrder;
            set
            {
                Set(ref _selectedSortOrder, value);
            }
        }

        public ICommand ApplyCommand { get; }

        public SortFilterPopupViewModel()
        {
            ApplyCommand = new Command(Apply);
            SelectedSortOption = SortOptions[0];
            SelectedSortOrder = OrderOptions[0];
        }

        private void Apply(object _)
        {
            Result = new SearchSortOptions
            {
                SortOrder = SelectedSortOrder?.SortOrder ?? SortOrderKind.Descending,
                SortByProperty = SelectedSortOption?.SortProperty
            };

            Close();
        }
    }

    public class SortOrderKindViewModel
    {
        public SortOrderKind SortOrder { get; set; }
        public string DisplayName { get; set; } = null!;

        public SortOrderKindViewModel() { }

        public SortOrderKindViewModel(SortOrderKind sortOrder, string displayName)
        {
            SortOrder = sortOrder;
            DisplayName = displayName;
        }
    }

    public class SortOptionViewModel
    {
        public string SortProperty { get; set; } = null!;
        public string DisplayName { get; set; } = null!;

        public SortOptionViewModel() { }

        public SortOptionViewModel(string sortProperty, string displayName)
        {
            SortProperty = sortProperty;
            DisplayName = displayName;
        }
    }
}
