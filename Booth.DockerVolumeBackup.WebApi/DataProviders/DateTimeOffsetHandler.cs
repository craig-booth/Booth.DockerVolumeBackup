using System.Data;
using System.Globalization;

using Dapper;

namespace Booth.DockerVolumeBackup.WebApi.DataProviders
{

    public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            return DateTimeOffset.Parse((string)value, null, DateTimeStyles.AssumeUniversal);
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value;
        }
    }
}
