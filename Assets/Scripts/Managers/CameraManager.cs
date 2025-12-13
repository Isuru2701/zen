using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [Header("LERP")]
    [SerializeField] private float _fallPanAmount = 0f;
    [SerializeField] private float _fallPanTime = 0.25f;
    public float _fallSpeedDampingChangeThreshold = -15f;

    private Coroutine _lerpYPanCorountine;

   [SerializeField]private CinemachineCamera CurrentCamera;
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



        _transposer = CurrentCamera.GetComponent<CinemachinePositionComposer>();



        _normYPanAmount = _transposer.Damping.y;
    }


    public void SetCamera(CinemachineCamera newCam)
    {
        CurrentCamera = newCam;
        _transposer = CurrentCamera.GetComponent<CinemachinePositionComposer>();
    }


    public string GetCurrentCamera()
    {
        return CurrentCamera.name;
    }

    #region Lerp the Y Damping

    // public void LerpYDamping(bool isPlayerFalling)
    // {
    //     _lerpYPanCorountine = StartCoroutine(LerpYAction(isPlayerFalling));
    // }


    // private IEnumerator LerpYAction(bool isPlayerFalling)
    // {
    //     IsLerpingYDamping = true;

    //     float startDampAmount = _transposer.Damping.y;
    //     Debug.Log("damping on Y " + startDampAmount);
    //     float endDampAmount = 0f;

    //     if (isPlayerFalling)
    //     {
    //         endDampAmount = _fallPanAmount;
    //         LerpedFromPlayerFalling = true;
    //     }

    //     else
    //     {
    //         endDampAmount = _normYPanAmount;
    //     }

    //     float elapsedTime = 0f;
    //     while (elapsedTime < _fallPanTime)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, ((elapsedTime / _fallPanTime)));
    //         _transposer.Damping.y = lerpedPanAmount;

    //         yield return null;
    //     }

    //     IsLerpingYDamping = false;
    // }

    #endregion


    #region Mouse Look

    public void OnLookInput(InputAction.CallbackContext context)
    {
        // Expect a screen-space pointer position (e.g. `Mouse/position` or `Pointer` binding).
        // When the pointer is close to the screen edge, produce a limited offset in that direction.
        Vector2 pointer = context.ReadValue<Vector2>();

        // If we receive a zero vector (some bindings provide deltas), bail out early.
        // We handle delta-style input by ignoring zero here; if needed, adapt bindings to provide absolute position.
        if (pointer == Vector2.zero)
        {
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        // dir ranges approximately -1..1 across each axis
        Vector2 dir = (pointer - screenCenter) / screenCenter;

        // When pointer is near edges only: edgeThreshold is the normalized distance from center
        // before we start panning (0..1). 0.85 means the outer 15% of the screen triggers panning.
        const float edgeThreshold = 0.85f;

        Vector2 absDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        Vector2 trigger = Vector2.zero;

        trigger.x = Mathf.Max(0f, (absDir.x - edgeThreshold) / (1f - edgeThreshold));
        trigger.y = Mathf.Max(0f, (absDir.y - edgeThreshold) / (1f - edgeThreshold));

        // Restore sign so we know direction to pan
        trigger.x *= Mathf.Sign(dir.x);
        trigger.y *= Mathf.Sign(dir.y);

        // Target offset limited by mouseLookStrength
        Vector2 targetOffset = trigger * mouseLookStrength;

        // Smooth the applied offset for less jitter
        float smoothSpeed = 10f;
        mouseLookOffset = Vector2.Lerp(mouseLookOffset, targetOffset, Mathf.Clamp01(smoothSpeed * Time.deltaTime));

        // Clamp to maximum strength
        if (mouseLookOffset.magnitude > mouseLookStrength)
            mouseLookOffset = mouseLookOffset.normalized * mouseLookStrength;
    }


    #endregion


}
