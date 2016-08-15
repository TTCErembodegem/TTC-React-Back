using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using log4net;

namespace Ttc.WebApi.Utilities
{
    public class TtcExceptionFilterAttribute: ExceptionFilterAttribute
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExceptionFilterAttribute));

        public override void OnException(HttpActionExecutedContext context)
        {
            if (TooManyConnections(context.Exception))
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent(TooManyConnectionsExMessage),
                    ReasonPhrase = TooManyConnectionsExMessage
                };
            }
            else
            {
                Logger.Error(context.Exception.ToString());

                context.Response = new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent(context.Exception.ToString()),
                    ReasonPhrase = "Something went terribly wrong!"
                };
            }
        }

        private const string TooManyConnectionsExMessage = "Too many connections";
        private const string OtherExMessage = "has exceeded the 'max_user_connections' resource";

        //ERROR System.Web.Http.Filters.ExceptionFilterAttribute [(null)] – System.Data.Entity.Core.EntityException: The underlying provider failed on Open. 
        // ---> MySql.Data.MySqlClient.MySqlException: error connecting: Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  
        // This may have occurred because all pooled connections were in use and max pool size was reached.

        // ERROR System.Web.Http.Filters.ExceptionFilterAttribute [(null)] – System.Data.Entity.Core.EntityCommandExecutionException: An error occurred while executing the 
        // command definition. See the inner exception for details. 
        // ---> MySql.Data.MySqlClient.MySqlException: Fatal error encountered during command execution. 
        // ---> MySql.Data.MySqlClient.MySqlException: Fatal error encountered attempting to read the resultset. 
        // ---> MySql.Data.MySqlClient.MySqlException: Reading from the stream has failed. 
        // ---> System.IO.IOException: The read operation failed, see inner exception. 
        // ---> System.TimeoutException: Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. 
        // ---> System.IO.IOException:   Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. 
        // ---> System.Net.Sockets.SocketException:                                         A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond

        private static bool TooManyConnections(Exception ex)
        {
            Func<string, bool> isTooManyConnections = msg => msg == TooManyConnectionsExMessage || msg.Contains(OtherExMessage);

            string fullErrorName = ex.GetType().FullName;
            if (fullErrorName == "System.Data.Entity.Core.EntityException")
            {
                if (ex.InnerException != null) 
                {
                    if (isTooManyConnections(ex.InnerException.Message))
                    {
                        return true;
                    }

                    if (ex.InnerException.InnerException != null && isTooManyConnections(ex.InnerException.InnerException.Message))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}