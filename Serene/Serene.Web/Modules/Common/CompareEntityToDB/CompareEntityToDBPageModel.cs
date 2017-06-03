
using Serene.Northwind.Entities;
using Serenity;
using Serenity.Data;
using Serenity.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Serene.Common
{
    public class CompareEntityToDBPageModel
    {
        public List<string> Issues { get; set; } = new List<string>();

        public CompareEntityToDBPageModel()
        {
            var assembly = Assembly.GetAssembly(typeof(CategoryRow));

            var rowClasses = assembly.GetTypes().Where(w => w.GetCustomAttribute<ConnectionKeyAttribute>() != null && w.IsSealed);


            foreach (var rowClass in rowClasses)
            {
                Row row = (Row)Activator.CreateInstance(rowClass);

                var connectionKey = rowClass.GetCustomAttribute<ConnectionKeyAttribute>().Value;

                using (var connection = SqlConnections.NewByKey(connectionKey))
                {
                    var schemaProvider = SchemaHelper.GetSchemaProvider(connection.GetDialect().ServerType);

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
                            string strNull = rowfield.Flags.HasFlag(FieldFlags.NotNull) ? "[NotNull]" : "";

                            if (dbField == null)
                                Issues.Add($"{Issues.Count + 1}. {strNull} {rowfield.Type} {rowfield.Name} <span class=\"label label-danger\">no corresponding field in database</span> at Table:  {row.Table}");
                            else
                            {
                                string strTypeMismatch = rowfield.Type.ToString() == SchemaHelper.SqlTypeNameToFieldType(dbField.DataType, dbField.Size) ?
                                    "" : "DataType Mismatch";

                                string strNullableMismatch = dbField.IsNullable == false && rowfield.Flags.HasFlag(FieldFlags.NotNull) == false ?
                                    "Nullable Mismatch" : "";

                                if (!strNullableMismatch.IsEmptyOrNull() || !strTypeMismatch.IsEmptyOrNull())
                                    Issues.Add($"{Issues.Count + 1}. {strNull} {rowfield.Type} {rowfield.Name} "
                                    + $"<span class=\"label label-danger\">{strTypeMismatch} {strNullableMismatch}</span> at Table: {row.Table}");
                            }
                        }
                    }

                }


            }
        }
    }
}