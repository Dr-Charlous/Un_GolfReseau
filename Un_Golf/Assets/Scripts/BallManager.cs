using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    private void Update()
    {
        GameManager.Instance.MovePieces(name, transform.position, transform.rotation.eulerAngles);
    }
}
