
using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// Static class used to retrieved registered <see cref="UnitDef{T}"/>.
    /// </summary>
    public static class Mapping
    {
        
        /// <summary>
        /// Variable allowing to know if the the static constructor was called once on all classes holding
        /// static variables of <see cref="UnitDef{T}"/> instances.
        /// </summary>
        private static bool s_StaticCall;
        
        /// <summary>
        /// All the classes that has <see cref="UnitDef{T}"/> instances assigned to static variables.
        /// Those classes will be called once by calling their static constructor and allowing all those units
        /// to be part of the Regex when decoding <see cref="Unit"/> from strings.
        /// </summary>
        private static IEnumerable<Type> UnitSystems
        {
            get
            {
                yield return typeof(Si);
                yield return typeof(Misc);
                yield return typeof(Imperial);
                yield return typeof(Uscs);
                yield return typeof(Money);
            }
        }
        
        /// <summary>
        /// Registered <see cref="UnitDef{T}"/> types with siblings.
        /// Siblings are <see cref="UnitDef{T}"/> types representing the same <see cref="UnitDef{T}"/> but with
        /// a specific <see cref="Unit.Power"/> value.
        ///
        /// For example, <see cref="Volume"/> is the <see cref="Length"/> unit with a power of 3.
        /// </summary>
        private static readonly Dictionary<Type, Type[]> OpTypes = new()
        {
            {typeof(Length), new[]{typeof(Area), typeof(Volume)}},
            {typeof(Area), new[]{typeof(Length), typeof(Volume)}},
            {typeof(Volume), new[]{typeof(Length), typeof(Area)}}
        };

        /// <summary>
        /// Registered <see cref="UnitDef{T}"/> instance for a specific <see cref="Unit"/>.<see cref="Unit.Power"/> value.
        ///
        /// The keys are the <see cref="UnitDef{T}"/> instance sources.
        /// The value is a <see cref="Dictionary{TKey,TValue}"/> with each <see cref="UnitDef{T}"/> instances
        /// corresponding for each registered power value.
        /// </summary>
        /// <example>
        /// <code>
        /// { Si.Meter,
        ///     { 2, Si.Meter2 },
        ///     { 3, Si.Meter3 }}};
        /// </code>
        /// </example>
        internal static readonly Dictionary<IUnitDef, Dictionary<byte, IUnitDef>> PowerUnits = new();

        /// <summary>
        /// All the registered <see cref="UnitDef{T}"/> instances grouped by type.
        /// They are split by type allowing to specialise decoding when the <see cref="UnitDef{T}"/> type is known.
        /// </summary>
        private static readonly Dictionary<Type, List<IUnitDef>> Registered = new();



        /// <summary>
        /// Validate two <see cref="UnitDef{T}"/> instances are of the same type or a type representing the other
        /// type with a different <see cref="Unit"/>.<see cref="Unit.Power"/> value.
        /// </summary>
        /// <param name="first">First <see cref="UnitDef{T}"/> instance to compare with.</param>
        /// <param name="second">The other <see cref="UnitDef{T}"/> instance to compare with.</param>
        /// <returns>true if both <see cref="UnitDef{T}"/> instances are compatible; false otherwise.</returns>
        /// <seealso cref="OpTypes"/>
        internal static bool AreCompatibleTypes(IUnitDef first, IUnitDef second) =>
            AreCompatibleTypes(first.GetType(), second.GetType());

        /// <summary>
        /// Validate two <see cref="UnitDef{T}"/> types are the same or a type representing the other
        /// type with a different <see cref="Unit.Power"/> value.
        /// </summary>
        /// <param name="first">First <see cref="UnitDef{T}"/> type to compare with.</param>
        /// <param name="second">The other <see cref="UnitDef{T}"/> type to compare with.</param>
        /// <returns>true if both <see cref="UnitDef{T}"/> types are compatible; false otherwise.</returns>
        /// <seealso cref="OpTypes"/>
        internal static bool AreCompatibleTypes(Type first, Type second) =>
            first == second
            || OpTypes.TryGetValue(first, out Type[] types)
            && types.Contains(second);
        
        /// <summary>
        /// Initialize the units.
        /// If not executed, regular expressions cannot be created properly if not all <see cref="IUnitDef"/> are instantiated.
        /// 
        /// If a <see cref="IUnitDef"/> conversion is called based on a string value, this get called to be sure all
        /// the static <see cref="IUnitDef"/> gets initialized. If not executed, you might end-up having partial
        /// <see cref="IUnitDef"/> unit collections and some regular expression could fail since the <see cref="RegexItem"/>
        /// would be incomplete.
        /// </summary>
        internal static void CallStaticUnits()
        {
            if (s_StaticCall)
                return;
            
            foreach (Type type in UnitSystems)
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            
            s_StaticCall = true;
        }
        
        /// <summary>
        /// Get the corresponding <see cref="IUnitDef"/> with a different <see cref="IUnit.Power"/> value.
        /// </summary>
        /// <param name="unitDef"><see cref="IUnitDef"/> instance to search its equivalent power instance.</param>
        /// <param name="power">Search the corresponding <see cref="IUnitDef"/> registered for this <see cref="IUnit.Power"/>.</param>
        /// <returns>The corresponding <see cref="IUnitDef"/> instance; null if no instance are registered for the given <see cref="IUnit.Power"/>.</returns>
        /// <seealso cref="RegisterPowerUnitDef(IUnitDef, byte, IUnitDef)"/>
        /// <seealso cref="RegisterPowerUnitDefs(IUnitDef, Dictionary{byte,IUnitDef})"/>
        /// <seealso cref="RegisterPowerUnitDefs(IUnitDef[])"/>
        public static IUnitDef GetPowerUnitDef(IUnitDef unitDef, byte power) =>
            PowerUnits.TryGetValue(unitDef, out Dictionary<byte, IUnitDef> mappingUnit)
            && mappingUnit.TryGetValue(power, out IUnitDef newUnit)
                ? newUnit
                : null;

        /// <summary>
        /// Get all registered <see cref="IUnitDef"/> instances for a specific type.
        /// </summary>
        /// <typeparam name="T">Get all <see cref="IUnitDef"/> instances registered for this type.</typeparam>
        /// <remarks>
        /// This method is not recursive, it does not return any <see cref="IUnitDef"/> instances if
        /// the given type is abstract and doesn't return instances of classes deriving from the given type.
        /// </remarks>
        /// <returns>Enumerate <see cref="IUnitDef"/> instances.</returns>
        /// <seealso cref="Register"/>
        /// <seealso cref="Register{T}"/>
        public static IEnumerable<T> GetRegistered<T>()
            where T : IUnitDef =>
            GetRegistered(typeof(T)).Cast<T>();

        /// <summary>
        /// Get all registered <see cref="IUnitDef"/> instances for a specific type.
        /// </summary>
        /// <param name="type">Get all <see cref="IUnitDef"/> instances registered for this type.</param>
        /// <remarks>
        /// This method is not recursive, it does not return any <see cref="IUnitDef"/> instances if
        /// the given type is abstract and doesn't return instances of classes deriving from the given type.
        /// </remarks>
        /// <returns>Enumerate <see cref="IUnitDef"/> instances.</returns>
        /// <seealso cref="Register"/>
        /// <seealso cref="Register{T}"/>
        public static IEnumerable<IUnitDef> GetRegistered(Type type)
        {
            CallStaticUnits();
            return Registered[type].AsEnumerable();
        }
        
        /// <summary>
        /// Search all registered <see cref="IUnitDef"/> instances where <see cref="IUnitDef.Name"/> is the given <paramref name="name"/>.
        /// </summary>
        /// <remarks>This method is used to deserialize <see cref="IUnitDef"/> data.</remarks>
        /// <param name="name">Correspond to the <see cref="IUnitDef"/>.<see cref="IUnitDef.Name"/> value.</param>
        /// <param name="type">Returned value of <see cref="IUnitDef"/>.<see cref="IUnitDef.GetType()"/>.</param>
        /// <returns>The first <see cref="IUnitDef"/> instance corresponding to the given <paramref name="name"/> and <paramref name="type"/>.</returns>
        /// <exception cref="InvalidOperationException">If no registered <see cref="IUnitDef"/> correspond to the given <paramref name="name"/> and <paramref name="type"/>.</exception>
        /// <seealso cref="Register"/>
        /// <seealso cref="Register{T}"/>
        public static IUnitDef GetUnitDefByName(string name, string type)
        {
            CallStaticUnits();
            return Registered
                .First(each => each.Key.Name == type)
                .Value
                .First(each => each.Name == name);
        }
        
        /// <summary>
        /// Get the corresponding <see cref="IUnitDef"/> with a different <see cref="IUnit.Power"/> value.
        /// </summary>
        /// <param name="unitDef"><see cref="IUnitDef"/> instance to search its equivalent power instance.</param>
        /// <param name="power">Search the corresponding <see cref="IUnitDef"/> registered for this <see cref="IUnit.Power"/>.</param>
        /// <param name="output">The corresponding <see cref="IUnitDef"/> instance; null if no instance are registered for the given <see cref="IUnit.Power"/>.</param>
        /// <returns>true if a corresponding <see cref="IUnitDef"/> was found; false otherwise.</returns>
        /// <seealso cref="RegisterPowerUnitDef(IUnitDef, byte, IUnitDef)"/>
        /// <seealso cref="RegisterPowerUnitDefs(IUnitDef, Dictionary{byte,IUnitDef})"/>
        /// <seealso cref="RegisterPowerUnitDefs(IUnitDef[])"/>
        public static bool TryPowerUnitDef(IUnitDef unitDef, byte power, out IUnitDef output)
        {
            output = GetPowerUnitDef(unitDef, power);
            return !(output is null);
        }

        

        /// <summary>
        /// Initialize a <see cref="Registered"/> key by creating a <see cref="List{T}"/> for the given type.
        /// </summary>
        /// <typeparam name="T">Prepare registration for this type.</typeparam>
        /// <seealso cref="GetRegistered"/>
        /// <seealso cref="GetRegistered{T}"/>
        /// <seealso cref="GetUnitDefByName"/>
        internal static void Register<T>()
            where T : IUnitDef =>
            Registered[typeof(T)] = new List<IUnitDef>();

        /// <summary>
        /// Register one <see cref="IUnitDef"/>.
        /// Registration allowing string decoding for a specified <see cref="IUnitDef"/> instance.
        /// </summary>
        /// <param name="unitDef">Unit definition to register.</param>
        /// <seealso cref="GetRegistered"/>
        /// <seealso cref="GetRegistered{T}"/>
        /// <seealso cref="GetUnitDefByName"/>
        internal static void Register(IUnitDef unitDef) => 
            Registered[unitDef.GetType()].Add(unitDef);
        
        /// <summary>
        /// Register a <see cref="IUnitDef"/> sibling by power.
        /// </summary>
        /// <param name="parentUnitDef">When searching for corresponding power <see cref="IUnitDef"/>, this is the parent unit searching for.</param>
        /// <param name="power">Sibling power value to register the <paramref name="powerUnitDef"/> for.</param>
        /// <param name="powerUnitDef">Sibling to register.</param>
        /// <seealso cref="GetPowerUnitDef"/>
        public static void RegisterPowerUnitDef(IUnitDef parentUnitDef, byte power, IUnitDef powerUnitDef)
        {
            if (!PowerUnits.TryGetValue(parentUnitDef, out Dictionary<byte, IUnitDef> mapping))
                PowerUnits[parentUnitDef] = new Dictionary<byte, IUnitDef> {{power, powerUnitDef}};
            else
                mapping[power] = powerUnitDef;
        }
        
        /// <summary>
        /// Register all the <see cref="IUnitDef"/> sibling by power for a single <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="parentUnitDef">When searching for corresponding power <see cref="IUnitDef"/>, this is the parent unit searching for.</param>
        /// <param name="mapping">
        /// <see cref="Dictionary{TKey,TValue}"/> where the keys are the <see cref="Unit"/>.<see cref="Unit.Power"/> value
        /// and the value is the corresponding <see cref="IUnitDef"/> to instantiating when converting the <see cref="IUnitDef"/>
        /// to a different power value.
        /// </param>
        /// <seealso cref="GetPowerUnitDef"/>
        public static void RegisterPowerUnitDefs(IUnitDef parentUnitDef, Dictionary<byte, IUnitDef> mapping) =>
            PowerUnits[parentUnitDef] = mapping;

        /// <summary>
        /// Register all the <see cref="IUnitDef"/> sibling by power for a single <see cref="IUnitDef"/>.
        /// </summary>
        /// <param name="unitDefs">
        /// Register those <see cref="IUnitDef"/> instances as siblings.
        /// Either each instance are restricted to one power value by having their <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMin"/> equals <see cref="IUnitDef"/>.<see cref="IUnitDef.PowerMax"/>.
        /// If the unit is not restricted to a single power value, the index of the given <see cref="Array"/> will be considered to
        /// be its registered power value.
        /// </param>
        /// <example>
        /// <code>
        /// Mapping.RegisterPowerUnitDefs(Si.Meter, Si.Meter2, Si.Meter3);
        /// </code>
        /// </example>
        /// <seealso cref="GetPowerUnitDef"/>
        public static void RegisterPowerUnitDefs(params IUnitDef[] unitDefs)
        {
            Dictionary<byte, IUnitDef> data = new Dictionary<byte, IUnitDef>();

            for (int index = 0; index < unitDefs.Length; index++)
            {
                IUnitDef unitDef = unitDefs[index];

                byte power;
                if (unitDef.PowerMin == unitDef.PowerMax)
                    power = unitDef.PowerMin;
                else if (index + 1 >= unitDef.PowerMin && index + 1 <= unitDef.PowerMax)
                    power = (byte)index;
                else
                    throw new WrongRegistrationPowerException(unitDef, index);

                data[power] = unitDef;
                PowerUnits[unitDef] = data;
            }
        }
        
    }
}
