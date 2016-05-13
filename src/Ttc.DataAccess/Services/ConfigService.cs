using MySql.Data.MySqlClient;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess.Services
{
    [MaxMysqlConnectionExceptionHandlerAspect]
    public class ConfigService
    {
        public object Get()
        {
            return null;
        }
    }
}