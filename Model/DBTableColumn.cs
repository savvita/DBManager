


namespace DBManager.Model
{
    public class DBTableColumn
    {
        public string? ColumnName { get; set; }
        public string? ColumnDataType { get; set; }
        public int? CharacterMaximumLength { get; set; }
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsNullable { get; set; }
    }
}
