using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera[] _allVirtualCameras;

    [Header("LERP")]
    [SerializeField] private float _fallPanAmount = 0f;
    [SerializeField] private float _fallPanTime = 0.25f;
    public float _fallSpeedDampingChangeThreshold = -15f;

    private Coroutine _lerpYPanCorountine;

    private CinemachineCamera _currentCamera;
    private CinemachinePositionComposer _transposer;

    public bool IsLerpingYDamping { get; private set; }

    public bool LerpedFromPlayerFalling { get; set; }

    private float _normYPanAmount;


    [SerializeField] private float mouseLookStrength = 0.5f;
    private Vector2 mouseLookOffset;

    private void LateUpdate()
{
    if (_transposer == null) return;

    // Apply mouse offset
    _transposer.TargetOffset = new Vector3(
        mouseLookOffset.x,
        mouseLookOffset.y,
        0
    );
}



    private void Awake()
    {
        if (instance == null)
            instance = this;


        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled)
            {
                _currentCamera = _allVirtualCameras[i];

                _transposer = _currentCamera.GetComponent<CinemachinePositionComposer>();


            }

        }

        _normYPanAmount = _transposer.Damping.y;
    }

    #region Lerp the Y Damping

    public void LerpYDamping(bool isPlayerFalling)
    {
        _lerpYPanCorountine = StartCoroutine(LerpYAction(isPlayerFalling));
    }


    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        float startDampAmount = _transposer.Damping.y;
        Debug.Log("damping on Y " + startDampAmount);
        float endDampAmount = 0f;

        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }

        else
        {
            endDampAmount = _normYPanAmount;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _fallPanTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, ((elapsedTime / _fallPanTime)));
            _transposer.Damping.y = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }

    #endregion


    #region Mouse Look

    public void OnLookInput(InputAction.CallbackContext context)
    {
        //TODO
    }


    #endregion


}
