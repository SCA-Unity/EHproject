#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Ability : MonoBehaviour
        {
                [SerializeField, HideInInspector] public int ID;
                [SerializeField, HideInInspector] public string abilityName;
                [SerializeField, HideInInspector] public bool pause = false;
                [SerializeField, HideInInspector] public List<string> exception = new List<string>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool editMask;
                [SerializeField, HideInInspector] private bool deleteAsk;
                [SerializeField, HideInInspector] private bool viewScript;
                [SerializeField, HideInInspector] private string tempName;
                [SerializeField, HideInInspector] private List<bool> foldOuts = new List<bool>();

                public virtual bool OnInspector (SerializedObject controller , SerializedObject parent , string[] inputList , Color barColor , Color labelColor)
                {
                        return false;
                }

                public static bool Open (SerializedObject parent , string name , Color barColor , Color labelColor)
                {
                        return FoldOut.Bar(parent , barColor).Label(name , labelColor).BR("deleteAsk" , "Delete").BR("delete" , "Close" , execute: parent.Bool("deleteAsk")).BRE("pause" , "Pause2" , Tint.Delete , Tint.PastelGreen).FoldOut();
                }

#pragma warning restore 0414
#endif
                #endregion

                public virtual void Initialize (Player player)
                {

                }

                // Some abilities are added via code/editor tools. When this happens, abilityName can be left empty,
                // which breaks exception/priority checks that compare by abilityName string.
                protected virtual void Awake ()
                {
                        if (string.IsNullOrEmpty(abilityName))
                        {
                                abilityName = GetType().Name;
                        }
                }

                public virtual void Reset (AbilityManager player)
                {

                }

                public virtual bool TurnOffAbility (AbilityManager player)
                {
                        return true;
                }

                public virtual bool IsAbilityRequired (AbilityManager player , ref Vector2 velocity)
                {
                        return false;
                }

                public virtual void ExecuteAbility (AbilityManager player , ref Vector2 velocity , bool isRunningAsException = false)
                {

                }

                public virtual void EarlyExecute (AbilityManager player , ref Vector2 velocity)
                {

                }

                public virtual void LateExecute (AbilityManager player , ref Vector2 velocity)
                {

                }

                public virtual void PostAIExecute (AbilityManager player)
                {

                }

                public virtual void PostCollisionExecute (AbilityManager player , Vector2 velocity)
                {

                }

                public virtual bool ContainsException (string typeName)
                {
                        return exception.Contains(typeName);
                }

                public void Pause (bool value)
                {
                        pause = value;
                }

        }
}
