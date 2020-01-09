using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;
using DynamicData;
using System.Linq;
using Xerpi.Models;
using System.Reactive.Linq;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private ApiImage? _backingImage;

        public ApiImage BackingImage
        {
            get => _backingImage;
            set => Set(ref _backingImage, value);
        }

        private ReadOnlyObservableCollection<ApiTag> _tags;
        private readonly IImageService _imageService;
        private readonly ISynchronizationContextService _syncContextService;

        public ReadOnlyObservableCollection<ApiTag> Tags
        {
            get => _tags;
            set => Set(ref _tags, value);
        }

        public DetailedImageViewModel(ApiImage backingImage,
            IImageService imageService,
            ISynchronizationContextService syncContextService)
        {
            BackingImage = backingImage;
            _imageService = imageService;
            _syncContextService = syncContextService;
            var disposable = _imageService.Tags.Connect()
                .Filter(x => BackingImage.TagIds.Contains(x.Id))
                .Sort(new ApiTagComparer())
                .ObserveOn(_syncContextService.UIThread)
                .Bind(out _tags)
                .DisposeMany()
                .Subscribe();
        }

        public async Task InitExternalData()
        {
            // Get tags and comments

            //TODO: Tags should come from a TagService that maintains an ID-based
            // cache. Should also sort by a) Category and b) Alphabetically
            if (BackingImage?.TagIds != null)
            {
                await _imageService.UpdateTags(BackingImage.TagIds).ConfigureAwait(false);                
            }
        }
    }
}
