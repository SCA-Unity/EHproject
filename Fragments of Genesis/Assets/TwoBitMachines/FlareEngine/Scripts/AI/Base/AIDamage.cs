using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class AIDamage : MonoBehaviour
        {
                [System.NonSerialized] private AIBase aiBase;
                public static float difficulty = 1f;
                public void Awake()
                {
                        aiBase = gameObject.GetComponent<AIBase>();
                }
                public void OnTriggerEnter2D(Collider2D other)
                {
                        TryDamage(other);
                }

                public void OnTriggerStay2D(Collider2D other)
                {
                        TryDamage(other);
                }

                private void TryDamage(Collider2D other)
                {
                        if (aiBase == null || other == null || aiBase.damage.pauseDamage || aiBase.damage.pauseDamageTimer)
                        {
                                return;
                        }
                        if (!Compute.ContainsLayer(aiBase.damage.layer, other.gameObject.layer))
                        {
                                return;
                        }
                        if (!Health.IsDamageable(other.transform))
                        {
                                return;
                        }
                        Health.IncrementHealth(transform, other.transform, -aiBase.damage.damage * difficulty, Damage.Direction(transform, other.transform, aiBase.damage.direction) * aiBase.damage.force);
                }
        }

        [System.Serializable]
        public class AIDamagePack
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public AttackDirection direction;
                [SerializeField] public bool canDealDamage;
                [SerializeField] public float pauseTimer;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float force = 1f;
                [SerializeField] public float counter = 0;
                [SerializeField] public bool damageFoldOut;
                [SerializeField] public bool pauseDamage = false;
                [SerializeField] public bool pauseDamageTimer = false;

                public void PauseTimer()
                {
                        if (!pauseDamageTimer) return;

                        if (TwoBitMachines.Clock.Timer(ref counter, pauseTimer))
                        {
                                pauseDamageTimer = false;
                        }
                }
        }
}
