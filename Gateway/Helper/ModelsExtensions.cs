namespace Gateway.Helper
{
    public static class ModelsExtensions
    {
        public static IEnumerable<T> Clone<T>(this ICollection<T> listToClone) where T : ICloneable<T> => listToClone.Select(item => item.Clone()).ToList();
        public static List<T> CloneList<T>(this ICollection<T> listToClone) where T : ICloneable<T> => listToClone.Select(item => item.Clone()).ToList();
        public static IReadOnlyCollection<T> CloneToReadOnly<T>(this ICollection<T> listToClone) where T : ICloneable<T> => listToClone.Select(item => item.Clone()).ToList().AsReadOnly();
    }

    public interface ICloneable<T>
    {
        public T Clone();
    }
}
