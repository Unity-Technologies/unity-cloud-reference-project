
using System;

namespace Unity.Geospatial.Unit
{
    /// <summary>
    /// International system of units.
    /// </summary>
    public static class Si
    {
        static Si()
        {
            Radian.RegisterAsBaseUnit();
            Steradian.RegisterAsBaseUnit();
            Meter2.RegisterAsBaseUnit();
            Hertz.RegisterAsBaseUnit();
            Meter.RegisterAsBaseUnit();
            Kilogram.RegisterAsBaseUnit();
            Becquerel.RegisterAsBaseUnit();
            Mole.RegisterAsBaseUnit();
            Celsius.RegisterAsBaseUnit();
            Second.RegisterAsBaseUnit();
            Liter.RegisterAsBaseUnit();
            
            Angle.RegisterPowerUnits(Radian, Steradian);

            Length.RegisterPowerUnits(Yottameter, Yottameter2, Yottameter3);
            Length.RegisterPowerUnits(Zettameter, Zettameter2, Zettameter3);
            Length.RegisterPowerUnits(Exameter, Exameter2, Exameter3);
            Length.RegisterPowerUnits(Petameter, Petameter2, Petameter3);
            Length.RegisterPowerUnits(Terameter, Terameter2, Terameter3);
            Length.RegisterPowerUnits(Gigameter, Gigameter2, Gigameter3);
            Length.RegisterPowerUnits(Megameter, Megameter2, Megameter3);
            Length.RegisterPowerUnits(Kilometer, Kilometer2, Kilometer3);
            Length.RegisterPowerUnits(Hectometer, Hectometer2, Hectometer3);
            Length.RegisterPowerUnits(Decameter, Decameter2, Decameter3);
            Length.RegisterPowerUnits(Meter, Meter2, Meter3);
            Length.RegisterPowerUnits(Decimeter, Decimeter2, Decimeter3);
            Length.RegisterPowerUnits(Centimeter, Centimeter2, Centimeter3);
            Length.RegisterPowerUnits(Millimeter, Millimeter2, Millimeter3);
            Length.RegisterPowerUnits(Micrometer, Micrometer2, Micrometer3);
            Length.RegisterPowerUnits(Nanometer, Nanometer2, Nanometer3);
            Length.RegisterPowerUnits(Picometer, Picometer2, Picometer3);
            Length.RegisterPowerUnits(Femtometer, Femtometer2, Femtometer3);
            Length.RegisterPowerUnits(Attometer, Attometer2, Attometer3);
            Length.RegisterPowerUnits(Zeptometer, Zeptometer2, Zeptometer3);
            Length.RegisterPowerUnits(Yoctometer, Yoctometer2, Yoctometer3);

            Length.RegisterPowerUnits(Astronomical, Astronomical2, Astronomical3);
            Length.RegisterPowerUnits(LightYear, LightYear2, LightYear3);
            Length.RegisterPowerUnits(Parsec, Parsec2, Parsec3);
            Length.RegisterPowerUnits(Angstrom, Angstrom2, Angstrom3);
        }
        

        /// <summary>
        /// SI derived unit for measuring angles.
        /// </summary>
        public static readonly Angle Radian = new(new UnitNaming("radian", " rad")); //  Degree * 180.0 / Math.PI

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 360 <see cref="Degree"/>.
        /// </summary>
        public static readonly Angle Degree = new(new UnitNaming("degree", "°", "deg"), Radian * Math.PI / 180.0, false);

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 24 <see cref="HourAngle"/>.
        /// </summary>
        public static readonly Angle HourAngle = new(new UnitNaming("hour angle", " HA"), Degree * 15.0);

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 60 <see cref="ArcMinute"/>.
        /// </summary>
        public static readonly Angle ArcMinute = new(new UnitNaming("arc minute", "'", "arc min", "arcm"), Degree * (1.0 / 60.0), false);

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 3600 <see cref="ArcSecond"/>.
        /// </summary>
        public static readonly Angle ArcSecond = new(new UnitNaming("arc second", "\"", "arc sec", "arcs"), Degree * (1.0 / 3600.0), false);

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 3 600 000 <see cref="MilliArcSecond"/>.
        /// </summary>
        public static readonly Angle MilliArcSecond = new(new UnitNaming("milli arc second", " mas", "milli arc sec", "milliarcs"), ArcSecond * 1E-3, false);

