using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common.Models;

namespace ORBatis.Core.Test.Tests;

public class Tests
{
    [Fact]
    public void Basic_AbleToSelectHolidays()
    {
        var sqlMapConfig = IBatisNet.Common.Utilities.Resources.GetEmbeddedResourceAsXmlDocument("ORBatis.Core.Test.Config.SqlMap.config, ORBatis.Core.Test");
        var defaultConnectionString = "server=localhost;pwd=38wk1dr28t34r7d82dir9;uid=ownerrez_dev;database=ownerrez_dev;charset=utf8mb4;ConnectionReset=true";
        var builder = new DomSqlMapBuilder()
        {
            Properties = new NameValueCollection()
            {
                { "ConnectionString", defaultConnectionString }
            }
        };

        var mapper = builder.Configure(sqlMapConfig);
        var context = mapper.CreateSqlMapSession();

        var holidays = mapper.QueryForList<Holiday>(
            "Holiday.SelectAll",
            new Hashtable
            {
                { "active", true },
                { "userId", 347317427 }
            },
            context
        );
    }
}