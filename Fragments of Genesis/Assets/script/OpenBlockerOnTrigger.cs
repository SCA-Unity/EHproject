using UnityEngine;
using UnityEngine.Events;

public class OpenBlockerOnTrigger : MonoBehaviour
{
    public enum OpenMode
    {
        DisableGameObject,
        DisableCollidersOnly
    }

    [Header("Who can open")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool requireTag = true;

    [Header("Blocked objects")]
    [SerializeField] private GameObject[] blockers;
    [SerializeField] private OpenMode openMode = OpenMode.DisableGameObject;

    [Header("Options")]
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool disableTriggerAfterUse = false;
    [SerializeField] private float openDelaySeconds = 0f;
    [SerializeField] private UnityEvent onOpened;

    private bool used;
    private bool opening;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryOpen(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryOpen(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void TryOpen(Transform hitTransform, Transform rigidbodyTransform)
    {
        if ((used && oneTimeUse) || opening)
        {
            return;
        }

        if (hitTransform == null)
        {
            return;
        }

        Transform target = ResolveTargetTransform(hitTransform, rigidbodyTransform);
        if (target == null)
        {
            return;
        }

        if (requireTag && !HasRequiredTag(hitTransform, rigidbodyTransform, target))
        {
            return;
        }

        used = true;
        opening = true;

        if (openDelaySeconds > 0f)
        {
            Invoke(nameof(OpenBlockers), openDelaySeconds);
        }
        else
        {
            OpenBlockers();
        }
    }

    private void OpenBlockers()
    {
        if (blockers != null)
        {
            for (int i = 0; i < blockers.Length; i++)
            {
                OpenBlocker(blockers[i]);
            }
        }

        onOpened?.Invoke();
        opening = false;

        if (disableTriggerAfterUse)
        {
            gameObject.SetActive(false);
        }
    }

    private void OpenBlocker(GameObject blocker)
    {
        if (blocker == null)
        {
            return;
        }

        if (openMode == OpenMode.DisableGameObject)
        {
            blocker.SetActive(false);
            return;
        }

        Collider2D[] colliders2D = blocker.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders2D.Length; i++)
        {
            colliders2D[i].enabled = false;
        }

        Collider[] colliders3D = blocker.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders3D.Length; i++)
        {
            colliders3D[i].enabled = false;
        }
    }

    private Transform ResolveTargetTransform(Transform hitTransform, Transform rigidbodyTransform)
    {
        if (rigidbodyTransform != null)
        {
            return rigidbodyTransform;
        }

        return hitTransform.root != null ? hitTransform.root : hitTransform;
    }

    private bool HasRequiredTag(Transform hitTransform, Transform rigidbodyTransform, Transform target)
    {
        return (hitTransform != null && hitTransform.CompareTag(requiredTag))
            || (rigidbodyTransform != null && rigidbodyTransform.CompareTag(requiredTag))
            || (target != null && target.CompareTag(requiredTag));
    }
}