        /// <summary>
        /// Accepted SI unit to measure plane angle in which one full rotation is 3 600 000 000 <see cref="MicroArcSecond"/>.
        /// </summary>
        public static readonly Angle MicroArcSecond = new(new UnitNaming("micro arc second", " uas", "micro arc sec", "microarcs"), ArcSecond * 1E-6, false);
        

        /// <summary>
        /// Si unit for measuring solid-angle.
        /// </summary>
        public static readonly AngularArea Steradian = new(new UnitNaming("steradian", " sr", "sterad", "square radian", "square rad"), Radian.Pow(2), false, false);

        /// <summary>
        /// Solid-angle unit based on <see cref="Degree"/>.
        /// </summary>
        public static readonly AngularArea Degree2 = new(Degree.Naming, Degree.Pow(2), true);

        /// <summary>
        /// Solid-angle unit based on <see cref="HourAngle"/>.
        /// </summary>
        public static readonly AngularArea HourAngle2 = new(HourAngle.Naming, HourAngle.Pow(2), true);

        /// <summary>
        /// Solid-angle unit based on <see cref="ArcMinute"/>.
        /// </summary>
        public static readonly AngularArea ArcMinute2 = new(ArcMinute.Naming, ArcMinute.Pow(2), true);

        /// <summary>
        /// Solid-angle unit based on <see cref="ArcSecond"/>.
        /// </summary>
        public static readonly AngularArea ArcSecond2 = new(ArcSecond.Naming, ArcSecond.Pow(2), true);

        /// <summary>
        /// Solid-angle unit based on <see cref="MilliArcSecond"/>.
        /// </summary>
        public static readonly AngularArea MilliArcSecond2 = new(MilliArcSecond.Naming, MilliArcSecond.Pow(2), true);

        /// <summary>
        /// Solid-angle unit based on <see cref="MicroArcSecond"/>.
        /// </summary>
        public static readonly AngularArea MicroArcSecond2 = new(MicroArcSecond.Naming, MicroArcSecond.Pow(2), true);
        


        /// <summary>
        /// International system substance amount unit.
        /// </summary>
        public static readonly SubstanceAmount Mole = new(new UnitNaming("mole", " mol"));

        

        /// <summary>
        /// International system length unit.
        /// </summary>
        public static readonly Length Meter = new(new UnitNaming("meter", " m"));
        

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Yottameter = new(new UnitNaming("yottameter", " Ym"), Meter * 1E24);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Zettameter = new(new UnitNaming("zettameter", " Zm"), Meter * 1E21);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Exameter = new(new UnitNaming("exameter", " Em"), Meter * 1E18);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Petameter = new(new UnitNaming("petameter", " Pm"), Meter * 1E15);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Terameter = new(new UnitNaming("terameter", " Tm"), Meter * 1E12);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Gigameter = new(new UnitNaming("gigameter", " Gm"), Meter * 1E9);

        /// <summary>
        /// Unit of length equivalent to 1 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Megameter = new(new UnitNaming("megameter", " Mm"), Meter * 1E6);

        /// <summary>
        /// Unit of length equivalent to 1000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Kilometer = new(new UnitNaming("kilometer", " km"), Meter * 1000);

        /// <summary>
        /// Unit of length equivalent to 100 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Hectometer = new(new UnitNaming("hectometer", " hm"), Meter * 100);

        /// <summary>
        /// Unit of length equivalent to 10 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Decameter = new(new UnitNaming("decameter", " dam"), Meter * 10);
        

        /// <summary>
        /// Unit of length equivalent to 1/10 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Decimeter = new(new UnitNaming("decimeter", " dm"), Meter * 0.1);

        /// <summary>
        /// Unit of length equivalent to 1/100 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Centimeter = new(new UnitNaming("centimeter", " cm"), Meter * 0.01);

        /// <summary>
        /// Unit of length equivalent to 1/1000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Millimeter = new(new UnitNaming("millimeter", " mm"), Meter * 0.001);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Micrometer = new(new UnitNaming("micrometer", " μm", "micron", "um"), Meter * 1E-6, false);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Nanometer = new(new UnitNaming("nanometer", " nm"), Meter * 1E-9);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Picometer = new(new UnitNaming("picometer", " pm"), Meter * 1E-12);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Femtometer = new(new UnitNaming("femtometer", " fm"), Meter * 1E-15);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Attometer = new(new UnitNaming("attometer", " am"), Meter * 1E-18);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Zeptometer = new(new UnitNaming("zeptometer", " zm"), Meter * 1E-21);

