using System.Collections.Generic;
using System.Linq;

public struct NumberDecryptor
{
    private readonly List<int> _receivedDigits;
    private readonly List<int> _writtenDigits;

    public NumberDecryptor(List<int> receivedDigits, List<int> writtenDigits)
    {
        _receivedDigits = receivedDigits;
        _writtenDigits = writtenDigits;
    }

    public List<int> DecryptNumbers()
    {
        var receivedStr = _receivedDigits.Join("");
        var writtenStr = _writtenDigits.Join("");
        
        var digitsToDecrypt = new List<int[]>();
        
        for (int i = 0; i < 14; i += 2)
            digitsToDecrypt.Add(new[] { int.Parse(receivedStr.Substring(i, 2)), int.Parse(writtenStr.Substring(i, 2)) });

        var decrypted = digitsToDecrypt.Select(x => Subtract(x[0], x[1])).ToList();
        
        return decrypted.Select(x => x.ToString("00")).Join("").Select(x => x - '0').ToList();
    }

    private static int Subtract(int a, int b)
    {
        var result = a < b ? (a + 100) - b : a - b;

        while (result > 36)
            result -= 36;

        if (result == 0)
            result = 36;

        return result;
    }
}