namespace FaceitParser.Abstractions
{
    /// <summary>
    /// Потокобезопасный класс для работы 
    /// с числовыми типами
    /// </summary>
    /// <typeparam name="T">Любой числовой тип</typeparam>
    public abstract class ConcurrentNumeric<T> where T : struct, IComparable, IFormattable, IConvertible, IEquatable<T>
    {
        public T value;

        public abstract void Increment();

        public abstract void Decrement();

        public abstract void Add(T value);

        public abstract void Substract(T value);
    }
}
