#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class FollowTargetX : Action
        {
                [SerializeField] public Blackboard target;
                [SerializeField] public float speed = 10f;
                [SerializeField] public float radius = 2f;
                [SerializeField] public float smooth = 0.5f;

                [System.NonSerialized] private float velocityX;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (target == null)
                        {
                                return NodeState.Failure;
                        }
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                velocityX = 0;
                        }

                        Vector2 t = target.GetTarget();

                        if (target.hasNoTargets)
                        {
                                return NodeState.Failure;
                        }

                        float directionX = t.x < root.position.x ? -1f : 1f;
                        root.velocity.x = velocityX = Compute.Lerp(velocityX, directionX * speed, smooth);

                        // This action only moves along X, so its completion condition must
                        // also ignore any vertical offset between the AI and its target.
                        // Using the full 2D distance can leave the node Running forever when
                        // the two characters use different pivot heights.
                        if (Mathf.Abs(t.x - root.position.x) <= radius)
                        {
                                return NodeState.Success;
                        }
                        return NodeState.Running;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(60, "Follow a target in the x direction. Returns Success if target is within the radius." +
                                        "\n \nReturns Running, Success, Failure");
                        }

                        FoldOut.Box(3, color, offsetY: -2);
                        {
                                AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.FieldDouble("Speed", "speed", "smooth");
                                Labels.FieldText("Smooth");
                                parent.Clamp("smooth");
                                parent.Field("Find Distance", "radius");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
