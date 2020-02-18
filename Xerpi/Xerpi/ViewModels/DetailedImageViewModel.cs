using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;
using DynamicData;
using System.Linq;
using Xerpi.Models;
using System.Collections.Generic;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private readonly IImageService _imageService;
        private readonly IDerpiNetworkService _networkService;

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
            IDerpiNetworkService networkService)
        {
            BackingImage = backingImage;
            _imageService = imageService;
            _networkService = networkService;
            var disposable = _imageService.Tags.Connect()
                .Filter(x => BackingImage.TagIds.Contains(x.Id))
                .Sort(new ApiTagComparer())
                .Bind(out _tags)
                .DisposeMany()
                .Subscribe();
        }

        public async Task InitExternalData()
        {
            // Get tags and comments
            if (BackingImage?.TagIds != null)
            {
                await _imageService.UpdateTags(BackingImage.TagIds);
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
