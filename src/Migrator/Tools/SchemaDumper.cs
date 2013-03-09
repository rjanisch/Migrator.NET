#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Migrator.Framework;

namespace Migrator.Tools
{
	public class SchemaDumper
	{
	    private readonly ITransformationProvider _provider;

		public SchemaDumper(string provider, string connectionString, string defaultSchema)
		{
			_provider = ProviderFactory.Create(provider, connectionString, defaultSchema);
		}

		public string Dump()
		{
			var writer = new StringWriter();

			writer.WriteLine("using Migrator;\n");
			writer.WriteLine("[Migration(1)]");
			writer.WriteLine("public class SchemaDump : Migration");
			writer.WriteLine("{");
			writer.WriteLine("\tpublic override void Up()");
			writer.WriteLine("\t{");

			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tDatabase.AddTable(\"{0}\",", table);
				var columnLines = new List<string>();
				foreach (Column column in _provider.GetColumns(table))
				{
                    if (column.Size>0)
				        columnLines.Add(string.Format("\t\t\tnew Column(\"{0}\", DbType.{1}, {2}, {3})", column.Name, column.Type, column.Size, getColumnPropertyString(column.ColumnProperty)));
                    else
                        columnLines.Add(string.Format("\t\t\tnew Column(\"{0}\", DbType.{1}, {2})", column.Name, column.Type, getColumnPropertyString(column.ColumnProperty)));
				}
				writer.WriteLine(string.Join(string.Format(",{0}", Environment.NewLine), columnLines.ToArray()));
				writer.WriteLine("\t\t);");
                writer.WriteLine("");
			}

			writer.WriteLine("\t}\n");
			writer.WriteLine("\tpublic override void Down()");
			writer.WriteLine("\t{");

			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tDatabase.RemoveTable(\"{0}\");", table);
                writer.WriteLine("");
			}

			writer.WriteLine("\t}");
			writer.WriteLine("}");

			return writer.ToString();
		}

        private string getColumnPropertyString(ColumnProperty prp)
        {
            string retVal = "";
            if ((prp & ColumnProperty.ForeignKey) > 0) retVal += "ColumnProperty.ForeignKey | ";
            if ((prp & ColumnProperty.Identity) > 0) retVal += "ColumnProperty.Identity | ";
            if ((prp & ColumnProperty.Indexed) > 0) retVal += "ColumnProperty.Indexed | ";
            if ((prp & ColumnProperty.NotNull) > 0) retVal += "ColumnProperty.NotNull | ";
            if ((prp & ColumnProperty.Null) > 0) retVal += "ColumnProperty.Null | ";
            if ((prp & ColumnProperty.PrimaryKey) > 0) retVal += "ColumnProperty.PrimaryKey | ";
            if ((prp & ColumnProperty.PrimaryKeyWithIdentity) > 0) retVal += "ColumnProperty.PrimaryKeyWithIdentity | ";
            if ((prp & ColumnProperty.Unique) > 0) retVal += "ColumnProperty.Unique | ";
            if ((prp & ColumnProperty.Unsigned) > 0) retVal += "ColumnProperty.Unsigned | ";

            if (retVal != "") retVal = retVal.Substring(0, retVal.Length - 3);

            if (retVal == "") retVal = "ColumnProperty.None";

            return retVal;
        }

		public void DumpTo(string file)
		{
			using (var writer = new StreamWriter(file))
			{
				writer.Write(Dump());
			}
		}
	}
}