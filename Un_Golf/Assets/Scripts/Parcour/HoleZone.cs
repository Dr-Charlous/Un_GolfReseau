using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<Ball>();

        if (ball != null)
        {
            ball.StopMove();
            ball.IsArrived = true;
            ball.gameObject.SetActive(false);
            GameManager.Instance.IsArrived(ball.name, true);
            GameManager.Instance.CheckEndLevel();
        }
    }
}
