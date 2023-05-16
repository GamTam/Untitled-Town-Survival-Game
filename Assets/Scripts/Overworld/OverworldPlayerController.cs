using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashLength;
    [SerializeField] private Rigidbody2D _char;
    
    private PlayerInput _playerInput;
    
    private float _dashTimer;

    private bool _hasDashed => _playerInput.actions["Overworld/Dash"].triggered;
    [HideInInspector] public bool _interacting => _playerInput.actions["Overworld/Interact"].triggered;
    private Vector2 _moveVector => _playerInput.actions["Overworld/MoveVector"].ReadValue<Vector2>();
    private Vector2 _prevMoveVector;

    private void Start()
    {
        Globals.Player = this;
        _playerInput = GameObject.FindWithTag("Controller Manager").GetComponent<PlayerInput>();
    }

    private void Update()
    {
        _char.velocity = _moveVector * _moveSpeed;
        if (_moveVector != Vector2.zero && _dashTimer <= 0) _prevMoveVector = _moveVector;
        
        if (_hasDashed)
        {
            _dashTimer = _dashLength;
        }

        if (_dashTimer > 0)
        {
            _char.velocity = _prevMoveVector * _dashSpeed;
            _dashTimer -= Time.deltaTime;
        } 
    }
}
