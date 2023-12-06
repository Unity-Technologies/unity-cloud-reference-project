
using System;
using System.Linq;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsConvert
    {
        private static readonly ITester[] GetConvertAngle = 
        {
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.Degree, 28.6479),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.Degree, 47.1544),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.Degree, 76.8909),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.Degree, -134.3586),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.HourAngle, 1.9099),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.HourAngle, 3.1436),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.HourAngle, 5.1261),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.HourAngle, -8.9572),
            
            new TesterConvert<Angle, Angle>(Si.HourAngle, 1.9099, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.HourAngle, 3.1436, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.HourAngle, 5.1261, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.HourAngle, -8.9572, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.HourAngle, 1.9099),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.HourAngle, 3.1436),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.HourAngle, 5.1261),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.HourAngle, -8.9572),
            
            new TesterConvert<Angle, Angle>(Si.HourAngle, 1.9099, Si.Degree, 28.6485),
            new TesterConvert<Angle, Angle>(Si.HourAngle, 3.1436, Si.Degree, 47.154, 0.001),
            new TesterConvert<Angle, Angle>(Si.HourAngle, 5.1261, Si.Degree, 76.8915),
            new TesterConvert<Angle, Angle>(Si.HourAngle, -8.9572, Si.Degree,-134.358, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.ArcMinute, 1718.8734),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.ArcMinute, 2829.2656),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.ArcMinute, 4613.4562),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.ArcMinute, -8061.5162),
            
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 1718.8734, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 2829.2656, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 4613.4562, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, -8061.5162, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.ArcMinute, 1718.874, 0.001),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.ArcMinute, 2829.264, 0.001),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.ArcMinute, 4613.454, 0.001),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.ArcMinute, -8061.516, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 1718.874, Si.Degree, 28.6479),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 2829.264, Si.Degree, 47.1544),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, 4613.454, Si.Degree, 76.8909),
            new TesterConvert<Angle, Angle>(Si.ArcMinute, -8061.516, Si.Degree,-134.3586),
            
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.ArcSecond, 103132.4031),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.ArcSecond, 169755.9355),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.ArcSecond, 276807.3700),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.ArcSecond, -483690.9706),
            
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 10_3132.4031, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 169_755.9355, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 276_807.3700, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, -483_690.9706, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.ArcSecond, 103_132.44, 0.01),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.ArcSecond, 169_755.84, 0.01),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.ArcSecond, 276_807.24, 0.01),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.ArcSecond, -483_690.96, 0.01),
            
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 103_132.44, Si.Degree, 28.6479),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 169_755.84, Si.Degree, 47.1544),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, 276_807.24, Si.Degree, 76.8909),
            new TesterConvert<Angle, Angle>(Si.ArcSecond, -483_690.96, Si.Degree, -134.3586),
            
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.MilliArcSecond, 103_132_403.12354),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.MilliArcSecond, 169_755_935.5413469),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.MilliArcSecond, 276_807_369.9835814),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.MilliArcSecond, -48_3690_970.6494026),
            
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 103_132_403.123_540, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 169_755_935.541_346_9, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 276_807_369.983_581_4, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, -483_690_970.649_402_6, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.MilliArcSecond, 103_132_440, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.MilliArcSecond, 169_755_840, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.MilliArcSecond, 276_807_240, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.MilliArcSecond, -483_690_960, 1),
            
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 103_132_440, Si.Degree, 28.6479),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 169_755_840, Si.Degree, 47.1544),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, 276_807_240, Si.Degree, 76.8909),
            new TesterConvert<Angle, Angle>(Si.MilliArcSecond, -483_690_960, Si.Degree, -134.3586),
            
            new TesterConvert<Angle, Angle>(Si.Radian, 0.5, Si.MicroArcSecond, 103_132_403_123.54, 0.01),
            new TesterConvert<Angle, Angle>(Si.Radian, 0.823, Si.MicroArcSecond, 169_755_935_541.36, 0.01),
            new TesterConvert<Angle, Angle>(Si.Radian, 1.342, Si.MicroArcSecond, 276_807_369_983.6, 0.1),
            new TesterConvert<Angle, Angle>(Si.Radian, -2.345, Si.MicroArcSecond, -483_690_970_649.441, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 103_132_403_123.540, Si.Radian, 0.5, 0.1),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 169_755_935_541.3469, Si.Radian, 0.823, 0.001),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 276_807_369_983.5814, Si.Radian, 1.342, 0.001),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, -483_690_970_649.4026, Si.Radian, -2.345, 0.001),
            
            new TesterConvert<Angle, Angle>(Si.Degree, 28.6479, Si.MicroArcSecond, 103_132_440_000, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, 47.1544, Si.MicroArcSecond, 169_755_840_000, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, 76.8909, Si.MicroArcSecond, 276_807_240_000, 1),
            new TesterConvert<Angle, Angle>(Si.Degree, -134.3586, Si.MicroArcSecond, -483_690_960_000, 1),
            
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 103132440000, Si.Degree, 28.6479),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 169755840000, Si.Degree, 47.1544),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, 276807240000, Si.Degree, 76.8909),
            new TesterConvert<Angle, Angle>(Si.MicroArcSecond, -483690960000, Si.Degree, -134.3586),
        };

        private static readonly ITester[] GetConvertLength =
        {
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Yottameter, 4.5E-23, 1E-24),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Zettameter, 4.5E-20, 1E-21),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Exameter, 4.5E-17, 1E-18),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Petameter, 4.5E-14, 1E-15),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Terameter, 4.5E-11, 1E-12),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Gigameter, 4.5E-8, 1E-9),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Megameter, 4.5E-5, 1E-6),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Kilometer, 0.045, 0.001),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Hectometer, 0.45, 0.01),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Decameter, 4.5, 0.1),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Meter, 45.0),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Decimeter, 450.0, 10.0),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Centimeter, 4500.0, 100.0),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Millimeter, 4.5E4, 1E3),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Micrometer, 4.5E7, 1E6),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Nanometer, 4.5E10, 1E9),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Picometer, 4.5E13, 1E12),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Femtometer, 4.5E16, 1E15),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Attometer, 4.5E19, 1E18),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Zeptometer, 4.5E22, 1E21),
            new TesterConvert<Length, Length>(Si.Meter, 45.0, Si.Yoctometer, 4.5E25, 1E25),

            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Yottameter, 4.5E-23, 1E-24),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Zettameter, 4.5E-20, 1E-21),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Exameter, 4.5E-17, 1E-18),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Petameter, 4.5E-14, 1E-15),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Terameter, 4.5E-11, 1E-12),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Gigameter, 4.5E-8, 1E-9),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Megameter, 4.5E-5, 1E-6),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Kilometer, 0.045, 0.001),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Hectometer, 0.45, 0.01),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Decameter, 4.5, 0.1),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Meter, 45.0),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Decimeter, 450.0, 10.0),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Centimeter, 4500.0, 100.0),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Millimeter, 4.5E4, 1E3),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Micrometer, 4.5E7, 1E6),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Nanometer, 4.5E10, 1E9),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Picometer, 4.5E13, 1E12),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Femtometer, 4.5E16, 1E15),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Attometer, 4.5E19, 1E18),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Zeptometer, 4.5E22, 1E21),
            new TesterConvert<Length, Length>(Si.Decimeter, 450.0, Si.Yoctometer, 4.5E25, 1E25),
            
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Yottameter, 4.5E-23, 1E-24),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Zettameter, 4.5E-20, 1E-21),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Exameter, 4.5E-17, 1E-18),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Petameter, 4.5E-14, 1E-15),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Terameter, 4.5E-11, 1E-12),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Gigameter, 4.5E-8, 1E-9),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Megameter, 4.5E-5, 1E-6),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Kilometer, 0.045, 0.001),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Hectometer, 0.45, 0.01),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Decameter, 4.5, 0.1),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Meter, 45.0),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Decimeter, 450.0, 10.0),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Centimeter, 4500.0, 100.0),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Millimeter, 4.5E4, 1E3),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Micrometer, 4.5E7, 1E6),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Nanometer, 4.5E10, 1E9),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Picometer, 4.5E13, 1E12),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Femtometer, 4.5E16, 1E15),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Attometer, 4.5E19, 1E18),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Zeptometer, 4.5E22, 1E21),
            new TesterConvert<Length, Length>(Si.Micrometer, 4.5E7, Si.Yoctometer, 4.5E25, 1E25),
            
            new TesterConvert<Length, Length>(Si.Centimeter, 6124.45, Imperial.Inch, 2411.2008),
            new TesterConvert<Length, Length>(Si.Centimeter, 6124.45, Imperial.Feet, 200.9334),
            new TesterConvert<Length, Length>(Si.Centimeter, 6124.45, Imperial.Yard, 66.9778),
            new TesterConvert<Length, Length>(Si.Centimeter, 6124.45, Imperial.Mile, 0.0381),
            
            new TesterConvert<Length, Length>(Imperial.Inch, 2411.2008, Si.Centimeter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.Feet, 200.9334, Si.Centimeter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.Yard, 66.9778, Si.Centimeter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.Mile, 0.0381, Si.Centimeter, 6131.6006),
            
            new TesterConvert<Length, Length>(Si.Meter, 6124.45, Imperial.Inch, 241120.0787),
            new TesterConvert<Length, Length>(Si.Meter, 6124.45, Imperial.Furlong, 30.4445),
            new TesterConvert<Length, Length>(Si.Meter, 6124.45, Imperial.Thou, 241_120_078.7402),
            new TesterConvert<Length, Length>(Si.Meter, 6124.45, Imperial.NauticalMile, 3.3069),
            
            new TesterConvert<Length, Length>(Imperial.Inch, 241120.0787, Si.Meter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.Furlong, 30.4445, Si.Meter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.Thou, 241120078.7402, Si.Meter, 6124.45, 0.01),
            new TesterConvert<Length, Length>(Imperial.NauticalMile, 3.3069, Si.Meter, 6124.3788),
            
            new TesterConvert<Length, Length>(Si.Micrometer, 2123141424.45, Imperial.Mile, 1.3193),
            new TesterConvert<Length, Length>(Imperial.Mile, 1.3193, Si.Micrometer, 2123207539.2, 0.1),
            
            new TesterConvert<Length, Length>(Si.Meter, 946_073_047_258_080.0, Si.LightYear, 0.1, 0.01),
            new TesterConvert<Length, Length>(Si.Meter, 4_730_365_236_290_400.0, Si.LightYear, 0.5, 0.1),
            new TesterConvert<Length, Length>(Si.Meter, 9_460_730_472_580_800.0, Si.LightYear, 1.0, 0.1),
            new TesterConvert<Length, Length>(Si.Meter, 18_921_460_945_161_600.0, Si.LightYear, 2.0, 0.1),
            
            new TesterConvert<Length, Length>(Si.LightYear, 0.1, Si.Meter, 946_073_047_258_080.0, 0.1),
            new TesterConvert<Length, Length>(Si.LightYear, 0.5, Si.Meter, 4_730_365_236_290_400.0, 0.1),
            new TesterConvert<Length, Length>(Si.LightYear, 1.0, Si.Meter, 9_460_730_472_580_800.0, 0.1),
            new TesterConvert<Length, Length>(Si.LightYear, 2.0, Si.Meter, 18_921_460_945_161_600.0, 0.1),

            new TesterConvert<Length, Length>(Si.Astronomical, 1, Si.Meter, 149_597_870_700, 0.1),
            new TesterConvert<Length, Length>(Si.Astronomical, 1, Si.Parsec, 4.84814E-6, 0.1E-7),
            new TesterConvert<Length, Length>(Si.Parsec, 1, Si.Astronomical, 206_264.806_247_096, 1E-7),
            new TesterConvert<Length, Length>(Si.Meter, 149_597_870_700, Si.Astronomical, 1, 0.1),
            new TesterConvert<Length, Length>(Si.Angstrom, 1E24, Si.LightYear, 0.010_570_008, 1E-9),
            new TesterConvert<Length, Length>(Si.LightYear, 2E-14, Si.Angstrom, 1_892_146_094_516.1, 0.1),
        };

        private static readonly ITester[] GetConvertMass =
        {
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Yottagram, 4.5E-23, 1E-24),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Zettagram, 4.5E-20, 1E-21),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Exagram, 4.5E-17, 1E-18),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Petagram, 4.5E-14, 1E-15),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Teragram, 4.5E-11, 1E-12),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Gigagram, 4.5E-8, 1E-9),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Megagram, 4.5E-5, 1E-6),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Kilogram, 0.045, 0.001),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Hectogram, 0.45, 0.01),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Decagram, 4.5, 0.1),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Gram, 45.0),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Decigram, 450.0, 10.0),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Centigram, 4500.0, 100.0),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Milligram, 4.5E4, 1E3),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Microgram, 4.5E7, 1E6),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Nanogram, 4.5E10, 1E9),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Picogram, 4.5E13, 1E12),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Femtogram, 4.5E16, 1E15),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Attogram, 4.5E19, 1E18),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Zeptogram, 4.5E22, 1E21),
            new TesterConvert<Mass, Mass>(Si.Gram, 45.0, Si.Yoctogram, 4.5E25, 1E25),
            
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Yottagram, 4.5E-23, 1E-24),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Zettagram, 4.5E-20, 1E-21),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Exagram, 4.5E-17, 1E-18),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Petagram, 4.5E-14, 1E-15),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Teragram, 4.5E-11, 1E-12),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Gigagram, 4.5E-8, 1E-9),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Megagram, 4.5E-5, 1E-6),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Kilogram, 0.045, 0.001),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Hectogram, 0.45, 0.01),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Decagram, 4.5, 0.1),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Gram, 45.0),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Decigram, 450.0, 10.0),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Centigram, 4500.0, 100.0),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Milligram, 4.5E4, 1E3),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Microgram, 4.5E7, 1E6),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Nanogram, 4.5E10, 1E9),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Picogram, 4.5E13, 1E12),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Femtogram, 4.5E16, 1E15),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Attogram, 4.5E19, 1E18),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Zeptogram, 4.5E22, 1E21),
            new TesterConvert<Mass, Mass>(Si.Milligram, 4.5E4, Si.Yoctogram, 4.5E25, 1E25),
            
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.34, Imperial.Ounce, 227775.9655),
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.34, Imperial.Pound, 14235.99784),
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.34, Imperial.Stone, 1016.8570),
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.34, Imperial.LongTon, 6.35536),
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.3399, Imperial.Slug, 442.4683376),
            
            new TesterConvert<Mass, Mass>(Imperial.Ounce, 227_775.9655, Si.Kilogram, 6457.340_001_261_5),
            new TesterConvert<Mass, Mass>(Imperial.Pound, 14_235.997_84, Si.Kilogram, 6457.339_999_560_5),
            new TesterConvert<Mass, Mass>(Imperial.Stone, 1016.8570, Si.Kilogram, 6457.340_072_1),
            new TesterConvert<Mass, Mass>(Imperial.LongTon, 6.355_36, Si.Kilogram, 6457.343_882),
            new TesterConvert<Mass, Mass>(Imperial.Slug, 442.468_337_6, Si.Kilogram, 6457.339_999_505_7),
            
            new TesterConvert<Mass, Mass>(Si.Kilogram, 6457.34, Uscs.ShortTon, 7.1180),
            new TesterConvert<Mass, Mass>(Uscs.ShortTon, 7.1180, Si.Kilogram, 6457.341),
        };

        private static readonly ITester[] GetConvertRadioactivity =
        {
            new TesterConvert<Radioactivity, Radioactivity>(Si.Becquerel, 23_485_168, Misc.Curie, 0.000_6347343, 0.000_000_1),
            new TesterConvert<Radioactivity, Radioactivity>(Si.Becquerel, 23_485_168, Misc.Rutherford, 23.485_168, 0.000_001),
            
            new TesterConvert<Radioactivity, Radioactivity>(Misc.Curie, 0.000_634_734_3, Si.Becquerel, 23485169.1),
            new TesterConvert<Radioactivity, Radioactivity>(Misc.Curie, 0.000_634_734_3, Misc.Rutherford, 23.485169, 0.000_001),
            
            new TesterConvert<Radioactivity, Radioactivity>(Misc.Rutherford, 23.485_168, Si.Becquerel, 23_485_168),
            new TesterConvert<Radioactivity, Radioactivity>(Misc.Rutherford, 23.485_168, Misc.Curie, 0.000_634_734_3, 0.000_000_1),
        };
        
        private static readonly ITester[] GetConvertSurface =
        {
            new TesterConvert<Area, Area>(Si.Centimeter2, 7123.2348, Imperial.Feet2, 7.6674),
            new TesterConvert<Area, Area>(Imperial.Feet2, 7.6674, Si.Centimeter2, 7123.2477),

            new TesterConvert<Area, Area>(Si.Kilometer2, 613.2348, Imperial.Acre, 151_533.6192),
            new TesterConvert<Area, Area>(Imperial.Acre, 151_533.6192, Si.Kilometer2, 613.2348),
            
            new TesterConvert<Area, Area>(Si.Centimeter2, 6124.45, Imperial.Inch2, 949.2916),
            new TesterConvert<Area, Area>(Si.Centimeter2, 6124.45, Imperial.Feet2, 6.5923),
            new TesterConvert<Area, Area>(Si.Centimeter2, 6124.45, Imperial.Yard2, 0.7325),
            
            new TesterConvert<Area, Area>(Imperial.Inch2, 2411.2008, Si.Centimeter2, 15_556.1031),
            new TesterConvert<Area, Area>(Imperial.Feet2, 200.9334, Si.Centimeter2, 186_673.2370),
            new TesterConvert<Area, Area>(Imperial.Yard2, 66.9778, Si.Centimeter2, 560_019.7109),
            
            new TesterConvert<Area, Area>(Si.Meter2, 6124.45, Imperial.Inch2, 9_492_916.4858),
            new TesterConvert<Area, Area>(Si.Meter2, 6124.45, Imperial.Mile2, 0.0024),
            new TesterConvert<Area, Area>(Si.Meter2, 6124.449_999_377_92, Imperial.Furlong2, 0.151_337_853_5),
            new TesterConvert<Area, Area>(Si.Meter2, 3.957_569_591_942, Imperial.Thou2, 6_134_245_136.0, 0.1),
            new TesterConvert<Area, Area>(Si.Meter2, 6_122_574.4499, Imperial.NauticalMile2, 1.785_05),
            
            new TesterConvert<Area, Area>(Imperial.Inch2, 9_492_916.4858, Si.Meter2, 6124.45, 0.01),
            new TesterConvert<Area, Area>(Imperial.Furlong2, 5.999_991_33, Si.Meter2, 242_811.03, 0.01),
            new TesterConvert<Area, Area>(Imperial.Mile2, 2525.1523, Si.Meter2, 6_540_114_433.7876),
            new TesterConvert<Area, Area>(Imperial.Thou2, 6_134_245_136, Si.Meter2, 3.957_56),
            new TesterConvert<Area, Area>(Imperial.NauticalMile2, 1.785_057_089_061, Si.Meter2, 6_122_574.4499),
            
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Yottameter2, 4.5E-47, 1E-48),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Zettameter2, 4.5E-41, 1E-42),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Exameter2, 4.5E-35, 1E-36),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Petameter2, 4.5E-29, 1E-30),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Terameter2, 4.5E-23, 1E-24),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Gigameter2, 4.5E-17, 1E-18),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Megameter2, 4.5E-11, 1E-12),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Kilometer2, 4.5E-5, 1E-6),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Hectometer2, 0.0045),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Decameter2, 0.45),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Meter2, 45.0),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Decimeter2, 4500.0),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Centimeter2, 4.5E5),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Millimeter2, 4.5E7, 1E6),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Micrometer2, 4.5E13, 1E12),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Nanometer2, 4.5E19, 1E18),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Picometer2, 4.5E25, 1E24),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Femtometer2, 4.5E31, 1E30),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Attometer2, 4.5E37, 1E36),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Zeptometer2, 4.5E43, 1E42),
            new TesterConvert<Area, Area>(Si.Meter2, 45.0, Si.Yoctometer2, 4.5E49, 1E48),
            
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Yottameter2, 4.5E-47, 1E-48),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Zettameter2, 4.5E-41, 1E-42),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Exameter2, 4.5E-35, 1E-36),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Petameter2, 4.5E-29, 1E-30),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Terameter2, 4.5E-23, 1E-24),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Gigameter2, 4.5E-17, 1E-18),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Megameter2, 4.5E-11, 1E-12),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Kilometer2, 4.5E-5, 1E-6),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Hectometer2, 0.0045),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Decameter2, 0.45),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Meter2, 45.0),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Decimeter2, 4500.0),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Centimeter2, 4.5E5),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Millimeter2, 4.5E7, 1E6),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Micrometer2, 4.5E13, 1E12),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Nanometer2, 4.5E19, 1E18),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Picometer2, 4.5E25, 1E24),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Femtometer2, 4.5E31, 1E30),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Attometer2, 4.5E37, 1E36),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Zeptometer2, 4.5E43, 1E42),
            new TesterConvert<Area, Area>(Si.Centimeter2, 4.5E5, Si.Yoctometer2, 4.5E49, 1E48),
            
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Yottameter2, 4.5E-47, 1E-48),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Zettameter2, 4.5E-41, 1E-42),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Exameter2, 4.5E-35, 1E-36),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Petameter2, 4.5E-29, 1E-30),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Terameter2, 4.5E-23, 1E-24),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Gigameter2, 4.5E-17, 1E-18),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Megameter2, 4.5E-11, 1E-12),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Kilometer2, 4.5E-5, 1E-6),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Hectometer2, 0.0045),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Decameter2, 0.45),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Meter2, 45.0),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Decimeter2, 4500.0),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Centimeter2, 4.5E5),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Millimeter2, 4.5E7, 1E6),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Micrometer2, 4.5E13, 1E12),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Nanometer2, 4.5E19, 1E18),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Picometer2, 4.5E25, 1E24),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Femtometer2, 4.5E31, 1E30),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Attometer2, 4.5E37, 1E36),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Zeptometer2, 4.5E43, 1E42),
            new TesterConvert<Area, Area>(Si.Megameter2, 4.5E-11, Si.Yoctometer2, 4.5E49, 1E48),
        };

        private static readonly ITester[] GetConvertTemperature =
        {
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 123.0, Imperial.Fahrenheit, 253.4),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 12.4, Imperial.Fahrenheit, 54.32),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 0.0, Imperial.Fahrenheit, 32.0),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, -36.3, Imperial.Fahrenheit, -33.34),
            
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 123.0, Si.Kelvin, 396.15),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 12.4, Si.Kelvin, 285.55),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, 0.0, Si.Kelvin, 273.15),
            new TesterConvert<Temperature, Temperature>(Si.Celsius, -36.3, Si.Kelvin, 236.85),
            
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 396.15, Si.Celsius, 123.0),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 285.55, Si.Celsius, 12.4),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 273.15, Si.Celsius, 0.0),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 236.85, Si.Celsius, -36.3),
            
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 253.4, Si.Celsius, 123.0),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 54.32, Si.Celsius, 12.4),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 32.0, Si.Celsius, 0.0),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, -33.34, Si.Celsius, -36.3),
            
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 253.4, Si.Kelvin, 396.15),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 54.32, Si.Kelvin, 285.55),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, 32.0, Si.Kelvin, 273.15),
            new TesterConvert<Temperature, Temperature>(Imperial.Fahrenheit, -33.34, Si.Kelvin, 236.85),
            
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 396.15, Imperial.Fahrenheit, 253.4),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 285.55, Imperial.Fahrenheit, 54.32),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 273.15, Imperial.Fahrenheit, 32.0),
            new TesterConvert<Temperature, Temperature>(Si.Kelvin, 236.85, Imperial.Fahrenheit, -33.34),
        };

        private static readonly ITester[] GetConvertTime =
        {
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Second, 518400),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Minute, 8640),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Hour, 144),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.SiderealDay, 6.01643),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Week, 0.857_142_857_1),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Fortnight, 0.428_571_428_6),
            new TesterConvert<Time, Time>(Si.Day, 6, Si.Month, 0.197_260_057_8),
            new TesterConvert<Time, Time>(Si.Day, 30, Si.Month, 0.986_300_289),
            new TesterConvert<Time, Time>(Si.Day, 300, Si.Year, 0.821_355_236_1),
            new TesterConvert<Time, Time>(Si.Day, 146_879, Si.SiderealYear, 402.125_784),
            new TesterConvert<Time, Time>(Si.Day, 146_879, Si.Year, 402.132_785_763),
            new TesterConvert<Time, Time>(Si.Day, 146_879, Si.Decade, 40.213_278_576_3),
            new TesterConvert<Time, Time>(Si.Day, 146_879, Si.Century, 4.021_327_857_63),
            new TesterConvert<Time, Time>(Si.Day, 146_879, Si.Millennium, 0.402_132_785_763),
            
            new TesterConvert<Time, Time>(Si.Second, 518_400, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Minute, 8640, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Hour, 144, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.SiderealDay, 6.01643, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Week, 0.857_142_857_1, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Fortnight, 0.428_571_428_6, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Month, 0.197_260_057_8, Si.Day, 6, 1),
            new TesterConvert<Time, Time>(Si.Month, 0.986_300_289, Si.Day, 30, 1),
            new TesterConvert<Time, Time>(Si.Year, 0.821_355_236_1, Si.Day, 300, 1),
            new TesterConvert<Time, Time>(Si.SiderealYear, 402.125_784, Si.Day, 146_879, 1),
            new TesterConvert<Time, Time>(Si.Year, 402.132_785_763, Si.Day, 146_879, 1),
            new TesterConvert<Time, Time>(Si.Decade, 40.213_278_576_3, Si.Day, 146_879, 1),
            new TesterConvert<Time, Time>(Si.Century, 4.021_327_857_63, Si.Day, 146_879, 1),
            new TesterConvert<Time, Time>(Si.Millennium, 0.402_132_785_763, Si.Day, 146_879, 1),
            
            new TesterConvert<Time, Time>(Si.Year, 6, Si.SiderealYear, 5.9999),
            new TesterConvert<Time, Time>(Si.Year, 66_482, Si.Decade, 6648.2),
            new TesterConvert<Time, Time>(Si.Year, 66_482, Si.Century, 664.82),
            new TesterConvert<Time, Time>(Si.Year, 66_482, Si.Millennium, 66.482),
            new TesterConvert<Time, Time>(Si.Year, 6_648_200, Si.Megayear, 6.6482),
            new TesterConvert<Time, Time>(Si.Year, 6_648_200_000, Si.Billionyear, 6.6482),
            
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.PlanckTime, 4.5E-43, 1E-44),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Yottasecond, 4.5E-23, 1E-24),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Zettasecond, 4.5E-20, 1E-21),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Exasecond, 4.5E-17, 1E-18),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Petasecond, 4.5E-14, 1E-15),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Terasecond, 4.5E-11, 1E-12),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Gigasecond, 4.5E-8, 1E-9),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Megasecond, 4.5E-5, 1E-6),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Kilosecond, 0.045, 0.001),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Hectosecond, 0.45, 0.01),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Decasecond, 4.5, 0.1),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Second, 45.0),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Decisecond, 450.0, 10.0),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Centisecond, 4500.0, 100.0),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Millisecond, 4.5E4, 1E3),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Microsecond, 4.5E7, 1E6),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Nanosecond, 4.5E10, 1E9),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Picosecond, 4.5E13, 1E12),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Femtosecond, 4.5E16, 1E15),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Attosecond, 4.5E19, 1E18),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Zeptosecond, 4.5E22, 1E21),
            new TesterConvert<Time, Time>(Si.Second, 45.0, Si.Yoctosecond, 4.5E25, 1E25),
            
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.PlanckTime, 4.5E-43, 1E-44),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Yottasecond, 4.5E-23, 1E-24),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Zettasecond, 4.5E-20, 1E-21),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Exasecond, 4.5E-17, 1E-18),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Petasecond, 4.5E-14, 1E-15),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Terasecond, 4.5E-11, 1E-12),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Gigasecond, 4.5E-8, 1E-9),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Megasecond, 4.5E-5, 1E-6),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Kilosecond, 0.045, 0.001),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Hectosecond, 0.45, 0.01),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Decasecond, 4.5, 0.1),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Second, 45.0),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Decisecond, 450.0, 10.0),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Centisecond, 4500.0, 100.0),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Millisecond, 4.5E4, 1E3),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Microsecond, 4.5E7, 1E6),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Nanosecond, 4.5E10, 1E9),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Picosecond, 4.5E13, 1E12),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Femtosecond, 4.5E16, 1E15),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Attosecond, 4.5E19, 1E18),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Zeptosecond, 4.5E22, 1E21),
            new TesterConvert<Time, Time>(Si.Millisecond, 4.5E4, Si.Yoctosecond, 4.5E25, 1E25),
        };
        
        private static readonly ITester[] GetConvertVolume =
        {
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 7123.2348, Imperial.Feet3, 0.2516),
            new TesterConvert<Volume, Volume>(Imperial.Feet3, 0.2516, Si.Centimeter3, 7124.5186),

            
            
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 6124.45, Imperial.Inch3, 373.736_869_521),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 6124.45, Imperial.Feet3, 0.216_282_910_5),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 612_324.45, Imperial.Yard3, 0.800_890_143_6),
            
            new TesterConvert<Volume, Volume>(Imperial.Inch3, 2411.2008, Si.Centimeter3, 39_512.501_826_451_2),
            new TesterConvert<Volume, Volume>(Imperial.Feet3, 200.9334, Si.Centimeter3, 5_689_800.263),
            new TesterConvert<Volume, Volume>(Imperial.Yard3, 66.977_800_003_818, Si.Centimeter3, 51_208_202.369_999_826),
            
            new TesterConvert<Volume, Volume>(Si.Meter3, 6124.45, Imperial.Inch3, 373_736_869.520_983_136_5),
            new TesterConvert<Volume, Volume>(Si.Meter3, 6_114_261_724.45, Imperial.Mile3, 1.466_889_397),
            new TesterConvert<Volume, Volume>(Si.Meter3, 61_243_485.999_631, Imperial.Furlong3, 7.522_819_084_7),
            new TesterConvert<Volume, Volume>(Si.Meter3, 648_135_466, Imperial.NauticalMile3, 0.102_033_513_016),
            
            new TesterConvert<Volume, Volume>(Si.Millimeter3, 0.539_999_970_32, Imperial.Thou3, 32_952.82),
            
            new TesterConvert<Volume, Volume>(Imperial.Inch3, 9_492_916.4858, Si.Meter3, 155.561_029_999_5),
            new TesterConvert<Volume, Volume>(Imperial.Mile3, 1.466_889_397, Si.Meter3, 6_114_261_724.506_891_655_5),
            new TesterConvert<Volume, Volume>(Imperial.Furlong3, 8, Si.Meter3, 65_127_841.0225),
            new TesterConvert<Volume, Volume>(Imperial.Thou3, 223_487_645_827, Si.Meter3, 0.003_662_306_355_376_38),
            new TesterConvert<Volume, Volume>(Imperial.NauticalMile3, 0.102_033_513_016, Si.Meter3, 648_135_466),
            
            
            
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Yottameter3, 4.5E-71, 1E-72),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Zettameter3, 4.5E-62, 1E-63),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Exameter3, 4.5E-53, 1E-54),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Petameter3, 4.5E-44, 1E-45),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Terameter3, 4.5E-35, 1E-36),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Gigameter3, 4.5E-26, 1E-27),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Megameter3, 4.5E-17, 1E-18),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Kilometer3, 4.5E-8, 1E-9),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Hectometer3, 4.5E-5),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Decameter3, 4.5E-2),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Meter3, 45.0),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Decimeter3, 4.5E4),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Centimeter3, 4.5E7),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Millimeter3, 4.5E10, 1E9),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Micrometer3, 4.5E19, 1E18),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Nanometer3, 4.5E28, 1E27),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Picometer3, 4.5E37, 1E36),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Femtometer3, 4.5E46, 1E45),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Attometer3, 4.5E55, 1E54),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Zeptometer3, 4.5E64, 1E63),
            new TesterConvert<Volume, Volume>(Si.Meter3, 45.0, Si.Yoctometer3, 4.5E73, 1E72),
            
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Yottameter3, 4.5E-71, 1E-72),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Zettameter3, 4.5E-62, 1E-63),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Exameter3, 4.5E-53, 1E-54),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Petameter3, 4.5E-44, 1E-45),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Terameter3, 4.5E-35, 1E-36),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Gigameter3, 4.5E-26, 1E-27),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Megameter3, 4.5E-17, 1E-18),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Kilometer3, 4.5E-8, 1E-9),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Hectometer3, 4.5E-5),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Decameter3, 4.5E-2),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Meter3, 45.0),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Decimeter3, 4.5E4),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Centimeter3, 4.5E7),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Millimeter3, 4.5E10, 1E9),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Micrometer3, 4.5E19, 1E18),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Nanometer3, 4.5E28, 1E27),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Picometer3, 4.5E37, 1E36),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Femtometer3, 4.5E46, 1E45),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Attometer3, 4.5E55, 1E54),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Zeptometer3, 4.5E64, 1E63),
            new TesterConvert<Volume, Volume>(Si.Centimeter3, 4.5E7, Si.Yoctometer3, 4.5E73, 1E72),
            
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Yottaliter, 4.5E-23, 1E-24),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Zettaliter, 4.5E-20, 1E-21),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Exaliter, 4.5E-17, 1E-18),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Petaliter, 4.5E-14, 1E-15),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Teraliter, 4.5E-11, 1E-12),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Gigaliter, 4.5E-8, 1E-9),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Megaliter, 4.5E-5, 1E-6),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Kiloliter, 0.045, 0.001),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Hectoliter, 0.45, 0.01),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Decaliter, 4.5, 0.1),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Liter, 45.0),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Deciliter, 450.0, 10.0),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Centiliter, 4500.0, 100.0),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Milliliter, 4.5E4, 1E3),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Microliter, 4.5E7, 1E6),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Nanoliter, 4.5E10, 1E9),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Picoliter, 4.5E13, 1E12),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Femtoliter, 4.5E16, 1E15),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Attoliter, 4.5E19, 1E18),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Zeptoliter, 4.5E22, 1E21),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Yoctoliter, 4.5E25, 1E25),
            
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Meter3, 0.045),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Decimeter3, 45.0, 0.1),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Si.Centimeter3, 45000.0, 10.0),
            
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Gallon, 9.898_62),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Quart, 39.594_48),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Pint, 79.188_96),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Cup, 158.377_793),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.FluidOunce, 1583.777_935),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Tablespoon, 2534.044_69),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Imperial.Teaspoon, 7602.134_07),
            
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Gallon, 11.8877),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Quart, 47.551),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Pint, 95.1019),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Cup, 190.2039, 0.01),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.FluidOunce, 1521.631),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Tablespoon, 3043.2620),
            new TesterConvert<Volume, Volume>(Si.Liter, 45.0, Uscs.Teaspoon, 9129.7861),
        };

        [TestCaseSource(nameof(GetConvertAngle))]
        public void TestConvertAngle(ITester tester) =>
            tester.Invoke();

        [Test]
        public void TestConvertAbstract()
        {
            Unit storeyA = Storey.Abstract.From(2);
            Unit meterA = storeyA.To(Si.Meter, Imperial.Feet * 14);
            Assert.AreEqual(8.5344, meterA.Value, 0.0001);
            
            Unit storeyB = new Unit(2, Storey.Abstract, Imperial.Feet * 14);
            Unit meterB = storeyB.To(Si.Meter);
            Assert.AreEqual(8.5344, meterB.Value, 0.0001);
            
            Unit storeyC = new Unit(2, Storey.Abstract, Si.Meter * 1);
            Unit storeyD = storeyC.To(Storey.Abstract, Si.Meter * 4);
            Assert.AreEqual(0.5, storeyD.Value, 0.01);
        }
        
        [Test]
        public void TestConvertAbstractArray()
        {
            
            double defaultTolerance = GlobalSettings.DefaultFloatingPointTolerance;
            SetFloatingPointTolerance(0.0001);
            
            double[] values = {2.0, 4.0, 8.0};
            
            Unit storeyA = Storey.Abstract.From(2);
            Func<double, double> formulaA = storeyA.ToFormula(Si.Meter, Imperial.Feet * 14);
            double[] resultA = values.Select(value => formulaA(value)).ToArray();
            Assert.That(new [] {8.5344, 17.0688, 34.1376}, Is.EquivalentTo(resultA));
            
            Unit storeyB = new Unit(4, Storey.Abstract, Imperial.Feet * 14);
            Func<double, double> formulaB = storeyB.ToFormula(Si.Meter);
            double[] resultB = values.Select(value => formulaB(value)).ToArray();
            Assert.That(new [] {8.5344, 17.0688, 34.1376}, Is.EquivalentTo(resultB));
            
            Func<double, double> formulaC = Storey.Abstract.ToFormula(Si.Meter, Si.Meter * 2);
            double[] resultC = values.Select(value => formulaC(value)).ToArray();
            Assert.That(new [] {4.0, 8.0, 16.0}, Is.EquivalentTo(resultC));

            double[] resultD = Storey.Abstract.To(values, Si.Meter, Si.Meter * 4).ToArray();
            Assert.That(new [] {8.0, 16.0, 32.0}, Is.EquivalentTo(resultD));
            
            SetFloatingPointTolerance(defaultTolerance);
        }

        public static void SetFloatingPointTolerance(double value) => GlobalSettings.DefaultFloatingPointTolerance = value;
        
        [TestCaseSource(nameof(GetConvertLength))]
        public void TestConvertLength(ITester tester) =>
            tester.Invoke();

        [TestCaseSource(nameof(GetConvertMass))]
        public void TestConvertMass(ITester tester) =>
            tester.Invoke();

        [TestCaseSource(nameof(GetConvertRadioactivity))]
        public void TestConvertRadioactivity(ITester tester) =>
            tester.Invoke();

        [TestCaseSource(nameof(GetConvertSurface))]
        public void TestConvertSurface(ITester tester) =>
            tester.Invoke();

        [TestCaseSource(nameof(GetConvertTemperature))]
        public void TestConvertTemperature(ITester tester) =>
            tester.Invoke();

        [TestCaseSource(nameof(GetConvertTime))]
        public void TestConvertTime(ITester tester) =>
            tester.Invoke();

        private static readonly object[][] GetToTimeSpan =
        {
            new object[] { 100, Si.Millisecond, new TimeSpan(0, 0, 0, 0, 100) },
            new object[] { 10000, Si.Millisecond, new TimeSpan(0, 0, 0, 0, 10000) },
            new object[] { 10000, Si.Millisecond, new TimeSpan(0, 0, 0, 10, 0) },
            new object[] { 10, Si.Second, new TimeSpan(0, 0, 0, 10, 0) },
            new object[] { 22, Si.Day, new TimeSpan(22, 0, 0, 0, 0) },
        };
            
        [TestCaseSource(nameof(GetToTimeSpan))]
        public void TestConvertToTimeSpan(int value, ITime unitDef, TimeSpan expected)
        {
            Unit source = unitDef.From(value);
            
            Assert.AreEqual(expected, source.ToTimeSpan());
            Assert.AreEqual(source, unitDef.From(expected));
        }

        [TestCaseSource(nameof(GetConvertVolume))]
        public void TestConvertVolume(ITester tester) =>
            tester.Invoke();
        

        [TestCaseSource(nameof(GetConvertAngle))]
        public void TestConvertAngleArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertLength))]
        public void TestConvertLengthArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertMass))]
        public void TestConvertMassArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertRadioactivity))]
        public void TestConvertRadioactivityArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertSurface))]
        public void TestConvertSurfaceArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertTemperature))]
        public void TestConvertTemperatureArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertTime))]
        public void TestConvertTimeArray(ITester tester) =>
            tester.InvokeOnArray();

        [TestCaseSource(nameof(GetConvertVolume))]
        public void TestConvertVolumeArray(ITester tester) =>
            tester.InvokeOnArray();
    }
    
    internal class TesterConvert<T1, T2> : ITester
        where T1 : UnitDef<T1>
        where T2 : UnitDef<T2>
    {
        private readonly UnitDef<T1> m_UnitDefSource;
        private readonly UnitDef<T2> m_UnitDefDestination;
        private readonly double m_Expected;
        private readonly double m_Value;
        private readonly double m_Precision;
        
        public TesterConvert(UnitDef<T1> unitDefSource, double value, UnitDef<T2> unitDefDestination, double expected, double precision = 0.0001)
        {
            m_UnitDefSource = unitDefSource;
            m_UnitDefDestination = unitDefDestination;
            m_Expected = expected;
            m_Value = value;
            m_Precision = precision;
        }

        public void Invoke()
        {
            double value = m_UnitDefSource.From(m_Value).To(m_UnitDefDestination).Value;
            
            Assert.AreEqual(m_Expected, value, m_Precision);
            Assert.AreEqual(value, m_UnitDefSource.To(m_Value, m_UnitDefDestination), 0.0001);
            Assert.AreEqual(value, m_UnitDefSource.To(m_Value, m_UnitDefDestination), 0.0001);
            Assert.AreEqual(value, m_UnitDefSource.From(m_Value).To(m_UnitDefDestination).Value, 0.0001);
        }

        public void InvokeOnArray()
        {
            double defaultTolerance = GlobalSettings.DefaultFloatingPointTolerance;
            TestsConvert.SetFloatingPointTolerance(m_Precision);
            
            double[] expected = {m_Expected, m_Expected, m_Expected, m_Expected, m_Expected};
            double[] toConvert = {m_Value, m_Value, m_Value, m_Value, m_Value};
            double[] converted = m_UnitDefSource.To(toConvert, m_UnitDefDestination).ToArray();
            
            Assert.That(expected, Is.EquivalentTo(converted));
            
            TestsConvert.SetFloatingPointTolerance(defaultTolerance);
        }

        public override string ToString()
        {
            string power = m_UnitDefSource.PowerMin == m_UnitDefSource.PowerMax
                ? m_UnitDefSource.ToStringPower(m_UnitDefSource.PowerMin)
                : "";
            
            return
                $"{m_UnitDefSource.GetType().Name}[{m_UnitDefSource.Name}{power}]({m_Value}) > {m_UnitDefDestination.GetType().Name}[{m_UnitDefDestination.Name}{power}]({m_Expected})";
        }
    }
}
