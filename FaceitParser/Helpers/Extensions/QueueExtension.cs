using System.Collections.Concurrent;

namespace FaceitParser.Helpers
{
    public static class QueueExtension
    {
        public static IEnumerable<string> DequeueAll(this ConcurrentQueue<string> queue)
        {
            while (queue.TryDequeue(out string result))
            {
                yield return result;
            }
        }
    }
}
