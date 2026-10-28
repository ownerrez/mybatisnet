using System.Data;

namespace IBatisNet.Common
{
    /// <summary>
    ///     Reads the key an insert generated from the command that ran it, so the insert's post selectKey
    ///     needs no second round trip.
    /// </summary>
    public interface IGeneratedKeyReader
    {
        /// <summary>
        ///     The generated key, or null when the command's reply carries none; the post selectKey then runs.
        /// </summary>
        /// <param name="command">The provider's command, after ExecuteNonQuery.</param>
        object ReadGeneratedKey(IDbCommand command);
    }
}
