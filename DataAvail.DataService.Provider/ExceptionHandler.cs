using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Services;
using System.Linq;
using System.Text;

namespace DataAvail.DataService.Provider
{
    public static class ExceptionHandler
    {
        public static void HandleException(HandleExceptionArgs args)
        {
            if (args.Exception.GetType() == typeof(DbEntityValidationException))
            {
                var ex = args.Exception as DbEntityValidationException;
                args.Exception = new DataServiceException(500, ex.Message);
            }
            else
            {
                var message = GetExceptionMessgae(args.Exception);
                if (message != null)
                {
                    args.Exception = new DataServiceException(500, message);
                }
            }
        }

        private static string GetExceptionMessgae(Exception Exception)
        {
            var message = Exception.Message;

            if (Exception.InnerException is System.Data.UpdateException)
            {
                message = Errors.SQL_UPDATE_FAIL;

                if (Exception.InnerException.InnerException is System.Data.SqlClient.SqlException)
                {
                    var msg = GetSqlExceptionMessage((System.Data.SqlClient.SqlException)Exception.InnerException.InnerException);

                    if (msg != null)
                        message = msg;
                }
            }

            return message;
        }

        private static  string GetSqlExceptionMessage(System.Data.SqlClient.SqlException Exception)
        {
            switch (Exception.Number)
            {
                case (int)SqlExceptionNumber.DeleteRef:
                    return Errors.SQL_DELETE_REF_FAIL;
            }

            return null;
        }

        private enum SqlExceptionNumber
        {
            DeleteRef = 547
        }

    }
}
