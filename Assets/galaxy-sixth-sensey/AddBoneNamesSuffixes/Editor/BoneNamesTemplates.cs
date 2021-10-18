using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GalaxySixthSensey.AddBoneNamesSuffixes {
  /// <summary>
  /// An immutable dictionary for bone names groups.
  /// </summary>
  public class BoneNamesPresets {
    // Be used this if the user has not saved the preference.
    private const string defaultTargetBoneNamesAvatar = "NecoMaid-RICH";

    // The element of a key (the bone names. e.g. Hips, Shoulder_L, ...)
    // are named by the key (an avatar name. e.g. NecoMaid-RICH, Mishe, ...).
    private static readonly Dictionary<string, string[]> defaultTargetBoneNames;

    // NOTE: Using static initializer because Dictionary<>'s constructor makes its illegal state {{{
    //       thas is
    //       ```
    //       defaultTargetBoneNames.ContainsKey(defaultTargetBoneNamesAvatar)
    //       && defaultTargetBoneNames[defaultTargetBoneNames] == null
    //       ```
    //       in Unity 2018.420f1.
    //       }}}
    static BoneNamesPresets() {
      defaultTargetBoneNames = new Dictionary<string, string[]>();

      var defaultTargetBoneNamesForNecoMaidRich = new [] { /*{{{*/
        "Hips",
        "Spine",
        "Chest",
        "Neck",
        "Head",
        "Shoulder_L",
        "UpperArm_L",
        "LowerArm_L",
        "Hand_L",
        "Shoulder_R",
        "UpperArm_R",
        "LowerArm_R",
        "Hand_R",
        "UpperLeg_L",
        "LowerLeg_L",
        "Foot_L",
        "Bese_L",
        "UpperLeg_R",
        "LowerLeg_R",
        "Foot_R",
        "Bese_R",
      }; /*}}}*/
      var defaultTargetBoneNamesForU = new [] { /*{{{*/
        "Hips",
        "L_UpperLeg",
        "L_LowerLeg",
        "L_Foot",
        "L_Toe",
        "LL_Sphere.L",
        "UL_Sphere.L",
        "oshiri",
        "oshiri_L",
        "oshiri_R",
        "R_UpperLeg",
        "R_LowerLeg",
        "R_Foot",
        "R_Toe",
        "LL_Sphere.R",
        "UL_Sphere.R",
        "Spine",
        "Chest",
        "mune_L",
        "oppai_L",
        "mune_R",
        "oppai_R",
        "L_Shoulder",
        "L_UpperArm",
        "L_LowerArm",
        "L_hand",
        "Neck",
        "Head",
        "R_Shoulder",
        "R_UpperArm",
        "R_LowerArm",
        "R_hand",
        "HIP_Capsule.A",
        "HIP_Capsule.A2",
        "HIP_Sphere.B1",
        "HIP_Sphere.B2",
        "HIP_Sphere.C1",
        "HIP_Sphere.C2",
      }; /*}}}*/
      var defaultTargetBoneNamesForMishe = new [] { /*{{{*/
        "Hips",
        "Spine",
        "Chest",
        "Chest.002",
        "Neck",
        "Head",
        "Shoulder.L",
        "Shoulder.R",
        "Hand.L",
        "Index Proximal.L",
        "Little Proximal.L",
        "Middle Proximal.L",
        "Ring Proximal.L",
        "Thumb Proximal.L",
        "Lower_arm.L",
        "Lower_arm.R",
        "Upper_arm.L",
        "Upper_arm.R",
        "Upper_leg.L",
        "Upper_leg.R",
        "lower_leg.L",
        "lower_leg.R",
        "foot.L",
        "foot.R",
        "foot.L.001",
        "foot.R.001",
      }; /*}}}*/

      defaultTargetBoneNames.Add(defaultTargetBoneNamesAvatar, defaultTargetBoneNamesForNecoMaidRich);
      defaultTargetBoneNames.Add("U", defaultTargetBoneNamesForU);
      defaultTargetBoneNames.Add("Mishe", defaultTargetBoneNamesForMishe);
    }

    public static string[] Get(string avatarName) {
      var x = defaultTargetBoneNames[avatarName];
      if (x == null) {
        Debug.LogError($"GalaxySixthSensey.AddBoneNamesSuffixes.BoneNamesPresets: couldn't get value from this index: {avatarName}");
        Debug.LogError($"Start: {defaultTargetBoneNames.ContainsKey(avatarName)}");
        foreach (var a in defaultTargetBoneNames) {
          var b = a.Value == null ? "null" : "something";
          Debug.LogError($"{a.Key}: {b}");
        }
        Debug.LogError("End");
      }

      return (string[]) x.Clone();
    }

    public static string[] GetDefault() {
      return Get(defaultTargetBoneNamesAvatar);
    }
  }
}
