using System.Collections.Generic;

namespace DBManager.Model
{
    public class DBTable
    {
        public string? TableName { get; set; }
        public string? TableSchema { get; set; }
        public List<DBTableColumn> Columns { get; set; } = new List<DBTableColumn>();
        public List<string> ForeignKeys { get; set; } = new List<string>();
        public List<string> Indexes { get; set; } = new List<string>();
    }
}
