using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/Health")]
        public class Health : WorldFloat
        {
                [SerializeField] public float recoveryTime = 0.1f;
                [SerializeField] public float counter = 0;
                [SerializeField] public bool hasShield = false;
                [SerializeField] public bool readImpactOnly = false;
                [SerializeField] public int shieldDirection = 1;
                [SerializeField] public string impactWE;
                [SerializeField] private Character character;
                [SerializeField] private UnityEventEffect onShield = new UnityEventEffect();
                [SerializeField] public UnityEventEffect onImpact = new UnityEventEffect();

                [System.NonSerialized] public int originLayer;
                [System.NonSerialized] public int currentLayer;
                [System.NonSerialized] public bool block = false;

                public static Dictionary<Transform, Health> health = new Dictionary<Transform, Health>();
                public bool cantTakeDamage => transform.gameObject.layer == 2;

                private void Awake ()
                {
                        originLayer = transform.gameObject.layer;
                        currentLayer = originLayer;
                }

                public override void Register ()
                {
                        if (register)
                        {
                                soReference.Register(this);
                        }
                        if (!health.ContainsKey(transform))
                        {
                                health.Add(transform, this);
                        }
                        if (initialized)
                        {
                                Initialize();
                        }
                }

                public override void Reset ()
                {
                        counter = 0;
                        transform.gameObject.layer = originLayer;
                        if (isSaved)
                        {
                                // SetRefreshValue(); might be causing reset issues, need to be careful with this
                        }
                }

                public void StartRecovery ()
                {
                        if (recoveryTime > 0)
                        {
                                counter = 0;
                                CanTakeDamage(false);
                                WorldManager.get.update -= Recover;
                                WorldManager.get.update += Recover;
                        }
                }

                public void Recover (bool gameReset = false)
                {
                        if (gameReset || Clock.Timer(ref counter, recoveryTime))
                        {
                                counter = 0;
                                CanTakeDamage(true);
                                WorldManager.get.update -= Recover;
                        }
                }

                public void SetOriginLayer (int layer)
                {
                        originLayer = layer;
                        currentLayer = layer;
                }

                public bool Recovering ()
                {
                        return recoveryTime > 0 && transform.gameObject.layer == 2;
                }

                public void OnImpact (Transform from, Transform to, float amount, Vector2 direction)
                {
                        if (colliderRef == null)
                        {
                                colliderRef = gameObject.GetComponent<Collider2D>();
                        }
                        ImpactPacket impact = ImpactPacket.impact.Set(impactWE, to, colliderRef, to.transform.position, from, direction, (int) Mathf.Sign(direction.x), amount);
                        onImpact.Invoke(impact);
                }

                public void HideLayer (bool isHiding)
                {
                        int layer = transform.gameObject.layer;
                        if (isHiding && layer != WorldManager.hideLayer)
                        {
                                currentLayer = WorldManager.hideLayer;
                        }
                        else if (!isHiding && layer != originLayer)
                        {
                                currentLayer = originLayer;
                        }
                        if (layer != 2 && layer != currentLayer)
                        {
                                transform.gameObject.layer = currentLayer;
                        }
                }

                public void CanTakeDamage (bool value)
                {
                        transform.gameObject.layer = value ? currentLayer : 2; // 2 = ignore raycast layer
                }

                public void HasShield (bool value)
                {
                        hasShield = value;
                }

                public bool ShieldActive (Transform from, Vector2 direction)
                {
                        if (character == null)
                        {
                                return false;
                        }
                        if (shieldDirection > 0)
                        {
                                if ((character.signals.characterDirection > 0 && direction.x < 0) || (character.signals.characterDirection < 0 && direction.x > 0))
                                {
                                        onShield.Invoke(BasicImpact(from, direction));
                                        return true;
                                }
                        }
                        if (shieldDirection < 0)
                        {
                                if ((character.signals.characterDirection > 0 && direction.x > 0) || (character.signals.characterDirection < 0 && direction.x < 0))
                                {
                                        onShield.Invoke(BasicImpact(from, direction));
                                        return true;
                                }
                        }
                        return false;
                }

                public static Health Get (Transform transform)
                {
                        if (transform == null)
                        {
                                return null;
                        }
                        if (health.TryGetValue(transform, out Health found) && found != null)
                        {
                                return found;
                        }
                        return transform.GetComponentInParent<Health>();
                }

                public static bool HitContactResults (Transform aggressor, List<Collider2D> contactResults, int hits, float damage, float damageForce, Vector2 origin)
                {
                        bool hit = false;
                        for (int i = 0; i < hits; i++)
                        {
                                if (contactResults[i] != null)
                                {
                                        Vector2 targetPosition = contactResults[i].transform.position;
                                        if (IncrementHealth(aggressor, contactResults[i].transform, damage, (targetPosition - origin).normalized * damageForce))
                                        {
                                                hit = true;
                                        }
                                }
                        }
                        return hit;
                }

                public static bool IncrementHealth (Transform from, Transform to, float amount, Vector2 direction)
                {
                        Health healthKey = Get(to);
                        if (healthKey == null)
                        {
                                return false;
                        }

                        if (healthKey.character == null)
                        {
                                healthKey.character = healthKey.gameObject.GetComponent<Character>();
                        }
                        if (healthKey.character != null)
                        {
                                healthKey.characterDirectionX = healthKey.character.signals.characterDirection;
                        }
                        if (healthKey.readImpactOnly)
                        {
                                healthKey.OnImpact(from, to, amount, direction);
                                return true; // return as a success
                        }
                        if (healthKey.Recovering() && Mathf.Abs(amount) < healthKey.maxValue) // bypass recovery with instant kill
                        {
                                return false;
                        }
                        if (healthKey.hasShield && healthKey.ShieldActive(from, direction))
                        {
                                return false;
                        }
                        if (amount < 0)
                        {
                                healthKey.StartRecovery(); // damage is negative
                        }
                        return healthKey.IncrementValue(from, amount, direction) && !healthKey.block;
                }

                public static bool IsDamageable (Transform transform)
                {
                        Health healthKey = Get(transform);
                        return healthKey != null && !healthKey.readImpactOnly;
                }
        }
}
