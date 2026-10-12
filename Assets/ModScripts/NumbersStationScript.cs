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

	public AudioClip[] CalebVoices, GhostSaltVoices, GoodHoodVoices, SeanVoices;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved;

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

	private void Awake()
    {

		_moduleId = _moduleIdCounter++;

    }

	
	private void Start()
    {
	    Radio.Setup(Audio, Bomb, new[] { CalebVoices, GhostSaltVoices, GoodHoodVoices, SeanVoices });
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