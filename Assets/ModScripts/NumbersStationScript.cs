using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
	
	private NumberDecryptor _decryptor;

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
		[ThisOldMan] = "This Old Man",
		[ThreeBlindMice] = "Three Blind Mice",
		[TwinkleTwinkle] = "Twinkle, Twinkle",
		[WheelsOnTheBus] = "Wheels on the Bus"
	};

	private static string GenerateStrip(string digits) => $"{digits.Substring(0, 5)} {digits.Substring(4, 5)} {digits.Substring(9, 4)}";

	private void Awake()
    {
		_moduleId = _moduleIdCounter++;
		
		Write.OnInteract += () => { WritePress(); return false; };
    }

	
	private void Start()
    {
	    Radio.Setup(Audio, Bomb, CalebVoices, GhostSaltVoices, GoodHoodVoices, SeanVoices);
	    _writeButtonPos = Write.transform.localPosition;

	    Strip.text = GenerateStrip(Radio.StickyNote.StripNumbers.Join(""));

	    _decryptor = new NumberDecryptor(Radio.ObtainCorrectNumbers(), Radio.ObtainStripNumbers());

	    _correctSequence = _decryptor.DecryptNumbers();
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
			_moduleSolved = true;
			Module.HandlePass();
			Radio.MarkSolve();
		}
		else
		{
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
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	private IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}