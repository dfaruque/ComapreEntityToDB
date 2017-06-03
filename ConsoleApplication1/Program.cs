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

            var rowClasses = assembly.GetTypes().Where(w => w.GetCustomAttribute<TableNameAttribute>() != null);

            using (var connection = SqlConnections.New(@"Data Source=(LocalDb)\MSSqlLocalDB; Initial Catalog=Serene_Northwind_v1; Integrated Security=True", "System.Data.SqlClient"))
            {
                connection.Open();
                var schemaProvider = SchemaHelper.GetSchemaProvider(connection.GetDialect().ServerType);

                foreach (var rowClass in rowClasses)
                {

                    Row row = (Row)Activator.CreateInstance(rowClass);
                    Console.WriteLine(row.Table);
                    var tableName = row.Table;
                    string schema = null;
                    if (tableName.IndexOf('.') > 0)
                    {
                        schema = tableName.Substring(0, tableName.IndexOf('.'));
                        tableName = tableName.Substring(tableName.IndexOf('.') + 1);
                    }

                    var rowFields = row.GetFields();
                    var dbFields = schemaProvider.GetFieldInfos(connection, schema ?? "dbo", tableName);

                    for (int i = 0; i < row.FieldCount; i++)
                    {
                        Field rowfield = rowFields[i];
                        if (EntityFieldExtensions.IsTableField(rowfield))
                        {
                            var dbField = dbFields.FirstOrDefault(f => f.FieldName == rowfield.Name);
                            string strNull = rowfield.Flags.HasFlag(FieldFlags.NotNull) ? "NOT NULL" : "NULL";

                            if (dbField == null)
                                Console.WriteLine($"\t{rowfield.Type} {rowfield.Name} {strNull} \t\t no corresponding field in database!");
                            else
                            {
                                string strTypeMismatch = rowfield.Type.ToString() == SchemaHelper.SqlTypeNameToFieldType(dbField.DataType, dbField.Size) ?
                                    "" : "datatype mismatch";

                                string strNullableMismatch = rowfield.Flags.HasFlag(FieldFlags.NotNull) == !dbField.IsNullable ?
                                    "" : "nullable mismatch";


                                Console.WriteLine($"\t{rowfield.Type} {rowfield.Name} {strNull}"
                                + $"\t\t {strTypeMismatch}\t{strNullableMismatch}");
                            }
                        }
                    }
                    Console.WriteLine();


                }


                connection.Close();
            }

            Console.ReadKey();

        }
    }


}
