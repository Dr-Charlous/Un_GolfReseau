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
    [SerializeField] float _power = 10;
    [SerializeField] float _powerInput = 0;

    Ball _ballComponent;

    private void Start()
    {
        _ballComponent = _ball.GetComponent<Ball>();
    }

    private void Update()
    {
        if (!_ballComponent.IsArrived)
        {
            _xRotation = Input.GetAxis("Mouse X");

            transform.position = _ball.position;
            transform.rotation *= Quaternion.Euler(new Vector3(0, _xRotation, 0));

            //Release button + power
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 rot = _targetDirection.position - _ball.position;
                _rb.AddForce(rot.normalized * (_powerInput * _power), ForceMode.Impulse);
            }

            //Push button + increase power
            if (Input.GetMouseButton(0))
            {
                _ball.GetComponent<Ball>().LastPos = transform.position;

                if (_powerInput < 1)
                    _powerInput += Time.deltaTime / 2;
                else
                    _powerInput = 1;

            }
            else if (_powerInput != 0)
                _powerInput = 0;

            //Ui
            _bar.fillAmount = _powerInput;
        }
    }

    private void FixedUpdate()
    {
        //Velocity stop
        if (_rb.velocity.magnitude < 0.1f)
            _rb.velocity = Vector3.zero;
    }
}
