# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2022-10-20

This update adds formatting tools for Units to aid in converting and casting units from one value to other values.

### Added
- Format Providers
  - UnitFormatter
  - FractionalUnitFormatter
  - MultiUnitFormatter
  - MultiFractionalUnitFormatter
- Mixed number support when parsing units
- Rich text is now stripped before units are parsed
- Unit and UnitDef's ToString and ToFullString now accept a Format provider, defaulting to UnitFormatter.

## [Unreleased]

### Modified

- [Breaking] Changed the UnitDef constructor signatures by creating the new UnitNaming struct.

## [0.1.0] - 2021-11-10

### Added

This is the first release of *Unity Package Geospatial Unit*.

Initial release

Implemented the following unit types:
 - Angle
 - AngularArea
 - Area
 - Currency
 - Frequency
 - Length
 - Mass
 - Radioactivity
 - SubstanceAmount
 - Temperature
 - Time
 - Volume
