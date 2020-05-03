using Xamarin.Forms.Platform.UWP;
using Xerpi.Controls;
using Xerpi.UWP.Renderers;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace Xerpi.UWP.Renderers
{
    public class BorderlessEntryRenderer : EntryRenderer { }
}
