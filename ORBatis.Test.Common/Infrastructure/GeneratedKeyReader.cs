using System;
using System.Data;
using IBatisNet.Common;

namespace ORBatis.Test.Common.Infrastructure;

/// <summary>
///     Configured on the MySqlGeneratedKeys provider. Tests using it run in the "Database" collection, which runs them one at a time.
/// </summary>
public class GeneratedKeyReader : IGeneratedKeyReader
{
    public static Func<IDbCommand, object> Read { get; set; }

    public static int Calls { get; private set; }

    public static void Reset()
    {
        Read = null;
        Calls = 0;
    }

    public object ReadGeneratedKey(IDbCommand command)
    {
        Calls++;

        return Read?.Invoke(command);
    }
}
