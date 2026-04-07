using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MaterialNameReplacerWindow : EditorWindow
{
    private const string DefaultMaterialsFolder = "Assets/Thirdparty/Tacos/Tacos/Materials";

    private DefaultAsset materialsFolder;
    private bool includeInactive = true;

    [MenuItem("Tools/Materials/Replace Materials By Name")]
    private static void OpenWindow()
    {
        GetWindow<MaterialNameReplacerWindow>("Material Replacer");
    }

    private void OnEnable()
    {
        if (materialsFolder == null)
        {
            materialsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultMaterialsFolder);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Replace Scene Materials By Name", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Cada material usado pelos Renderers sera trocado por um material da pasta selecionada quando os nomes coincidirem.",
            MessageType.Info);

        materialsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Materials Folder",
            materialsFolder,
            typeof(DefaultAsset),
            false);

        includeInactive = EditorGUILayout.Toggle("Include Inactive", includeInactive);

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Replace In Selection"))
            {
                ReplaceInSelection();
            }
        }

        if (GUILayout.Button("Replace In Active Scene"))
        {
            ReplaceInActiveScene();
        }
    }

    private void ReplaceInSelection()
    {
        if (!TryBuildMaterialLookup(out var materialLookup, out var folderPath))
        {
            return;
        }

        var renderers = new HashSet<Renderer>();
        foreach (var gameObject in Selection.gameObjects)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(includeInactive))
            {
                renderers.Add(renderer);
            }
        }

        var result = ReplaceMaterials(renderers, materialLookup);
        ShowResultDialog("selection", folderPath, result);
    }

    private void ReplaceInActiveScene()
    {
        if (!TryBuildMaterialLookup(out var materialLookup, out var folderPath))
        {
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            EditorUtility.DisplayDialog("Material Replacer", "Nao foi possivel acessar a cena ativa.", "OK");
            return;
        }

        var renderers = new List<Renderer>();
        foreach (var root in activeScene.GetRootGameObjects())
        {
            renderers.AddRange(root.GetComponentsInChildren<Renderer>(includeInactive));
        }

        var result = ReplaceMaterials(renderers, materialLookup);
        ShowResultDialog("active scene", folderPath, result);
    }

    private bool TryBuildMaterialLookup(out Dictionary<string, Material> materialLookup, out string folderPath)
    {
        materialLookup = null;
        folderPath = null;

        if (materialsFolder == null)
        {
            EditorUtility.DisplayDialog("Material Replacer", "Selecione a pasta que contem os materiais.", "OK");
            return false;
        }

        folderPath = AssetDatabase.GetAssetPath(materialsFolder);
        if (string.IsNullOrWhiteSpace(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            EditorUtility.DisplayDialog("Material Replacer", "O caminho selecionado nao e uma pasta valida do projeto.", "OK");
            return false;
        }

        var guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Material Replacer", "Nenhum material foi encontrado na pasta selecionada.", "OK");
            return false;
        }

        materialLookup = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        foreach (var guid in guids)
        {
            var materialPath = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                continue;
            }

            if (!materialLookup.ContainsKey(material.name))
            {
                materialLookup.Add(material.name, material);
            }
            else
            {
                Debug.LogWarning($"Material Replacer ignored duplicate material name '{material.name}' at '{materialPath}'.");
            }
        }

        return true;
    }

    private static ReplacementResult ReplaceMaterials(IEnumerable<Renderer> renderers, IReadOnlyDictionary<string, Material> materialLookup)
    {
        var result = new ReplacementResult();

        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            result.RenderersScanned++;

            var sharedMaterials = renderer.sharedMaterials;
            var changed = false;

            for (var index = 0; index < sharedMaterials.Length; index++)
            {
                var currentMaterial = sharedMaterials[index];
                if (currentMaterial == null)
                {
                    continue;
                }

                var currentName = NormalizeMaterialName(currentMaterial.name);
                if (!materialLookup.TryGetValue(currentName, out var replacementMaterial))
                {
                    continue;
                }

                if (sharedMaterials[index] == replacementMaterial)
                {
                    continue;
                }

                sharedMaterials[index] = replacementMaterial;
                changed = true;
                result.MaterialSlotsReplaced++;
            }

            if (!changed)
            {
                continue;
            }

            Undo.RecordObject(renderer, "Replace Materials By Name");
            renderer.sharedMaterials = sharedMaterials;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            EditorUtility.SetDirty(renderer);
            EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
            result.RenderersChanged++;
        }

        return result;
    }

    private static string NormalizeMaterialName(string materialName)
    {
        const string instanceSuffix = " (Instance)";
        if (materialName.EndsWith(instanceSuffix, StringComparison.Ordinal))
        {
            return materialName.Substring(0, materialName.Length - instanceSuffix.Length);
        }

        return materialName;
    }

    private static void ShowResultDialog(string scopeName, string folderPath, ReplacementResult result)
    {
        EditorUtility.DisplayDialog(
            "Material Replacer",
            $"Source folder: {folderPath}\nScope: {scopeName}\nRenderers scanned: {result.RenderersScanned}\nRenderers changed: {result.RenderersChanged}\nMaterial slots replaced: {result.MaterialSlotsReplaced}",
            "OK");
    }

    private struct ReplacementResult
    {
        public int RenderersScanned;
        public int RenderersChanged;
        public int MaterialSlotsReplaced;
    }
}