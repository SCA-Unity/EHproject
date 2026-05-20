#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("Flare Engine/Player/Abilities/PushBack")]
        public class PushBack : Ability
        {
                [SerializeField] public float distanceX = 2;
                [SerializeField] public float jumpForce = 5;
                [SerializeField] public float minJumpForce = 0;
                [SerializeField] public float pushTime = 0.25f;
                [SerializeField] public float flashTime = 0.5f;
                [SerializeField] public int flash = 1;

                [SerializeField] public Color color = Color.white;
                [SerializeField] public SpriteRenderer spriteRenderer;
                [SerializeField] public Material material;
                [SerializeField] public UnityEvent onPushBack = new UnityEvent();

                [System.NonSerialized] private Material originMaterial;
                [System.NonSerialized] private Color originColor;
                [System.NonSerialized] private Vector2 appliedForce;
                [System.NonSerialized] private Vector2 direction;

                [System.NonSerialized] private bool pushBack;
                [System.NonSerialized] private bool flashActive;
                [System.NonSerialized] private bool beginPushBack;
                [System.NonSerialized] private bool flashToggle;
                [System.NonSerialized] private int pushBackDirection;
                [System.NonSerialized] private float acceleration;
                [System.NonSerialized] private float flashCounter;
                [System.NonSerialized] private float flashTimer;
                [System.NonSerialized] private float flashLimit;
                [System.NonSerialized] private float counter;

                public override void Initialize (Player player)
                {
                        if (material != null)
                        {
                                material = new Material(material);
                        }
                        if (spriteRenderer != null)
                        {
                                originMaterial = spriteRenderer.material;
                        }
                        if (spriteRenderer != null)
                        {
                                originColor = spriteRenderer.color;
                        }
                }

                public override void Reset (AbilityManager player)
                {
                        ResetPush(player);
                        ResetFlash();
                }

                public void ResetPush (AbilityManager player)
                {
                        player.character.pushBackActive = false;
                        player.pushBackActive = false;
                        beginPushBack = false;
                        pushBack = false;
                        counter = 0;
                }

                public void ResetFlash ()
                {
                        if (spriteRenderer != null && spriteRenderer.material != originMaterial)
                        {
                                spriteRenderer.material = originMaterial;
                        }
                        if (spriteRenderer != null)
                        {
                                spriteRenderer.color = originColor;
                        }
                        flashActive = false;
                        flashToggle = false;
                        flashCounter = 0;
                        flashTimer = 0;
                        flashLimit = 0;
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (beginPushBack)
                        {
                                beginPushBack = false;
                                pushBackDirection = player.playerDirection;
                        }
                        player.pushBackActive = player.character.pushBackActive = pushBack;
                        return !pause && pushBack;
                }

                public void ActivatePushBack (ImpactPacket impact)
                {
                        if (impact.damageValue >= 0)
                                return; // only apply push back on negative values

                        counter = 0;
                        pushBack = true;
                        beginPushBack = true;
                        direction = impact.direction;
                        acceleration = (-2f * distanceX) / Mathf.Pow(pushTime, 2f);
                        appliedForce = acceleration * direction * pushTime;

                        flashActive = true;
                        flashToggle = false;
                        flashCounter = 1000f; // go into flash immediately
                        flashLimit = Time.time + flashTime;
                        flashTimer = flash <= 1 ? flashTime / 2f : flashTime / ((float) flash * 2f);
                        onPushBack.Invoke();
                }

                public override void EarlyExecute (AbilityManager player, ref Vector2 velocity)
                {
                        if (!flashActive)
                                return;

                        if (Clock.Timer(ref flashCounter, flashTimer) && spriteRenderer != null)
                        {
                                flashToggle = !flashToggle;
                                if (material != null)
                                {
                                        material.color = color;
                                        spriteRenderer.material = flashToggle ? material : originMaterial;
                                }
                                else
                                {
                                        spriteRenderer.color = flashToggle ? color : originColor;
                                }
                        }
                        if (flashLimit < Time.time)
                        {
                                ResetFlash();
                        }
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        if (counter == 0 && Time.deltaTime != 0) //                                                   distance x is pushed linearly, vel y is treated as a jump
                        {
                                velocity.y = jumpForce * direction.y;
                                if (jumpForce != 0 && velocity.y == 0 && player.world.onGround)
                                {
                                        velocity.y = minJumpForce;
                                }
                                player.hasJumped = velocity.y > 0 ? true : player.hasJumped;
                        }

                        if (Clock.Timer(ref counter, pushTime))
                        {
                                // if (!Compute.SameSign(pushBackDirection , -appliedForce.x))
                                // {
                                //       velocity.x = 0.001f * Mathf.Sign(appliedForce.x); //  when push back is complete, make sure player velocity and pushBackDirection align, or else sprite will flip
                                // }
                                // player.playerDirection = (int) Mathf.Sign(velocity.x);
                                //   player.signals.SetDirection(player.playerDirection);
                                player.playerDirection = pushBackDirection;
                                player.signals.ForceDirection(pushBackDirection);
                                ResetPush(player);
                                return;
                        }

                        appliedForce -= acceleration * direction * Time.deltaTime;
                        velocity.x = -appliedForce.x;
                        player.signals.Set("pushBack", true);
                        player.signals.Set("pushBackLeft", pushBackDirection < 0);
                        player.signals.Set("pushBackRight", pushBackDirection > 0);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool pushBackFoldOut;

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Push Back", barColor, labelColor))
                        {
                                FoldOut.Box(3, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.Field("Distance", "distanceX");
                                        parent.FieldDouble("Jump Force", "minJumpForce", "jumpForce");
                                        Labels.FieldDoubleText("Default", "", rightSpacing: 4);
                                        parent.Field("Push Time", "pushTime");
                                }
                                Layout.VerticalSpacing(3);

                                FoldOut.Box(5, FoldOut.boxColorLight);
                                {
                                        parent.Field("Sprite Renderer", "spriteRenderer");
                                        parent.Field("Material", "material");
                                        parent.Field("Color", "color");
                                        parent.Field("Flash Time", "flashTime");
                                        parent.Field("Flashes", "flash");
                                }
                                Layout.VerticalSpacing(5);

                                Fields.EventFoldOut(parent.Get("onPushBack"), parent.Get("pushBackFoldOut"), "On Push Back", color: FoldOut.boxColorLight);

                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
