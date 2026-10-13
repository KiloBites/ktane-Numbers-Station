using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class NumberEncryptor
{
    private readonly List<int> _stripNumbers;
    public List<int> GeneratedAnswer { get; private set; }

    public NumberEncryptor(List<int> stripNumbers)
    {
        _stripNumbers = stripNumbers;
        GeneratedAnswer = GenerateAnswer();
    }

    private static List<int> GenerateAnswer()
    {
        var generated = new List<int>();

        while (generated.Count < 14)
        {
            var num = Range(0, 36) + 1;
            generated.AddRange(num.ToString("00").Select(x => x - '0'));
        }
        
        return generated;
    }

    public int[] EncryptNumbers()
    {
        var stripDigitStr = _stripNumbers.Join("");
        var decryptedAnswer = GeneratedAnswer.Join("");

        var digitGroup = new List<int[]>();
        
        for (int i = 0; i < 14; i += 2)
            digitGroup.Add(new[] { int.Parse(decryptedAnswer.Substring(i, 2)), int.Parse(stripDigitStr.Substring(i, 2)) });

        return digitGroup.Select(x => (x.Sum() % 100).ToString("00")).Join("").Select(x => x - '0').ToArray();
    }
}
