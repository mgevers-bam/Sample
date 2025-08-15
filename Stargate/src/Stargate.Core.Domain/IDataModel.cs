namespace Stargate.Core.Domain;

public interface IDataModel : IDataModel<int> { }

public interface IDataModel<TKey>
{
    public TKey Id { get; }
}
