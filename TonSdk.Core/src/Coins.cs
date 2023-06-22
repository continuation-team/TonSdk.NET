using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TonSdk.Core;
public class CoinsOptions
{
    public bool IsNano { get; set; }
    public int Decimals { get; set; }

    public CoinsOptions(bool IsNano, int Decimals) 
    {
        this.IsNano = IsNano;
        this.Decimals = Decimals;
    }
}

public class Coins
{
    private decimal Value { get; set; }
    private int Decimals { get; set; }
    private decimal Multiplier { get; set; }

    public Coins(object value, CoinsOptions? options = null)
    {
        bool isNano = false;
        int decimals = 9;

        if (options != null)
        {
            isNano = options?.IsNano != null ? options.IsNano : false;
            decimals = options?.Decimals != null ? options.Decimals : 9;
        }

        CheckCoinsType(value);
        CheckCoinsDecimals(decimals);

        decimal decimalValue = decimal.Parse(value.ToString());

        int digitsValue = GetDigitsAfterDecimalPoint(decimalValue);
        if (digitsValue > decimals)
        {
            throw new Exception($"Invalid Coins value, decimals places \"{digitsValue}\" can't be greater than selected \"{decimals}\"");
        }

        Decimals = decimals;
        Multiplier = (1 * 10) ^ Decimals;
        Value = !isNano ? decimalValue * Multiplier : decimalValue;
    }

    public Coins Add(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);

        Value = decimal.Add(coins.Value, Value);
        return this;
    }

    public Coins Sub(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);

        Value = decimal.Subtract(Value, coins.Value);
        return this;
    }

    public Coins Mul(object value)
    {
        CheckValue(value);
        CheckConvertability(value);

        var multiplier = Convert.ToInt64(value);

        Value = decimal.Multiply(Value, multiplier);
        return this;
    }

    public Coins Div(object value)
    {
        CheckValue(value);
        CheckConvertability(value);

        var divider = Convert.ToInt64(value);

        Value = decimal.Divide(Value, divider);
        return this;
    }

    public bool Eq(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value == coins.Value;
    }

    public bool Gt(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value > coins.Value;
    }

    public bool Gte(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value >= coins.Value;
    }

    public bool Lt(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value < coins.Value;
    }

    public bool Lte(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value <= coins.Value;
    }

    public bool IsNegative() => Value < 0;

    public bool IsPositive() => Value > 0;

    public bool IsZero() => Value == 0;

    public string ToNano() => Value.ToString("F0");

    public override string ToString()
    {
        var value = (Value / Multiplier).ToString($"F{Decimals}");

        var re1 = new Regex($@"\.0{{{Decimals}}}$");
        var re2 = new Regex(@"(\.[0-9]*?[0-9])0+$");
        var coins = re2.Replace(re1.Replace(value, ""), "$1");

        return coins;
    }

    private static void CheckCoinsType(object value)
    {
        if (IsValid(value) && IsConvertable(value)) return;
        if (IsCoins(value)) return;

        throw new Exception("Invalid Coins value");
    }

    private static void CheckCoinsDecimals(int decimals)
    {
        if (decimals < 0 || decimals > 18)
        {
            throw new Exception("Invalid decimals value, must be 0-18");
        }
    }

    private static void CheckCoins(object value)
    {
        if (IsCoins(value)) return;
        throw new Exception("Invalid value");
    }

    private static void CompareCoinsDecimals(Coins a, Coins b)
    {
        if (a.Decimals != b.Decimals)
        {
            throw new Exception("Can't perform mathematical operation of Coins with different decimals");
        }
    }

    private static void CheckValue(object value)
    {
        if (IsValid(value)) return;
        throw new Exception("Invalid value");
    }

    private static void CheckConvertability(object value)
    {
        if (IsConvertable(value)) return;

        throw new Exception("Invalid value");
    }

    private static bool IsValid(object value)
    {
        // TODO: узнать про тип данных
        return value is string || value is int || value is decimal;
    }

    private static bool IsConvertable(object value)
    {
        try
        {
            new decimal(Convert.ToDouble(value.ToString()));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsCoins(object value)
    {
        return value is Coins;
    }

    private static int GetDigitsAfterDecimalPoint(decimal number)
    {
        string[] parts = number.ToString().Split('.');
        if (parts.Length == 2)
        {
            return parts[1].Length;
        }
        else
        {
            return 0;
        }
    }

    public static Coins FromNano(object value, int decimals = 9)
    {
        CheckCoinsType(value);
        CheckCoinsDecimals(decimals);

        return new Coins(value, new CoinsOptions(false, decimals));
    }
}