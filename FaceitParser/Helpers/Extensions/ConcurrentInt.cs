using FaceitParser.Abstractions;

namespace FaceitParser.Helpers.Extensions
{
    public class ConcurrentInt : ConcurrentNumeric<int>
    {
        private static object _lock = new object();

        public ConcurrentInt() 
        {
            this.value = 0;
        }

        public ConcurrentInt(int value) 
        {
            this.value = value;
        }

        public override void Add(int value)
        {
            lock (_lock)
            {
                this.value += value;
            }
        }

        public override void Decrement()
        {
            lock (_lock)
            {
                this.value--;
            }
        }

        public override void Increment()
        {
            lock (_lock)
            {
                this.value++;
            }
        }

        public override void Substract(int value)
        {
            lock (_lock)
            {
                this.value -= value;
            }
        }
    }
}
