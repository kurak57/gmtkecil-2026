using System.Collections.Generic;
using UnityEngine;

namespace YanderesFrequency.Mechanics
{
    public static class MorseDictionary
    {
        private static readonly Dictionary<char, string> morseAlphabet = new Dictionary<char, string>()
        {
            {'A', ".-"},   {'B', "-..."}, {'C', "-.-."}, {'D', "-.."},  {'E', "."},
            {'F', "..-."}, {'G', "--."},  {'H', "...."}, {'I', ".."},   {'J', ".---"},
            {'K', "-.-"},  {'L', ".-.."}, {'M', "--"},   {'N', "-."},   {'O', "---"},
            {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."},  {'S', "..."},  {'T', "-"},
            {'U', "..-"},  {'V', "...-"}, {'W', ".--"},  {'X', "-..-"}, {'Y', "-.--"},
            {'Z', "--.."}
        };

        /// <summary>
        /// Returns the Morse code string for a given character.
        /// Dots are represented by '.' and dashes by '-'.
        /// Returns empty string if character is not found.
        /// </summary>
        public static string GetMorse(char c)
        {
            char upperChar = char.ToUpper(c);
            if (morseAlphabet.TryGetValue(upperChar, out string morseCode))
            {
                return morseCode;
            }
            return ""; // Not found
        }
    }
}
