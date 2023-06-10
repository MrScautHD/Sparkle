using System.Diagnostics;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Sparkle.csharp.graphics.vulkan; 

public unsafe class GraphicsBuffer : IDisposable {
    
    private readonly Vk _vk;
    private readonly GraphicsDevice _device;

    public ulong BufferSize { get; private set; }
    public uint InstanceCount { get; private set; }
    public ulong InstanceSize { get; private set; }
    public ulong AlignmentSize { get; private set; }

    public BufferUsageFlags UsageFlags { get; private set; }
    public MemoryPropertyFlags MemoryPropertyFlags { get; private set; }

    private void* _mapped;
    public readonly Buffer Buffer;
    
    private DeviceMemory _memory;

    public GraphicsBuffer(Vk vk, GraphicsDevice device, ulong instanceSize, uint instanceCount, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags, ulong minOffsetAlignment = 1) {
        this._vk = vk;
        this._device = device;
        this.InstanceSize = instanceSize;
        this.InstanceCount = instanceCount;
        this.UsageFlags = usageFlags;
        this.MemoryPropertyFlags = memoryPropertyFlags;
        this.AlignmentSize = this.GetAlignment(instanceSize, minOffsetAlignment);
        this.BufferSize = this.AlignmentSize * instanceCount;
        device.CreateBuffer(this.BufferSize, usageFlags, memoryPropertyFlags, ref this.Buffer, ref this._memory);
    }

    private ulong GetAlignment(ulong instanceSize, ulong minOffsetAlignment) {
        if (minOffsetAlignment > 0) {
            return (instanceSize + minOffsetAlignment - 1) & ~(minOffsetAlignment - 1);
        }
        
        return instanceSize;
    }
    
    public Result Map(ulong size = Vk.WholeSize, ulong offset = 0) {
        Debug.Assert(this.Buffer.Handle != 0 && this._memory.Handle != 0, "Called map on buffer before create");
        return this._vk.MapMemory(this._device.VkDevice, this._memory, offset, size, 0, ref this._mapped);
    }

    public void UnMap() {
        if (this._mapped != null) {
            this._vk.UnmapMemory(this._device.VkDevice, this._memory);
            this._mapped = null;
        }
    }

    public T ReadFromBuffer<T>() where T : unmanaged {
        var data = new T[1];
        
        fixed (void* dataPtr = data) {
            int dataSize = sizeof(T) * data.Length;
            System.Buffer.MemoryCopy(this._mapped, dataPtr, dataSize, dataSize);
        }
        
        return data[0];
    }
    
    public void WriteToBuffer<T>(T[] data, ulong size = Vk.WholeSize, ulong offset = 0) where T : unmanaged {
        if (size == Vk.WholeSize) {
            fixed (void* dataPtr = data) {
                int dataSize = sizeof(T) * data.Length;
                System.Buffer.MemoryCopy(dataPtr, this._mapped, dataSize, dataSize);
            }
        }
        else {
            throw new NotImplementedException("Don't have offset stuff working yet!");
        }
    }
    public void WriteToBuffer<T>(T data, ulong size = Vk.WholeSize, ulong offset = 0) where T : unmanaged {
        if (size == Vk.WholeSize) {
            var data2 = new T[] { data };
            fixed (void* dataPtr = data2) {
                var dataSize = sizeof(T) * data2.Length;
                System.Buffer.MemoryCopy(dataPtr, this._mapped, dataSize, dataSize);
            }
        }
        else {
            throw new NotImplementedException("Don't have offset stuff working yet!");
        }
    }

    public void WriteBytesToBuffer(byte[] data) {
        fixed (void* dataPtr = data) {
            int dataSize = sizeof(byte) * data.Length;
            System.Buffer.MemoryCopy(dataPtr, this._mapped, dataSize, dataSize);
        }
    }

    public void WriteToIndex<T>(T[] data, int index) {
        Span<T> tmpSpan = new Span<T>(this._mapped, data.Length);
        data.AsSpan().CopyTo(tmpSpan[index..]);
    }
    
    public Result Flush(ulong size = Vk.WholeSize, ulong offset = 0) {
        MappedMemoryRange mappedRange = new() {
            SType = StructureType.MappedMemoryRange,
            Memory = this._memory,
            Offset = offset,
            Size = size
        };
        
        return this._vk.FlushMappedMemoryRanges(this._device.VkDevice, 1, mappedRange);
    }

    public Result Invalidate(ulong size = Vk.WholeSize, ulong offset = 0) {
        MappedMemoryRange mappedRange = new() {
            SType = StructureType.MappedMemoryRange,
            Memory = this._memory,
            Offset = offset,
            Size = size
        };
        
        return this._vk.InvalidateMappedMemoryRanges(this._device.VkDevice, 1, mappedRange);
    }

    public DescriptorBufferInfo DescriptorInfo(ulong size = Vk.WholeSize, ulong offset = 0) {
        return new DescriptorBufferInfo() {
            Buffer = this.Buffer,
            Offset = offset,
            Range = size
        };
    }
    
    public Result FlushIndex(int index) { 
        return this.Flush(this.AlignmentSize, (ulong) index * this.AlignmentSize); 
    }
    
    public DescriptorBufferInfo DescriptorInfoForIndex(int index) {
        return this.DescriptorInfo(this.AlignmentSize, (ulong) index * this.AlignmentSize);
    }
    
    public Result InvalidateIndex(int index) {
        return this.Invalidate(this.AlignmentSize, (ulong) index * this.AlignmentSize);
    }

    public void Dispose() {
        this.UnMap();
        this._vk.DestroyBuffer(this._device.VkDevice, this.Buffer, null);
        this._vk.FreeMemory(this._device.VkDevice, this._memory, null);
        GC.SuppressFinalize(this);
    }
}