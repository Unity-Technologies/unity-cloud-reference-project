fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## iOS

### ios test

```sh
[bundle exec] fastlane ios test
```



### ios match_test

```sh
[bundle exec] fastlane ios match_test
```

Test Match

### ios development_build

```sh
[bundle exec] fastlane ios development_build
```

Build a development APK

### ios distribution_build

```sh
[bundle exec] fastlane ios distribution_build
```

Builds a distribution APK

### ios run_build

```sh
[bundle exec] fastlane ios run_build
```

Makes the build!

### ios push_to_test_flight

```sh
[bundle exec] fastlane ios push_to_test_flight
```

Push build to testflight

### ios increment

```sh
[bundle exec] fastlane ios increment
```

Update Build Number

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
