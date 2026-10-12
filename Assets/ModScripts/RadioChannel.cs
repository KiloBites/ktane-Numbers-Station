using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public struct RadioChannel
{
    public NurseryRhyme Song { get; private set; }
    public int RepeatCount { get; private set; }
    public Voice Voice { get; private set; }
    public int[] Decoy { get; private set; }
    public int[] CorrectCode { get; private set; }

    private RadioChannel(NurseryRhyme song, int repeatCount, Voice voice, int[] decoy, int[] correctCode)
    {
        Song = song;
        RepeatCount = repeatCount;
        Voice = voice;
        Decoy = decoy;
        CorrectCode = correctCode;
    }

    private static readonly int[][] _childSongIndices =
    {
        new[] { 23, 2 },
        new[] { 18, 6 },
        new[] { 35, 3 },
        new[] { 27, 25 },
        new[] { 5, 14 },
        new[] { 21, 31 },
        new[] { 15, 12 },
        new[] { 10, 7 },
        new[] { 1, 9 },
        new[] { 8, 13 },
        new[] { 16, 19 },
        new[] { 20, 4 },
        new[] { 28, 11 },
        new[] { 33, 30 },
        new[] { 24, 34 },
        new[] { 26, 29 },
        new[] { 0, 18 },
        new[] { 22, 32 }
    };
    
    public static int[] ObtainIndices(bool needFirst) => _childSongIndices.Select(x => needFirst ? x[0] : x[1]).ToArray();

    public static List<RadioChannel> GenerateChannels(int correctIndex, int oppositeIndex)
    {
        var childrenSongFromIndex = Enumerable.Range(0, 36).Select(x => (NurseryRhyme)Enumerable.Range(0, 18).FirstOrDefault(y => _childSongIndices[y].Contains(x))).ToList();

        var childrenSongs = (NurseryRhyme[])Enum.GetValues(typeof(NurseryRhyme));
        var voices = (Voice[])Enum.GetValues(typeof(Voice));
        
        var channels = new List<RadioChannel>();

        while (channels.Count < 36)
        {
            int[] correctCode, decoy;

            do
            {
                correctCode = GenerateDigits();
                decoy = GenerateDigits();
            }
            while (channels.Select(x => x.Decoy).Any(decoy.SequenceEqual) || correctCode.SequenceEqual(decoy));
            
            channels.Add(new RadioChannel(channels.Count == oppositeIndex ? childrenSongFromIndex[correctIndex] : childrenSongs.Where(x => x != childrenSongFromIndex[correctIndex]).PickRandom(), Range(1, 6), voices.PickRandom(), decoy, channels.Count == correctIndex ? correctCode : null));
        }
        
        return channels;
    }

    public static int[] GenerateDigits() => Enumerable.Range(0, 14).Select(_ => Range(0, 10)).ToArray();
}