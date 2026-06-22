using UnityEngine;
using TwoBitMachines.Safire2DCamera;

public class CombatCameraLock : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Safire2DCamera safireCamera;
    [SerializeField] private Behaviour[] followScriptsToDisable;
    [SerializeField] private Transform lockPoint;
    [SerializeField] private Vector3 lockPosition;
    [SerializeField] private bool useLockPoint = true;
    [SerializeField] private bool smoothTransition = false;
    [SerializeField] private float transitionSpeed = 8f;
    [SerializeField] private bool restoreTimeScaleOnEnter = true;

    [Header("Trigger")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool requireTag = true;
    [SerializeField] private bool unlockOnExit = true;

    private CameraController cameraController;
    private bool inCombat;
    private bool transitioning;
    private Vector3 targetPosition;

    private void Awake()
    {
        ResolveCameraReferences();
    }

    private void ResolveCameraReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null && cameraController == null)
        {
            cameraController = targetCamera.GetComponent<CameraController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTarget(other))
        {
            return;
        }

        EnterCombat();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidTarget(other))
        {
            return;
        }

        EnterCombat();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!unlockOnExit || !IsValidTarget(other))
        {
            return;
        }

        ExitCombat();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!unlockOnExit || !IsValidTarget(other))
        {
            return;
        }

        ExitCombat();
    }

    public void EnterCombat()
    {
        ResolveCameraReferences();

        if (targetCamera == null || inCombat)
        {
            return;
        }

        inCombat = true;
        targetPosition = ResolveLockPosition();

        if (restoreTimeScaleOnEnter && Time.timeScale <= 0f)
        {
            Time.timeScale = 1f;
        }

        if (safireCamera != null)
        {
            safireCamera.timeManager.Reset();
            safireCamera.PauseFollowMechanics(true);
            safireCamera.ModuleDisable("Cinematics");
        }

        SetFollowEnabled(false);

        if (smoothTransition)
        {
            transitioning = true;
            return;
        }

        SetCameraPosition(targetPosition);
    }

    public void ExitCombat()
    {
        if (targetCamera == null || !inCombat)
        {
            return;
        }

        inCombat = false;
        transitioning = false;
        SetFollowEnabled(true);

        if (safireCamera != null)
        {
            safireCamera.PauseFollowMechanics(false);
            safireCamera.ModuleEnable("Cinematics");
            safireCamera.ForceFollowSmooth();
        }
    }

    private void LateUpdate()
    {
        if (!inCombat || targetCamera == null)
        {
            return;
        }

        if (transitioning)
        {
            Vector3 current = targetCamera.transform.position;
            Vector3 next = Vector3.Lerp(current, targetPosition, transitionSpeed * Time.deltaTime);
            SetCameraPosition(next);

            if (Vector3.Distance(next, targetPosition) <= 0.05f)
            {
                SetCameraPosition(targetPosition);
                transitioning = false;
            }

            return;
        }

        SetCameraPosition(targetPosition);
    }

    private void SetCameraPosition(Vector3 position)
    {
        if (safireCamera != null)
        {
            safireCamera.SetCameraPosition(position);
            return;
        }

        targetCamera.transform.position = position;
    }

    private void SetFollowEnabled(bool enabled)
    {
        if (cameraController != null)
        {
            cameraController.enabled = enabled;
        }

        if (followScriptsToDisable == null)
        {
            return;
        }

        for (int i = 0; i < followScriptsToDisable.Length; i++)
        {
            Behaviour behaviour = followScriptsToDisable[i];
            if (behaviour == null || behaviour is Safire2DCamera)
            {
                continue;
            }

            behaviour.enabled = enabled;
        }
    }

    private Vector3 ResolveLockPosition()
    {
        Vector3 position = useLockPoint && lockPoint != null ? lockPoint.position : lockPosition;

        if (targetCamera != null)
        {
            position.z = targetCamera.transform.position.z;
        }

        return position;
    }

    private bool IsValidTarget(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        return !requireTag || other.CompareTag(requiredTag);
    }

    private bool IsValidTarget(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        return !requireTag || other.CompareTag(requiredTag);
    }
}
