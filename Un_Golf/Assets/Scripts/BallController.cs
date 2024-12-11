using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    [SerializeField] Image _bar;
    [SerializeField] Rigidbody _rb;
    [SerializeField] Transform _ball;
    [SerializeField] Transform _targetDirection;
    [SerializeField] float _xRotation;
    [SerializeField] float _power = 0;

    private void Update()
    {
        _xRotation = Input.GetAxis("Mouse X");

        transform.position = _ball.position;
        transform.rotation *= Quaternion.Euler(new Vector3(0, _xRotation, 0));

        //Release button + power
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 rot = _targetDirection.position - _ball.position;
            _rb.AddForce(rot.normalized * (_power * 10), ForceMode.Impulse);
        }
        
        //Push button + increase power
        if (Input.GetMouseButton(0))
        {
            if (_power < 1)
                _power += Time.deltaTime;
            else
                _power = 1;

        }
        else if (_power != 0)
            _power = 0;

        //Ui
        _bar.fillAmount = _power;

        //Velocity stop
        if (_rb.velocity.magnitude < 0.1f)
            _rb.velocity = Vector3.zero;
    }
}