        /// <summary>
        /// Unit of length equivalent to 1/1 000 000 000 000 000 000 000 000 of a <see cref="Meter"/>.
        /// </summary>
        public static readonly Length Yoctometer = new(new UnitNaming("yoctometer", " ym"), Meter * 1E-24);


        /// <summary>
        /// Roughly the distance from Earth to the Sun.
        /// </summary>
        public static readonly Length Astronomical = new(new UnitNaming("astronomical", " au", "astronomical unit"), Meter * 149_597_870_700, false);
        /// <summary>
        /// Astronomical measure of distance outside the Solar System. Approx. 3.26 <see cref="LightYear"/>.
        /// </summary>
        public static readonly Length Parsec = new(new UnitNaming("parsec", " pc"), Astronomical * (6.48E5 / Math.PI));

        /// <summary>
        /// Astronomical measure of distance equivalent to 9.46 trillion kilometers.
        /// </summary>
        public static readonly Length LightYear = new(new UnitNaming("light-year", "ly", "lightyear"), Si.Meter * 9460730472580800, false);

        /// <summary>
        /// Unit of length equivalent to 0.1 <see cref="Nanometer"/>.
        /// </summary>
        public static readonly Length Angstrom = new(new UnitNaming("ångström", " Å"), Nanometer * 0.1);
        

