using System;
using System.Text.RegularExpressions;

namespace Modules.Acl.Internal
{
    public static class AccessControlIdentifier
    {
        private static readonly Regex IdentifierRegex;

        static AccessControlIdentifier()
        {
            IdentifierRegex = new Regex("^[A-Za-z0-9]+$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        internal static string Clean(string identier)
        {
            if (string.IsNullOrWhiteSpace(identier))
            {
                throw new ArgumentException(
                    "Argument 'identier' must not be blank, whitespace " +
                    "only, or empty.");
            }

            if (false == IdentifierRegex.IsMatch(identier))
            {
                throw new ArgumentException(
                    "Argument 'identifier' must contain alphanumeric " +
                    "characters only. No spaces, hyphens or other special " +
                    "characters are allowed.");
            }

            return identier.ToLowerInvariant();
        }
    }
}