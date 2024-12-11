using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 LastPos = Vector3.zero;
    public bool IsArrived = false;
    public bool IsTurn = false;

    [SerializeField] Rigidbody _rb;

    private void Update()
    {
        GameManager.Instance.MovePieces(name, transform.position, transform.rotation.eulerAngles);
    }

    public void Die()
    {
        transform.position = LastPos;
        StopMove();
    }

    public void StopMove()
    {
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}
