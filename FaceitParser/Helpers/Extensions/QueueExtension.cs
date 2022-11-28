using System.Collections.Concurrent;

namespace FaceitParser.Helpers
{
    public static class QueueExtension
    {
        public static IEnumerable<T> DequeueAll<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out T result) && result != null)
            {
                yield return result;
            }
        }
    }
}
