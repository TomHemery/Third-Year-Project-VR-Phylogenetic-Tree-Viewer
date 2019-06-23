using System.Collections.Generic;

public static class Tokeniser {

    public const char openBracket = '(', closeBracket = ')', childSeparator = ',',
          treeTerminator = ';', quote = '\'', doubleQuote = '"', infoSeparator = ':';

    public static List<string> Tokenise(string target)
    {
        List<string> tokens = new List<string>();

        string word = "";
        bool readingWord = false;
        foreach (char c in target)
        {
            switch (c)
            {
                case openBracket:
                case closeBracket:
                case childSeparator:
                case treeTerminator:
                case quote:
                case doubleQuote:
                case infoSeparator:
                    {
                        if (readingWord)
                        {
                            tokens.Add(word);
                            readingWord = false;
                        }
                        tokens.Add("" + c);
                        break;
                    }
                default:
                    {
                        if (!readingWord)
                        {
                            readingWord = true;
                            word = "";
                        }
                        word += c;
                        break;
                    }
            }
        }

        return tokens;
    } 
}
