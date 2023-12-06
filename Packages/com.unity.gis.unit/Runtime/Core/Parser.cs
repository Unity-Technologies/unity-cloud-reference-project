
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Toolset to decode string to <see cref="Unit"/> instance.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// true if <see cref="RegisterUnitTypes"/> was called once; false otherwise.
        /// This allow to compile the global <see cref="System.Text.RegularExpressions.Regex"/> only once.
        /// </summary>
        private static bool s_RegisteredGlobal;
        
        /// <summary>
        /// Registered <see cref="RegexItem"/> by <see cref="UnitDef{T}"/> type allowing to decode string with only one type.
        /// </summary>
        private static readonly Dictionary<Type, RegexItem> RegexItems = new Dictionary<Type, RegexItem>();

        /// <summary>
        /// Find characters to not consider between the value, the unit and the power.
        /// </summary>
        private static readonly Regex Space = 
            new Regex(
                @"[\s\\\/_|]", 
                RegexOptions.Compiled, TimeSpan.FromSeconds(2));
        
        /// <summary>
        /// Registered <see cref="IUnitDef"/> with the <see cref="IUnitDef"/>.<see cref="IUnitDef.GetHashCode()"/> result as the key.
        /// Since the <see cref="System.Text.RegularExpressions.Group"/> names are the hash code of the corresponding
        /// <see cref="IUnitDef"/>, it allow to easily find the matching unit.
        /// </summary>
        private static readonly Dictionary<int, IUnitDef> Units = new Dictionary<int, IUnitDef>();

        
        /// <summary>
        /// Recursively check if a <see cref="Type"/> derive from a generic class.
        /// </summary>
        /// <param name="generic"><paramref name="toCheck"/> must derive from this generic class.</param>
        /// <param name="toCheck">Class to validate if it derive from <paramref name="generic"/>.</param>
        /// <returns>true if <paramref name="toCheck"/> derive from <paramref name="generic"/>; false otherwise.</returns>
        private static bool IsGenericSubClass(Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) 
            {
                Type current = toCheck.IsGenericType 
                    ? toCheck.GetGenericTypeDefinition() 
                    : toCheck;
                
                if (generic == current) 
                    return true;
                
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Get the global <see cref="RegexItem"/> allowing to decode string when you don't know the type of <see cref="IUnitDef"/>.
        /// </summary>
        /// <returns>The registered global <see cref="RegexItem"/>.</returns>
        private static RegexItem Regex()
        {
            RegisterUnitTypes();
            return RegexItems[typeof(UnitDef<>)];
        }

        /// <summary>
        /// Get the <see cref="RegexItem"/> allowing to decode string when you know the <see cref="IUnitDef"/> of the value.
        /// </summary>
        /// <param name="type">Unit type to get the <see cref="RegexItem"/> for (<see cref="Length"/>, <see cref="Mass"/>, <see cref="Volume"/>...).</param>
        /// <returns>The registered <see cref="RegexItem"/> for the given type.</returns>
        private static RegexItem Regex(Type type)
        {
            RegisterUnitType(type);
            return RegexItems[type];
        }

        /// <summary>
        /// Create a custom <see cref="RegexItem"/> for multiple <see cref="IUnitDef"/> instances that derive from the given types.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="RegexItem"/> will be registered only if one type is given.
        /// If multiple types are given, try not calling this method multiple times for the types since
        /// <see cref="System.Text.RegularExpressions.Regex"/> would be compiled each time.
        /// </remarks>
        /// <param name="types">Types deriving from <see cref="IUnitDef"/>.</param>
        /// <returns>Returns a <see cref="RegexItem"/> instance corresponding to the given types.</returns>
        /// <exception cref="ArgumentException">If <paramref name="types"/> has no items.</exception>
        private static RegexItem Regex(IEnumerable<Type> types)
        {
            Type[] array = types.ToArray();
            switch (array.Length)
            {
                case 1: return Regex(array[0]);
                case 0: throw new ArgumentException("Empty Type IEnumerable", nameof(types));
            }

            string[] regex = new string[array.Length];

            for (int index = 0; index < array.Length; index++)
            {
                Type type = array[index];
                RegisterUnitType(type);
                regex[index] = RegexItems[type].UnitRegex;
            }

            return new RegexItem(string.Join("|", regex));
        }

        /// <summary>
        /// Register all the <see cref="IUnitDef"/> instances deriving from the given type.
        /// </summary>
        /// <param name="type">Type deriving from <see cref="IUnitDef"/>.</param>
        private static void RegisterUnitType(Type type)
        {
            if (RegexItems.ContainsKey(type))
                return;

            List<string> regex = new List<string>();
            
            foreach (IUnitDef unitDef in Mapping.GetRegistered(type))
            {
                int hash = unitDef.GetHashCode();
                Units[hash] = unitDef;
                
                regex.Add(RegEx.CreateUnitDefRegex(unitDef));
            }
            
            RegexItems[type] = new RegexItem(string.Join("|", regex));
        }
        
        /// <summary>
        /// Register all the <see cref="IUnitDef"/> instances.
        /// </summary>
        private static void RegisterUnitTypes()
        {
            if (s_RegisteredGlobal)
                return;
            
            Type unitType = typeof(UnitDef<>);
            
            foreach (Type type in 
                from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in domainAssembly.GetTypes()
                where assemblyType.IsClass
                      && !assemblyType.IsAbstract
                      && typeof(IUnitDef).IsAssignableFrom(assemblyType)
                      && IsGenericSubClass(unitType, assemblyType)
                select assemblyType)
            
                RegisterUnitType(type);

            string regex = string.Join("|", RegexItems.Values.Select(each => each.UnitRegex));
            RegexItems[unitType] = new RegexItem(regex);
            
            s_RegisteredGlobal = true;
        }
        
        
        
        /// <summary>
        /// Convert a string to double-precision floating-point without validation on the pattern.
        /// If a unit definition si part of the string, it will fail. Use <see cref="FromStringToDouble"/> in this case.
        /// </summary>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5
        /// 1.6e2
        /// 1 000E-3
        /// </param>
        /// <returns>The decoded value.</returns>
        public static double FromResultToDouble(string text)
        {
            string cleaned = Space.Replace(text, "");
            return double.Parse(cleaned, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple values, will calculate the total converted to the left most <see cref="IUnitDef"/>.
        /// This allow to decode values like 99'9".
        /// If you prefer to have a list of units, you need to spit the string by your self.
        /// </remarks>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9'9").
        /// Use <see cref="FromStringToComponentUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> if you already know the unit type.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">If the given <paramref name="power"/> value is not compatible with the decoded <paramref name="unitDef"/>.</exception>
        public static void FromStringToComponentUnit(string text, out double value, out IUnitDef unitDef, out byte power) => 
            FromStringToComponentUnit(text, Regex(), out value, out unitDef, out power);

        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple values, will calculate the total converted to the left most <see cref="IUnitDef"/>.
        /// This allow to decode values like 99'9".
        /// If you prefer to have a list of units, you need to spit the string by your self.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <param name="unitTypes">Restrict the search with only <see cref="UnitDef{T}"/> instances of those types.</param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match any unit pattern.</exception>
        /// <exception cref="ExponentPatternException">If the given <paramref name="power"/> value is not compatible with the decoded <paramref name="unitDef"/>.</exception>
        public static void FromStringToComponentUnit(string text, IEnumerable<Type> unitTypes, out double value, out IUnitDef unitDef, out byte power) => 
            FromStringToComponentUnit(text, Regex(unitTypes), out value, out unitDef, out power);

        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple values, will calculate the total converted to the left most <see cref="IUnitDef"/>.
        /// This allow to decode values like 99'9".
        /// If you prefer to have a list of units, you need to spit the string by your self.
        /// </remarks>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9'9").
        /// Use <see cref="FromStringToComponentUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> if you already know the unit type.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <param name="regex">
        /// Decode the string based on this regex. Take note the regex must have <see cref="System.Text.RegularExpressions.Group"/>
        /// named with the hash code of the <see cref="IUnitDef"/> prefixed by "u".(and replace negative symbol by a underscore)
        /// </param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        /// <exception cref="UnitPatternException">If the given <paramref name="text"/> does not match the given <paramref name="regex"/>.</exception>
        /// <exception cref="ExponentPatternException">If the given <paramref name="power"/> value is not compatible with the decoded <paramref name="unitDef"/>.</exception>
        private static void FromStringToComponentUnit(string text, RegexItem regex, out double value, out IUnitDef unitDef, out byte power)
        {
            text = text.Trim();

            MatchCollection collection = SelectMatchCollection(text, regex);
            int length = collection.Count;

            IUnitDef[] units = new IUnitDef[length];
            double[] values = new double[length];
            bool isNeg = false;
            
            unitDef = null;
            power = 0;
            value = 0;

            for (int index = 0; index < length; index++)
            {
                Match match = collection[index];

                FromStringToSingleUnit(match, regex, out double partValue, out IUnitDef partUnit, out byte partPower);
                
                values[index] = partValue;
                if (index == 0)
                {
                    units[index] = partUnit;
                    unitDef = partUnit;
                    power = partPower;
                    isNeg = partValue < 0;
                    continue;
                }
                AssignUnitPart(index, length, partUnit, partPower, 
                    ref units, ref unitDef, ref power);
            }

            var unitDefConvert = GetConversionUnit(length, unitDef);

            for (int index = 0; index < length; index++)
                value = ApplyValue(index, values[index], value, units[index], unitDefConvert, isNeg);
        }

        /// <summary>
        /// Assigns the unit, and configures the power from an item in the match collection. Used when converting from a string to a component unit.
        /// </summary>
        /// <param name="index">Index of component from matched text</param>
        /// <param name="length">Number of matches from the text</param>
        /// <param name="partUnit">Unit for this index</param>
        /// <param name="partPower">Power from this unit's index</param>
        /// <param name="units">List of component units</param>
        /// <param name="unitDef">Base unit</param>
        /// <param name="power">Power of the base unit</param>
        /// <exception cref="ExponentPatternException">Unit & Exponents are incompatable</exception>
        static void AssignUnitPart(
            int index, int length, IUnitDef partUnit, byte partPower, 
            ref IUnitDef[] units, ref IUnitDef unitDef, ref byte power)
        {
            if (partUnit.PowerMin != 1 && Mapping.TryPowerUnitDef(partUnit, 1, out IUnitDef newPartUnitDef)) 
                partUnit = newPartUnitDef;
                
            units[index] = partUnit;
                
            if (partPower == power) 
                return;
                
            if (index != length - 1 || power != 1)
                throw new ExponentPatternException(
                    $"Index {index} of {length} where the power is {power} but this part is {partPower}"
                    , index.ToString());
                
            if (Mapping.TryPowerUnitDef(unitDef, partPower, out IUnitDef newUnitDef))
                unitDef = newUnitDef;
                
            power = partPower;
        }

        /// <summary>
        /// Applies the component's value using the given parameters
        /// </summary>
        /// <param name="index">Compoent's index</param>
        /// <param name="inputValue">Compoent's value</param>
        /// <param name="currentOutputValue">Current value state</param>
        /// <param name="inputUnit">Component's unit</param>
        /// <param name="conversionUnit">Unit to convert to</param>
        /// <param name="isNeg">Negitive state</param>
        /// <returns>output value with component unit applied</returns>
        static double ApplyValue(int index, double inputValue, double currentOutputValue, IUnitDef inputUnit, IUnitDef conversionUnit, bool isNeg)
        {
            double newValue = index != 0
                ? inputUnit.To(inputValue, conversionUnit, 1)
                : inputValue;

            if (index == 0)
                return newValue;
            else
                return isNeg
                    ? currentOutputValue - newValue
                    : currentOutputValue + newValue;
        }

        /// <summary>
        /// Map unit to a power unit if needed.
        /// </summary>
        /// <param name="length">Index length</param>
        /// <param name="inputDef">Input's base unit</param>
        /// <returns>Base unit, or a power unit if available.</returns>
        static IUnitDef GetConversionUnit(int length, IUnitDef inputDef)
        {
            IUnitDef unitDefConvert;
            if (length == 1)
                unitDefConvert = inputDef;
            else
                unitDefConvert = Mapping.TryPowerUnitDef(inputDef, 1, out IUnitDef powerUnit)
                    ? powerUnit
                    : inputDef;
            return unitDefConvert;
        }

        /// <summary>
        /// Returns a match collection of the string from the given regex.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="regex">Input regex pattern</param>
        /// <returns>A match collection of component units</returns>
        /// <exception cref="UnitPatternException">No numbers found in given text.</exception>
        static MatchCollection SelectMatchCollection(string text, RegexItem regex)
        {
            MatchCollection numbers = RegEx.MatchDoubles(text);

            int nbr = numbers.Count;

            if (nbr == 0)
                throw new UnitPatternException(text, nameof(text));

            MatchCollection collection = regex.CaseSensitive.Matches(text);
            int length = collection.Count;

            if (length != nbr)
            {
                collection = regex.CaseInsensitive.Matches(text);
                length = collection.Count;
            }

            if (length != nbr)
                throw new UnitPatternException(text, nameof(text));
            return collection;
        }

        /// <summary>
        /// Extract the double-precision floating-point part of the given string.
        /// </summary>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5
        /// 1.6e2
        /// 1 000E-3
        /// </param>
        /// <returns>The decoded value.</returns>
        /// <exception cref="DoublePatternException">If the given string has does not have a double-precision floating point pattern.</exception>
        public static double FromStringToDouble(string text)
        {
            Match match = RegEx.MatchDouble(text);

            if (!match.Success)
                throw new DoublePatternException(text, nameof(text));

            return FromResultToDouble(match.Value);
        }

        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="FromStringToComponentUnit(string,out double,out IUnitDef,out byte)"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> if you already know the unit type.
        /// </remarks>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        public static void FromStringToSingleUnit(string text, out double value, out IUnitDef unitDef, out byte power) => 
            FromStringToSingleUnit(text, Regex(), out value, out unitDef, out power);

        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="FromStringToComponentUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> if you already know the unit type.
        /// </remarks>
        /// <param name="unitTypes">Restrict the search with only <see cref="UnitDef{T}"/> instances of those types.</param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        public static void FromStringToSingleUnit(string text, IEnumerable<Type> unitTypes, out double value, out IUnitDef unitDef, out byte power) => 
            FromStringToSingleUnit(text, Regex(unitTypes), out value, out unitDef, out power);
        
        /// <summary>
        /// Decode a string to its corresponding <see cref="IUnitDef"/>, value and power.
        /// </summary>
        /// <remarks>
        /// If the string has multiple units, like 99'9" or speed 99 km/h, use <see cref="FromStringToComponentUnit(string,RegexItem,out double,out IUnitDef,out byte)"/> instead.
        /// </remarks>
        /// <param name="text">
        /// Text to decode.
        /// Supported formats:
        /// 5cm
        /// 1.6e2cm
        /// 1 000E-3 cm
        /// 5 cm²
        /// 5²cm
        /// 5 cubic cm
        /// 5 centimeters cubic
        /// </param>
        /// <remarks>
        /// This method could return a wrong value in case of duplicate registered units.
        /// This is the case for feet and arcMin (9').
        /// Use <see cref="FromStringToSingleUnit(string,IEnumerable{Type},out double,out IUnitDef,out byte)"/> if you already know the unit type.
        /// </remarks>
        /// <param name="regex">
        /// Decode the string based on this regex. Take note the regex must have <see cref="System.Text.RegularExpressions.Group"/>
        /// named with the hash code of the <see cref="IUnitDef"/> prefixed by "u".(and replace negative symbol by a underscore)
        /// </param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        private static void FromStringToSingleUnit(string text, RegexItem regex, out double value, out IUnitDef unitDef, out byte power)
        {
            Match match = regex.CaseSensitive.Match(text.Trim());
            
            if (!match.Success)  // Try with the ignore case flag only if we consider the text to have mistakes.
                match = regex.CaseInsensitive.Match(text);
            
            if (!match.Success)
                throw new UnitPatternException(text, nameof(text));
            
            FromStringToSingleUnit(match, regex, out value, out unitDef, out power);
        }

        /// <summary>
        /// When decoding a string to its corresponding <see cref="IUnitDef"/>, value and power, regular expression is used.
        /// This method is used to evaluate the matching object to the unit values.
        /// </summary>
        /// <param name="match">Regex match result to convert.</param>
        /// <param name="regex">
        /// Decode the string based on this regex. Take note the regex must have <see cref="System.Text.RegularExpressions.Group"/>
        /// named with the hash code of the <see cref="IUnitDef"/> prefixed by "u".(and replace negative symbol by a underscore)
        /// </param>
        /// <param name="value">Will output the amount of units as a double-precision floating-point value.</param>
        /// <param name="unitDef">Unit the decoded value represent.</param>
        /// <param name="power">Output the decoded value power directive if any; 1 otherwise.</param>
        private static void FromStringToSingleUnit(Match match, RegexItem regex, out double value, out IUnitDef unitDef, out byte power)
        {
            GroupCollection groups = match.Groups;
            value = RegexItem.GetValue(groups);
            double fraction = 0.0;
            if (FromStringToFraction(match, out var numerator, out var denominator))
                fraction = numerator / denominator;

            value = RegexItem.GetValue(groups) + fraction;
            power = RegexItem.GetPower(groups);

            string unitHash = regex
                .GroupNames
                .First(name =>
                    name != "unit"
                    && name[0] == 'u'
                    && groups[name].Success);

            unitDef = Units[int.Parse(unitHash.Replace("_", "-").Replace("u", ""))];

            if (power == 1 || !Mapping.PowerUnits.TryGetValue(unitDef, out Dictionary<byte, IUnitDef> map))
            {
                if (power == 0)
                    power = unitDef.PowerMin;
                return;
            }

            if (map.TryGetValue(power, out IUnitDef newUnit))
                unitDef = newUnit;

            else if (power == 0 && unitDef.PowerMax > unitDef.PowerMin && map.TryGetValue(1, out newUnit))
                unitDef = newUnit;

            else if (power == 0 && unitDef.PowerMax > 1 && unitDef.ToString(unitDef.PowerMin) != "" && map.TryGetValue(1, out newUnit))
                unitDef = newUnit;

            if (power == 0)
                power = unitDef.PowerMin;
        }

        /// <summary>
        /// Converts mixed & fractional numbers into decimal from a regex match.
        /// 1 1/2 will become 1.5
        /// </summary>
        /// <param name="match">Regex match from matching pattern containing double, numerator, and denominator groups</param>
        /// <param name="input">The string used for replacement.</param>
        /// <returns>input string with matched number converted to decimal</returns>
        static bool FromStringToFraction(Match match, out double numerator, out double denominator)
        {
            numerator = 0.0;
            denominator = 0.0;

            var numeratorMatch = match.Groups["numerator"].Value;
            string denominatorMatch;

            if (string.IsNullOrWhiteSpace(numeratorMatch))
            {
                numeratorMatch = match.Groups["numeratorPost"].Value;
                if (string.IsNullOrWhiteSpace(numeratorMatch))
                    return false;

                denominatorMatch = match.Groups["denominatorPost"].Value;
            }
            else
            {
                denominatorMatch = match.Groups["denominator"].Value;
            }

            numerator = FromResultToDouble(numeratorMatch);
            denominator = FromResultToDouble(denominatorMatch);

            return true;
        }

    }
}
