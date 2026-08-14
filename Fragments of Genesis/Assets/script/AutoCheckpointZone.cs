using UnityEngine;
using UnityEngine.Events;
using TwoBitMachines;
using TwoBitMachines.FlareEngine;

public class AutoCheckpointZone : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private CheckPoint checkPoint;
    [SerializeField] private int checkpointIndex;
    [SerializeField] private Transform respawnPoint;

    [Header("Who triggers save")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool requireTag = true;

    [Header("Options")]
    [SerializeField] private bool oneTimePerEntry = true;
    [SerializeField] private bool respectCheckpointPriority = false;
    [SerializeField] private bool logWhenSaved = true;
    [SerializeField] private UnityEvent onCheckpointSaved;

    private bool savedThisSession;

    private void Awake()
    {
        if (checkPoint == null)
        {
            checkPoint = FindObjectOfType<CheckPoint>();
        }
    }

    private void Start()
    {
        SyncRespawnPoint();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrySaveCheckpoint(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySaveCheckpoint(other.transform, other.attachedRigidbody != null ? other.attachedRigidbody.transform : null);
    }

    private void TrySaveCheckpoint(Transform hitTransform, Transform rigidbodyTransform)
    {
        if (savedThisSession && oneTimePerEntry)
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

        if (checkPoint == null)
        {
            Debug.LogWarning($"{nameof(AutoCheckpointZone)} on {name} needs a {nameof(CheckPoint)} in the scene.", this);
            return;
        }

        if (WorldManager.saveFolder == null)
        {
            Debug.LogWarning($"{nameof(AutoCheckpointZone)} on {name} needs {nameof(WorldManager)} to be active.", this);
            return;
        }

        CheckPoint.Checks entry = FindCheckpointEntry();
        if (entry == null)
        {
            Debug.LogWarning(
                $"{nameof(AutoCheckpointZone)} on {name} could not find checkpoint index {checkpointIndex} on {checkPoint.name}.",
                this);
            return;
        }

        if (respectCheckpointPriority
            && checkPoint.type == CheckPoint.CheckPointType.Priority
            && checkpointIndex < GetCurrentIndex())
        {
            if (logWhenSaved)
            {
                Debug.Log(
                    $"{nameof(AutoCheckpointZone)} on {name} skipped save: index {checkpointIndex} is lower than saved index {GetCurrentIndex()}.",
                    this);
            }

            return;
        }

        entry.Save(checkPoint);
        savedThisSession = true;
        onCheckpointSaved?.Invoke();

        if (logWhenSaved)
        {
            Debug.Log($"{nameof(AutoCheckpointZone)} on {name} saved checkpoint index {checkpointIndex}.", this);
        }
    }

    private void SyncRespawnPoint()
    {
        if (checkPoint == null || respawnPoint == null)
        {
            return;
        }

        CheckPoint.Checks entry = FindCheckpointEntry();
        if (entry == null)
        {
            return;
        }

        entry.bounds.position = (Vector2)respawnPoint.position - Vector2.right * entry.bounds.size.x * 0.5f;
        entry.bounds.Initialize();
    }

    private CheckPoint.Checks FindCheckpointEntry()
    {
        for (int i = 0; i < checkPoint.checkPoints.Count; i++)
        {
            if (checkPoint.checkPoints[i].index == checkpointIndex)
            {
                return checkPoint.checkPoints[i];
            }
        }

        return null;
    }

    private int GetCurrentIndex()
    {
        SaveFloat saveFloat = new SaveFloat
        {
            value = checkPoint.hasDefault ? checkPoint.defaultIndex : -1f
        };
        return (int)Storage.Load<SaveFloat>(saveFloat, WorldManager.saveFolder, checkPoint.checkPointName).value;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (checkPoint == null || checkPoint.checkPoints == null)
        {
            return;
        }

        bool found = false;
        for (int i = 0; i < checkPoint.checkPoints.Count; i++)
        {
            if (checkPoint.checkPoints[i].index == checkpointIndex)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning(
                $"{nameof(AutoCheckpointZone)} on {name}: CheckPoint has no entry with Index = {checkpointIndex}.",
                this);
        }
    }
#endif

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
