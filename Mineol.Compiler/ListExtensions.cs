static class ListExtensions
{
    public static void ThrowIfDuplicates(this IList<string> values, Func<string, string> callBack, Position position)
    {
        HashSet<string> names = new HashSet<string>();

        foreach (string value in values)
            if (!names.Add(value))
                throw new Error(callBack(value), position);
    }
}