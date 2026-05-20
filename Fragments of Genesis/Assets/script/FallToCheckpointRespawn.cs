using UnityEngine;
using TwoBitMachines.FlareEngine;
using TBMPlayer = TwoBitMachines.FlareEngine.ThePlayer.Player;

public class FallToCheckpointRespawn : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float respawnY = -15f;
    [SerializeField] private float cooldownSeconds = 0.35f;
    [Header("Target")]
    [SerializeField] private Transform targetOverride;
    private float nextAllowedTime;
    private void Update()
    {
        Transform target = ResolveTarget();
        if (target == null)
        {
            return;
        }
        if (Time.time < nextAllowedTime)
        {
            return;
        }
        if (target.position.y <= respawnY)
        {
            RequestCheckpointRespawn();
            nextAllowedTime = Time.time + cooldownSeconds;
        }
    }

    private Transform ResolveTarget()
    {
        if (targetOverride != null)
        {
            return targetOverride;
        }
         return TBMPlayer.mainPlayer != null ? TBMPlayer.PlayerTransform() : null;
    }
    private static void RequestCheckpointRespawn()
    {
        if (WorldManager.get == null)
        {
            return;
        }
        // FlareEngine reset flow includes CheckPoint.ResetPlayerAll().
        WorldManager.get.ResetAll();
    }
}
