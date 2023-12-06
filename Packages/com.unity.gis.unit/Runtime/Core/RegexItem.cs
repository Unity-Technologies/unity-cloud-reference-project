
using System.Globalization;
using System.Text.RegularExpressions;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Collection of regular expression for a single <see cref="IUnitDef"/> type.
    /// All instances of this <see cref="IUnitDef"/> type will be part of this <see cref="RegexItem"/> allowing
    /// to create a single <see cref="Regex"/> instance that enable you to decode units from a string.
    /// </summary>
    internal readonly struct RegexItem
    {
        
        /// <summary>
        /// <see cref="Regex"/> string <see cref="UnitDef{T}"/> part allowing to figure out what if the unit.
        /// </summary>
        internal readonly string UnitRegex;
        
        /// <summary>
        /// Compiled <see cref="Regex"/> with the case sensitive flag.
        /// This is the first <see cref="Regex"/> to evaluate since <see cref="UnitDef{T}"/>.<see cref="UnitDef{T}.Symbol"/> can be the
        /// same if you don't consider the casing.
        /// </summary>
        internal readonly Regex CaseSensitive;
        
        /// <summary>
        /// Compiled <see cref="Regex"/> with the ignore case flag.
        /// This <see cref="Regex"/> is only evaluated if the <see cref="CaseSensitive"/> does not match.
        /// </summary>
        internal readonly Regex CaseInsensitive;
        
        /// <summary>
        /// All the <see cref="CaseSensitive"/> group names allowing to link the <see cref="Match"/> result with
        /// the corresponding <see cref="IUnitDef"/> based on its hash code.
        /// </summary>
        /// <remarks>
        /// This is used since <see cref="Group"/>.Name is only in .NET 5.
        /// </remarks>
        internal readonly string[] GroupNames;


        /// <summary>
        /// Construct a new <see cref="RegexItem"/> instance based on a <see cref="Regex"/> formatted string.
        /// </summary>
        /// <param name="unitRegex">
        /// String used to create <see cref="Regex"/> instances.
        /// This string is the <see cref="IUnitDef"/> <see cref="Regex"/> part. The <see cref="RegexItem"/> will
        /// create two <see cref="Regex"/> allowing to find the corresponding <see cref="IUnitDef"/> based on this string
        /// and also the value and power (not part of the given string).
        /// </param>
        internal RegexItem(string unitRegex)
        {
            UnitRegex = unitRegex;
            RegEx.CreateUnitRegex(UnitRegex, out CaseSensitive, out CaseInsensitive);
            GroupNames = CaseSensitive.GetGroupNames();
        }

        /// <summary>
        /// Return the <see cref="Regex"/> pattern given at initialization.
        /// </summary>
        /// <returns>The <see cref="IUnitDef"/> <see cref="Regex"/> part with the class type as the prefix.</returns>
        public override string ToString() => 
            $"RegexItem<{UnitRegex}>";


        /// <summary>
        /// Extract the <see cref="IUnit"/>.<see cref="IUnit.Power"/> part of a <see cref="Regex"/>.<see cref="Regex.Match(string)"/> result.
        /// </summary>
        /// <param name="groups">All the <see cref="Group"/> instances part of the <see cref="Regex"/>.<see cref="Regex.Match(string)"/> result.</param>
        /// <returns>All the <see cref="Group"/> of a matching <see cref="Regex"/>.</returns>
        internal static byte GetPower(GroupCollection groups)
        {
            byte output = ParsePower(groups["postPower"].Value);

            if (output == 0)
                output = ParsePower(groups["prePower"].Value);
            
            return output;
        }

        /// <summary>
        /// Extract the <see cref="IUnit"/>.<see cref="IUnit.Value"/> part of a <see cref="Regex"/>.<see cref="Regex.Match(string)"/> result.
        /// </summary>
        /// <param name="groups">All the <see cref="Group"/> instances part of the <see cref="Regex"/>.<see cref="Regex.Match(string)"/> result.</param>
        /// <returns>All the <see cref="Group"/> of a matching <see cref="Regex"/>.</returns>
        internal static double GetValue(GroupCollection groups) =>
            (string.IsNullOrWhiteSpace(groups["value"].Value))? // Value may be empty if input is fractional number without an integer
                0.0 :
                Parser.FromResultToDouble(groups["value"].Value);

        /// <summary>
        /// Convert text to byte considering the characters to be either digits, superscript digits (⁰¹²³⁴⁵⁶⁷⁸⁹) or a word representing a power value.
        /// </summary>
        /// <param name="text">Text with power directives to convert.</param>
        /// <returns>The text converted to byte.</returns>
        private static byte ParsePower(string text)
        {
            text = text.Trim();
            
            if (text == "")
                return 0;

            return RegEx.IsSuperscript(text[0]) 
                ? SuperscriptToByte(text) 
                : WordToByte(text);
        }

        /// <summary>
        /// Convert superscript numbers (⁰¹²³⁴⁵⁶⁷⁸⁹) to byte.
        /// </summary>
        /// <param name="text">Text with power directives to convert.</param>
        /// <returns>The text converted to byte.</returns>
        private static byte SuperscriptToByte(string text)
        {
            string[] output = new string[text.Length];

            for (int index = 0; index < text.Length; index++) 
                output[index] = RegEx.SuperscriptToInt(text[index]).ToString();

            return (byte)int.Parse(string.Join("", output), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Find word representing power directives (square, cubic, linear...).
        /// </summary>
        /// <param name="text">Text with power directives to convert.</param>
        /// <returns>The text converted to byte.</returns>
        private static byte WordToByte(string text) =>
            text.ToLower() switch
            {
                "square" => 2,
                "quadratic" => 2,
                "cubic" => 3,
                "linear" => 1,
                "quartic" => 4,
                "biquadratic" => 4,
                "quintic" => 5,
                "sextic" => 6,
                "hexic" => 6,
                "septic" => 7,
                "heptic" => 7,
                "octic" => 8,
                "nonic" => 9,
                "decic" => 10,
                _ => text[0] switch
                {
                    's' => 2,
                    'S' => 2,
                    'c' => 3,
                    'C' => 3,
                    'q' => 2,
                    'Q' => 2,
                    'l' => 1,
                    'L' => 1,
                    'o' => 8,
                    'O' => 8,
                    'n' => 9,
                    'N' => 9,
                    'd' => 10,
                    'D' => 10,
                    _ => byte.Parse(text)
                }
            };

    }
}
