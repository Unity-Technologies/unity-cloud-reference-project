fastlane documentation
================
# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```
xcode-select --install
```

Install _fastlane_ using
```
[sudo] gem install fastlane -NV
```
or alternatively using `brew install fastlane`

# Available Actions
## iOS
### ios test
```
fastlane ios test
```

### ios development_build
```
fastlane ios development_build
```
Build a development APK
### ios distribution_build
```
fastlane ios distribution_build
```
Builds a distribution APK
### ios run_build
```
fastlane ios run_build
```
Makes the build!
### ios increment
```
fastlane ios increment
```
Update Build Number

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.
More information about fastlane can be found on [fastlane.tools](https://fastlane.tools).
The documentation of fastlane can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
