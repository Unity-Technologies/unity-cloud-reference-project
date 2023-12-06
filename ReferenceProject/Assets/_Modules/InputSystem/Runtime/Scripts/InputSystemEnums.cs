namespace Unity.ReferenceProject.InputSystem
{
    /// <summary>
    /// Used to describes InputScheme uses.
    /// InputScheme.Other will be treated differently as other types.
    /// An new InputScheme will always be created for InputSchemeType.Other.
    /// </summary>
    public enum InputSchemeType
    {
        FlyOrbital,
        WalkMode,
        Annotation,
        ObjectSelection,
        Follow,
        Measurement,
        Other
    }

    /// <summary>
    /// Used to regroup multiple InputScheme together to enable/disable them.
    /// </summary>
    public enum InputSchemeCategory
    {
        Controller,
        Tools,
        Other
    }
}