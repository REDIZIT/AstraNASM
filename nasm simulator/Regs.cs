public class Regs
{
    public Reg64 rax = new();
    public Reg64 rbx = new();
    public Reg64 rcx = new();
    public Reg64 rdx = new();

    public Reg64 rsp = new();
    public Reg64 rbp = new();

    public Reg64 rdi = new();
    public Reg64 rsi = new();

    //public Reg64 rip = new();

    public FakeEFlags eflags = new();

    public Regs()
    {
        rsp.value = 256;
        rbp.value = rsp.value;
    }

    public Reg64 GetReg(string name)
    {
        if (TryGetReg(name, out Reg64 reg))
        {
            return reg;
        }
        throw new($"Unknown reg name '{name}'");
    }
    public bool TryGetReg(string name, out Reg64 reg)
    {
        reg = TryGetReg(name);
        return reg != null;
    }
    public Reg64 TryGetReg(string name)
    {
        if (name == "rax" || name == "eax" || name == "ax" || name == "al" || name == "ah") return rax;
        if (name == "rbx" || name == "ebx" || name == "bx" || name == "bl" || name == "bh") return rbx;
        if (name == "rcx" || name == "ecx" || name == "cx" || name == "cl" || name == "ch") return rcx;
        if (name == "rdx" || name == "edx" || name == "dx" || name == "dl" || name == "dh") return rdx;

        if (name == "rsp") return rsp;
        if (name == "rbp") return rbp;

        if (name == "rdi") return rdi;
        if (name == "rsi") return rsi;

        return null;
    }

    public void Set(string name, object value)
    {
        Reg64 reg = GetReg(name);
        int bits = GetRegBits(name);

        if (bits == 64)
        {
            reg.Set64((long)value);
        }
        else if (bits == 32)
        {
            reg.Set32(Convert.ToInt32(value));
        }
        else if (bits == 16)
        {
            reg.Set16(Convert.ToInt16(value));
        }
        else if (bits == 8 && name.EndsWith('l'))
        {
            reg.Set8(Convert.ToByte(value), false);
        }
        else if (bits == 8 && name.EndsWith('h'))
        {
            reg.Set8(Convert.ToByte(value), true);
        }
        else
        {
            throw new($"Unknown reg name '{name}'");
        }
    }
    public object Get(string name)
    {
        object value = TryGet(name);
        if (value == null)
        {
            throw new($"Unknown reg name '{name}'");
        }
        return value;
    }
    public bool TryGet(string name, out object value)
    {
        value = TryGet(name);
        return value != null;
    }
    public object TryGet(string name)
    {
        if (TryGetReg(name, out Reg64 reg) == false) return null;
        int bits = GetRegBits(name);

        if (bits == 64)
        {
            return reg.Get64();
        }
        else if (bits == 32)
        {
            return reg.Get32();
        }
        else if (bits == 16)
        {
            return reg.Get16();
        }
        else if (bits == 8 && name.EndsWith('l'))
        {
            return reg.Get8(false);
        }
        else if (bits == 8 && name.EndsWith('h'))
        {
            return reg.Get8(true);
        }
        else
        {
            return null;
        }
    }

    public int GetRegBits(string name)
    {
        if (name.StartsWith('r'))
        {
            return 64;
        }
        else if (name.StartsWith('e'))
        {
            return 32;
        }
        else if (name.Length == 2 && name.EndsWith('x'))
        {
            return 16;
        }
        else if (name.Length == 2 && (name.EndsWith('l') || name.EndsWith('h')))
        {
            return 8;
        }
        else
        {
            throw new($"Unknown reg name '{name}'");
        }
    }
}
public class FakeEFlags
{
    public long a, b;

    public void RememberComprassion(long a, long b)
    {
        this.a = a;
        this.b = b;
    }
}