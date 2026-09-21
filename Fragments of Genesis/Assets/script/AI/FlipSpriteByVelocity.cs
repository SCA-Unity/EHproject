#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
    [AddComponentMenu("")]
    public class FlipSpriteByVelocity : Action
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool invert;

        public override NodeState RunNodeLogic(Root root)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer == null)
            {
                return NodeState.Failure;
            }
            if (Mathf.Abs(root.velocity.x) > 0.0001f)
            {
                spriteRenderer.flipX = invert ? root.velocity.x > 0f : root.velocity.x < 0f;
            }
            return NodeState.Running;
        }

#if UNITY_EDITOR
        public override bool HasNextState() { return false; }

        public override bool OnInspector(AIBase ai, SerializedObject parent, Color color, bool onEnable)
        {
            FoldOut.Box(2, color, offsetY: -2);
            parent.Field("Sprite Renderer", "spriteRenderer");
            parent.FieldToggle("Invert", "invert");
            Layout.VerticalSpacing(3);
            return true;
        }
#endif
    }
}
