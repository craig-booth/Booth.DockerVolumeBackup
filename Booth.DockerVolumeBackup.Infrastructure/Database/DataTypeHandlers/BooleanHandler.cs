using System.Data;
using System.Globalization;
using Dapper;

namespace Booth.DockerVolumeBackup.Infrastructure.Database.DataTypeHandlers
{

    internal class BooleanHandler : SqlMapper.TypeHandler<bool>
    {
        public override bool Parse(object value)
        {
            return (long)value == 1;
        }

        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }
    }
}
