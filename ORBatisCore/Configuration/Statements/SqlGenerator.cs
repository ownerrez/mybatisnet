#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 512878 $
 * $Date: 2007-02-28 10:57:11 -0700 (Wed, 28 Feb 2007) $
 *
 * iBATIS.NET Data Mapper
 * Copyright (C) 2004 - Gilles Bayon
 *
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 ********************************************************************************/
#endregion


using System.Text;

namespace IBatisNet.DataMapper.Configuration.Statements;

/// <summary>
///     Summary description for SqlGenerator.
/// </summary>
public sealed class SqlGenerator
{
    /// <summary>
    ///     Creates SQL command text for a specified statement
    /// </summary>
    /// <param name="statement">The statement to build the SQL command text.</param>
    /// <returns>The SQL command text for the statement.</returns>
    public static string BuildQuery(IStatement statement)
    {
        var sqlText = string.Empty;

        if (statement is Select)
            sqlText = BuildSelectQuery(statement);
        else if (statement is Insert)
            sqlText = BuildInsertQuery(statement);
        else if (statement is Update)
            sqlText = BuildUpdateQuery(statement);
        else if (statement is Delete) sqlText = BuildDeleteQuery(statement);

        return sqlText;
    }


    /// <summary>
    ///     Creates an select SQL command text for a specified statement
    /// </summary>
    /// <param name="statement">The statement to build the SQL command text.</param>
    /// <returns>The SQL command text for the statement.</returns>
    private static string BuildSelectQuery(IStatement statement)
    {
        var output = new StringBuilder();
        var select = (Select)statement;
        var columnCount = statement.ParameterMap.PropertiesList.Count;

        output.Append("SELECT ");

        // Create the list of columns
        for (var i = 0; i < columnCount; i++)
        {
            var property = statement.ParameterMap.PropertiesList[i];

            if (i < columnCount - 1)
                output.Append("\t" + property.ColumnName + " as " + property.PropertyName + ",");
            else
                output.Append("\t" + property.ColumnName + " as " + property.PropertyName);
        }

        output.Append(" FROM ");
        output.Append("\t" + select.Generate.Table + "");

        // Create the where clause

        var compositeKeyList = select.Generate.By.Split(',');
        if (compositeKeyList.Length > 0 && select.Generate.By.Length > 0)
        {
            output.Append(" WHERE ");
            var length = compositeKeyList.Length;
            for (var i = 0; i < length; i++)
            {
                var columnName = compositeKeyList[i];

                if (i > 0)
                    output.Append("\tAND " + columnName + " = ?");
                else
                    output.Append("\t" + columnName + " = ?");
            }
        }

        // 'Select All' case
        if (statement.ParameterClass == null)
            // The ParameterMap is just used to build the query
            // to avoid problems later, we set it to null
            statement.ParameterMap = null;

        return output.ToString();
    }


    /// <summary>
    ///     Creates an insert SQL command text for a specified statement
    /// </summary>
    /// <param name="statement">The statement to build the SQL command text.</param>
    /// <returns>The SQL command text for the statement.</returns>
    private static string BuildInsertQuery(IStatement statement)
    {
        var output = new StringBuilder();
        var insert = (Insert)statement;
        var columnCount = statement.ParameterMap.PropertiesList.Count;

        output.Append("INSERT INTO " + insert.Generate.Table + " (");

        // Create the parameter list
        for (var i = 0; i < columnCount; i++)
        {
            var property = statement.ParameterMap.PropertiesList[i];

            // Append the column name as a parameter of the insert statement
            if (i < columnCount - 1)
                output.Append("\t" + property.ColumnName + ",");
            else
                output.Append("\t" + property.ColumnName + "");
        }

        output.Append(") VALUES (");

        // Create the values list
        for (var i = 0; i < columnCount; i++)
        {
            var property = statement.ParameterMap.PropertiesList[i];

            // Append the necessary line breaks and commas
            if (i < columnCount - 1)
                output.Append("\t?,");
            else
                output.Append("\t?");
        }

        output.Append(")");

        return output.ToString();
    }


    /// <summary>
    ///     Creates an update SQL command text for a specified statement
    /// </summary>
    /// <param name="statement">The statement to build the SQL command text.</param>
    /// <returns>The SQL command text for the statement.</returns>
    private static string BuildUpdateQuery(IStatement statement)
    {
        var output = new StringBuilder();
        var update = (Update)statement;
        var columnCount = statement.ParameterMap.PropertiesList.Count;
        var keysList = update.Generate.By.Split(',');

        output.Append("UPDATE ");
        output.Append("\t" + update.Generate.Table + " ");
        output.Append("SET ");

        // Create the set statement
        for (var i = 0; i < columnCount; i++)
        {
            var property = statement.ParameterMap.PropertiesList[i];

            // Ignore key columns
            if (update.Generate.By.IndexOf(property.ColumnName) < 0)
            {
                if (i < columnCount - keysList.Length - 1)
                    output.Append("\t" + property.ColumnName + " = ?,");
                else
                    output.Append("\t" + property.ColumnName + " = ? ");
            }
        }

        output.Append(" WHERE ");

        // Create the where clause
        var length = keysList.Length;
        for (var i = 0; i < length; i++)
        {
            var columnName = keysList[i];

            if (i > 0)
                output.Append("\tAND " + columnName + " = ?");
            else
                output.Append("\t " + columnName + " = ?");
        }

        if (!string.IsNullOrWhiteSpace(update.Generate.RowVersion))
        {
            if (keysList.Length > 0)
                output.Append("\tAND " + update.Generate.RowVersion + " = #" + update.Generate.RowVersion + "# - 1");
            else
                output.Append("\t " + update.Generate.RowVersion + " = #" + update.Generate.RowVersion + "# - 1");
        }

        return output.ToString();
    }

    /// <summary>
    ///     Creates an delete SQL command text for a specified statement
    /// </summary>
    /// <param name="statement">The statement to build the SQL command text.</param>
    /// <returns>The SQL command text for the statement.</returns>
    private static string BuildDeleteQuery(IStatement statement)
    {
        var output = new StringBuilder();
        var delete = (Delete)statement;
        var keysList = delete.Generate.By.Split(',');

        output.Append("DELETE FROM");
        output.Append("\t" + delete.Generate.Table + "");
        output.Append(" WHERE ");

        // Create the where clause
        var length = keysList.Length;
        for (var i = 0; i < keysList.Length; i++)
        {
            var columnName = keysList[i].Trim();

            if (i > 0)
                output.Append("\tAND " + columnName + " = ?");
            else
                output.Append("\t " + columnName + " = ?");
        }

        return output.ToString();
    }
}