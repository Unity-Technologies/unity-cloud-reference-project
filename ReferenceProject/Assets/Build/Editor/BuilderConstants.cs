using System;

namespace Unity.ReferenceProject.Editor
{
    public static class BuilderConstants
    {
        public const string ANDROID_BUILD_TARGET = "Android";
        public const string IOS_BUILD_TARGET = "iOS";
        public const string OSX_BUILD_TARGET = "StandaloneOSX";
        public const string WIN_BUILD_TARGET = "StandaloneWindows64";
        public const string WEBGL_BUILD_TARGET = "WebGL";

        public const string DEFAULT_BUILD_DIRECTORY = "Builds";
    }

    public static class BuilderArguments
    {
        public const string APP_ID = "-appId";
        public const string APP_NAME = "-appName";
        public const string APP_DISPLAY_NAME = "-appDisplayName";
        public const string BUILD_TARGET = "-buildTarget";
        public const string OUTPUT_PATH = "-outputPath";
    }
}
