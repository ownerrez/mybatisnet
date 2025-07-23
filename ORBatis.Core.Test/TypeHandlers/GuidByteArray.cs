using System;
using IBatisNet.DataMapper.TypeHandlers;


namespace ORBatis.Core.Test.TypeHandlers;

public class GuidByteArray : ITypeHandlerCallback
{
    public object NullValue
    {
        get { return null as Guid?; }
    }

    public object GetResult(IResultGetter getter)
    {
        try
        {
            if (getter.Value is byte[])
                return new Guid((byte[])getter.Value);
            else
                return getter.Value;
        }
        catch (Exception ex)
        {
            // https://github.com/ownerrez/orez/issues/8876
            throw new InvalidOperationException("iBatis GUID load error.", ex);
        }
    }

    public void SetParameter(IParameterSetter setter, object parameter)
    {
        if (parameter != null && (parameter is Guid || parameter is Guid?))
            setter.Value = ((Guid)parameter).ToByteArray();
    }

    public object ValueOf(string s)
    {
        throw new NotImplementedException();
    }
}