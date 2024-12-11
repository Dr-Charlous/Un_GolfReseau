using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ClearChat : MonoBehaviour
{
    [HideInInspector]
    public float LastUpdate = 0;

    TextMeshProUGUI _textComponent;
    float _elapsedTime = 0;

    void Start()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
        _textComponent.text = "";
        LastUpdate = Time.time;
    }

    void Update()
    {
        _elapsedTime = Time.time - LastUpdate;
        if (_elapsedTime > 3)
        {
            _textComponent.text = "";
        }
    }
}