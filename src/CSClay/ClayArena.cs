using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace CSClay;

public class ClayArena
{
    private readonly byte[] _memory;
    private int _nextAllocation;

    public ClayArena(int capacity)
    {
        _memory = new byte[capacity];
        _nextAllocation = 0;
    }

    public byte[] Memory => _memory;
    public int Capacity => _memory.Length;
    public int NextAllocation => _nextAllocation;

    public void Reset()
    {
        _nextAllocation = 0;
        _memory.AsSpan().Clear();
    }

    /// <summary>
    /// Allocates a chunk of memory from the arena.
    /// Returns the offset into the underlying byte array.
    /// </summary>
    public int Allocate<T>(int count) where T : struct
    {
        int size = Unsafe.SizeOf<T>() * count;
        
        // Ensure alignment (8 bytes for simplicity)
        int alignedNextAllocation = (_nextAllocation + 7) & ~7;

        if (alignedNextAllocation + size > _memory.Length)
        {
            throw new InvalidOperationException("Clay Arena: Out of memory.");
        }

        int offset = alignedNextAllocation;
        _nextAllocation = alignedNextAllocation + size;
        
        return offset;
    }

    public Span<T> GetSpan<T>(int offset, int count) where T : struct
    {
        return MemoryMarshal.Cast<byte, T>(_memory.AsSpan(offset, Unsafe.SizeOf<T>() * count));
    }
}
