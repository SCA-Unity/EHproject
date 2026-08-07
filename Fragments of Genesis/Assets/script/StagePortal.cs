using UnityEngine;
using UnityEngine.SceneManagement;
using TwoBitMachines.FlareEngine;

public class StagePortal : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetSceneName = "2stage";

    [Header("Who can use the portal")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool requireTag = true;

    [Header("Options")]
    [SerializeField] private float loadDelaySeconds = 0f;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private ManageScenes manageScenes;

    private bool used;
    private bool loading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryLoadScene(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryLoadScene(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void TryLoadScene(Transform hitTransform, Transform rigidbodyTransform)
    {
        if (loading || (used && oneTimeUse))
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

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"{nameof(StagePortal)} on {name} has no target scene name.", this);
            return;
        }

        used = true;
        loading = true;

        if (loadDelaySeconds > 0f)
        {
            Invoke(nameof(LoadTargetScene), loadDelaySeconds);
        }
        else
        {
            LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        if (manageScenes != null)
        {
            manageScenes.LoadScene(targetSceneName);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
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
