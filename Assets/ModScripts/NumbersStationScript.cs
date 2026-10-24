using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Debug;
using static NurseryRhyme;

public class NumbersStationScript : MonoBehaviour 
{

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public Radio Radio;
	public KMSelectable Write;

	public TextMesh Strip;

	public AudioClip[] CalebVoices, GhostSaltVoices, GoodHoodVoices, SeanVoices;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved;

	private static Vector3 _writeButtonPos;
	private Coroutine _writePressAnim;
    
	private List<int> _correctSequence;

	private static readonly Dictionary<NurseryRhyme, string> _songTitles = new Dictionary<NurseryRhyme, string>
	{
		[BaaBaaBlackSheep] = "Baa Baa Black Sheep",
		[Bingo] = "Bingo",
		[HickoryDickoryDock] = "Hickory Dickory Dock",
		[HotCrossBuns] = "Hot Cross Buns",
		[ImALittleTeapot] = "I'm A Little Teapot",
		[ItsRainingItsPouring] = "It's Raining, It's Pouring",
		[ItsyBitsySpider] = "Itsy Bitsy Spider",
		[LondonBridge] = "London Bridge",
		[OldMacDonald] = "Old MacDonald",
		[PopGoesTheWeasel] = "Pop! Goes The Weasel",
		[RingAroundTheRosie] = "Ring Around The Rosie",
		[RowRowRowYourBoat] = "Row, Row, Row Your Boat",
		[TheAlphabetSong] = "The Alphabet Song",
		[TheMuffinMan] = "The Muffin Man",
		[ThisOldMan] = "This Old Man",
		[ThreeBlindMice] = "Three Blind Mice", 
		[TwinkleTwinkle] = "Twinkle, Twinkle",
		[WheelsOnTheBus] = "Wheels on the Bus"
	};

	private static readonly NurseryRhyme[] _leftHalf =
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

	private static string GenerateStrip(string digits) => $"{digits.Substring(0, 5)} {digits.Substring(5, 5)} {digits.Substring(10, 4)}";

	private void Awake()
    {
		_moduleId = _moduleIdCounter++;
		
		Write.OnInteract += () => { WritePress(); return false; };
    }

	
	private void Start()
    {
	    Radio.Setup(Audio, Bomb, out _correctSequence, CalebVoices, GhostSaltVoices, GoodHoodVoices, SeanVoices);
	    _writeButtonPos = Write.transform.localPosition;

	    Strip.text = GenerateStrip(Radio.StickyNote.StripNumbers.Join(""));

	    var oddCount = Bomb.GetBatteryCount() % 2 != 0;
	    
	    Log($"[Numbers Station #{_moduleId}] The timestamp(s) written on the sticky note is/are: {Radio.StickyNote.UnmodifiedTimes.Select(x => $"[{(int)TimeSpan.FromSeconds(x).TotalMinutes}:{TimeSpan.FromSeconds(x).Seconds:00}]").Join(", ")}");

	    var voltageMeter = Bomb.QueryWidgets("volt", string.Empty).Count == 0 ? null : (double?)double.Parse(Bomb.QueryWidgets("volt", string.Empty).First().Substring(12).Replace("\"}", string.Empty));
	    
	    if ((voltageMeter != null && voltageMeter < 3) || Bomb.IsPortPresent(Port.AC))
		    Log($"[Numbers Station #{_moduleId}] A voltage meter with less than 3 volts and/or an AC port is present. The timestamps are not modified at all.");
	    else
	    {
		    Log($"[Numbers Station #{_moduleId}] The nursery rhyme playing on the correct channel ({(Radio.ObtainChannelIndex(false) + 1):00}) is: {_songTitles[Radio.ObtainChannel(false).Song]}, repeating {Radio.ObtainChannel(false).RepeatCount} time(s).");
		    Log($"[Numbers Station #{_moduleId}] The sum of the rightmost digits of each timestamp is: {Radio.StickyNote.UnmodifiedTimes.Sum(x => x % 10)}");
		    
		    var offset = (Radio.ObtainChannel(false).RepeatCount * 10) + Radio.StickyNote.UnmodifiedTimes.Sum(x => x % 10);

		    if (_leftHalf.Contains(Radio.ObtainChannel(false).Song))
		    {
			    offset /= 2;
			    Log($"[Numbers Station #{_moduleId}] The correct channel's nursery rhyme is on the left half, dividing the offset in half.");
		    }
		    
		    Log($"[Numbers Station #{_moduleId}] The final offset is: {offset}");
		    
		    Log($"[Numbers Station #{_moduleId}] After modifying timestamps: {Radio.StickyNote.GeneratedTimes.Select(x => $"[{(int)TimeSpan.FromSeconds(x).TotalMinutes}:{TimeSpan.FromSeconds(x).Seconds:00}]").Join(", ")}");
	    }
	    
	    Log($"[Numbers Station #{_moduleId}] The written sequence on the strip is: {GenerateStrip(Radio.StickyNote.StripNumbers.Join(""))}");
	    Log($"[Numbers Station #{_moduleId}] The channel number in green is: {(Radio.ObtainChannelIndex(true) + 1):00}. The nursery rhyme playing in that channel is: {_songTitles[Radio.ObtainChannel(true).Song]}");
	    Log($"[Numbers Station #{_moduleId}] Since the number of batteries is {(oddCount ? "odd" : "even")}, use the {(oddCount ? "left" : "right")} number, which should be channel {(Radio.ObtainChannelIndex(false) + 1):00}");
	    
	    Log($"[Numbers Station #{_moduleId}] The actual number sequence when tuning in to channel {(Radio.ObtainChannelIndex(false) + 1):00} at one of the scheduled timestamps is: {Radio.ObtainCorrectNumbers().Join("")}");
	    Log($"[Numbers Station #{_moduleId}] After calculating, the final sequence to input is: {_correctSequence.Join("")}");
    }

