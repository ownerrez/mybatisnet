#region Apache Notice
/*****************************************************************************
 * $Revision: 450157 $
 * $LastChangedDate: 2007-02-21 13:23:49 -0700 (Wed, 21 Feb 2007) $
 * $LastChangedBy: gbayon $
 *
 * iBATIS.NET Data Mapper
 * Copyright (C) 2006/2005 - The Apache Software Foundation
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

#region Using
using System.Collections.Specialized;
using System.Data;
using System.Xml.Serialization;
using IBatisNet.Common.Utilities;
using IBatisNet.Common.Utilities.Objects;
using IBatisNet.DataMapper.DataExchange;
#endregion

namespace IBatisNet.DataMapper.Configuration.ResultMapping;

/// <summary>
///     Implementation of <see cref="IResultMap" /> interface for auto mapping
/// </summary>
public class AutoResultMap : IResultMap
{
    [NonSerialized] private readonly IFactory _resultClassFactory;

    [NonSerialized] private IDataExchange _dataExchange;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoResultMap" /> class.
    /// </summary>
    /// <param name="resultClass">The result class.</param>
    /// <param name="resultClassFactory">The result class factory.</param>
    /// <param name="dataExchange">The data exchange.</param>
    public AutoResultMap(Type resultClass, IFactory resultClassFactory, IDataExchange dataExchange)
    {
        Class = resultClass;
        _resultClassFactory = resultClassFactory;
        _dataExchange = dataExchange;
    }

    /// <summary>
    ///     Clones this instance.
    /// </summary>
    /// <returns></returns>
    public AutoResultMap Clone()
    {
        return new AutoResultMap(Class, _resultClassFactory, _dataExchange);
    }

    /// <summary>
    ///     Create an instance of result class.
    /// </summary>
    /// <returns>An object.</returns>
    public object CreateInstanceOfResultClass()
    {
        if (Class.IsPrimitive || Class == typeof(string))
        {
            var typeCode = Type.GetTypeCode(Class);
            return TypeUtils.InstantiatePrimitiveType(typeCode);
        }

        if (Class.IsValueType)
        {
            if (Class == typeof(DateTime)) return new DateTime();

            if (Class == typeof(decimal)) return new decimal();

            if (Class == typeof(Guid)) return Guid.Empty;

            if (Class == typeof(TimeSpan))
                return new TimeSpan(0);
            if (Class.IsGenericType && typeof(Nullable<>).IsAssignableFrom(Class.GetGenericTypeDefinition())) return TypeUtils.InstantiateNullableType(Class);

            throw new NotImplementedException("Unable to instanciate value type");
        }

        return _resultClassFactory.CreateInstance(null);
    }

    #region IResultMap Members
    /// <summary>
    ///     The GroupBy Properties.
    /// </summary>
    [XmlIgnore]
    public StringCollection GroupByPropertyNames => throw new NotImplementedException("The property 'GroupByPropertyNames' is not implemented.");

    /// <summary>
    ///     The collection of ResultProperty.
    /// </summary>
    [XmlIgnore]
    [field: NonSerialized]
    public ResultPropertyCollection Properties { get; } = new();

    /// <summary>
    ///     The GroupBy Properties.
    /// </summary>
    /// <value></value>
    public ResultPropertyCollection GroupByProperties => throw new NotImplementedException("The property 'GroupByProperties' is not implemented.");

    /// <summary>
    ///     The collection of constructor parameters.
    /// </summary>
    [XmlIgnore]
    public ResultPropertyCollection Parameters => throw new NotImplementedException("The property 'Parameters' is not implemented.");

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is initalized.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is initalized; otherwise, <c>false</c>.
    /// </value>
    [field: NonSerialized]
    public bool IsInitalized { get; set; }

    /// <summary>
    ///     Identifier used to identify the resultMap amongst the others.
    /// </summary>
    /// <value></value>
    /// <example>GetProduct</example>
    public string Id => Class.Name;


    /// <summary>
    ///     The output type class of the resultMap.
    /// </summary>
    /// <value></value>
    [field: NonSerialized]
    public Type Class { get; }


    /// <summary>
    ///     Sets the IDataExchange
    /// </summary>
    /// <value></value>
    public IDataExchange DataExchange
    {
        set => _dataExchange = value;
    }


    /// <summary>
    ///     Create an instance Of result.
    /// </summary>
    /// <param name="parameters">
    ///     An array of values that matches the number, order and type
    ///     of the parameters for this constructor.
    /// </param>
    /// <returns>An object.</returns>
    public object CreateInstanceOfResult(object[] parameters)
    {
        return CreateInstanceOfResultClass();
    }

    /// <summary>
    ///     Set the value of an object property.
    /// </summary>
    /// <param name="target">The object to set the property.</param>
    /// <param name="property">The result property to use.</param>
    /// <param name="dataBaseValue">The database value to set.</param>
    public void SetValueOfProperty(ref object target, ResultProperty property, object dataBaseValue)
    {
        _dataExchange.SetData(ref target, property, dataBaseValue);
    }

    /// <summary>
    /// </summary>
    /// <param name="dataReader"></param>
    /// <returns></returns>
    public IResultMap ResolveSubMap(IDataReader dataReader)
    {
        return this;
    }
    #endregion
}