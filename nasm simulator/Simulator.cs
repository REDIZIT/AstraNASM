﻿using Astra.Simulation;

public class Simulator
{
    public Regs regs;
    public RAM ram;

    public long currentInstructionPointer;

    public const bool FILL_ZERO_ON_POP = true;

    public static string ramDumpFilepath = "ram.bin";

    private List<string> instructions;

    public void Execute(string[] lines, int stackAddress = 256)
    {
        currentInstructionPointer = 0;

        regs = new(stackAddress);
        ram = new();

        instructions = Compiler.Compile_NASM_to_Instructions(lines);

        while (currentInstructionPointer < instructions.Count)
        {
            Execute(instructions[(int)currentInstructionPointer]);
            currentInstructionPointer++;
        }

        ram.Dump(ramDumpFilepath);
    }
    public void Execute(string line)
    {
        string[] args = Utils.Split(line);
        string cmd = args[0];

        if (args.Length == 0 || string.IsNullOrWhiteSpace(cmd)) return;

        if (cmd == "mov")
        {
            string typeStr = args[1];
            int bytes = 8;

            if (typeStr == "byte") bytes = 1;
            else if (typeStr == "word") bytes = 2;
            else if (typeStr == "dword") bytes = 4;
            else if (typeStr == "qword") bytes = 8;

            string destStr = args[args.Length - 2];
            string valueStr = args[args.Length - 1];

            if (regs.TryGetReg(destStr, out Reg64 destReg))
            {
                long value = Utils.ParseDec(valueStr, regs);

                if (valueStr.StartsWith('['))
                {
                    value = ram.ReadAs(value, bytes);
                }

                regs.Set(destStr, value);
            }
            else
            {
                long toAddress = Utils.ParseDec(destStr, regs);

                long value = Utils.ParseDec(valueStr, regs);
                if (valueStr.StartsWith('['))
                {
                    value = ram.ReadAs(value, bytes);
                }

                ram.WriteAs(toAddress, value, bytes);
            }
        }
        else if (cmd == "add" || cmd == "sub" || cmd == "mul" || cmd == "div")
        {
            long a = Utils.ParseDec(args[1], regs);
            long b = Utils.ParseDec(args[2], regs);

            if (cmd == "add") a += b;
            if (cmd == "sub") a -= b;
            if (cmd == "mul") a *= b;
            if (cmd == "div") a /= b;

            regs.Set(args[1], a);
        }
        else if (cmd.StartsWith("j"))
        {
            int.TryParse(args[1], out int absAddress);

            if (cmd == "jmp")
            {
                JumpTo(absAddress);
            }
            else
            {
                long a = regs.eflags.a;
                long b = regs.eflags.b;

                if (cmd == "je" && a == b) JumpTo(absAddress);
                else if (cmd == "jne" && a != b) JumpTo(absAddress);
                else if (cmd == "jg" && a > b) JumpTo(absAddress);
                else if (cmd == "jge" && a >= b) JumpTo(absAddress);
                else if (cmd == "jl" && a < b) JumpTo(absAddress);
                else if (cmd == "jle" && a <= b) JumpTo(absAddress);
            }
        }
        else if (cmd == "cmp")
        {
            long a = Utils.ParseDec(args[1], regs);
            long b = Utils.ParseDec(args[2], regs);

            regs.eflags.RememberComprassion(a, b);
        }
        else if (cmd == "push")
        {
            long value = Convert.ToInt64(regs.Get(args[1]));
            Push(value);
        }
        else if (cmd == "pop")
        {
            long value = Pop();
            regs.Set(args[1], value);
        }
        else if (cmd == "call")
        {
            Push(currentInstructionPointer);

            long address = long.Parse(args[1]);
            JumpTo(address);
        }
        else if (cmd == "ret")
        {
            currentInstructionPointer = Pop();
        }
        else if (cmd == "test")
        {
            Console.WriteLine("test is not implemented");
        }
        else if (cmd == "loop")
        {
            long i = regs.rcx.Get64();
            i--;
            regs.rcx.Set64(i);

            if (i > 0)
            {
                currentInstructionPointer = int.Parse(args[1]);
            }
        }
        else if (cmd == "exit")
        {
            currentInstructionPointer = int.MaxValue - 1;
        }
        else if (cmd == "#")
        {
            // Breakpoint

            ram.Dump(ramDumpFilepath);
            Console.ReadLine();
        }
        else if (cmd == "print")
        {
            if (args[1].StartsWith('"'))
            {
                int begin = line.IndexOf('"');
                int end = line.LastIndexOf('"');
                Console.WriteLine(line.Substring(begin, end - begin + 1));
            }
            else
            {
                long value = Utils.ParseDec(args[1], regs);

                if (args[1].StartsWith("["))
                {
                    value = ram.Read64(value);
                }

                Console.WriteLine(args[1] + " = " + value + " (0x" + value.ToString("x") + ")");
            }
        }
        else if (cmd == "neg")
        {
            Reg64 reg = regs.GetReg(args[1]);
            long value = reg.Get64();
            value = -value;
            reg.Set64(value);
        }
        else if (cmd.StartsWith("set"))
        {
            string p = cmd[3..];
            regs.Set(args[1], CanSet(p) ? 1 : 0);
        }
        else
        {
            throw new($"Unknown instruction '{line}'");
        }
    }

    private bool CanSet(string p)
    {
        if (p == "e") return regs.eflags.a == regs.eflags.b;
        if (p == "ne") return regs.eflags.a != regs.eflags.b;
        if (p == "g") return regs.eflags.a > regs.eflags.b;
        if (p == "ge") return regs.eflags.a >= regs.eflags.b;
        if (p == "l") return regs.eflags.a < regs.eflags.b;
        if (p == "le") return regs.eflags.a <= regs.eflags.b;

        throw new Exception($"Unknown setXXX instruction {p}");
    }

    private void JumpTo(long absAddress)
    {
        currentInstructionPointer = absAddress - 1; // -1 due to instructionPointer++ in Execute
    }

    private void Push(long value)
    {
        long address = regs.rsp.Get64();
        address -= 8; // 64 bit = 8 bytes = 1 long
        regs.rsp.Set64(address); 

        ram.Write64(address, value);
    }
    private long Pop()
    {
        long address = regs.rsp.Get64();
        long value = ram.Read64(address);

        if (FILL_ZERO_ON_POP)
        {
            ram.Write64(address, 0);
        }

        address += 8; // // 64 bit = 8 bytes = 1 long
        regs.rsp.Set64(address);

        return value;
    }
}