using System.Data;
using System.Globalization;
using Dapper;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{

    internal class TimeOnlyHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(object value)
        {
            return TimeOnly.Parse((string)value);
        }

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.Value = value.ToString("HH:mm:ss");
        }
    }
}
