using System;
using System.Linq;
using System.Text;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///     Applies the same masking and length policy to every audit sink field.
/// </summary>
public sealed class AuditLogSanitizer
{
    private const string Redacted = "[REDACTED]";
    private readonly int _maxValueLength;
    private readonly string[] _sensitiveNames;

    /// <summary>
    ///     Creates a sanitizer from Loki audit options.
    /// </summary>
    public AuditLogSanitizer(LokiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.MaxValueLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxValueLength));
        }

        _maxValueLength = options.MaxValueLength;
        _sensitiveNames = options.SensitivePropertyNames ?? [];
    }

    /// <summary>
    ///     Masks sensitive property values and caps all other values.
    /// </summary>
    public string SanitizeValue(string propertyName, string value)
    {
        if (IsSensitive(propertyName))
        {
            return Redacted;
        }

        return Truncate(value ?? string.Empty);
    }

    /// <summary>
    ///     Masks sensitive URL query values without logging their decoded form.
    /// </summary>
    public string SanitizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        var questionMark = url.IndexOf('?');
        if (questionMark < 0)
        {
            return Truncate(url);
        }

        var result = new StringBuilder(url[..(questionMark + 1)]);
        var query = url[(questionMark + 1)..];
        var fragmentIndex = query.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? query[fragmentIndex..] : string.Empty;
        if (fragmentIndex >= 0)
        {
            query = query[..fragmentIndex];
        }

        var first = true;
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!first)
            {
                result.Append('&');
            }

            first = false;
            var equals = pair.IndexOf('=');
            var rawName = equals < 0 ? pair : pair[..equals];
            var name = Uri.UnescapeDataString(rawName.Replace('+', ' '));
            result.Append(rawName);
            if (equals >= 0)
            {
                result.Append('=').Append(IsSensitive(name) ? Redacted : pair[(equals + 1)..]);
            }
        }

        result.Append(fragment);
        return Truncate(result.ToString());
    }

    private bool IsSensitive(string propertyName)
    {
        var normalized = new string((propertyName ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
        return _sensitiveNames.Any(candidate =>
            !string.IsNullOrWhiteSpace(candidate) &&
            normalized.Contains(new string(candidate.Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant).ToArray()), StringComparison.Ordinal));
    }

    private string Truncate(string value)
    {
        if (value.Length <= _maxValueLength)
        {
            return value;
        }

        return _maxValueLength <= 3 ? value[.._maxValueLength] : value[..(_maxValueLength - 3)] + "...";
    }
}