	private void WritePress()
	{
		if (_moduleSolved || _writePressAnim != null)
			return;
		
		Audio.PlaySoundAtTransform("write press", Write.transform);
		Write.AddInteractionPunch();
		
		_writePressAnim = StartCoroutine(WritePressAnimation());

		bool ready;
		
		Radio.WriteDigit(out ready);

		if (!ready)
			return;

		if (Radio.InputDisplay.GetInput().SequenceEqual(_correctSequence))
		{
			Log($"[Numbers Station #{_moduleId}] The sequence has been entered correctly. Solved!");
			_moduleSolved = true;
			Module.HandlePass();
			Radio.MarkSolve();
		}
		else
		{
			Log($"[Numbers Station #{_moduleId}] Expected {_correctSequence.Join("")}, but entered {Radio.InputDisplay.GetInput().Join("")}, which is incorrect. Strike!");
			
			Module.HandleStrike();
			Radio.InputDisplay.DisplayText.text = string.Empty;
		}

	}

	private IEnumerator WritePressAnimation()
	{
		var elapsed = 0f;
		const float duration = 0.075f;
		
		var newPos = new Vector3(_writeButtonPos.x, -_writeButtonPos.y, _writeButtonPos.z);

		while (elapsed < duration)
		{
			Write.transform.localPosition = new Vector3(_writeButtonPos.x, Easing.InOutQuad(elapsed, _writeButtonPos.y, newPos.y, duration), _writeButtonPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}
		
		Write.transform.localPosition = newPos;

		elapsed = 0;

		while (elapsed < duration)
		{
			Write.transform.localPosition = new Vector3(_writeButtonPos.x, Easing.InOutQuad(elapsed, newPos.y, _writeButtonPos.y, duration), _writeButtonPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}
		
		Write.transform.localPosition = _writeButtonPos;

		_writePressAnim = null;
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} channel # ###:## [turns the channel knob to said channel (timestamp parameter is optional)] || volume 0-4 [turns the volume knob to that setting] || add [writes the current channel to the input display]";
	private bool TwitchZenMode;
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		switch (split[0])
		{
			case "CHANNEL":
				if (split.Length == 1)
				{
					yield return "sendtochaterror Please specify what channel to go to!";
					yield break;
				}

				if (split.Length > 3)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				int channel;

				if (!int.TryParse(split[1], out channel))
				{
					yield return $"{split[1]} is not a valid number!";
					yield break;
				}

				channel--;

				if (!Enumerable.Range(0, 36).Contains(channel))
				{
					yield return "sendtochaterror You must keep it in range of 1-36 inclusive!";
					yield break;
				}

				int? totalSeconds = null;
				
				if (split.Length == 3)
				{
					var time = split[2].Split(':');
					
					int minutes, seconds;

					if (time[1].Length != 2)
					{
						yield return "sendtochaterror The number of seconds is invalid!";
						yield break;
					}

					if (!int.TryParse(time[0], out minutes) || !int.TryParse(time[1], out seconds))
					{
						yield return "sendtochaterror Timestamp is invalid!";
						yield break;
					}

					if (seconds >= 60)
					{
						yield return "sendtochaterror Seconds digits must not go over 60!";
						yield break;
					}

					totalSeconds = minutes * 60 + seconds;

					if (!TwitchZenMode && (int)Bomb.GetTime() < totalSeconds)
					{
						yield return "sendtochaterror Due to the timer being past the timestamp given, the command has been canceled!";
						yield break;
					}
					
					channel--;
				}
				
				var dist = channel - Radio.ObtainIndex(true);

				yield return null;

				if (dist >= -30 && (dist < 0 || dist > 5))
				{
					Radio.Dials[0].OnInteract();
					yield return new WaitUntil(() => channel - Radio.ObtainIndex(true) < -30 || (channel - Radio.ObtainIndex(true) >= 0 && channel - Radio.ObtainIndex(true) <= 5));
					Radio.Dials[0].OnInteractEnded();
					yield return new WaitForSeconds(0.1f);
				}

				while (channel != Radio.ObtainIndex(true))
				{
					Radio.Dials[0].OnInteract();
					Radio.Dials[0].OnInteractEnded();
					yield return new WaitForSeconds(0.05f);
				}

				if (totalSeconds == null)
					yield break;

				yield return $"sendtochat Tuning to channel {split[1]} at {split[2]}";

				var music = (TwitchZenMode ? totalSeconds.Value - (int)Bomb.GetTime() : (int)Bomb.GetTime() - totalSeconds.Value) > 10;

				if (music)
					yield return "waiting music";

				while ((int)Bomb.GetTime() != totalSeconds.Value)
					yield return $"trycancel Tuning to channel {split[1]} at {split[2]} has been canceled!";

				if (music)
					yield return "end waiting music";

				Radio.Dials[0].OnInteract();
				Radio.Dials[0].OnInteractEnded();
				yield return new WaitForSeconds(0.5f);
				
				yield break;
			case "VOLUME":
				if (split.Length == 1)
				{
					yield return "sendtochaterror Specify what volume value to set it in!";
					yield break;
				}

				if (split.Length > 2)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				int volume;

				if (!int.TryParse(split[1], out volume))
				{
					yield return $"sendtochaterror {split[1]} is not a valid number!";
					yield break;
				}

				if (!Enumerable.Range(0, 5).Contains(volume))
				{
					yield return "sendtochaterror You must set the value between 0-4 inclusive!";
					yield break;
				}

				yield return null;

				while (volume != Radio.ObtainIndex(false))
				{
					Radio.Dials[1].OnInteract();
					Radio.Dials[1].OnInteractEnded();
					yield return new WaitForSeconds(0.1f);
				}

				yield break;
			case "ADD":
				if (split.Length > 1)
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				yield return null;

				Write.OnInteract();
				yield return new WaitUntil(() => _writePressAnim == null);
				
				yield break;
			default:
				yield return "sendtochaterror Invalid command!";
				yield break;
		}
    }

	private IEnumerator TwitchHandleForcedSolve()
    {
	    var inputSoFar = Radio.InputDisplay.GetInput();

	    if (!_correctSequence.Take(inputSoFar.Count).SequenceEqual(inputSoFar))
		    Radio.InputDisplay.DisplayText.text = string.Empty;

	    var sequenceStr = _correctSequence.Join("");

	    for (int i = 0; i < 14; i += 2)
	    {
		    var digit = int.Parse(sequenceStr.Substring(i, 2)) - 1;

		    var dist = digit - Radio.ObtainIndex(true);

		    if (dist >= -30 && (dist < 0 || dist > 5))
		    {
			    Radio.Dials[0].OnInteract();
			    yield return new WaitUntil(() => digit - Radio.ObtainIndex(true) < -30 || (digit - Radio.ObtainIndex(true) >= 0 && digit - Radio.ObtainIndex(true) <= 5));
			    Radio.Dials[0].OnInteractEnded();
			    yield return new WaitForSeconds(0.05f);
		    }

		    while (digit != Radio.ObtainIndex(true))
		    {
			    Radio.Dials[0].OnInteract();
			    Radio.Dials[0].OnInteractEnded();
			    yield return new WaitForSeconds(0.05f);
		    }
		    
		    Write.OnInteract();
		    yield return new WaitUntil(() => _writePressAnim == null);
	    }
    }


}