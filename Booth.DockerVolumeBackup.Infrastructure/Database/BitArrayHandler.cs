using System.Collections;
using System.Data;
using System.Linq;

using Dapper;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{

    internal class BitArrayHandler : SqlMapper.TypeHandler<BitArray>
    {
        public override BitArray Parse(object value)
        {
            return new BitArray(7);
        }

        public override void SetValue(IDbDataParameter parameter, BitArray value)
        {
            var byteArray = new byte[4];
            value.CopyTo(byteArray, 0);

            parameter.Value = BitConverter.ToInt32(byteArray, 0);
        }
    }
}
