using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] Material[] _materials;
    [SerializeField] Image _canHit;
    [SerializeField] TextMeshProUGUI _textTurn;

    public void ChangeHitUi(bool value)
    {
        if (value)
            _canHit.material = _materials[0];
        else
            _canHit.material = _materials[1];
    }

    public void ChangeTextUi(string name, int value)
    {
        _textTurn.text = $"{name} : {value} hits";
    }
}
