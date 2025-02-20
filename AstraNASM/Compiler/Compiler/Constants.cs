namespace AVM.Compiler;

public static class Constants
{
    /// <summary>
    /// Size in bytes of pushed by 'call' instruction number
    /// </summary>
    public const int CALL_PUSH_SIZE = 4;
    
    /// <summary>
    /// Size in bytes of RBP register. Used for Prologue and Epilogue.
    /// </summary>
    public const int RBP_REG_SIZE = 4;
}