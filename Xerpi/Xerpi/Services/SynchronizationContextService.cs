using System.Threading;
using System.Threading.Tasks;

namespace Xerpi.Services
{
    public interface ISynchronizationContextService
    {
        SynchronizationContext UIThread { get; }
    }

    public class SynchronizationContextService : ISynchronizationContextService
    {
        public SynchronizationContext UIThread { get; set; }

        public SynchronizationContextService()
        {
            Init();
        }

        private async Task Init()
        {
            UIThread = await Xamarin.Essentials.MainThread.GetMainThreadSynchronizationContextAsync();
        }
    }
}
