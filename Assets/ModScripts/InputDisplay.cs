using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputDisplay : MonoBehaviour
{
    public TextMesh DisplayText;

    private void Start() => DisplayText.text = string.Empty;

    public List<int> GetInput() => DisplayText.text.Length != 14 ? null : DisplayText.text.Select(x => x - '0').ToList();
}