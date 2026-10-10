using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;
using static UnityEngine.Random;

public class Radio : MonoBehaviour
{
    public TextMesh SevenSegment;
    public SpriteRenderer VolumeDisplay;
    public Sprite[] VolumeSprites;
    public Transform Speaker;
    public AudioSource[] AudioGroup;
    public AudioClip[] NurseryRhymes;

    public StickyNote StickyNote;

    public KMSelectable[] Dials;

    private List<RadioChannel> _channels;

    private int _channelIndex, _volumeIndex;
    private int _correctIndex, _oppositeIndex;

    private KMAudio _audio;
    private KMAudio.KMAudioRef _sound;
    private AudioClip[][] _voices;

    private bool _isSolved;

    private static readonly float[] _volumeValues = { 0, 0.25f, 0.5f, 0.75f, 1 };

    private static readonly Color32[] _probabilityColors = { new Color32(255, 128, 0, 255), Color.green };

    public void Setup(KMAudio audio, KMBombInfo bomb, AudioClip[][] voices)
    {
        _audio = audio;
        _voices = voices;
        
        _correctIndex = RadioChannel.ObtainIndices(bomb.GetBatteryCount() % 2 != 0).PickRandom();
        _oppositeIndex = RadioChannel.ObtainIndices(bomb.GetBatteryCount() % 2 != 0).Where(x => x != _correctIndex).PickRandom();
        
        _channels = RadioChannel.GenerateChannels(_correctIndex, _oppositeIndex);

        do
            StickyNote.StripNumbers = RadioChannel.GenerateDigits();
        while (new[] { _correctIndex, _oppositeIndex }.Select(x => _channels[x].Decoy).Any(StickyNote.StripNumbers.SequenceEqual) || _channels[_correctIndex].CorrectCode.SequenceEqual(StickyNote.StripNumbers));
        
        var voltage = bomb.QueryWidgets("volt", string.Empty).Count == 0 ? null : (double?)double.Parse(bomb.QueryWidgets("volt", string.Empty).First().Substring(12).Replace("\"}", string.Empty));
        
        StickyNote.GenerateTimes(bomb, voltage, _channels[_correctIndex]);
    }

    private void Start()
    {
        _channelIndex = Range(0, 36);
    }

    private void DialPress(KMSelectable dial)
    {
        
    }

    private void DialRelease(KMSelectable dial)
    {
        
    }

    private void DialClickSingle(KMSelectable dial)
    {
        _channelIndex = (_channelIndex + 1) % 36;
        
        dial.transform.localEulerAngles = new Vector3(dial.transform.localEulerAngles.x, dial.transform.localEulerAngles.y, (_channelIndex * 10) % 360);
        
        
    }

    private void OnDestroy()
    {
        _sound?.StopSound();
        
        foreach (var source in AudioGroup)
            source.Stop();
    }

    public void MarkSolve() => _isSolved = true;
}