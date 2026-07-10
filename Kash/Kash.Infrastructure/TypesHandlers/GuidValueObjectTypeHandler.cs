using SergioIzq.Domain.Kernel.Interfaces;
using Dapper;
using System.Data;

namespace Kash.Infrastructure.TypesHandlers
{

    public class GuidValueObjectTypeHandler<T> : SqlMapper.TypeHandler<T> where T : IGuidValueObject
    {
        private readonly Func<Guid, T> _factory;

        public GuidValueObjectTypeHandler(Func<Guid, T> factory)
        {
            _factory = factory;
        }

        public override void SetValue(IDbDataParameter parameter, T? value)
        {
            parameter.Value = value?.Value ?? Guid.Empty;
            parameter.DbType = DbType.Guid;
        }

        public override T Parse(object value)
        {
            if (value is Guid g)
                return _factory(g);

            if (value is byte[] bytes && bytes.Length == 16)
                return _factory(new Guid(bytes));

            throw new DataException($"Cannot convert {value?.GetType()} to {typeof(T).Name}");
        }
    }

}
