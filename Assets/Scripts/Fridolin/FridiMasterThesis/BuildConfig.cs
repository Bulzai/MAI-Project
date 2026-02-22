public static class BuildConfig
{
#if AI_VERSION_A
    public const string BuildId = "AI_A";
#elif AI_VERSION_B
    public const string BuildId = "AI_B";
#else
    public const string BuildId = "AI_UNKNOWN";
#endif
}
