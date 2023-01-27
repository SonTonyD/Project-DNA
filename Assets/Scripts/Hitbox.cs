using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private BoxCollider _boxCollider;
    private MeshRenderer _meshRenderer;

    private bool _isRecovering;
    private bool _isAttacking;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _meshRenderer = GetComponent<MeshRenderer>(); 
    }

    private void Start()
    {
        SetReset();
        _isRecovering = false;
    }

    public void LaunchAttack(float startupFrame, float activeFrame, float recoveryFrame)
    {
        if (_isRecovering == false && _isAttacking == false)
        {
            _isAttacking = true;
            Invoke(nameof(SetActive), startupFrame);
            Invoke(nameof(SetReset), activeFrame + startupFrame);
            Invoke(nameof(StartRecovering), activeFrame + startupFrame);
            Invoke(nameof(EndRecovering), activeFrame + startupFrame + recoveryFrame);
        }
    }

    private void SetActive()
    {
        _boxCollider.enabled = true;
        _meshRenderer.enabled = true;
    }

    private void SetReset()
    {
        _boxCollider.enabled = false;
        _meshRenderer.enabled = false;
    }

    private void StartRecovering()
    {
        _isRecovering = true;
    }
    private void EndRecovering()
    {
        _isRecovering = false;
        _isAttacking = false;
    }
}
