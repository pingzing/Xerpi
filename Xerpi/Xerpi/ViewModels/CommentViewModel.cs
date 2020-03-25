using Xerpi.Models.API;

namespace Xerpi.ViewModels
{
    public class CommentViewModel : BaseViewModel
    {
        private ApiComment _backingComment;

        public string Author => _backingComment.Author;
        public string Body => _backingComment.Body;

        // TODO, Maybe: get user profile for their avatar URL and store it in here

        public CommentViewModel()
        {
        }

        public CommentViewModel(ApiComment comment)
        {
            _backingComment = comment;
        }
    }
}
