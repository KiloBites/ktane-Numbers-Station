using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using static UnityEngine.Random;
using static NurseryRhyme;

public class StickyNote : MonoBehaviour
{
    private static readonly int[] _penColorProbabilities = { 16, 8, 4, 2, 1 };

    private static readonly Color32[] _possiblePenColors =
    {
        new Color32(0, 0, 0, 230),
        new Color32(0, 0, 128, 230),
        new Color32(200, 0, 0, 230),
        new Color32(0, 128, 0, 230),
        new Color32(147, 28, 137, 230)
    };

    private static readonly NurseryRhyme[] _leftHalfSongs =
    {
        HickoryDickoryDock,
        OldMacDonald,
        TheMuffinMan,
        TheAlphabetSong,
        WheelsOnTheBus,
        Bingo,
        ItsyBitsySpider,
        PopGoesTheWeasel,
        ThreeBlindMice
    };

    public TextMesh TimesText;
    public SpriteRenderer StickyNoteSprite;

    [NonSerialized]
    public List<int> GeneratedTimes, UnmodifiedTimes;

    [NonSerialized] 
    public int[] StripNumbers;

    private void Start()
    {
        var rand = Range(0, _penColorProbabilities.Sum());
        
        transform.localPosition = new Vector3(Range(-0.0015f, 0.0065f), transform.localPosition.y, transform.localPosition.z);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Range(-15f, 15f));

        StickyNoteSprite.color = Color.HSVToRGB(Range(0, 1f), 0.5f, 1);

        var colorIndex = _possiblePenColors.Length - 1 - Enumerable.Range(0, 5).First(x => rand < _penColorProbabilities.Reverse().Take(x + 1).Sum());
        
        TimesText.color = _possiblePenColors[colorIndex];
    }

    public void GenerateTimes(KMBombInfo bomb, double? voltage, RadioChannel usedChannel)
    {
        var startedTime = (int)bomb.GetTime();
        var randomTimes = Range(3, 6);
        
        var unmodifiedTimes = new List<int>();

        if (startedTime != 0 && startedTime < 5) // The game will break without this check.
            unmodifiedTimes = Enumerable.Range(0, startedTime).ToList();
        else if (startedTime == 0) // Zen mode
        {
            unmodifiedTimes = new List<int> { Range(120, 601) };

            while (unmodifiedTimes.Count < randomTimes)
            {
                var newTime = Range(120, 601);
                
                if (!unmodifiedTimes.Contains(newTime))
                    unmodifiedTimes.Add(newTime);
            }
        }
        else
        {
            unmodifiedTimes = new List<int> { Range(120, (startedTime / 2) + 1) };

            while (unmodifiedTimes.Count < randomTimes)
            {
                var newTime = Range(120, (startedTime / 2) + 1);
                
                if (!unmodifiedTimes.Contains(newTime))
                    unmodifiedTimes.Add(newTime);
            }
        }

        unmodifiedTimes.Sort();
        
        UnmodifiedTimes = unmodifiedTimes.ToList();
        
        TimesText.text = unmodifiedTimes.Select(x => $"{(int)TimeSpan.FromSeconds(x).TotalMinutes}:{TimeSpan.FromSeconds(x).Seconds:00}").Join("\n");

        if ((voltage != null && voltage < 3) || bomb.IsPortPresent(Port.AC))
        {
            GeneratedTimes = unmodifiedTimes.ToList();
            return;
        }

        var offset = usedChannel.RepeatCount * 10 + unmodifiedTimes.Sum(x => x % 10);

        if (_leftHalfSongs.Contains(usedChannel.Song))
            offset /= 2;

        GeneratedTimes = unmodifiedTimes.Select(x => x + offset).ToList();
    }
}