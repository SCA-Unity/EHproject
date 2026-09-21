#region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class MeleeAttack : Action
        {
                [SerializeField] public Collider2D colliderRef;
                [SerializeField] public LayerMask layer;
                [SerializeField] public string animationSignal;
                [SerializeField] public float damage = 1f;
                [SerializeField] public MeleeCollider enableCollider;
                [SerializeField] public Vector2 forceDirection = Vector2.right;
                [SerializeField] public Vector2 velocity;

                [System.NonSerialized] private ContactFilter2D filter = new ContactFilter2D();
                [System.NonSerialized] private List<Collider2D> list = new List<Collider2D>();
                [System.NonSerialized] private List<Transform> targetList = new List<Transform>();
                [System.NonSerialized] private bool success = false;
                [System.NonSerialized] private Animator fallbackAnimator;
                [System.NonSerialized] private bool fallbackAnimationStarted;
                [System.NonSerialized] private int fallbackAnimationHash;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (colliderRef == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                success = false;
                                fallbackAnimationStarted = false;
                                fallbackAnimationHash = Animator.StringToHash(animationSignal);
                                filter.useLayerMask = true;
                                filter.useTriggers = true;
                                filter.layerMask = layer;
                                targetList.Clear();
                                if (enableCollider == MeleeCollider.EnableOnStart)
                                {
                                        colliderRef.enabled = true;
                                }
                                if (velocity.y != 0)
                                {
                                        root.velocity.y = velocity.y;
                                        root.hasJumped = true;
                                }

                                // AnimationSignals only drive a Flare SpriteEngine. Some AI
                                // prefabs (such as Dark Knight) use a regular child Animator
                                // instead, so forward the configured signal to its trigger.
                                if (root.signals.spriteEngine == null)
                                {
                                        fallbackAnimator = GetComponentInChildren<Animator>();
                                        if (HasTrigger(fallbackAnimator, animationSignal))
                                        {
                                                fallbackAnimator.SetTrigger(fallbackAnimationHash);
                                        }
                                        else
                                        {
                                                fallbackAnimator = null;
                                        }
                                }
                        }
                        if (velocity.x != 0)
                        {
                                root.velocity.x = velocity.x * Mathf.Sign(root.direction);
                        }
                        root.signals.Set("meleeCombo", true);
                        root.signals.Set(animationSignal, true);

                        UpdateFallbackAnimator();

                        int size = colliderRef.OverlapCollider(filter, list);
                        for (int i = 0; i < size; i++)
                        {
                                if (list[i].transform == this.transform)
                                        continue;
                                float direction = colliderRef.transform.position.x < list[i].transform.position.x ? 1f : -1f;
                                Vector2 newForceDirection = new Vector2(forceDirection.x * direction, forceDirection.y);
                                Transform target = GetHealthTarget(list[i]);
                                if (target != null && target != this.transform && !targetList.Contains(target))
                                {
                                        if (Health.IncrementHealth(transform, target, -damage, newForceDirection))
                                        {
                                                targetList.Add(target);
                                        }
                                }
                        }

                        FlipCollider(root.direction, colliderRef.transform);
                        return success ? NodeState.Success : NodeState.Running;
                }

                private void UpdateFallbackAnimator ()
                {
                        if (fallbackAnimator == null || success)
                        {
                                return;
                        }

                        AnimatorStateInfo state = fallbackAnimator.GetCurrentAnimatorStateInfo(0);
                        bool isAttackState = state.shortNameHash == fallbackAnimationHash || state.fullPathHash == fallbackAnimationHash;
                        if (isAttackState)
                        {
                                fallbackAnimationStarted = true;
                        }
                        else if (fallbackAnimationStarted && !fallbackAnimator.IsInTransition(0))
                        {
                                CompleteAttack();
                        }
                }

                private static bool HasTrigger (Animator animator, string parameterName)
                {
                        if (animator == null || string.IsNullOrEmpty(parameterName))
                        {
                                return false;
                        }

                        foreach (AnimatorControllerParameter parameter in animator.parameters)
                        {
                                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == parameterName)
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                private Transform GetHealthTarget (Collider2D collider)
                {
                        if (collider == null)
                        {
                                return null;
                        }
                        if (Health.IsDamageable(collider.transform))
                        {
                                return collider.transform;
                        }
                        Health parentHealth = collider.GetComponentInParent<Health>();
                        return parentHealth != null ? parentHealth.transform : null;
                }

                public void FlipCollider (float direction, Transform transform)
                {
                        transform.localPosition = Util.FlipXSign(transform.localPosition, direction); // change weapon position x depending on side
                        Vector3 r = transform.localEulerAngles;
                        transform.localRotation = Quaternion.Euler(r.x, direction < 0 ? 180f : 0f, r.z);
                }

                public void CompleteAttack ()
                {
                        success = true;
                        targetList.Clear();
                        if (colliderRef != null)
                                colliderRef.enabled = false;
                }

                public override void OnReset (bool skip = false, bool enteredState = false)
                {
                        CompleteAttack();
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(120, "Perform a melee attack. If velocity is non zero, the y velocity will be treated as a jump force. CompleteAttack() must be called once the animation is complete, or the FSM will get stuck on this state. This method is available on the Melee Attack class. Signals: meleeCombo, customSignal" +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(7, color, offsetY: -2);
                        parent.Field("Collider2D", "colliderRef");
                        parent.Field("Collider Enable", "enableCollider");
                        parent.Field("Layer", "layer");
                        parent.Field("Animation Signal", "animationSignal");
                        parent.Field("Damage", "damage");
                        parent.Field("Force", "forceDirection");
                        parent.Field("Velocity", "velocity");
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
