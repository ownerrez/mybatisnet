using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Framewor.Tests;

public class Tests
{
    [Fact]
    public void Basic_AbleToSelectHolidays()
    {
        var sqlMapConfig = IBatisNet.Common.Utilities.Resources.GetEmbeddedResourceAsXmlDocument("ORBatis.Test.Framework.Config.SqlMap.config, ORBatis.Test.Framework");
        var builder = new DomSqlMapBuilder()
        {
            Properties = new NameValueCollection()
            {
                { "ConnectionString", Constants.ConnectionString }
            }
        };

        var mapper = builder.Configure(sqlMapConfig);
        var context = mapper.CreateSqlMapSession();

        Hashtable parameters = new Hashtable
        {
            { "active", true },
            { "userId", 347317427 }
        };
        var holidays = mapper.QueryForList<Holiday>( "Holiday.SelectAll", parameters, context );
        
        Assert.NotNull(holidays);
    }
}