using System.Threading;

namespace VarelaAloisio.Core.Utils
{
    public static class TokenUtils
    {
        public static void CancelAndDispose(ref CancellationTokenSource source)
        {
            source?.Cancel();
            source?.Dispose();
            source = null;
        }

        public static void Recreate(ref CancellationTokenSource source)
        {
            source?.Cancel();
            source?.Dispose();
            source = new CancellationTokenSource();
        }
    }
}