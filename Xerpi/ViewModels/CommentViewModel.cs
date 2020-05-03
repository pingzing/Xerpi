using System;
using Xerpi.Models.API;

namespace Xerpi.ViewModels
{
    public class CommentViewModel : BaseViewModel
    {
        private ApiComment _backingComment;

        public string Author => _backingComment.Author;
        public string Body => _backingComment.Body;
        public string Avatar => _backingComment.Avatar;
        public DateTimeOffset? PostedAt => _backingComment.CreatedAt;
        public bool HasSvgAvatar => _backingComment?.Avatar?.Contains("data:image/svg+xml") ?? false;

        public CommentViewModel(ApiComment comment)
        {
            _backingComment = comment;
        }
    }
}
