using Ganss.Xss;

namespace EstateHub.SharedKernel.Helpers;

/// <summary>
/// Helper class for sanitizing HTML content to prevent XSS attacks
/// </summary>
public static class HtmlSanitizerHelper
{
    private static readonly HtmlSanitizer Sanitizer = new();

    static HtmlSanitizerHelper()
    {
        // Configure allowed HTML tags for rich text content
        // Allow basic formatting tags
        Sanitizer.AllowedTags.UnionWith(new[]
        {
            "p", "br", "strong", "b", "em", "i", "u", "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "blockquote", "pre", "code", "hr"
        });

        // Allow basic attributes
        Sanitizer.AllowedAttributes.UnionWith(new[]
        {
            "class", "style"
        });

        // Allow safe CSS properties in style attributes
        Sanitizer.AllowedCssProperties.UnionWith(new[]
        {
            "color", "background-color", "text-align", "font-weight", "font-style",
            "text-decoration", "margin", "padding"
        });

        // Remove dangerous protocols
        Sanitizer.AllowedSchemes.Clear();
        Sanitizer.AllowedSchemes.Add("http");
        Sanitizer.AllowedSchemes.Add("https");
    }

    /// <summary>
    /// Sanitizes HTML content, removing potentially dangerous elements and attributes
    /// </summary>
    /// <param name="html">The HTML content to sanitize</param>
    /// <returns>Sanitized HTML content</returns>
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return Sanitizer.Sanitize(html);
    }

    /// <summary>
    /// Sanitizes HTML content for rich text descriptions (allows more formatting)
    /// </summary>
    /// <param name="html">The HTML content to sanitize</param>
    /// <returns>Sanitized HTML content</returns>
    public static string SanitizeRichText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return Sanitizer.Sanitize(html);
    }
}

