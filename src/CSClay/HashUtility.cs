using System;

namespace CSClay;

internal static class HashUtility
{
    private const uint FNV_OFFSET_BASIS = 2166136261;
    private const uint FNV_PRIME = 16777619;

    public static uint HashString(ReadOnlySpan<char> str, uint seed)
    {
        uint hash = seed != 0 ? seed : FNV_OFFSET_BASIS;
        for (int i = 0; i < str.Length; i++)
        {
            hash ^= (uint)str[i];
            hash *= FNV_PRIME;
        }
        return hash;
    }

    public static uint HashId(ReadOnlySpan<char> idString, uint parentId)
    {
        return HashString(idString, parentId);
    }
}
