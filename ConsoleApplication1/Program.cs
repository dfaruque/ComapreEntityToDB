using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Serenity.Data.Mapping;
using Serenity.Data;
using Serenity.Data.Schema;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.GetAssembly(typeof(Serene.Northwind.Entities.CategoryRow));

            var entityClasses = assembly.GetTypes().Where(w => w.GetCustomAttribute<TableNameAttribute>() != null);

            foreach (var entityClass in entityClasses)
            {
                var tableName = entityClass.GetCustomAttribute<TableNameAttribute>().Name;
                Console.WriteLine(tableName);

                Row row = (Row)Activator.CreateInstance(entityClass);

                var fields = row.GetFields();

                for (int i = 0; i < row.FieldCount; i++)
                {
                    Field field = fields[i];
                    if (EntityFieldExtensions.IsTableField(field))
                    {
                        var strnull = field.Flags.HasFlag(FieldFlags.NotNull) ? "NotNull" : "Null";
                        Console.WriteLine($"\t{field.Type.ToString()} {field.Name} {strnull}");
                    }
                }

            }

            Console.WriteLine();

            using (var connection = SqlConnections.New(@"Data Source=(LocalDb)\MSSqlLocalDB; Initial Catalog=Serene_Northwind_v1; Integrated Security=True", "System.Data.SqlClient"))
            {
                connection.Open();

                var schemaProvider = SchemaHelper.GetSchemaProvider(connection.GetDialect().ServerType);
                foreach (var t in schemaProvider.GetTableNames(connection))
                {
                    Console.WriteLine(t.Tablename);
                }
            }
            Console.ReadKey();

        }
    }

    public class SchemaHelper
    {
        public static ISchemaProvider GetSchemaProvider(string serverType)
        {
            var providerType = Type.GetType("Serenity.Data.Schema." + serverType + "SchemaProvider, Serenity.Data");
            if (providerType == null || !typeof(ISchemaProvider).GetTypeInfo().IsAssignableFrom(providerType))
                throw new ArgumentOutOfRangeException("serverType", (object)serverType, "Unknown server type");

            return (ISchemaProvider)Activator.CreateInstance(providerType);
        }

        private static Dictionary<string, string> SqlTypeToFieldTypeMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "bigint", "Int64" },
                { "bit", "Boolean" },
                { "blob sub_type 1", "String" },
                { "char", "String" },
                { "character varying", "String" },
                { "character", "String" },
                { "date", "DateTime" },
                { "datetime", "DateTime" },
                { "datetime2", "DateTime" },
                { "datetimeoffset", "DateTimeOffset" },
                { "decimal", "Decimal" },
                { "double", "Double" },
                { "doubleprecision", "Double" },
                { "float", "Double" },
                { "guid", "Guid" },
                { "int", "Int32" },
                { "int4", "Int32" },
                { "int8", "Int64" },
                { "integer", "Int32" },
                { "money", "Decimal" },
                { "nchar", "String" },
                { "ntext", "String" },
                { "numeric", "Decimal" },
                { "nvarchar", "String" },
                { "real", "Single" },
                { "rowversion", "ByteArray" },
                { "smalldatetime", "DateTime" },
                { "smallint", "Int16" },
                { "text", "String" },
                { "time", "TimeSpan" },
                { "timestamp", "DateTime" },
                { "timestamp without time zone", "DateTime" },
                { "timestamp with time zone", "DateTimeOffset" },
                { "tinyint", "Int16" },
                { "uniqueidentifier", "Guid" },
                { "varbinary", "Stream" },
                { "varchar", "String" }
            };

        public static string SqlTypeNameToFieldType(string sqlTypeName, int size, out string dataType)
        {
            dataType = null;
            string fieldType;
            sqlTypeName = sqlTypeName.ToLowerInvariant();

            if (sqlTypeName == "varbinary")
            {
                if (size == 0 || size > 256)
                    return "Stream";

                dataType = "byte[]";
                return "ByteArray";
            }
            else if (sqlTypeName == "timestamp" || sqlTypeName == "rowversion")
            {
                dataType = "byte[]";
                return "ByteArray";
            }
            else if (SqlTypeToFieldTypeMap.TryGetValue(sqlTypeName, out fieldType))
                return fieldType;
            else
                return "Stream";
        }
    }

}
