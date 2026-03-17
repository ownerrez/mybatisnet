using System.Collections;
using System.Collections.Specialized;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using IBatisNet.DataMapper.Exceptions;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

public class Tests
{
    private static ISqlMapper BuildMapper()
    {
        var path = "ORBatis.Test.Config.SqlMap.config, ORBatis.Test";
        var sqlMapConfig = Resources.GetEmbeddedResourceAsXmlDocument(path);
        var builder = new DomSqlMapBuilder()
        {
            Properties = new NameValueCollection()
            {
                { "ConnectionString", Constants.ConnectionString }
            }
        };
        return builder.Configure(sqlMapConfig);
    }

    [Fact]
    public void Basic_AbleToSelectHolidays()
    {
        var mapper = BuildMapper();
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
    }

    [Fact]
    public void GridForOverview_New_WorksWhenDefinedInSameNamespaceFile()
    {
        var mapper = BuildMapper();
        var context = mapper.CreateSqlMapSession();

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

    [Fact]
    public void GridForOverview_Old_FailsWhenAccessedViaFileName()
    {
        var mapper = BuildMapper();
        var context = mapper.CreateSqlMapSession();

        var reviewParams = new Hashtable()
        {
            { "orderBy", "Id" },
            { "orderDirection", "Desc" },
            { "startAtRowNumber", 0 },
            { "pageSize", 10 }
        };

        // GridReview.xml declares namespace="Review", but its lazy-load is registered
        // under the filename "GridReview". Accessing via "GridReview.GridForOverview"
        // fails because cross-file namespace resolution was removed.
        Assert.Throws<DataMapperException>(
            () => mapper.QueryForList("Review.GridForOverviewOld", reviewParams, context)
        );
    }
}