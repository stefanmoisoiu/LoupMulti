using System;
using System.Collections.Generic;

public interface IArithmetic<T>
{
    T Multiply(T a, T b);
    T Add(T a, T b);
    T One { get; }
    T Zero { get; }
}

public class IntArithmetic : IArithmetic<int>
{
    public int Multiply(int a, int b) => a * b;
    public int Add(int a, int b) => a + b;
    public int One => 1;
    public int Zero => 0;
}
public class FloatArithmetic : IArithmetic<float>
{
    public float Multiply(float a, float b) => a * b;
    public float Add(float a, float b) => a + b;
    public float One => 1;
    public float Zero => 0;
}

public class StatModifier<T>
{
    private readonly IArithmetic<T> _arithmetic;
    public List<ModifierComponent> _modifiers = new();

    public StatModifier()
    {
        if (typeof(T) == typeof(int)) _arithmetic = new IntArithmetic() as IArithmetic<T>;
        else if (typeof(T) == typeof(float)) _arithmetic = new FloatArithmetic() as IArithmetic<T>;
        else throw new Exception("Unsupported type");
    }

    public T GetFactor()
    {
        T factor = _arithmetic.One;
        foreach (var modifier in _modifiers) factor = _arithmetic.Multiply(factor, modifier.factor);
        return factor;
    }

    public T GetAdded()
    {
        T added = _arithmetic.Zero;
        foreach (var modifier in _modifiers) added = _arithmetic.Add(added, modifier.added);
        return added;
    }

    public T Apply(T value)
    {
        return _arithmetic.Multiply(GetFactor(),_arithmetic.Add(value, GetAdded()));
    }
    
    public void AddModifier(ModifierComponent modifier)
    {
        _modifiers.Add(modifier);
    }
    public void RemoveModifier(ModifierComponent modifier)
    {
        _modifiers.Remove(modifier);
    }

    [Serializable]
    public class ModifierComponent
    {
        public T factor;
        public T added;

        public ModifierComponent(T factor, T added)
        {
            this.factor = factor;
            this.added = added;
        }
    }
}