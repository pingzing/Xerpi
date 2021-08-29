using System.Threading.Tasks;
using Xerpi.Models.API;
using Xerpi.Services;
using System.Linq;
using Xerpi.Models;
using System.Collections.Generic;
using DynamicData.Binding;
using System.Threading;
using System.Diagnostics;
using Xamarin.Forms;
using FFImageLoading.Args;

namespace Xerpi.ViewModels
{
    public class DetailedImageViewModel : BaseViewModel
    {
        private const int BytesPerMb = 1_048_576;
        private const int BytesPerKb = 1024;

        private readonly IImageService _imageService;
        private readonly IDerpiNetworkService _networkService;
        private static readonly ApiTagComparer _tagComparer = new ApiTagComparer();

        private bool _externalDataLoaded = false;
        private ApiImage? _backingImage;

        public ApiImage BackingImage
        {
            get => _backingImage;
            set => Set(ref _backingImage, value);
        }

        private ObservableCollectionExtended<ApiTag> _tags = new ObservableCollectionExtended<ApiTag>();
        public ObservableCollectionExtended<ApiTag> Tags
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

        private bool _isImageLoading = false;
        public bool IsImageLoading
        {
            get => _isImageLoading;
            set => Set(ref _isImageLoading, value);
        }

        private string _imageProgressMessage = "";
        public string ImageProgressMesssage
        {
            get => _imageProgressMessage;
            set => Set(ref _imageProgressMessage, value);
        }

        public Command DownloadStartedCommand { get; private set; }
        public Command DownloadProgressCommand { get; private set; }
        public Command FinishCommand { get; private set; }

        public DetailedImageViewModel(ApiImage backingImage,
            IImageService imageService,
            IDerpiNetworkService networkService)
        {
            BackingImage = backingImage;
            _imageService = imageService;
            _networkService = networkService;

            DownloadStartedCommand = new Command<DownloadStartedEventArgs>(ImageDownloadStarted);
            DownloadProgressCommand = new Command<DownloadProgressEventArgs>(ImageDownloadProgressCommand);
            FinishCommand = new Command<FinishEventArgs>(ImageFinished);
        }

        public async Task InitExternalData(CancellationToken token)
        {
            if (_externalDataLoaded)
            {
                return;
            }

            await Task.Delay(250); // Wait a bit before we start doing expensive stuff
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling full load of image before doing anything expensive...");
                return;
            }

            // Get tags and comments
            if (BackingImage?.TagIds != null)
            {
                // TODO: Tag caching
                var tags = (await _networkService.GetTags(BackingImage.TagIds)).OrderBy(x => x, _tagComparer);
                if (tags != null)
                {
                    using (_tags.SuspendNotifications())
                    {
                        _tags.Clear();
                        _tags.AddRange(tags);
                    }
                }
            }
            if (token.IsCancellationRequested)
            {
                Debug.WriteLine("Cancelling full load of image after getting tags...");
                return;
            }

            // TODO: Comment caching
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

        private void ImageDownloadStarted(DownloadStartedEventArgs e)
        {
            IsImageLoading = true;
        }

        private void ImageDownloadProgressCommand(DownloadProgressEventArgs e)
        {
            if (e.DownloadProgress.Current == e.DownloadProgress.Total)
            {
                ImageProgressMesssage = "Download complete, rendering...";
                return;
            }

            string unit = "B";
            double current = e.DownloadProgress.Current;
            double total = e.DownloadProgress.Total;
            if (total > BytesPerMb)
            {
                unit = "MB";
                total /= BytesPerMb;
                current /= BytesPerMb;
            }
            else if (total > BytesPerKb)
            {
                unit = "kB";
                total /= BytesPerKb;
                current /= BytesPerKb;
            }
            ImageProgressMesssage = $"{current:F2} {unit} / {total:F2} {unit}";
        }

        private void ImageFinished(FinishEventArgs e)
        {
            IsImageLoading = false;
        }
    }
}
