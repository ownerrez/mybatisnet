using System.Data;

namespace IBatisNet.Common
{
    public interface IConnectionAdapter
    {
        IDbConnection Adapt(IDbConnection cn);
    }
}