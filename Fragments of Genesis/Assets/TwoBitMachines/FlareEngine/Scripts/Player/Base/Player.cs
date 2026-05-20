using System.Collections.Generic;
using TwoBitMachines.Safire2DCamera.TestMode;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("Flare Engine/Player")]
        [RequireComponent(typeof(BoxCollider2D))]
        public class Player : Character
        {
                [SerializeField] public List<Ability> ability = new List<Ability>();
                [SerializeField] public List<string> priority = new List<string>();
                [SerializeField] public UserInputs inputs = new UserInputs();
                [SerializeField] public AbilityManager abilities = new AbilityManager();
                [SerializeField] public bool isVehicle = false;

                public static Player mainPlayer;
                public static List<Player> players = new List<Player>();


                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private int signalIndex;
                [SerializeField, HideInInspector] private int shift;
                [SerializeField, HideInInspector] private int index;
                [SerializeField, HideInInspector] private int viewIndex;
                [SerializeField, HideInInspector] private bool hide;
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private bool view;
                [SerializeField, HideInInspector] private bool addAbility;
                [SerializeField, HideInInspector] private bool initInputs;
                [SerializeField, HideInInspector] private bool inputFoldOut;
                [SerializeField, HideInInspector] private bool settings;
                [SerializeField, HideInInspector] private bool abilityFoldOut;
                [SerializeField, HideInInspector] private bool collisionFoldOut;
                [SerializeField, HideInInspector] private bool priorityFoldOut;
                [SerializeField, HideInInspector] private bool clearInputs;
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        inputs.Initialize();
                        world.Initialize(transform);
                        signals.InitializeToSpriteEngine(transform);
                        abilities.Initialize(this, inputs, ability, this);
                        
                }

                public override void OnEnable ()
                {
                        if (!players.Contains(this))
                        {
                                players.Add(this);
                        }
                        if (!characters.Contains(world))
                        {
                                characters.Add(world);
                        }
                        if (world.useMovingPlatform && !passengers.Contains(world))
                        {
                                passengers.Add(world);
                        }
                        mainPlayer = !isVehicle ? this : mainPlayer;
                }

                public override void OnDestroy ()
                {
                        if (players.Contains(this))
                        {
                                players.Remove(this);
                        }
                        if (characters.Contains(world))
                        {
                                characters.Remove(world);
                        }
                        if (world.useMovingPlatform && passengers.Contains(world))
                        {
                                passengers.Remove(world);
                        }
                }

                public static void ResetPlayers ()
                {
                        for (int i = 0; i < players.Count; i++)
                        {
                                players[i]?.abilities.ResetAll();
                        }
                }

                public static void BlockAllInputs (bool value)
                {
                        for (int i = 0; i < players.Count; i++)
                        {
                                players[i]?.inputs.Block(value);
                        }
                }

                public static void BlockAllInputsRemember (bool value)
                {
                        for (int i = 0; i < players.Count; i++)
                        {
                                players[i]?.inputs.RememberBlock(value);
                        }
                }

                public static void Run ()
                {
                        for (int i = players.Count - 1; i >= 0; i--)
                        {
                                if (players[i] == null)
                                {
                                        players.RemoveAt(i);
                                }
                                else
                                {
                                        players[i].Execute();
                                }
                        }
                }

                public static void PostAIRun ()
                {
                        for (int i = players.Count - 1; i >= 0; i--)
                        {
                                players[i]?.PostAIExecute();
                        }
                }

                public static Vector2 PlayerPosition (Vector2 returnPosition)
                {
                        return mainPlayer != null ? (Vector2) mainPlayer.transform.position : returnPosition;
                }

                public static Transform PlayerTransform ()
                {
                        return mainPlayer?.transform;
                }

                public static void SetPlayerDirection (int direction)
                {
                        mainPlayer?.signals.SetDirection(direction);
                }

                public override void Execute ()
                {
                        if (!enabled || !gameObject.activeInHierarchy)
                                return;
                        world.box.Update();
                        abilities.Execute();
                        world.Move(ref abilities.finalVelocity, ref abilities.velocity.y, abilities.hasJumped, Time.deltaTime, ref abilities.onSurface);
                        abilities.PostCollisionExecute(abilities.finalVelocity);
                        signals.SetSignals(abilities.finalVelocity, abilities.ground, world.onWallStop);
                        setVelocity = abilities.finalVelocity;
                }

                public void Control (float speedX, bool hasJumped, bool onSurface)
                {
                        world.box.Update();
                        Vector2 velocity = new Vector2(speedX, 0);
                        world.Move(ref velocity, ref velocity.y, hasJumped, Time.deltaTime, ref onSurface);
                        signals.SetSignals(velocity, abilities.ground, world.onWallStop);
                        setVelocity = velocity;
                }

                public override void PostAIExecute ()
                {
                        abilities.PostAIExecute();
                }

                public void BlockInput (bool value)
                {
                        inputs.block = value;
                }

                public void ApplyVelocityX (float speed)
                {
                        abilities.walk.externalVelX = speed * abilities.playerDirection;
                }

                public void Run (bool value)
                {
                        abilities.walk.run = value;
                }

                public void Walk (bool value)
                {
                        abilities.walk.walk = value;
                }

                public void WalkSpeed (float speed)
                {
                        abilities.walk.speed = speed;
                }

                public void ResetAbilities ()
                {
                        abilities.ResetAll();
                }

                
        }
}
