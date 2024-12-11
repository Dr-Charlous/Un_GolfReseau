using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private void Update()
    {
        GameManager.Instance.MovePieces(name, transform.position, transform.rotation.eulerAngles);
    }
}
