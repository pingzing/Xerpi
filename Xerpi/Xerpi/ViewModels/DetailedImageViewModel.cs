using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;
using DynamicData;
using System.Linq;
using Xerpi.Models;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private readonly IImageService _imageService;
        private readonly IDerpiNetworkService _networkService;
        private readonly ISynchronizationContextService _syncContextService;

        private ApiImage? _backingImage;

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
            IImageService imageService,
            IDerpiNetworkService networkService,
            ISynchronizationContextService syncContextService))
        {
            BackingImage = backingImage;
            _imageService = imageService;
            _networkService = networkService;
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
            if (BackingImage?.TagIds != null)
            {
                await _imageService.UpdateTags(BackingImage.TagIds).ConfigureAwait(false);                
            }

            if (BackingImage?.CommentCount > 0)
            {
                var commentsResponse = await _networkService.GetComments(BackingImage.Id);
                if (commentsResponse == null)
                {
                    return;
                }

                List<CommentViewModel> commentVms = new List<CommentViewModel>();
                foreach (var comment in commentsResponse.Comments)
                {
                    var user = await _networkService.GetUserProfile(comment.Author);
                    if (user != null)
                    {
                        //TODO:  Use Comment User info to construct a commentVM
                        commentVms.Add(new CommentViewModel());
                    }
                }
            }
        }
    }
}
