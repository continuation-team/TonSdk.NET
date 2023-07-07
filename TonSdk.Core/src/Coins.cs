﻿using System.Numerics;
using System.Text.RegularExpressions;

namespace TonSdk.Core;
public class CoinsOptions
{
    public bool IsNano { get; set; }
    public int Decimals { get; set; }

    public CoinsOptions(bool IsNano = false, int Decimals = 9)
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

    /// <summary>
    /// Creates a new instance of the Coins class.
    /// </summary>
    /// <param name="value">The value of the coins.</param>
    /// <param name="options">Optional options for customizing the coins.</param>
    public Coins(object value, CoinsOptions? options = null)
    {
        bool isNano = false;
        int decimals = 9;

        if (options != null)
        {
            isNano = options?.IsNano != null ? options.IsNano : false;
            decimals = options?.Decimals != null ? options.Decimals : 9;
        }

        if (value is string) value = value.ToString().Replace(".", ",");

        CheckCoinsType(value);
        CheckCoinsDecimals(decimals);

        decimal decimalValue = Convert.ToDecimal(value.ToString());

        int digitsValue = GetDigitsAfterDecimalPoint(decimalValue);
        if (digitsValue > decimals)
        {
            throw new Exception($"Invalid Coins value, decimals places \"{digitsValue}\" can't be greater than selected \"{decimals}\"");
        }

        Decimals = decimals;
        Multiplier = new(Math.Pow(10, Decimals));
        Value = !isNano ? decimalValue * Multiplier : decimalValue;
    }

    /// <summary>
    /// Adds the specified Coins to the current instance.
    /// </summary>
    /// <param name="coins">The Coins to add.</param>
    /// <returns>A new Coins instance with the sum of the values.</returns>
    public Coins Add(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);

        Value += coins.Value;
        return this;
    }

    /// <summary>
    /// Subtracts the specified Coins from the current instance.
    /// </summary>
    /// <param name="coins">The Coins to subtract.</param>
    /// <returns>A new Coins instance with the difference of the values.</returns>
    public Coins Sub(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);

        Value -= coins.Value;
        return this;
    }

    /// <summary>
    /// Multiplies the current instance of Coins by the specified value.
    /// </summary>
    /// <param name="value">The value to multiply by.</param>
    /// <returns>A new Coins instance with the multiplied value.</returns>
    public Coins Mul(object value)
    {
        CheckValue(value);
        CheckConvertibility(value);

        var multiplier = Convert.ToDecimal(value);

        Value *= multiplier;
        return this;
    }

    /// <summary>
    /// Divides the current instance of Coins by the specified value.
    /// </summary>
    /// <param name="value">The value to divide by.</param>
    /// <returns>A new Coins instance with the divided value.</returns>
    public Coins Div(object value)
    {
        CheckValue(value);
        CheckConvertibility(value);

        var divider = Convert.ToDecimal(value);

        Value /= divider;
        return this;
    }

    /// <summary>
    /// Checks if the current instance of Coins is equal to the specified Coins.
    /// </summary>
    /// <param name="coins">The Coins to compare.</param>
    /// <returns>True if the values are equal, false otherwise.</returns>
    public bool Eq(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value == coins.Value;
    }

    /// <summary>
    /// Checks if the current instance of Coins is greater than the specified Coins.
    /// </summary>
    /// <param name="coins">The Coins to compare.</param>
    /// <returns>True if the current value is greater, false otherwise.</returns>
    public bool Gt(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value > coins.Value;
    }

    /// <summary>
    /// Checks if the current instance of Coins is greater or equal than the specified Coins.
    /// </summary>
    /// <param name="coins">The Coins to compare.</param>
    /// <returns>True if the current value is greater or equal, false otherwise.</returns>
    public bool Gte(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value >= coins.Value;
    }

    /// <summary>
    /// Checks if the current instance of Coins is less than the specified Coins.
    /// </summary>
    /// <param name="coins">The Coins to compare.</param>
    /// <returns>True if the current value is less, false otherwise.</returns>
    public bool Lt(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value < coins.Value;
    }

    /// <summary>
    /// Checks if the current instance of Coins is less or equal than the specified Coins.
    /// </summary>
    /// <param name="coins">The Coins to compare.</param>
    /// <returns>True if the current value is less or equal, false otherwise.</returns>
    public bool Lte(Coins coins)
    {
        CheckCoins(coins);
        CompareCoinsDecimals(this, coins);
        return Value <= coins.Value;
    }

    /// <summary>
    /// Checks if the coins is negative.
    /// </summary>
    /// <returns>True if the coins is negative; otherwise, false.</returns>
    public bool IsNegative() => Value < 0;

    /// <summary>
    /// Checks if the coins is positive.
    /// </summary>
    /// <returns>True if the coins is positive; otherwise, false.</returns>
    public bool IsPositive() => Value > 0;

    /// <summary>
    /// Checks if the coins is equal to zero.
    /// </summary>
    /// <returns>True if the coins is equal to zero; otherwise, false.</returns>
    public bool IsZero() => Value == 0;

    /// <summary>
    /// Converts the Coins to its nano string representation.
    /// </summary>
    /// <returns>The string representation of the nano Coins.</returns>
    public string ToNano() => Value.ToString("F0");

    /// <summary>
    /// Returns a string representation of the Coins value.
    /// </summary>
    /// <returns>A string representation of the Coins value.</returns>
    public override string ToString()
    {
        decimal value = Value / Multiplier;
        string formattedValue = value.ToString("F" + Decimals);

        // Remove trailing zeros
        var re1 = new Regex($"\\.{new string('0', Decimals)}$");
        var re2 = new Regex("(\\.[0-9]*?[0-9])0+$");

        string coins = re2.Replace(re1.Replace(formattedValue, string.Empty), "$1");

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

    private static void CheckCoins(Coins value)
    {
        //if (IsCoins(value)) return;
        //throw new Exception("Invalid value");
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

    private static void CheckConvertibility(object value)
    {
        if (IsConvertable(value)) return;

        throw new Exception("Invalid value");
    }

    private static bool IsValid(object value)
    {
        return value is string || value is int || value is decimal || value is double || value is float || value is long;
    }

    private static bool IsConvertable(object value)
    {
        try
        {
            Convert.ToDecimal(value.ToString());
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
        string[] parts = number.ToString().Split(',');
        if (parts.Length == 2)
        {
            return parts[1].Length;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Creates a new Coins instance from the specified value in nano.
    /// </summary>
    /// <param name="value">The value in nano.</param>
    /// <param name="decimals">The number of decimal places.</param>
    /// <returns>A new Coins instance representing the value in nano.</returns>
    public static Coins FromNano(object value, int decimals = 9)
    {
        CheckCoinsType(value);
        CheckCoinsDecimals(decimals);

        return new Coins(value, new CoinsOptions(true, decimals));
    }

    /// <summary>
    /// Converts the value of the Coins instance to a BigInteger.
    /// </summary>
    /// <returns>A BigInteger representation of the value.</returns>
    public BigInteger ToBigInt()
    {
        return new BigInteger(Value);
    }
}
