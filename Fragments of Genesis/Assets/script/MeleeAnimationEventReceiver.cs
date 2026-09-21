using System.Collections.Generic;
using TwoBitMachines.FlareEngine;
using PlayerMelee = TwoBitMachines.FlareEngine.ThePlayer.Melee;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MeleeAnimationEventReceiver : MonoBehaviour
{
    private const float HitStartTime = 12f / 60f;
    private const float HitEndTime = 24f / 60f;
    private const float AttackClipLength = 35f / 60f;

    [Tooltip("The Melee ability on the player. It is found automatically when empty.")]
    public PlayerMelee meleeAbility;

    private Animator animator;
    private bool attackAnimationActive;
    private bool hitboxActive;
    private float attackTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        InstallOnPlayers();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallOnPlayers();
    }

    private static void InstallOnPlayers()
    {
        foreach (PlayerMelee playerMelee in FindObjectsOfType<PlayerMelee>(true))
        {
            EnsurePlayerHealth(playerMelee.gameObject);
            if (playerMelee.GetComponent<MeleeAnimationEventReceiver>() == null)
            {
                MeleeAnimationEventReceiver receiver = playerMelee.gameObject.AddComponent<MeleeAnimationEventReceiver>();
                receiver.meleeAbility = playerMelee;
            }
        }
    }

    private static void EnsurePlayerHealth(GameObject player)
    {
        Health health = player.GetComponent<Health>();
        if (health == null)
        {
            health = player.AddComponent<Health>();
        }

        health.isHealth = true;
        health.variableName = "PlayerHealth";
        health.minValue = 0f;
        health.maxValue = 10f;
        health.currentValue = Mathf.Clamp(health.currentValue, health.minValue, health.maxValue);
        if (health.currentValue <= health.minValue)
        {
            health.currentValue = health.maxValue;
        }
        health.recoveryTime = 0.5f;
        health.Register();
    }

    private void Awake()
    {
        ResolveMeleeAbility();
        animator = GetComponentInChildren<Animator>();
        ConfigureEventDrivenHitboxes();
        SetHitboxes(false);
    }

    private void Update()
    {
        UpdateHitboxWindow();
    }

    public void OnAttackHitStart()
    {
        if (hitboxActive)
        {
            return;
        }
        hitboxActive = true;
        ConfigureEventDrivenHitboxes();
        SetHitboxes(true, activeWeaponsOnly: true);
    }

    public void OnAttackHitEnd()
    {
        hitboxActive = false;
        SetHitboxes(false);
    }

    public void OnAttackAnimationEnd()
    {
        attackAnimationActive = false;
        hitboxActive = false;
        attackTimer = 0f;
        SetHitboxes(false);
        ResolveMeleeAbility();
        meleeAbility?.CompleteAttack();
    }

    private void OnDisable()
    {
        attackAnimationActive = false;
        hitboxActive = false;
        attackTimer = 0f;
        SetHitboxes(false);
    }

    private void UpdateHitboxWindow()
    {
        if (UpdateHitboxFromFlareMelee())
        {
            return;
        }

        UpdateHitboxFromAnimator();
    }

    private bool UpdateHitboxFromFlareMelee()
    {
        TwoBitMachines.FlareEngine.Melee[] weapons = GetMeleeWeapons();
        if (weapons.Length == 0)
        {
            return false;
        }

        bool inMelee = false;
        foreach (TwoBitMachines.FlareEngine.Melee weapon in weapons)
        {
            if (weapon.gameObject.activeInHierarchy && weapon.melee.inMelee)
            {
                inMelee = true;
                break;
            }
        }

        if (!inMelee)
        {
            attackTimer = 0f;
            attackAnimationActive = false;
            if (hitboxActive)
            {
                OnAttackHitEnd();
            }
            return true;
        }

        if (!attackAnimationActive)
        {
            attackAnimationActive = true;
            attackTimer = 0f;
            hitboxActive = false;
        }

        attackTimer += Time.deltaTime;
        if (!hitboxActive && attackTimer >= HitStartTime)
        {
            OnAttackHitStart();
        }
        if (hitboxActive && attackTimer >= HitEndTime)
        {
            OnAttackHitEnd();
        }
        return true;
    }

    private void UpdateHitboxFromAnimator()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            return;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool isAttack = state.IsName("Attack") || state.IsName("Base Layer.Attack");
        if (!isAttack)
        {
            if (attackAnimationActive)
            {
                OnAttackHitEnd();
            }
            attackAnimationActive = false;
            return;
        }

        float normalizedTime = state.normalizedTime % 1f;
        if (!attackAnimationActive)
        {
            attackAnimationActive = true;
            hitboxActive = false;
        }

        if (!hitboxActive && normalizedTime >= HitStartTime / AttackClipLength)
        {
            OnAttackHitStart();
        }
        if (hitboxActive && normalizedTime >= HitEndTime / AttackClipLength)
        {
            OnAttackHitEnd();
        }
    }

    private void ResolveMeleeAbility()
    {
        if (meleeAbility == null)
        {
            meleeAbility = GetComponent<PlayerMelee>();
        }
    }

    private TwoBitMachines.FlareEngine.Melee[] GetMeleeWeapons()
    {
        return GetComponentsInChildren<TwoBitMachines.FlareEngine.Melee>(true);
    }

    private void ConfigureEventDrivenHitboxes()
    {
        foreach (TwoBitMachines.FlareEngine.Melee weapon in GetMeleeWeapons())
        {
            weapon.melee.enableCollider = MeleeCollider.LeaveAsIs;
        }
    }

    private void SetHitboxes(bool enabled, bool activeWeaponsOnly = false)
    {
        foreach (TwoBitMachines.FlareEngine.Melee weapon in GetMeleeWeapons())
        {
            if (weapon.collider2DRef == null)
            {
                continue;
            }
            weapon.collider2DRef.enabled = enabled &&
                (!activeWeaponsOnly || weapon.gameObject.activeInHierarchy);
        }
    }

#if UNITY_EDITOR
    private const string AttackClipPath = "Assets/Low_Swordman/2.Animation/Attack.anim";

    [UnityEditor.MenuItem("Tools/Fragments of Genesis/Configure Melee Hit Events")]
    public static void ConfigureMeleeHitEvents()
    {
        AnimationClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(AttackClipPath);
        if (clip == null)
        {
            Debug.LogError($"Attack clip was not found: {AttackClipPath}");
            return;
        }

        List<AnimationEvent> events = new List<AnimationEvent>();
        foreach (AnimationEvent animationEvent in UnityEditor.AnimationUtility.GetAnimationEvents(clip))
        {
            if (animationEvent.functionName != nameof(OnAttackHitStart) &&
                animationEvent.functionName != nameof(OnAttackHitEnd))
            {
                events.Add(animationEvent);
            }
        }

        events.Add(CreateEvent(nameof(OnAttackHitStart), HitStartTime));
        events.Add(CreateEvent(nameof(OnAttackHitEnd), HitEndTime));
        events.Sort((a, b) => a.time.CompareTo(b.time));

        UnityEditor.AnimationUtility.SetAnimationEvents(clip, events.ToArray());
        UnityEditor.EditorUtility.SetDirty(clip);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Configured melee hit events at {HitStartTime:0.###}s and {HitEndTime:0.###}s on {AttackClipPath}");
    }

    private static AnimationEvent CreateEvent(string functionName, float time)
    {
        return new AnimationEvent
        {
            functionName = functionName,
            time = time,
            messageOptions = SendMessageOptions.RequireReceiver
        };
    }
#endif
}
