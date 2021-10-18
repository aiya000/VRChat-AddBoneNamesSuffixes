#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GalaxySixthSensey.AddBoneNamesSuffixes {
  public class AddBoneNamesSuffixes : EditorWindow {
    private const string prefsKeyTargetBoneNamesLength = "AddBoneNamesSuffixes/targetBoneNames.length";
    private const string prefsKeyTargetBoneNamesPrefix = "AddBoneNamesSuffixes/targetBoneNames";

    // The core of this class's state.
    [SerializeField]
    private string[] targetBoneNames;

    [SerializeField]
    private GameObject selectedRootBone;

    [SerializeField]
    private string suffix;

    private string[] olderTargetBoneNames = new string[0];

    // To handle when the windows opened or initiazing required.
    private bool hasRestored = false;

    // If this is true,
    // append or remove suffixes for all children of this.selectedRootBone when applied,
    // and don't show the selection of this.targetBoneNames.
    private bool targetingForAll = false;

    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("galaxy-sixth-sensey/AddBoneNamesSuffixes")]
    private static void Init() {
      EditorWindow.GetWindow(typeof(AddBoneNamesSuffixes));
    }

    private void OnGUI() {
      // Save a user's preferences automatically.
      if (object.ReferenceEquals(this.targetBoneNames, this.olderTargetBoneNames)) {
        this.SaveInputState();
      }
      this.olderTargetBoneNames = this.targetBoneNames;

      if (!this.hasRestored) {
        this.RestoreSavedProperties();
        this.hasRestored = true;
      }

      this.MakeUI();
    }

    private void RestoreSavedProperties() {
      this.targetBoneNames = EditorPrefs.HasKey(prefsKeyTargetBoneNamesLength)
        ? RestoreSavedTargetBoneNames()
        : (string[]) BoneNamesPresets.GetDefault();
    }

    private void MakeBoneNamesLayoutWithSync() {
      var self = new SerializedObject(this);
      self.Update();
      EditorGUILayout.PropertyField(self.FindProperty("targetBoneNames"), true);
      self.ApplyModifiedProperties();
    }

    // Older EditorPrefs is still staying when overwriting with new `this.targetBoneNames`, but removing it maybe unnecessary.
    private void SaveInputState() {
      EditorPrefs.SetInt(prefsKeyTargetBoneNamesLength, this.targetBoneNames.Length);

      foreach (var i in Enumerable.Range(0, this.targetBoneNames.Length)) {
        EditorPrefs.SetString(prefsKeyTargetBoneNamesPrefix + i, this.targetBoneNames[i]);
      }

    }

    private void InitalizeInputState(string avatarName) {
      var length = this.targetBoneNames.Length;

      try {
        this.targetBoneNames = BoneNamesPresets.Get(avatarName);
      } catch (Exception e) {
        Debug.LogError($"Fatal error!: {e}", this);
        EditorUtility.DisplayDialog("エラー", $"Fatal error!: {e}", "OK");
        return;
      }
      this.SaveInputState();

      this.hasRestored = false;
    }

    private void MakeUI() {
      using (new GUILayout.VerticalScope()) {
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

        this.selectedRootBone = (GameObject) EditorGUILayout.ObjectField("Root Bone", this.selectedRootBone, typeof(GameObject), true);
        this.suffix = EditorGUILayout.TextField("Suffix", this.suffix);

        EditorGUILayout.LabelField("");
        this.MakeUIApplyingSection();

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("- - - - - ↓ 設定 ↓ - - - - -");

        EditorGUILayout.LabelField("");
        this.targetingForAll = EditorGUILayout.Toggle("全てのObjectを対象にする", this.targetingForAll);
        if (!this.targetingForAll) {
          this.MakeUIBonesSection();
        }

        EditorGUILayout.LabelField("");
        this.MakeUIConfigurationsSection();

        EditorGUILayout.LabelField("");
        this.MakeUIHelpSection();

        EditorGUILayout.EndScrollView();
      }
    }

    private void MakeUIBonesSection() {
      this.MakeBoneNamesLayoutWithSync();

      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("- - - Target Bone Namesの設定のバックアップ・上書き - - -");

      if (GUILayout.Button("jsonにバックアップする")) {
        SaveTargetBoneNamesToJSON(this.targetBoneNames);
      }

      if (GUILayout.Button("jsonから読み込んで上書きする")) {
        this.InitializeTargetBoneNamesWithJSON();
      }
    }

    private void MakeUIConfigurationsSection() {
      EditorGUILayout.LabelField("- - - Target Bone Namesを初期化 - - -");
      EditorGUILayout.LabelField("※ 今設定しているTarget Bone Namesは上書きされます");

      if (GUILayout.Button("NecoMaid-RICH用ボーン設定で初期化")) {
        this.InitalizeInputState("NecoMaid-RICH");
        EditorUtility.DisplayDialog("AddBoneNamesSuffixes > Target Bone Namesを初期化", "完了", "OK");
      }

      if (GUILayout.Button("U用ボーン設定で初期化")) {
        this.InitalizeInputState("U");
        EditorUtility.DisplayDialog("AddBoneNamesSuffixes > Target Bone Namesを初期化", "完了", "OK");
      }

      if (GUILayout.Button("ミーシェ用ボーン設定で初期化")) {
        this.InitalizeInputState("Mishe");
        EditorUtility.DisplayDialog("AddBoneNamesSuffixes > Target Bone Namesを初期化", "完了", "OK");
      }
    }

    private void MakeUIApplyingSection() {
      EditorGUILayout.LabelField("- - - 実行 - - -");

      if (GUILayout.Button("追加を実行")) {
        this.TryAddingSuffixToBoneNames();
      }

      if (GUILayout.Button("削除を実行")) {
        this.TryRemovingSuffixToBoneNames();
      }
    }

    private void MakeUIHelpSection() {
      EditorGUILayout.LabelField("- - - その他 - - -");

      if (GUILayout.Button("ヘルプを開く（サムネあたりを参照してね！）")) {
        Application.OpenURL("https://aiya000.booth.pm/items/2615466");
      }

      EditorGUILayout.LabelField("※使い方はTwitterで@public_ai000yaに質問してくれてもOK！");
      EditorGUILayout.LabelField("　答えられるときに答えるよ！");
    }

    private void TryAddingSuffixToBoneNames() {
      var message = this.GetErrorMessage();
      if (message != null) {
        EditorUtility.DisplayDialog("エラー", message, "OK");
        return;
      }

      AddSuffixToBoneNames(this.selectedRootBone, this.suffix, this.targetBoneNames);
      EditorUtility.DisplayDialog("AddBoneNamesSuffixes > 追加", "完了", "OK");
    }

    private void TryRemovingSuffixToBoneNames() {
      var message = this.GetErrorMessage();
      if (message != null) {
        EditorUtility.DisplayDialog("エラー", message, "OK");
        return;
      }

      RemoveSuffixToBoneNames(this.selectedRootBone, this.suffix, this.targetBoneNames);
      EditorUtility.DisplayDialog("AddBoneNamesSuffixes > 削除", "完了", "OK");
    }

    private void InitializeTargetBoneNamesWithJSON() {
      try {
        var x = LoadTargetBoneNamesFromJSON();
        if (x == null) {
          EditorUtility.DisplayDialog(
            "AddBoneNamesSuffixes > JSON読み込み",
            "エラー: 有効な形式のJSONではありませんでした。",
            "OK"
          );
          return;
        }

        this.targetBoneNames = x;
      } catch (Exception e) {
        EditorUtility.DisplayDialog(
          "AddBoneNamesSuffixes > JSON読み込み",
          e.Message,
          "OK"
          );
        return;
      }
    }

    /// <summary>
    /// Returns null if no invalid items found.
    /// </summary>
    private string GetErrorMessage() {
      if (this.selectedRootBone == null) {
        return "Root Boneを指定してください。";
      }

      if (this.suffix == null || this.suffix == "") {
        return "Suffixを指定してください。";
      }

      return null;
    }

    /// <summary>
    /// Adds the suffix onto the target name
    /// if it is a target name or the target is all.
    ///
    /// Please also see this.targetBoneNames.
    /// </summary>
    private static void AddSuffixToBoneNames(GameObject target, string suffix, string[] targetBoneNames) {
      if (targetBoneNames.Length == 0 || targetBoneNames.Contains(target.name)) {
        target.name += suffix;
      }

      // for each children of GameObject[]
      foreach (Transform x in target.transform) {
        AddSuffixToBoneNames(x.gameObject, suffix, targetBoneNames);
      }
    }

    /// <summary>
    /// Removes the suffix from the target name
    /// if it is a target name or (the target is all and it contains the suffix).
    ///
    /// Please also see this.targetBoneNames.
    /// </summary>
    private static void RemoveSuffixToBoneNames(GameObject target, string suffix, string[] targetBoneNames) {
      RemoveSuffix(target, suffix, targetBoneNames);

      // for each children of GameObject[]
      foreach (Transform x in target.transform) {
        RemoveSuffixToBoneNames(x.gameObject, suffix, targetBoneNames);
      }
    }

    /// <summary>
    /// When the target is actually a target bone.
    /// </summary>
    private static void RemoveSuffix(GameObject target, string suffix, string[] targetBoneNames) {
      var plainBoneName = Array.Find(targetBoneNames, prefix => target.name.StartsWith(prefix));
      if (plainBoneName != null) {
        target.name = plainBoneName;
      }
    }

    /// <summary>
    /// Requirement: EditorPrefs.HasKey(prefsKeyTargetBoneNamesLength)
    /// </summary>
    private static string[] RestoreSavedTargetBoneNames() {
      var length = EditorPrefs.GetInt(prefsKeyTargetBoneNamesLength);
      var result = new string[length];

      foreach (var i in Enumerable.Range(0, length)) {
        result[i] = EditorPrefs.GetString(prefsKeyTargetBoneNamesPrefix + i);
      }

      return result;
    }

    private static void SaveTargetBoneNamesToJSON(string[] targetBoneNames) {
      var path = EditorUtility.SaveFilePanel("Save Target Bone Names", Application.dataPath, "AddBoneNamesSuffixesBoneNames", "json");
      if (string.IsNullOrEmpty(path)) {
        return;
      }

      File.WriteAllText(
        path,
        JsonUtility.ToJson(new JSONWorkaround() {
          AddBoneNamesSuffixesBones = targetBoneNames,
        })
      );
    }

    private static string[] LoadTargetBoneNamesFromJSON() {
      var path = EditorUtility.OpenFilePanel("Select JSON",  Application.dataPath, "json");
      if (string.IsNullOrEmpty(path)) {
        return null;
      }

      var x = JsonUtility.FromJson<JSONWorkaround>(
        File.ReadAllText(path)
      );
      return x.AddBoneNamesSuffixesBones;
    }

  }
}

#endif

