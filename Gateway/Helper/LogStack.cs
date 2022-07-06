namespace Gateway.Helper
{
    public class LogStack<T> : LinkedList<T>
    {
        private readonly int _maxSize;
        public LogStack(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Push(T item)
        {
            this.AddFirst(item);

            if (this.Count > _maxSize)
                this.RemoveLast();
        }
    }
}
