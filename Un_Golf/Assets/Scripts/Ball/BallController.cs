using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    [SerializeField] Image _bar;
    [SerializeField] Rigidbody _rb;
    [SerializeField] Ball _ball;
    [SerializeField] Transform _targetDirection;
    [SerializeField] int _hits = 0;
    [SerializeField] float _xRotation;
    [SerializeField] float _power = 10;
    [SerializeField] float _powerInput = 0;

    private void Update()
    {
        if (!_ball.IsArrived && _ball.IsTurn)
        {
            _xRotation = Input.GetAxis("Mouse X");

            transform.position = _ball.transform.position;
            transform.rotation *= Quaternion.Euler(new Vector3(0, _xRotation, 0));

            //Ui turn
            if (_ball.IsTurn)
            {
                _targetDirection.gameObject.SetActive(true);
                GameManager.Instance.Ui.ChangeHitUi(true);
            }

            //Release button + power
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 rot = _targetDirection.position - _ball.transform.position;
                _rb.AddForce(rot.normalized * (_powerInput * _power), ForceMode.Impulse);
                _hits++;
                GameManager.Instance.Ui.ChangeTextUi(_ball.name, _hits);
                GameManager.Instance.Ui.ChangeHitUi(false);
                GameManager.Instance.ChangeTurn();
                _targetDirection.gameObject.SetActive(false);
                _ball.IsTurn = false;
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
        else if (_ball.IsArrived && _ball.IsTurn)
            GameManager.Instance.ChangeTurn();
    }

    private void FixedUpdate()
    {
        //Velocity stop
        if (_rb.velocity.magnitude < 0.1f)
        {
            _ball.StopMove();
        }
    }
}
