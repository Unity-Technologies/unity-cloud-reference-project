# Unity Geospatial Unit
Library allowing to encapsulate and convert **numeric** values from one **measurement** unit to another.

- [Unit Definition](#unit-definition)
   * [Create a New Unit Definition](#create-a-new-unit-definition)
- [Unit](#unit)
   * [Constructor](#constructor)
   * [Convert](#convert)
   * [Operators](#operators)
   * [String Decoding](#string-decoding)
- [String Formatting](#string-formatting)
   * [Unit Formatter](#unitformatter)
   * [Fractional Unit Formatter](#fractionalunitformatter)
   * [Multiple Unit Formatter](#multiunitformatter)
   * [Multiple Fractional Unit Formatter](#multifractionalunitformatter)

## Unit Definition
**Definitions** are a measurement system that, once defined, can be converted to an other **unit definition** to the same type.
For example, you can convert from *centimeter* **length** to *inches*.
Or you can convert from *cubic centimeter* to *liter*.
But you cannot convert from *gram* to *celsius* since *mass* is not the same type as *temperature*.

### Create a New Unit Definition
Many **Unit Definitions** are available in the unit systems `Si`, `Astro`, `Imperial`, `Misc`, `Money`, `Storey`, `Uscs`.
Still, if you need to create a new one, create a new instance of either `Angle`, `AngularArea`, `Area`, `Currency`, `Frequency`, `Length`, `Mass`, `Radioactivity`, `SubstanceAmount`, `Temperature`, `Time` or `Volume`.
And make sure you call the static constructor before using string decoding, otherwise your custom definition will not be considered.
<br/>
<br/>

## Unit
**Unit** strut is the encapsulation of a **unit definition**, its amount and its power value.
<br/>
<br/>

### Constructor
Creating a new **Unit** instance.

| units                | code                                         |
|----------------------|----------------------------------------------|
| 5 centimeters        | `Unit unit = new Unit(5, Si.Centimeter);`    |
| 5 square centimeters | `Unit unit = new Unit(5, Si.Centimeter, 2);` |
| 5 cubic centimeter   | `Unit unit = new Unit(5, Si.Centimeter, 3);` |
| 5 meters             | `Unit unit = Si.Meter.From(5);`              |

<br/>
<br/>

### Convert
Units can be converted to other **Unit Definitions** as long the **UnitDef** type is the same.
For example, you cannot convert from a **Length** to a **Temperature**.

```
Unit feet = new Unit(5, Imperial.Feet);
Unit cm = feet.To(Si.Centimeter);
```

### Operators
Mathematical operators are implemented and can be used between units even if the units are not using the same **UnitDef** (as long the type is the same).
The left operator side is the **Unit** defining the new **Unit.UnitDef**.

```
Unit cm = new Unit(5, Si.Centimeter);
Unit feet = new Unit(5, Imperial.Feet);

Unit totalCm = cm + feet - 3.0;
```
<br/>

Multiplication and division will set a new **Power** value if both sides are **Unit** instances.

```
Unit m = new Unit(2, Si.Meter);
Unit feet = new Unit(5, Imperial.Feet);

Unit squareResult = m * feet;  // 3.048 meters²
Unit linearResult = squareResult / m;  // 1.524 meters
```

```
Unit m = new Unit(2, Si.Meter);
Unit feet = new Unit(5, Imperial.Feet);

Unit cubicResult = m * feet * m;  // 6.096 meters³
Unit squareResult = cubicResult / feet;  // 4 meters²
Unit linearResult = squareResult / m;  // 2 meters
```

```
Unit m = new Unit(2, Si.Meter2);
Unit feet = new Unit(5, Imperial.Feet);

Unit cubicResult = m * feet;  // 3.048 meters³
```

```
Unit m = new Unit(2, Si.Meter);
Unit feet = new Unit(5, Imperial.Feet2);

Unit cubicResult = m * feet;  // 3.048 meters³
```
<br/>

Multiplying by a **double** will not change the **Power** value.
```
Unit m = new Unit(2, Si.Meter);
Unit linearResult = m * 2;  // 4 meters
Unit squareResult = m * m;  // 4 meters²
Unit result = squareResult / 2;  // 2 meters²
```
<br/>

You can also compare **Units** together as long the **UnitDef** type is the same.

```
Unit m = new Unit(2, Si.Meter);
Unit feet = new Unit(5, Imperial.Feet);

bool result = m > feet;  // true
```
<br/>
<br/>

### String Decoding
You can can decode units from strings, but since this is a longer process, it is better to the other
instantiation ways when possible. If you don't know the values (from a file for example), string decoding
is then the best way.

Be sure the string you give has only one **Unit** value. If multiple **Units** are part of the string,
it will merged to one allowing to convert imperial units as of `5'6"` would be converted to `5.5'`.

To decode one string when you don't know anything about the content:

| units         | code                             |
|---------------|----------------------------------|
| 5 centimeters | `Unit unit = Unit.From("5 cm");` |
| 5 meters      | `Unit unit = (Unit)"5cm";`       |
| 5 grams       | `Unit unit = Mass.From("5g");`   |

<br/>
<br/>

Some units uses the same symbol. To prevent wrong decoding and to accelerate the evaluation, it is better to
restrict the search to only the expected **Unit Definition** types.

| units         | code                                                                          |
|---------------|-------------------------------------------------------------------------------|
| 5 feet        | `Unit unit = Unit.From("5'", typeof(Length));`                                |
| 5 arc minutes | `Unit unit = Unit.From<Angle>("5'");`                                         |
| 5 feet        | `Unit unit = Unit.From("5'", typeof(Length), typeof(Area), typeof(Volume));`  |
| 5 square feet | `Unit unit = Unit.From("5'²", typeof(Length), typeof(Area), typeof(Volume));` |
| 5 cubic feet  | `Unit unit = Unit.From("5'³", typeof(Length), typeof(Area), typeof(Volume));` |

<br/>
<br/>

When decoding one string with multiple unit values, the sum of them will be returned:

| units            | code                                              |
|------------------|---------------------------------------------------|
| 5.25 feet        | `Unit unit = Unit.From("5'3\"", typeof(Length));` |
| 5.25 arc minutes | `Unit unit = Unit.From<Angle>("5'15\"");`         |

<br/>
<br/>

That's why if you have list of unit values, you should split them before:

| units                          | code                                                                          |
|--------------------------------|-------------------------------------------------------------------------------|
| 12 meters                      | `Unit unit = Unit.From("5m;3m;4m");`                                          |
| {5 meters, 3 meters, 4 meters} | `Unit[] units = Unit.From("5m;3m;4m".Split(';')).ToArray();`                  |
| {5 feet, 3 feet, 4 feet}       | ```Unit[] units = Unit.From<Length>("5 ft;3';4feet".Split(';')).ToArray();``` |

<br/>
<br/>


## String Formatting
String formatters are included in the package, they can be configured and passed into the 
`Unit.ToString(IUnitFormatter)` and `Unit.ToFullString(IUnitFormatter)` methods or as an IFormatProvider for `String.Format()`

### UnitFormatter
Basic default unit formatter

`UnitFormatter(bool fullStringFormat = false, bool hasSuperscript = true, int decimalPrecision = Int32.MaxValue, int minimumPrecision = 0)`

| output                     | code                                                                                             |
|----------------------------|--------------------------------------------------------------------------------------------------|
| 5.75'                      | `Unit.From<Length>("5' 7 1/2\"").ToString();`                                                    |
| 5 centimeters              | `Unit.From<Length>("5cm").ToFullString();`                                                       |
| Area: 5 square centimeters | `String.Format(new UnitFormatter(fullStringFormat:true), "Area: {0}", Unit.From<Area>("5cm²"));` |
| 5.7 meters                 | `Si.Meter.From(5.68138).ToString(new UnitFormatter(fullStringFormat:true, decimalPrecision:1));` |

#### Full String Format
When true, the full string name & power are used when formatting. (Default: False)

#### Has Superscript
When true, if the passed unit has a power higher than 1 then the resulting string will include the power or superscript symbol. 
(Default: True)

#### Decimal Precision
The maximum zero place to output when formatting values. (Default: Int32.MaxValue)

#### Minimum Precision
The minimum zero place to output when formatting values. (Default: 0)



### FractionalUnitFormatter
Formats units so decimal values are fractions. This is common with Imperial units.

`FractionalUnitFormatter(bool fullStringFormat = false, bool hasSuperscript = false, int precision = 16, bool richText = false)`

| output     | code                                                                              |
|------------|-----------------------------------------------------------------------------------|
| 1 1/2 Cups | `Uscs.Cup.From(1.5).ToString(new FractionalUnitFormatter(fullStringFormat:true))` |
| 7/8"       | `Imperial.Inch.From(0.859).ToString(new FractionalUnitFormatter(precision:8));`   |
| 1/4 Mile   | `Imperial.Mile.From(0.25).ToString(new FractionalUnitFormatter());`               |

#### Full String Format
When true, the full string name & power are used when formatting. (Default: False)

#### Has Superscript
When true, if the passed unit has a power higher than 1 then the resulting string will include the power or superscript symbol.
(Default: True)

#### Precision
The finest precision that should be used, the value should be a power of 2. 8 = 1/8, 16 = 1/16, 32 = 1/32. (Default: 16)

#### Rich Text
When true, the fraction is wrapped in rich text symbols `<sup>` and `<sub>` for TextMeshPro & HTML. (Default: False)



### MultiUnitFormatter
Converts a single unit into a composite of output units where the remainder of larger units is 
converted into smaller units. Each value is formatted with [UnitFormatter](#UnitFormatter), and concatenated into the output string.

`MultiUnitFormatter(IUnitDef[] units, bool dropZeroScales = true, bool fullStringFormat = false, bool hasSuperscript = true, int decimalPrecision = Int32.MaxValue, int minimumPrecision = 0)`

#### Units
List of Unit Definitions passed in the order they are printed. The remainder of an integer of the first value is passed 
to the second, for that reason it is recommended to order the values from larger to smaller.

#### Drop Zero Scales
When set to true, if a unit would print a zero then it would be ignored unless it's the only unit. (Default: True)

#### Full String Format
When true, the full string name & power are used when formatting. (Default: False)

#### Has Superscript
When true, if the passed unit has a power higher than 1 then the resulting string will include the power or superscript symbol.
(Default: True)

#### Decimal Precision
The maximum zero place to output when formatting values. (Default: Int32.MaxValue)

#### Minimum Precision
The minimum zero place to output when formatting values. (Default: 0)



### MultiFractionalUnitFormatter
Converts a single unit into a composite of output units where the remainder of larger units is
converted into smaller units. Each value is formatted with [FractionalUnitFormatter](#FractionalUnitFormatter), and concatenated into the output string.

`MultiFractionalUnitFormatter(IUnitDef[] units, bool dropZeroScales = true, bool fullStringFormat = false, bool hasSuperscript = false, int precision = 16, bool richText = false)`

#### Units
List of Unit Definitions passed in the order they are printed. The remainder of an integer of the first value is passed
to the second, for that reason it is recommended to order the values from larger to smaller.

#### Drop Zero Scales
When set to true, if a unit would print a zero then it would be ignored unless it's the only unit. (Default: True)

#### Full String Format
When true, the full string name & power are used when formatting. (Default: False)

#### Has Superscript
When true, if the passed unit has a power higher than 1 then the resulting string will include the power or superscript symbol.
(Default: True)

#### Precision
The finest precision that should be used, the value should be a power of 2. 8 = 1/8, 16 = 1/16, 32 = 1/32. (Default: 16)

#### Rich Text
When true, the fraction is wrapped in rich text symbols `<sup>` and `<sub>` for TextMeshPro & HTML. (Default: False)
