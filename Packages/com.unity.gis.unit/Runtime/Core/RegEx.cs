
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Regex tools to create <see cref="System.Text.RegularExpressions.Regex"/> for the <see cref="Parser"/>.
    /// </summary>
    internal static class RegEx
    {
        /// <summary>
        /// Regular expression part to find a numerical value.
        ///
        /// Supported formats:
        /// 5
        /// 5.
        /// 5.1
        /// .1
        /// 5_000
        /// 5 000
        /// 5,000
        /// 5_000.000_1
        /// 5 000,000 1
        /// </summary>
        private const string k_Digit = @"(?:[\d,\t _]*\d[,\.]?[\d,\t _]*|[,\.][\d,\t _]+)";
        // [\d,\t _]*\d          Numbers before the decimal delimiter (1; 1 000 000; 1,000,000)
        // [,\.]?                Decimal delimiter
        // [\d,\t _]*            Numbers after the decimal delimiter


        /// <summary>
        /// Regular expression which matches XML, HTML, and RichText tags
        /// </summary>
        private static readonly string k_RichText = "<.*?>";

        /// <summary>
        /// Regular expression part to find a double value.
        ///
        /// Supported formats:
        /// 5
        /// 5.
        /// 5.1
        /// .1
        /// 5_000
        /// 5 000
        /// 5,000ff
        /// 5_000.000_1
        /// 5 000,000 1
        /// 5E4
        /// 5.1E4
        /// </summary>
        private static readonly string k_Double = $@"(?:-[\t_ ]*)?{k_Digit}(?:[eE]-?\d+)?(?!{k_RichText}*)?";
        // -[\t_ ]*              Negative digit
        // [eE]-?\d[\d ]*        Exponent directives


        /// <summary>
        /// Regular expression used to convert string to a double-precision floating-point.
        ///
        /// Supported formats:
        /// 5
        /// 1.6e2
        /// 1 000E-3
        /// 5
        /// 5.
        /// 5.1
        /// .1
        /// 5_000
        /// 5 000
        /// 5,000
        /// 5_000.000_1
        /// 5 000,000 1
        /// -5
        /// 1/5
        /// 5 1/2
        /// 5.6 1.2/3.4
        /// <see cref="k_MixedNumber"/>
        /// </summary>
        private static readonly Regex Number =
            new Regex($@"(?:(?:(?<double>{k_Double}))?[ \t-]*(?:(?:{k_RichText})?(?<![\d.,_])(?<numerator>(?:-[\t_ ]*)?{k_Digit})(?:[\t ]|(?:{k_RichText}))*/(?:[\t ]|(?:{k_RichText}))*(?<denominator>{k_Digit})(?:{k_RichText})?)|{k_Double})",
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            // [ \t]*(?:(?<![\d.,_])  Space before the fraction part
            // [\t ]*\/[\t ]*           Fraction / Division symbol


        /// <summary>
        /// Regular expression used to convert string to a double-precision floating-point only if not ended by a unit.
        /// This allow to find power numbers not written in superscript format.
        /// 
        /// Supported formats:
        /// 5 cm         = 5
        /// 1.6e2 cm     = 1600
        /// 1 000E-3 cm  = 1
        /// 1 000E-3 cm2 = 1
        ///
        /// Not supported formats:
        /// 5     FAIL
        /// 1.6e2 = 1.6
        /// </summary>
        private static readonly Regex DoubleNotAtEnd =
            new Regex(
                $@"{Number}(?! */| *$)",
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));


        /// <summary>
        /// All the possible superscript numbers allowing to convert them to byte values since char numbers are not all in the same range.
        /// </summary>
        internal const string Powers = "⁰¹²³⁴⁵⁶⁷⁸⁹";
        
        /// <summary>
        /// Regex format allowing to find square and cubic words.
        /// </summary>
        private const string k_PowerWording = "(?:sq|cb|cub|squa|linear|quadratic|quartic|biquadratic|quintic|sextic|hexic|septic|heptic|octic|nonic|decic)";

        /// <summary>
        /// Find characters to not consider between the value, the unit and the power.
        /// </summary>
        private static readonly Regex Space = 
            new Regex(
                @"[\s\\\/_|\.-]", 
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        
        /// <summary>
        /// <see cref="Regex"/> that find all characters that needs an escape char as a prefix when creating a <see cref="Regex"/> from a string.
        /// </summary>
        private static readonly Regex SpecialCharacter = 
            new Regex(
            @"[\[\](){}*+?|^$\.\\]", 
            RegexOptions.Compiled, TimeSpan.FromSeconds(2));

        /// <summary>
        /// Used by <see cref="FormatTextToRegex"/> to add the escape char in front of special characters.
        /// </summary>
        private static readonly MatchEvaluator SpecialCharacterMatchEvaluator = match => $"\\{match}";
        
        /// <summary>
        /// Regular expression pattern to find the unit and power without the number value.
        /// </summary>
        private static readonly string UnitDef =
            $@"(?<prePower>[{Powers}]+|{k_PowerWording}[a-zA-Z]*)?{Space}*(?:{{0}})s?(?![a-zA-Z]){Space}*(?<postPower>[{Powers}]+|{k_PowerWording}|\d+(?=$))?";
            // ⁰¹²³⁴⁵⁶⁷⁸⁹ or cubic or sq or square...
            // (?<u1234>unit)
            // ⁰¹²³⁴⁵⁶⁷⁸⁹ or cubic or sq or square...

        /// <summary>
        /// Regular expression pattern to the value, the unit and its power.
        /// </summary>
        static readonly string Unit =
            $@"(?:(?:(?<!/)(?<value>{k_Double}))|(?:[_: -]*(?:{k_RichText})?(?<![\d.,_])(?<numeratorPost>(?:-[\t_ ]*)?{k_Digit})(?:[\t ]|(?:{k_RichText}))*/(?:[\t ]|(?:{k_RichText}))*(?<denominatorPost>{k_Digit})(?:{k_RichText})?))+[_: ]*{UnitDef}(?!{k_RichText})";

        /// <summary>
        /// Method used to build a <see cref="Regex"/> and evaluate if a string represent a <see cref="IUnitDef"/> instance.
        /// This method will remove the heading/trailing spaces add the given word and its plural.
        /// </summary>
        /// <param name="word">Word to add to the given <paramref name="list"/>.</param>
        /// <param name="index">Index where to add the given <paramref name="word"/> in the given <paramref name="list"/>.</param>
        /// <param name="list">List to populate.</param>
        private static void AddWordToArray(string word, int index, IList<string> list)
        {
            word = FormatTextToRegex(word);

            list[index] = word;
            list[index + 1] = word[word.Length - 1] == 's' ? word + '+' : word + "s+";  // s+ is added in case someone made a typo error.
        }

        /// <summary>
        /// Create a <see cref="Regex"/> pattern when searching for a single <see cref="IUnitDef"/> instance part of a string.
        /// </summary>
        /// <param name="unitDef">Unit definition to search for.</param>
        /// <returns>The string to used when creating <see cref="Regex"/>.</returns>
        internal static string CreateUnitDefRegex(IUnitDef unitDef)
        {
            string[] names = new string[unitDef.AlternateNames.Length * 2 + 5];

            AddWordToArray(unitDef.Name, 0, names);
            AddWordToArray(unitDef.NamePlural, 2, names);
            names[4] = FormatTextToRegex(unitDef.Symbol);

            for (int index = 0; index < unitDef.AlternateNames.Length; index++)
                AddWordToArray(unitDef.AlternateNames[index], index * 2 + 5, names);
            
            Array.Sort(names, (x,y) => y.Length - x.Length);
            
            return $"(?<u{unitDef.GetHashCode().ToString().Replace('-', '_')}>{string.Join("|", names)})";
        }

        /// <summary>
        /// Compile <see cref="Regex"/> instances for the given <paramref name="regex"/> within the <see cref="Unit"/> pattern.
        /// </summary>
        /// <param name="regex"><see cref="IUnitDef"/> regular expression to add with the <see cref="Unit"/> pattern.</param>
        /// <param name="caseSensitive">Returns a compiled <see cref="Regex"/> with the case sensitive flag.</param>
        /// <param name="caseInsensitive">Returns a compiled <see cref="Regex"/> with the ignore case flag.</param>
        internal static void CreateUnitRegex(string regex, out Regex caseSensitive, out Regex caseInsensitive)
        {
            string formatted = string.Format(Unit, regex);
            
            caseSensitive = new Regex(formatted, 
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            caseInsensitive = new Regex(formatted, 
                RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        }
        
        /// <summary>
        /// Format the given <paramref name="text"/> to be compatible with <see cref="Regex"/> format.
        /// </summary>
        /// <param name="text">String to format to <see cref="Regex"/> compliant.</param>
        /// <returns>The formatted string.</returns>
        private static string FormatTextToRegex(string text)
        {
            text = Space.Replace(
                text.Trim(), 
                @"[_\W]?");  // If the unit has multiple words, set the spaces as optional or be a none letter.

            return SpecialCharacter.Replace(text, SpecialCharacterMatchEvaluator);
        }
        
        /// <summary>
        /// Get if the given <paramref name="character"/> is a superscript character (⁰¹²³⁴⁵⁶⁷⁸⁹).
        /// </summary>
        /// <param name="character">Character to evaluate if its a character.</param>
        /// <returns>true if the given <paramref name="character"/> is a superscript character; false otherwise.</returns>
        internal static bool IsSuperscript(char character) =>
            Powers.Contains(character.ToString());

        /// <summary>
        /// Evaluate the <see cref="Double"/> <see cref="Regex"/>.
        /// </summary>
        /// <param name="text">String to evaluate the <see cref="Regex"/> with.</param>
        /// <returns>The <see cref="Match"/> result of the evaluated <see cref="Double"/>.</returns>
        internal static Match MatchDouble(string text) =>
            Number.Match(text);

        /// <summary>
        /// Evaluate the <see cref="DoubleNotAtEnd"/> <see cref="Regex"/>.
        /// </summary>
        /// <param name="text">String to evaluate the <see cref="Regex"/> with.</param>
        /// <returns>
        /// The <see cref="MatchCollection"/> result of the evaluated <see cref="DoubleNotAtEnd"/> allowing
        /// to find all <see cref="UnitDef"/> values part of a string.
        /// </returns>
        internal static MatchCollection MatchDoubles(string text) =>
            DoubleNotAtEnd.Matches(text);

        /// <summary>
        /// Evaluate the <see cref="Number"/> <see cref="Regex"/>.
        /// </summary>
        /// <param name="text">String to evaluate the <see cref="Regex"/> with.</param>
        /// <returns>The <see cref="MatchCollection"/> result of the evaluated <see cref="Number"/> allowing
        /// to find all <see cref="UnitDef"/> values part of a string.</returns>
        internal static MatchCollection MatchNumbers(string text) =>
            Number.Matches(text);

        /// <summary>
        /// Convert a superscript character to integer.
        /// </summary>
        /// <param name="character">Character part of ⁰¹²³⁴⁵⁶⁷⁸⁹ to be converted.</param>
        /// <returns>The converted superscript.</returns>
        internal static int SuperscriptToInt(char character) =>
            Powers.IndexOf(character);
    }
}
