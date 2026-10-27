using Core;
using Core.Abstractions;
using Core.Storage;

internal static class StoreFactory
{
    public static IOrderStore Create(string[] args, string dataPath)
    {
        IOrderStore store = args.Contains("--file")
            ? new FileOrderStore(dataPath, SampleData.Orders())
            : new InMemoryOrderStore(SampleData.Orders());

        return args.Contains("--cache") ? new CachingOrderStore(store) : store;
    }
}
