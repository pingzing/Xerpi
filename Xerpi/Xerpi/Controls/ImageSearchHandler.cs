using Xamarin.Forms;

namespace Xerpi.Controls
{
    public class ImageSearchHandler : SearchHandler
    {
        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);
            // TODO: Give up tags that we've seen from Tags in ImageService?
        }

        protected override void OnItemSelected(object item)
        {
            base.OnItemSelected(item);
            // Add to list of tags, with comma separation if necessary?
        }
    }
}
