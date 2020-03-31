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
using DynamicData.Binding;
using System.Threading;
using System.Diagnostics;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private readonly IImageService _imageService;
        private readonly IDerpiNetworkService _networkService;
        private readonly ISynchronizationContextService _syncContextService;
        private static readonly ApiTagComparer _tagComparer = new ApiTagComparer();

        private bool _externalDataLoaded = false;
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

        private ObservableCollectionExtended<CommentViewModel>? _comments;
        public ObservableCollectionExtended<CommentViewModel> Comments
        {
            get => _comments;
            set => Set(ref _comments, value);
        }

        public DetailedImageViewModel(ApiImage backingImage,
            IImageService imageService,
            IDerpiNetworkService networkService,
            ISynchronizationContextService syncContextService)
        {
            BackingImage = backingImage;
            _imageService = imageService;
            _networkService = networkService;
            _syncContextService = syncContextService;

            _ = _imageService.Tags.Connect()
                .Filter(x => BackingImage.TagIds.Contains(x.Id))
                .Sort(_tagComparer)
                .ObserveOn(_syncContextService.UIThread)
                .Bind(out _tags)
                .DisposeMany()
                .Subscribe();
        }

        public async Task InitExternalData(CancellationToken token)
        {
            if (_externalDataLoaded)
            {
                return;
            }

            await Task.Delay(200); // Wait a bit before we start doing expensive stuff
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling full load of image before doing anything expensive...");
                return;
            }

            // Get tags and comments
            if (BackingImage?.TagIds != null)
            {
                await _imageService.UpdateTags(BackingImage.TagIds).ConfigureAwait(false);
            }
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling full load of image after getting tags...");
                return;
            }

            List<CommentViewModel> commentVms = new List<CommentViewModel>();
            if (BackingImage?.CommentCount > 0)
            {
                var commentsResponse = await _networkService.GetComments(BackingImage.Id);
                if (commentsResponse == null)
                {
                    return;
                }

                foreach (var comment in commentsResponse.Comments)
                {
                    commentVms.Add(new CommentViewModel(comment));
                }
            }

            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling full load of after retrieving comments, but before displaying...");
                return;
            }

            Comments = new ObservableCollectionExtended<CommentViewModel>(commentVms);
            _externalDataLoaded = true;
        }
    }
}
