using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  public class SkyStudioAssetPostProcessor : AssetPostprocessor
  {
    public static string migrationVersionKey = "SkyStudio-Migration-Version";
    public static int migrationVersion = 1;
    public static string migrationUnityVersionKey = "SkyStudio-Migration-Unity-Version";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
      if (importedAssets == null || importedAssets.Length == 0) {
        return;
      }
    
      // If they moved the Sky Studio root folder, clear our cached path.
      SkyEditorUtility.ClearSkyStudioRootDirectoryCachedPath();
      
      // Certain asset paths force trigger migration, others we check using a version.
      if (!FileMigrationTriggers(importedAssets) && !ShouldRunFullMigration()) {
        return;
      }

      RunFullMigration();
    }
    
    // Check for certain paths that when imported indicate a need to migrate.
    static bool FileMigrationTriggers(string[] assets) {
      if (assets == null) {
        return false;
      }

      foreach (string asset in assets) {
        if (asset.Contains("SkyStudio3DStandard-GlobalKeywords")) {
          return true;
        }
      }

      return false;
    }

    static bool ShouldRunFullMigration() {
      // Check if migration is already current to this version.
      int lastVersion = EditorPrefs.GetInt(migrationVersionKey, 0);
      string lastUnityVersion = EditorPrefs.GetString(migrationUnityVersionKey, "");

      if (lastVersion == migrationVersion && lastUnityVersion == Application.version) {
        return false;
      }

      return true;
    }

    static void RunFullMigration() {
      EditorPrefs.SetInt(migrationVersionKey, migrationVersion);
      EditorPrefs.SetString(migrationUnityVersionKey, Application.version);
      
      // No migration necessary since we're on an older version of Unity.
      if (!SkyEditorUtility.SupportsLocalKeywords()) {
        return;
      }

      // Upgrade all the skybox materials in the project.
      string[] guids = AssetDatabase.FindAssets("t:material");

      foreach (string guid in guids) {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);        
        if (assetPath == null) {
          continue;
        }

        // Upgrade the material if it's a legacy sky studio material.
        CheckAndRepairSkyStudioMaterial(assetPath);
      }

      // Delete the legacy shader since it's no longer being used by materials.
      CheckAndDeleteGlobalShaderKeywordsFile();
    }

    // Delete the version of the sky studio shader that uses global keywords if the
    // platform supports using local keywords.
    static void CheckAndDeleteGlobalShaderKeywordsFile() {
      if (!SkyEditorUtility.SupportsLocalKeywords()) {
        return;
      }
      
      Shader globalShader = Shader.Find(SkyProfile.DefaultLegacyShaderName);
      
      // Check if it's already been deleted.
      if (globalShader == null) {
        return;
      }

      string assetPath = AssetDatabase.GetAssetPath(globalShader);
      if (assetPath == null) {
        return;
      }
      
      if (!AssetDatabase.DeleteAsset(assetPath)) {
        Debug.LogWarning("Failed to delete legacy Sky Studio shader.");
      }

      AssetDatabase.SaveAssets();
    }

    static bool IsLegacySkyboxMaterial(Material material) {
      if (material == null || material.shader == null || material.shader.name == null) {
        return false;
      }

      return material.shader.name == SkyProfile.DefaultLegacyShaderName;
    }

    static bool UpgradeLegacySkyboxMaterial(Material material) {
      if (material == null) {
        return false;
      }

      material.shader = Shader.Find(SkyProfile.DefaultShaderName);

      EditorUtility.SetDirty(material);
      AssetDatabase.SaveAssets();

      return true;
    }

    // Update sky studio maerials to use to correct shader depending on Unity versions and features.
    static void CheckAndRepairSkyStudioMaterial(string assetPath) {
      Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

      if (material == null) {
        return;
      }

      if (!IsLegacySkyboxMaterial(material)) {
        return;
      }
      
      if (!UpgradeLegacySkyboxMaterial(material)) {
        Debug.LogWarning("Failed to upgrade legacy skybox material at path: " + assetPath);
        return;
      }
    }
  }
}

