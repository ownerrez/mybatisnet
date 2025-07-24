using System;
using IBatisNet.DataMapper.TypeHandlers;

namespace ORBatis.Test.Framework.TypeHandlers;

public class NullableEnum<T> : ITypeHandlerCallback
    where T : struct
{
    readonly static Type _underlyingType = Enum.GetUnderlyingType(typeof(T));

    public object NullValue
    {
        get { return null; }
    }

    public object GetResult(IResultGetter getter)
    {
        if (getter.Value != null && getter.Value != DBNull.Value)
            return (T)(object)Convert.ChangeType(getter.Value, _underlyingType);
        else
            return NullValue;
    }

    public void SetParameter(IParameterSetter setter, object parameter)
    {
        if (parameter != null && parameter is T)
            setter.Value = Convert.ChangeType(parameter, _underlyingType);
        else
            setter.Value = null;
    }

    public object ValueOf(string s)
    {
        throw new NotImplementedException();
    }
}