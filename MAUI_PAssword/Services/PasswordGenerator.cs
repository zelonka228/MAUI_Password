using System.Security.Cryptography;

namespace MAUI_PAssword.Services;

public static class PasswordGenerator
{
    const string Lower = "abcdefghijklmnopqrstuvwxyz";
    const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string Digits = "0123456789";
    const string Symbols = "!@#$%^&*()-_=+[]{};:,.?";

    public static string Generate(int length, bool lower, bool upper, bool digits, bool symbols)
    {
        var sets = Sets(lower, upper, digits, symbols);
        if (sets.Count == 0)
            return "";

        length = Math.Max(length, sets.Count);
        var all = string.Concat(sets);
        var result = new char[length];

        // по одному символу з кожного вибраного набору, решта з усього пулу
        for (int i = 0; i < length; i++)
            result[i] = Pick(i < sets.Count ? sets[i] : all);

        for (int i = length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return new string(result);
    }

    public static double Entropy(int length, bool lower, bool upper, bool digits, bool symbols)
    {
        int pool = string.Concat(Sets(lower, upper, digits, symbols)).Length;
        return pool == 0 ? 0 : length * Math.Log2(pool);
    }

    static List<string> Sets(bool lower, bool upper, bool digits, bool symbols)
    {
        var sets = new List<string>();
        if (lower) sets.Add(Lower);
        if (upper) sets.Add(Upper);
        if (digits) sets.Add(Digits);
        if (symbols) sets.Add(Symbols);
        return sets;
    }

    static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
