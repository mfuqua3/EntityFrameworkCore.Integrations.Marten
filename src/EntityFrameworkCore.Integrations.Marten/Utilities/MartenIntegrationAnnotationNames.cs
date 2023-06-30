using Weasel.Postgresql.Tables;
using Index = Marten.Schema.ComputedIndex;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public static class MartenIntegrationAnnotationNames
{
    public static class ComputedIndex
    {
        private const string Category = "MartenComputedIndex";
        public const string Type = "MartenIndexType";
        public static string Casing = string.Join(':', Category, nameof(Index.Casing));
        public static string Collation = string.Join(':', Category, nameof(Index.Collation));
        public static string Columns = string.Join(':', Category, nameof(Index.Columns));
        public static string CustomMethod = string.Join(':', Category, nameof(Index.CustomMethod));
        public static string IncludeColumns = string.Join(':', Category, nameof(Index.IncludeColumns));
        public static string IsConcurrent = string.Join(':', Category, nameof(Index.IsConcurrent));
        public static string IsUnique = string.Join(':', Category, nameof(Index.IsUnique));
        public static string Mask = string.Join(':', Category, nameof(Index.Mask));
        public static string Method = string.Join(':', Category, nameof(Index.Method));
        public static string Name = string.Join(':', Category, nameof(Index.Name));
        public static string NullsSortOrder = string.Join(':', Category, nameof(Index.NullsSortOrder));
        public static string Predicate = string.Join(':', Category, nameof(Index.Predicate));
        public static string SortOrder = string.Join(':', Category, nameof(Index.SortOrder));
        public static string TableSpace = string.Join(':', Category, nameof(Index.TableSpace));
        public static string FillFactor = string.Join(':', Category, nameof(Index.FillFactor));
    }
}