using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

public class Tests
{
    private static XmlDocument GetSqlMapConfig()
    {
        var path = $"ORBatis.Test.Config.SqlMap.config, ORBatis.Test";
        var sqlMapConfig = IBatisNet.Common.Utilities.Resources.GetEmbeddedResourceAsXmlDocument(path);
        return sqlMapConfig;
    }

    [Fact]
    public void Basic_AbleToSelectHolidays()
    {
        var sqlMapConfig = GetSqlMapConfig();
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
        var holidays = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, context);
        Assert.NotNull(holidays);

        // This one should use the previously LazyLoaded version!
        var holidays2 = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, context);
        Assert.NotNull(holidays2);

        var cannedParams = new Hashtable()
        {
            { "orderBy", "Id" },
            { "orderDirection", "Desc" },
            { "startAtRowNumber", 0 },
            { "pageSize", 10 }
        };
        var cannedQueries = mapper.QueryForList(
            "CannedQuery.GridForOverview",
            cannedParams,
            context
        );
        Assert.NotNull(cannedQueries);
        
        var reviews = mapper.QueryForList<Review>("Review.SelectAll", parameters, context);
        Assert.NotNull(reviews);

        var reviewParams = new Hashtable()
        {
            { "orderBy", "Id" },
            { "orderDirection", "Desc" },
            { "startAtRowNumber", 0 },
            { "pageSize", 10 }
        };
        var gridReview = mapper.QueryForList("Review.GridForOverview", reviewParams, context);
        Assert.NotNull(gridReview);
    }
}