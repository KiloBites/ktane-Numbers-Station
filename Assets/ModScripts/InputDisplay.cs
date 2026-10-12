using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputDisplay : MonoBehaviour
{
    public TextMesh DisplayText;

    private void Start()
    {
        DisplayText.text = string.Empty;
        StartCoroutine(InputTextPulse());
    }

    public List<int> GetInput() => DisplayText.text.Select(x => x - '0').ToList();

    private IEnumerator InputTextPulse()
    {
        while (true)
        {
            yield return null;
            DisplayText.color = new Color(DisplayText.color.r, DisplayText.color.g, DisplayText.color.b, Easing.InOutSine(Mathf.PingPong(Time.time, 1.75f), 1f, 0.75f, 1.75f));
        }
    }
}