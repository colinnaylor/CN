
using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.Serialization;
namespace BBFieldValueRetrieverTests
{
    public class SqlExceptionCreator
    {
        public static SqlException CreateSqlException(string errorMessage, int errorNumber)
        {
            SqlErrorCollection collection = GetErrorCollection();
            SqlError error = GetError(errorNumber, errorMessage);

            MethodInfo addMethod = collection.GetType().
                GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            addMethod.Invoke(collection, new object[] { error });

            Type[] types = new Type[] { typeof(string), typeof(SqlErrorCollection) };
            object[] parameters = new object[] { errorMessage, collection };

            ConstructorInfo constructor = typeof(SqlException).
                GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            SqlException exception = (SqlException)constructor.Invoke(parameters);

            return exception;
        }

        private static SqlError GetError(int errorCode, string message)
        {
            object[] parameters = new object[] {
            errorCode, (byte)0, (byte)10, "server", message, "procedure", 0 };
            Type[] types = new Type[] {
            typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string),
            typeof(string), typeof(int) };

            ConstructorInfo constructor = typeof(SqlError).
                GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
            SqlError error = (SqlError)constructor.Invoke(parameters);
            return error;
        }

        private static SqlErrorCollection GetErrorCollection()
        {
            ConstructorInfo constructor = typeof(SqlErrorCollection).
                GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            SqlErrorCollection collection = (SqlErrorCollection)constructor.Invoke(new object[] { });
            return collection;
        }
    }
}