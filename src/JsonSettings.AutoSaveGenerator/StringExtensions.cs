using System;

namespace Nucs.JsonSettings.Autosave;

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        input = ConvertToPropertyName(input);
        return input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };
    }

    public static string ConvertToPropertyName(string inputName)
    {
        // Remove common prefixes
        if (inputName.StartsWith("_"))
            inputName = inputName.Substring(1);
        else if (inputName.StartsWith("m_"))
            inputName = inputName.Substring(2);

        return inputName;
    }
}
