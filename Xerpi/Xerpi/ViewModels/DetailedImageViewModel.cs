using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private ApiImage? _backingImage;
        private readonly IDerpiNetworkService _networkService;

        public ApiImage BackingImage
        {
            get => _backingImage;
            set => Set(ref _backingImage, value);
        }

        private ReadOnlyObservableCollection<ApiTag> _tags;
        public ReadOnlyObservableCollection<ApiTag> Tags
        {
            get => _tags;
            set => Set(ref _tags, value);
        }

        public DetailedImageViewModel(ApiImage backingImage,
            IDerpiNetworkService networkService)
        {
            BackingImage = backingImage;
            _networkService = networkService;
        }

        public async Task InitExternalData()
        {
            // Get tags and comments

            //TODO: Tags should come from a TagService that maintains an ID-based
            // cache. Should also sort by a) Category and b) Alphabetically
            var tags = await _networkService.GetTags(BackingImage?.TagIds);
            if (tags == null)
            {
                return;
            }

            Tags = new ReadOnlyObservableCollection<ApiTag>(
                new ObservableCollection<ApiTag>(tags.Tags));
        }
    }
}
