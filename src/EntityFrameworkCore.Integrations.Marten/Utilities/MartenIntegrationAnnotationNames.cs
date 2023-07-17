using Weasel.Postgresql.Tables;
using Index = Marten.Schema.ComputedIndex;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

public static class MartenIntegrationAnnotationNames
{
    public static class ComputedIndex
    {
        private const string Category = "MartenComputedIndex";
        public const string Type = "MartenIndexType";
        public static readonly string Casing = string.Join(':', Category, nameof(Index.Casing));
        public static readonly string Collation = string.Join(':', Category, nameof(Index.Collation));
        public static readonly string Columns = string.Join(':', Category, nameof(Index.Columns));
        public static readonly string CustomMethod = string.Join(':', Category, nameof(Index.CustomMethod));
        public static readonly string IncludeColumns = string.Join(':', Category, nameof(Index.IncludeColumns));
        public static readonly string IsConcurrent = string.Join(':', Category, nameof(Index.IsConcurrent));
        public static readonly string IsUnique = string.Join(':', Category, nameof(Index.IsUnique));
        public static readonly string Mask = string.Join(':', Category, nameof(Index.Mask));
        public static readonly string Method = string.Join(':', Category, nameof(Index.Method));
        public static string Name = string.Join(':', Category, nameof(Index.Name));
        public static readonly string NullsSortOrder = string.Join(':', Category, nameof(Index.NullsSortOrder));
        public static readonly string Predicate = string.Join(':', Category, nameof(Index.Predicate));
        public static readonly string SortOrder = string.Join(':', Category, nameof(Index.SortOrder));
        public static readonly string TableSpace = string.Join(':', Category, nameof(Index.TableSpace));
        public static readonly string FillFactor = string.Join(':', Category, nameof(Index.FillFactor));
    }

    public const string EntityManagement = "EntityManagement";
}