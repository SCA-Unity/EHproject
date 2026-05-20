using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TwoBitMachines.FlareEngine
{
      public class OptionsBindingMenu : MonoBehaviour
      {
            [SerializeField] public InputActionAsset inputAction;
            [SerializeField] public AudioManager audioManager;
            [SerializeField] public Toggle fullScreen;
            [SerializeField] public Slider music;
            [SerializeField] public Slider sfx;
            [SerializeField] public Dropdown resolutions;
            [SerializeField] public Button resetAll;
            [System.NonSerialized] private Resolution[] res;

            private void Start ( )
            {
                  if (fullScreen != null) fullScreen.isOn = PlayerPrefs.GetInt ("IsFullScreen") <= 0 ? true : false;
                  if (music != null) music.value = audioManager.musicVolume;
                  if (sfx != null) sfx.value = audioManager.sfxVolume;
                  fullScreen?.onValueChanged.AddListener (SetFullScreen);
                  resolutions?.onValueChanged.AddListener (SetResolution);
                  music?.onValueChanged.AddListener (OnMusicVolumeChanged);
                  sfx?.onValueChanged.AddListener (OnSFXVolumeChanged);
                  resetAll?.onClick.AddListener (ResetAll);
                  SetResolution ( );
            }

            private static int GetRefreshRateHz (Resolution resolution)
            {
                  // Resolution.refreshRate is obsolete; use refreshRateRatio instead.
                  // Convert numerator/denominator fraction into an integer Hz for UI + PlayerPrefs.
                  var rr = resolution.refreshRateRatio;
                  if (rr.denominator == 0)
                        return 0;
                  return Mathf.RoundToInt ((float) rr.numerator / (float) rr.denominator);
            }

            public void OnEnable ( ) // for saving new input values
            {
                  var rebinds = PlayerPrefs.GetString ("rebinds");
                  if (!string.IsNullOrEmpty (rebinds))
                  {
                        inputAction.LoadBindingOverridesFromJson (rebinds);
                  }
            }

            public void OnDisable ( )
            {
                  var rebinds = inputAction.SaveBindingOverridesAsJson ( );
                  PlayerPrefs.SetString ("rebinds", rebinds);
            }

            public void OnMusicVolumeChanged (float value)
            {
                  audioManager?.MasterMusicVolume (value);
            }

            public void OnSFXVolumeChanged (float value)
            {
                  audioManager?.MasterSFXVolume (value);
            }

            public void SetFullScreen (bool isFullScreen)
            {
                  Screen.fullScreen = isFullScreen;
                  PlayerPrefs.SetInt ("IsFullScreen", isFullScreen ? 0 : 1);
            }

            private void SetResolution ( )
            {
                  res = Screen.resolutions;
                  resolutions.ClearOptions ( );

                  System.Array.Sort (res, (x, y) =>
                  {
                        // Sort by refresh rate first
                        int refreshRateComparison = GetRefreshRateHz (x).CompareTo (GetRefreshRateHz (y));
                        if (refreshRateComparison != 0)
                        {
                              return refreshRateComparison;
                        }

                        // If refresh rates are the same, sort by width and height
                        int widthComparison = x.width.CompareTo (y.width);
                        if (widthComparison != 0)
                        {
                              return widthComparison;
                        }

                        return x.height.CompareTo (y.height);
                  });

                  int current = 0;
                  int refreshRate = PlayerPrefs.GetInt ("RefreshRateTBM", GetRefreshRateHz (Screen.currentResolution));
                  List<string> options = new List<string> ( );
                  for (int i = 0; i < res.Length; i++)
                  {
                        int hz = GetRefreshRateHz (res[i]);
                        string option = res[i].width + " x " + res[i].height + " @ " + hz;
                        options.Add (option);

                        if (res[i].width == Screen.width && res[i].height == Screen.height && GetRefreshRateHz (res[i]) == refreshRate)
                        {
                              current = i;
                        }
                  }
                  resolutions.AddOptions (options);
                  resolutions.value = current;
                  resolutions.RefreshShownValue ( );
            }

            public void SetResolution (int index)
            {
                  Resolution resolution = res[index];
                  bool isFullScreen = PlayerPrefs.GetInt ("IsFullScreen") <= 0 ? true : false;
                  int hz = GetRefreshRateHz (resolution);

                  // Refresh rate switching is only supported for exclusive fullscreen.
                  if (isFullScreen)
                  {
                        Screen.SetResolution (resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen, resolution.refreshRateRatio);
                  }
                  else
                  {
                        Screen.SetResolution (resolution.width, resolution.height, false);
                  }

                  PlayerPrefs.SetInt ("RefreshRateTBM", hz);
            }

            public void ResetAll ( )
            {
                  if (inputAction == null)
                  {
                        return;
                  }
                  foreach (InputActionMap map in inputAction.actionMaps)
                  {
                        map.RemoveAllBindingOverrides ( );
                  }
                  RebindInputButtonSO[] list = GetComponentsInChildren<RebindInputButtonSO> ( );
                  for (int i = 0; i < list.Length; i++)
                  {
                        list[i].ResetBinding ( );
                  }
            }

            public void ResetSpecific (string controlScheme)
            {
                  if (inputAction == null)
                  {
                        Debug.LogWarning ("InputAction is missing reference.");
                        return;
                  }
                  foreach (InputActionMap map in inputAction.actionMaps)
                  {
                        foreach (InputAction action in map.actions)
                        {
                              action.RemoveBindingOverride (InputBinding.MaskByGroup (controlScheme));
                        }
                  }
            }
      }
}