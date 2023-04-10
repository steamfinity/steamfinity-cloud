namespace Steamfinity.Cloud.Constants;

public static class PropertyLengthConstraints
{
    public const int MinUserNameLength = 3;
    public const int MaxUserNameLength = 16;

    public const int MinLibraryNameLength = 1;
    public const int MaxLibraryNameLength = 32;
    public const int MaxLibraryDescriptionLength = 1024;

    public const int MaxAccountNameLength = 32;
    public const int MaxAliasLength = 32;
    public const int MaxLaunchParametersLength = 1024;
    public const int MaxNotesLength = 1024;
    public const int MinHashtagLength = 1;
    public const int MaxHashtagLength = 32;

    public const int MaxOtherLenght = 1024;
}
