using System.Globalization;
using System.Reflection;
using System.Text;

namespace FusionOS.BuildingBlocks.Application.Csv;

/// <summary>
/// Generic reflection-based CSV serializer for list-endpoint exports (Phase M6, 2026-07-15).
/// Reuses whatever paged-query handler already ran — this only changes the serialization,
/// per 08_API_STANDARDS.md's "?format=csv" convention.
///
/// Only scalar properties are written (primitives, string, enum, Guid, decimal,
/// DateTime/DateTimeOffset, and their nullable forms). Collection/reference properties
/// (e.g. SalesOrderDto.Lines) are silently skipped — there's no generic, non-arbitrary way
/// to flatten a nested collection into a single CSV row, and guessing would be worse than
/// omitting (same "don't invent something meaningless" reasoning as the orphaned-events
/// audit and the Companies-search decision; see docs/PROJECT_TRACKER.md).
/// </summary>
public static class CsvWriter
{
    public static string Write<T>(IEnumerable<T> rows)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
            .ToArray();

        var sb = new StringBuilder();
        sb.Append(string.Join(',', properties.Select(p => Escape(p.Name))));
        sb.Append("\r\n");

        foreach (var row in rows)
        {
            sb.Append(string.Join(',', properties.Select(p => Escape(FormatValue(p.GetValue(row))))));
            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    private static bool IsScalar(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(Guid)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset);
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    /// <summary>RFC 4180 escaping — quote any field containing a comma, quote, or newline.</summary>
    private static string Escape(string field)
    {
        if (field.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return field;
        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }
}
