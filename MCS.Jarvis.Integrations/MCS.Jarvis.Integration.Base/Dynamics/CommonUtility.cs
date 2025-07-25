namespace MCS.Jarvis.Integration.Base.Dynamics
{
    using System.Diagnostics;
    using System.Globalization;

    public static class CommonUtility
    {
        public static IEnumerable<IEnumerable<T>> GetBatchesAs<T>(this IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, inx) => new { item, inx })
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }

        public static string GetRecordsPerSecond(Stopwatch t, int numberOfOperations)
        {
            if (t == null)
            {
                throw new ArgumentException("null stpwatch object", nameof(t));
            }

            return (numberOfOperations * 1.0 / t.ElapsedMilliseconds * 1000).ToString(CultureInfo.InvariantCulture) + " records/second";
        }
    }
}
