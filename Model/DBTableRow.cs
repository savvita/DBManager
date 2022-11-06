using System.Collections.Generic;

namespace DBManager.Model
{
    public class DBTableRow
    {
        public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();

        public DBTableRow(DBTable table)
        {
            foreach(DBTableColumn column in table.Columns)
            {
                Values.Add(column.ColumnName!, null);
            }
        }
    }
}
