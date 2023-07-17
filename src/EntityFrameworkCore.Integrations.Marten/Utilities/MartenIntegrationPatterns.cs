using System.Text.RegularExpressions;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static partial class MartenIntegrationPatterns
{
    public static readonly Regex JsonPropertyExtraction = JsonPropertyExtractionPattern();
    public static readonly Regex JsonBColumnNameExtraction = JsonBColumnNameExtractionPattern();
    public static readonly Regex SequenceStartValueFromInsert = SequenceStartValueFromInsertPattern();
    [GeneratedRegex("(\\w+)\\s*->>\\s*'[^']+'")]
    private static partial Regex JsonBColumnNameExtractionPattern();
    [GeneratedRegex("->>\\s*'([^']+)'")]
    private static partial Regex JsonPropertyExtractionPattern();
    [GeneratedRegex(@"START (\d+)")]
    private static partial Regex SequenceStartValueFromInsertPattern();
}