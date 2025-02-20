namespace Astra.Compilation;

public struct Range
{
    public int begin, end;

    public Range(int begin, int end)
    {
        this.begin = begin;
        this.end = end;
    }

    public bool IsOverlap(Range another)
    {
        if (begin <= another.begin && another.begin < end) return true;
        if (begin <= another.end && another.end < end) return true;
        return false;
    }
}