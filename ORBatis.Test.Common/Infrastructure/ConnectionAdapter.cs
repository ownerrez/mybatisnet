using System.Data;
using IBatisNet.Common;

namespace ORBatis.Test.Common.Infrastructure;

public class ConnectionAdapter : IConnectionAdapter
{
    public IDbConnection Adapt(IDbConnection cn) => cn;
}