        /// <summary>
        /// Two-dimensional region unit equivalent to 1² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Meter2 = new(Meter.Naming, true);
        

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Yottameter2 = new(Yottameter.Naming, Yottameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Zettameter2 = new(Zettameter.Naming, Zettameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Exameter2 = new(Exameter.Naming, Exameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Petameter2 = new(Petameter.Naming, Petameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Terameter2 = new(Terameter.Naming, Terameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Gigameter2 = new(Gigameter.Naming, Gigameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Megameter2 = new(Megameter.Naming, Megameter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Kilometer2 = new(Kilometer.Naming, Kilometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 100² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Hectometer2 = new(Hectometer.Naming, Hectometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 10² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Decameter2 = new(Decameter.Naming, Decameter.Pow(2), true);
        

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/10² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Decimeter2 = new(Decimeter.Naming, Decimeter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/100² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Centimeter2 = new(Centimeter.Naming, Centimeter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Millimeter2 = new(Millimeter.Naming, Millimeter.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Micrometer2 = new(Micrometer.Naming, Micrometer.Pow(2), true, false);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Nanometer2 = new(Nanometer.Naming, Nanometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Picometer2 = new(Picometer.Naming, Picometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Femtometer2 = new(Femtometer.Naming, Femtometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Attometer2 = new(Attometer.Naming, Attometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Zeptometer2 = new(Zeptometer.Naming, Zeptometer.Pow(2), true);

        /// <summary>
        /// Two-dimensional region unit equivalent to 1/1 000 000 000 000 000 000 000 000² <see cref="Meter"/>.
        /// </summary>
        public static readonly Area Yoctometer2 = new(Yoctometer.Naming, Yoctometer.Pow(2), true);


        /// <summary>
        /// Roughly the distance from Earth to the Sun to the square.
        /// </summary>
        public static readonly Area Astronomical2 = new(Astronomical.Naming, Astronomical.Pow(2), true, false);

        /// <summary>
        /// Astronomical two-dimensional region equivalent to 9.46 trillion square kilometers.
        /// </summary>
        public static readonly Area LightYear2 = new(LightYear.Naming, LightYear.Pow(2), true, false);

        /// <summary>
        /// Astronomical measure of distance outside the Solar System. Approx. 3.26 square <see cref="LightYear"/>.
        /// </summary>
        public static readonly Area Parsec2 = new(Parsec.Naming, Parsec.Pow(2), true, false);

        /// <summary>
        /// Unit of length equivalent to 0.1 square <see cref="Nanometer"/>.
        /// </summary>
        public static readonly Area Angstrom2 = new(Angstrom.Naming, Angstrom.Pow(2), true, false);
        

        /// <summary>
        /// Three-dimensional space unit equivalent to 1³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Meter3 = new(Meter.Naming, true);
        

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Yottameter3 = new(Yottameter.Naming, Yottameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Zettameter3 = new(Zettameter.Naming, Zettameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Exameter3 = new(Exameter.Naming, Exameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Petameter3 = new(Petameter.Naming, Petameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Terameter3 = new(Terameter.Naming, Terameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Gigameter3 = new(Gigameter.Naming, Gigameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Megameter3 = new(Megameter.Naming, Megameter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Kilometer3 = new(Kilometer.Naming, Kilometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 100³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Hectometer3 = new(Hectometer.Naming, Hectometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 10³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Decameter3 = new(Decameter.Naming, Decameter.Pow(3), true);
        

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/10³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Decimeter3 = new(Decimeter.Naming, Decimeter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/100³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Centimeter3 = new(Centimeter.Naming, Centimeter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Millimeter3 = new(Millimeter.Naming, Millimeter.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Micrometer3 = new(Micrometer.Naming, Micrometer.Pow(3), true, false);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Nanometer3 = new(Nanometer.Naming, Nanometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Picometer3 = new(Picometer.Naming, Picometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Femtometer3 = new(Femtometer.Naming, Femtometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Attometer3 = new(Attometer.Naming, Attometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Zeptometer3 = new(Zeptometer.Naming, Zeptometer.Pow(3), true);

        /// <summary>
        /// Three-dimensional space unit equivalent to 1/1 000 000 000 000 000 000 000 000³ <see cref="Meter"/>.
        /// </summary>
        public static readonly Volume Yoctometer3 = new(Yoctometer.Naming, Yoctometer.Pow(3), true);


        /// <summary>
        /// Roughly the distance from Earth to the Sun to the cube.
        /// </summary>
        public static readonly Volume Astronomical3 = new(Astronomical.Naming, Astronomical.Pow(3), true, false);

        /// <summary>
        /// Astronomical three-dimensional space equivalent to 9.46 trillion cubic <see cref="Kilometer"/>.
        /// </summary>
        public static readonly Volume LightYear3 = new(LightYear.Naming, LightYear.Pow(3), true, false);

        /// <summary>
        /// Astronomical measure of distance outside the Solar System. Approx. 3.26 cubic <see cref="LightYear"/>.
        /// </summary>
        public static readonly Volume Parsec3 = new(Parsec.Naming, Parsec.Pow(3), true, false);

        /// <summary>
        /// Unit of length equivalent to 0.1 square <see cref="Nanometer"/>.
        /// </summary>
        public static readonly Volume Angstrom3 = new(Angstrom.Naming, Angstrom.Pow(3), true, false);


        /// <summary>
        /// Unit of volume equivalent to 1 <see cref="Decimeter3"/>.
        /// </summary>
        public static readonly Volume Liter = new(new UnitNaming("liter", " L"), Decimeter3);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Yottaliter = new(new UnitNaming("yottaliter", " YL"), Liter * 1E24, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Zettaliter = new(new UnitNaming("zettaliter", " ZL"), Megameter3, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Exaliter = new(new UnitNaming("exaliter", " EL"), Liter * 1E18, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Petaliter = new(new UnitNaming("petaliter", " PL"), Liter * 1E15, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Teraliter = new(new UnitNaming("teraliter", " TL"), Kilometer3, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Gigaliter = new(new UnitNaming("gigaliter", " GL"), Hectometer3, true);

        /// <summary>
        /// Unit of volume equivalent to 1 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Megaliter = new(new UnitNaming("megaliter", " ML"), Decameter3, true);

        /// <summary>
        /// Unit of volume equivalent to 1000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Kiloliter = new(new UnitNaming("kiloliter", " kL"), Meter3, true);

        /// <summary>
        /// Unit of volume equivalent to 100 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Hectoliter = new(new UnitNaming("hectoliter", " hL"), Liter * 100, true);

        /// <summary>
        /// Unit of volume equivalent to 10 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Decaliter = new(new UnitNaming("decaliter", " daL"), Liter * 10, true);
        

        /// <summary>
        /// Unit of volume equivalent to 1/10 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Deciliter = new(new UnitNaming("deciliter", " dL"), Liter * 0.1, true);

        /// <summary>
        /// Unit of volume equivalent to 1/100 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Centiliter = new(new UnitNaming("centiliter", " cL"), Liter * 0.01, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Milliliter = new(new UnitNaming("milliliter", " mL"), Centimeter3, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Microliter = new(new UnitNaming("microliter", " μL", "uL"), Millimeter3, true, false);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Nanoliter = new(new UnitNaming("nanoliter", " nL"), Liter * 1E-9, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Picoliter = new(new UnitNaming("picoliter", " pL"), Liter * 1E-12, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Femtoliter = new(new UnitNaming("femtoliter", " fL"), Micrometer3, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Attoliter = new(new UnitNaming("attoliter", " aL"), Liter * 1E-18, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Zeptoliter = new(new UnitNaming("zeptoliter", " zL"), Liter * 1E-21, true);

        /// <summary>
        /// Unit of volume equivalent to 1/1 000 000 000 000 000 000 000 000 <see cref="Liter"/>.
        /// </summary>
        public static readonly Volume Yoctoliter = new(new UnitNaming("yoctoliter", " yL"), Nanometer3, true);
        
        
        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 0.001 <see cref="Kilogram"/>.
        /// </summary>
        public static readonly Mass Gram = new(new UnitNaming("gram", " g", "gramme"), false);
        

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Yottagram = new(new UnitNaming("yottagram", " Yg", "yottagramme"), Gram * 1E24, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Zettagram = new(new UnitNaming("zettagram", " Zg", "zettagramme"), Gram * 1E21, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Exagram = new(new UnitNaming("exagram", " Eg", "exagramme"), Gram * 1E18, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Petagram = new(new UnitNaming("petagram", " Pg", "petagramme"), Gram * 1E15, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Teragram = new(new UnitNaming("teragram", " Tg", "teragramme"), Gram * 1E12, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Gigagram = new(new UnitNaming("gigagram", " Gg", "gigagramme"), Gram * 1E9, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Megagram = new(new UnitNaming("megagram", " Mg", "megagramme"), Gram * 1E6, false);

        /// <summary>
        /// International system mass unit.
        /// </summary>
        public static readonly Mass Kilogram = new(new UnitNaming("kilogram", " kg", "kilogramme"), Gram * 1000, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 100 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Hectogram = new(new UnitNaming("hectogram", " hg", "hectogramme"), Gram * 100, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 10 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Decagram = new(new UnitNaming("decagram", " dag", "decagramme"), Gram * 10, false);
        

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/10 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Decigram = new(new UnitNaming("decigram", " dg", "decigramme"), Gram * 0.1, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/100 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Centigram = new(new UnitNaming("centigram", " cg", "centigramme"), Gram * 0.01, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Milligram = new(new UnitNaming("milligram", " mg", "milligramme"), Gram * 0.001, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Microgram = new(new UnitNaming("microgram", " μg", "ug", "microgramme"), Gram * 1E-6, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Nanogram = new(new UnitNaming("nanogram", " ng", "nanogramme"), Gram * 1E-9, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Picogram = new(new UnitNaming("picogram", " pg", "picogramme"), Gram * 1E-12, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Femtogram = new(new UnitNaming("femtogram", " fg", "femtogramme"), Gram * 1E-15, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Attogram = new(new UnitNaming("attogram", " ag", "attogramme"), Gram * 1E-18, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Zeptogram = new(new UnitNaming("zeptogram", " zg", "zeptogramme"), Gram * 1E-21, false);

        /// <summary>
        /// Unit measure of resistance to acceleration when a net force is applied equivalent to 1/1 000 000 000 000 000 000 000 <see cref="Gram"/>.
        /// </summary>
        public static readonly Mass Yoctogram = new(new UnitNaming("yoctogram", " yg", "yoctogramme"), Gram * 1E-24, false);
        
        
        /// <summary>
        /// Centigrade temperature scale unit.
        /// </summary>
        public static readonly Temperature Celsius = new(new UnitNaming("celsius", "°C"));

        /// <summary>
        /// International system temperature unit.
        /// </summary>
        public static readonly Temperature Kelvin = new(new UnitNaming("kelvin", " K"), value => value, 273.15);
        
        

        /// <summary>
        /// International system time unit.
        /// </summary>
        public static readonly Time Second = new(new UnitNaming("second", "seconds", " sec", "s"), false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time PlanckTime = new(new UnitNaming("planck time", " tP", "plancktime", "planck"), Second * 1E44, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Yottasecond = new(new UnitNaming("yottasecond", " Ym", "yotta sec"), Second * 1E24, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Zettasecond = new(new UnitNaming("zettasecond", " Zm", "zetta sec"), Second * 1E21, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Exasecond = new(new UnitNaming("exasecond", " Em", "exa sec"), Second * 1E18, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Petasecond = new(new UnitNaming("petasecond", " Pm", "peta sec"), Second * 1E15, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Terasecond = new(new UnitNaming("terasecond", " Tm", "tera sec"), Second * 1E12, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Gigasecond = new(new UnitNaming("gigasecond", " Gm", "giga sec"), Second * 1E9, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Megasecond = new(new UnitNaming("megasecond", " Mm", "mega sec"), Second * 1E6, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Kilosecond = new(new UnitNaming("kilosecond", " km", "kilo sec"), Second * 1000, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 100 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Hectosecond = new(new UnitNaming("hectosecond", " hm", "hecto sec"), Second * 100, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 10 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Decasecond = new(new UnitNaming("decasecond", " dam", "deca sec"), Second * 10, false);
        

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/10 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Decisecond = new(new UnitNaming("decisecond", " dm", "deci sec"), Second * 0.1, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/100 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Centisecond = new(new UnitNaming("centisecond", " cm", "centi sec"), Second * 0.01, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Millisecond = new(new UnitNaming("millisecond", " mm", "milli sec"), Second * 0.001, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Microsecond = new(new UnitNaming("microsecond", " μm", "micro sec", "us"), Second * 1E-6, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Nanosecond = new(new UnitNaming("nanosecond", " nm", "nano sec"), Second * 1E-9, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Picosecond = new(new UnitNaming("picosecond", " pm", "pico sec"), Second * 1E-12, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Femtosecond = new(new UnitNaming("femtosecond", " fm", "femto sec"), Second * 1E-15, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Attosecond = new(new UnitNaming("attosecond", " am", "atto sec"), Second * 1E-18, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Zeptosecond = new(new UnitNaming("zeptosecond", " zm", "zepto sec"), Second * 1E-21, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/1 000 000 000 000 000 000 000 000 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Yoctosecond = new(new UnitNaming("yoctosecond", " ym", "yocto sec"), Second * 1E-24, false);
        

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 60 <see cref="Second"/>.
        /// </summary>
        public static readonly Time Minute = new(new UnitNaming("minute", " min"), Second * 60);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 60 <see cref="Minute"/>.
        /// </summary>
        public static readonly Time Hour = new(new UnitNaming("hour", " h"), Second * 3600);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 24 <see cref="Hour"/>.
        /// </summary>
        public static readonly Time Day = new(new UnitNaming("day", " d"), Hour * 24);

        /// <summary>
        /// The time it takes for the Earth to complete one rotation about its axis with respect to the fixed stars.
        /// </summary>
        public static readonly Time SiderealDay = new(new UnitNaming("sidereal day", " sday", "sd"), Second * 86164.09053, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 7 <see cref="SiderealDay"/>.
        /// </summary>
        public static readonly Time Week = new(new UnitNaming("week", " wk"), Day * 7);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 2 <see cref="Week"/>.
        /// </summary>
        public static readonly Time Fortnight = new(new UnitNaming("fortnight", " fortn"), Week * 2);
        
        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1/12 <see cref="Year"/>.
        /// </summary>
        public static readonly Time Month = new(new UnitNaming("month", " mo"), Day * 30.4167);
        
        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 365.25 <see cref="Day"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Year = new(new UnitNaming("year", " yr"), Day * 365.25);

        /// <summary>
        /// The time it takes for the Earth to complete one rotation around the sun with respect to the fixed stars.
        /// </summary>
        public static readonly Time SiderealYear = new(new UnitNaming("sidereal day", " syear", "syr"), Hour * 8766.152638889, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 10 <see cref="Year"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Decade = new(new UnitNaming("decade", " dec"), Year * 10);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 100 <see cref="Year"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Century = new(new UnitNaming("century", " century"), Year * 100);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1000 <see cref="Year"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Millennium = new(new UnitNaming("millennium", " ka", "kiloannum", "kiloyear", "ky"), Year * 1000, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 <see cref="Year"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Megayear = new(new UnitNaming("megayear", " Ma", "millionyear"), Year * 1E6, false);

        /// <summary>
        /// Unit to quantify rates of change of quantities in material reality equivalent to 1 000 000 000 <see cref="Year"/>.
        /// The quarter day representing the leap year.
        /// </summary>
        public static readonly Time Billionyear = new(new UnitNaming("billionyear", " Ga"), Year * 1E9);
        
        
        /// <summary>
        /// International unit to quantify occurrences of a repeating event per unit of <see cref="Time"/>.
        /// </summary>
        public static readonly Frequency Hertz = new(new UnitNaming("hertz", " Hz"), 1 / Second);


        /// <summary>
        /// International system unit representing radioactive decay.
        /// </summary>
        public static readonly Radioactivity Becquerel = new(new UnitNaming("becquerel", " Bq"), Hertz);
    }
}
