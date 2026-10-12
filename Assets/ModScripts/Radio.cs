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
    public AudioSource[] AudioGroup;
    public AudioClip[] NurseryRhymes;

    public StickyNote StickyNote;
    public InputDisplay InputDisplay;

    public KMSelectable[] Dials;

    private List<RadioChannel> _channels;

    private int _channelIndex, _volumeIndex;
    private int _correctIndex, _oppositeIndex;

    private KMBombInfo _bomb;
    private KMAudio _audio;
    private KMAudio.KMAudioRef _sound;
    private AudioClip[][] _voices;

    private bool _isSolved;

    private static readonly float[] _volumeValues = { 0, 0.25f, 0.5f, 0.75f, 1 };

    private static readonly Color32[] _probabilityColors = { new Color32(255, 128, 0, 255), Color.green };

    private Coroutine _textBlinker, _radioPlayer, _dialClick;

    private bool CheckTime() => StickyNote.GeneratedTimes.Any(x => Math.Abs((int)_bomb.GetTime() - x) <= 2);

    public List<int> ObtainStripNumbers() => StickyNote.StripNumbers.ToList();
    public List<int> ObtainCorrectNumbers() => _channels[_correctIndex].CorrectCode.ToList();

    public void Setup(KMAudio bombAudio, KMBombInfo bomb, params AudioClip[][] voices)
    {
        _audio = bombAudio;
        _voices = voices;
        _bomb = bomb;
        
        _channelIndex = Range(0, 36);
        _correctIndex = RadioChannel.ObtainIndices(bomb.GetBatteryCount() % 2 != 0).PickRandom();
        _oppositeIndex = RadioChannel.ObtainIndices(bomb.GetBatteryCount() % 2 != 0).Where(x => x != _correctIndex).PickRandom();
        
        _channels = RadioChannel.GenerateChannels(_correctIndex, _oppositeIndex);

        do
            StickyNote.StripNumbers = RadioChannel.GenerateDigits();
        while (new[] { _correctIndex, _oppositeIndex }.Select(x => _channels[x].Decoy).Any(StickyNote.StripNumbers.SequenceEqual) || _channels[_correctIndex].CorrectCode.SequenceEqual(StickyNote.StripNumbers));
        
        var voltage = bomb.QueryWidgets("volt", string.Empty).Count == 0 ? null : (double?)double.Parse(bomb.QueryWidgets("volt", string.Empty).First().Substring(12).Replace("\"}", string.Empty));
        
        StickyNote.GenerateTimes(bomb, voltage, _channels[_correctIndex]);
        
        _radioPlayer = StartCoroutine(PlayAudio());
    }

    private void Awake()
    {
        foreach (var dial in Dials)
        {
            dial.OnInteract += () => { DialPress(dial); return false; };
            dial.OnInteractEnded += () => { DialRelease(dial); };
        }
    }

    private void Start()
    {
        DialClickSingle(Dials[0].transform, true);

        _textBlinker = StartCoroutine(SevenSegmentBlink());
    }

    private void DialPress(KMSelectable dial)
    {
        if (_isSolved)
            return;
        
        dial.AddInteractionPunch(0.5f);

        switch ((Dial)Array.IndexOf(Dials, dial))
        {
            case Dial.Channel:
                _sound?.StopSound();
                if (_dialClick != null)
                {
                    StopCoroutine(_dialClick);
                    _dialClick = null;
                }
                if (_radioPlayer != null)
                {
                    StopCoroutine(_radioPlayer);
                    _radioPlayer = null;
                    AudioGroup[0].Stop();
                }

                _dialClick = StartCoroutine(DialClick(dial.transform));
                break;
            case Dial.Volume:
                _audio.PlaySoundAtTransform("dial click", dial.transform);

                _volumeIndex = (_volumeIndex + 1) % 5;

                VolumeDisplay.sprite = VolumeSprites[_volumeIndex];
                VolumeDisplay.color = Color.HSVToRGB(Mathf.Lerp(0, 1 / 3f, _volumeIndex / 4f), 1, 1);
                
                dial.transform.localEulerAngles = new Vector3(dial.transform.localEulerAngles.x, dial.transform.localEulerAngles.y, (_volumeIndex * 72) % 360);

                foreach (var source in AudioGroup)
                    source.volume = _volumeValues[_volumeIndex];
                break;
        }
    }

    private void DialRelease(KMSelectable dial)
    {
        if (_dialClick != null)
        {
            StopCoroutine(_dialClick);
            _dialClick = null;
        }

        _sound?.StopSound();
        
        if (_isSolved || (Dial)Array.IndexOf(Dials, dial) != Dial.Channel)
            return;
        
        AudioGroup[0].Stop();
        
        _radioPlayer = StartCoroutine(PlayAudio());
    }

    private void DialClickSingle(Transform dial, bool setup)
    {
        if (!setup)
            _channelIndex = (_channelIndex + 1) % 36;
        
        dial.localEulerAngles = new Vector3(dial.localEulerAngles.x, dial.localEulerAngles.y, (_channelIndex * 10) % 360);
        
        SevenSegment.text = SevenSegment.text.Length == 3 ? $"{(_channelIndex + 1):00}." : (_channelIndex + 1).ToString("00");
        SevenSegment.color = _probabilityColors[_oppositeIndex == _channelIndex ? 1 : 0];
    }

    private IEnumerator DialClick(Transform dial)
    {
        DialClickSingle(dial, false);
        _audio.PlaySoundAtTransform("dial click", dial);

        var timer = 0f;

        while (timer < 0.5f)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        _sound = _audio.PlaySoundAtTransformWithRef("dial click continuous", dial);

        var timeSinceHold = 0f;

        while (true)
        {
            DialClickSingle(dial, false);
            timer = 0;

            while (timer < Math.Max(0.05f - (timeSinceHold * 0.03f), 0.02f))
            {
                yield return null;
                timer += Time.deltaTime;
                timeSinceHold += Time.deltaTime;
            }
        }
    }

    private IEnumerator PlayAudio()
    {
        while (true)
        {
            var digits = CheckTime() && _channels[_channelIndex].CorrectCode != null ? _channels[_channelIndex].CorrectCode : _channels[_channelIndex].Decoy;
            AudioGroup[0].clip = NurseryRhymes[(int)_channels[_channelIndex].Song];

            var repeatCounter = 0;

            yield return new WaitForSeconds(3);

            while (repeatCounter < _channels[_channelIndex].RepeatCount)
            {
                AudioGroup[0].Play();
                yield return new WaitForSeconds(NurseryRhymes[(int)_channels[_channelIndex].Song].length);
                yield return new WaitForSeconds(1);
                repeatCounter++;
            }

            foreach (var digit in digits)
            {
                AudioGroup[0].clip = _voices[(int)_channels[_channelIndex].Voice][digit];
                AudioGroup[0].Play();
                yield return new WaitForSeconds(1);
            }
        }
    }

    private IEnumerator SevenSegmentBlink()
    {
        while (true)
        {
            var timer = 0f;
            const float interval = 0.5f;

            while (timer < interval)
            {
                yield return null;
                timer += Time.deltaTime; 
            }

            if (SevenSegment.text.Length == 2)
                SevenSegment.text += '.';

            timer = 0;

            while (timer < interval)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            if (SevenSegment.text.Length == 3)
                SevenSegment.text = SevenSegment.text.Substring(0, 2);
        }
    }

    private void OnDestroy()
    {
        _sound?.StopSound();
        
        foreach (var source in AudioGroup)
            source.Stop();
    }

    public void MarkSolve()
    {
        _isSolved = true;
        
        AudioGroup[0].Stop();
        
        StopCoroutine(_textBlinker);
        _textBlinker = null;
        
        StartCoroutine(SolveAnimation());
    }

    public void WriteDigit(out bool readyToCheck)
    {
        InputDisplay.DisplayText.text += (_channelIndex + 1).ToString("00");
        readyToCheck = InputDisplay.DisplayText.text.Length == 14;
    }

    private IEnumerator SolveAnimation()
    {
        SevenSegment.text = "GG";
        SevenSegment.color = Color.green;

        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 14; j++)
            {
                InputDisplay.DisplayText.text = Enumerable.Range(0, 14).Select(x => x == j ? '8' : '-').Join("");
                yield return new WaitForSeconds(0.08f);
            }

        SevenSegment.text = "--.";
        InputDisplay.DisplayText.text = $"{Enumerable.Repeat('-', 14).Join("")}.";
        AudioGroup[1].Stop();
    }
}