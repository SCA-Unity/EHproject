using UnityEngine;

public class TeleportOnTrigger : MonoBehaviour
{
    [Header("Who can be teleported")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool requireTag = true;

    [Header("Destination")]
    [SerializeField] private Transform destination;
    [SerializeField] private Vector3 destinationPosition;
    [SerializeField] private bool useDestinationTransform = true;

    [Header("Options")]
    [SerializeField] private bool oneTimeUse = false;
    [SerializeField] private bool disableAfterUse = false;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTeleport(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTeleport(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void TryTeleport(Transform hitTransform, Transform rigidbodyTransform)
    {
        if (used && oneTimeUse)
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

        Vector3 finalPosition = useDestinationTransform && destination != null
            ? destination.position
            : destinationPosition;

        target.position = finalPosition;
        used = true;

        if (disableAfterUse)
        {
            gameObject.SetActive(false);
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
        // Child colliders are common: check hit object, rigidbody object, and root object.
        return (hitTransform != null && hitTransform.CompareTag(requiredTag))
            || (rigidbodyTransform != null && rigidbodyTransform.CompareTag(requiredTag))
            || (target != null && target.CompareTag(requiredTag));
    }
}
