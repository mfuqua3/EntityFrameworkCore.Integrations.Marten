using System.Text.RegularExpressions;

namespace EntityFrameworkCore.Integrations.Marten.Utilities;

internal static partial class MartenIntegrationPatterns
{
    public static readonly Regex JsonPropertyExtraction = JsonPropertyExtractionPattern();
    public static readonly Regex JsonBColumnNameExtraction = JsonBColumnNameExtractionPattern();
    [GeneratedRegex("(\\w+)\\s*->>\\s*'[^']+'")]
    private static partial Regex JsonBColumnNameExtractionPattern();
    [GeneratedRegex("->>\\s*'([^']+)'")]
    private static partial Regex JsonPropertyExtractionPattern();
}