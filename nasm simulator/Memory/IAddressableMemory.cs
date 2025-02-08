public interface IAddressableMemory
{
    bool IsOutOfBounds(long address, int bytesToWrite);
    void Write(long address, byte value);
    byte Read(long address);
    void Dump(string filepath);
}