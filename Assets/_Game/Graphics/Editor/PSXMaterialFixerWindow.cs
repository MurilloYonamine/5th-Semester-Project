// Autor: Murillo Gomes Yonamine
// Data: 14/08/2026

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FifthSemester.EditorTools.Graphics {
    public class PSXMaterialFixerWindow : EditorWindow {
        private const string PSX_VERTEX_WARPING_SHADER = "PSX/Vertex_Warping";
        private const string INTERNAL_ERROR_SHADER = "Hidden/InternalErrorShader";

        private Vector2 _scrollPos;
        private List<BrokenMaterialInfo> _brokenMaterials = new List<BrokenMaterialInfo>();
        private List<BrokenRendererInfo> _brokenRenderers = new List<BrokenRendererInfo>();

        private bool _includeBuiltinShaders = true;
        private bool _scanSceneRenderers = true;
        private bool _scanPrefabs = true;
        private string _searchFolder = "Assets";
        private Material _defaultReplacementMaterial;

        private struct BrokenMaterialInfo {
            public Material Material;
            public string AssetPath;
            public string ShaderName;
            public string IssueDescription;
        }

        private struct BrokenRendererInfo {
            public GameObject GameObject;
            public string AssetPath;
            public int MissingSlotIndex;
            public string IssueDescription;
        }

        [MenuItem("Tools/PSX/Material & Shader Fixer", false, 100)]
        public static void OpenWindow() {
            var window = GetWindow<PSXMaterialFixerWindow>("PSX Material Fixer");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        [MenuItem("Tools/PSX/Quick Fix All Magenta Materials", false, 101)]
        public static void QuickFixAll() {
            Shader targetShader = Shader.Find(PSX_VERTEX_WARPING_SHADER);
            if (targetShader == null) {
                Debug.LogError($"[PSXMaterialFixer] Target shader '{PSX_VERTEX_WARPING_SHADER}' not found in project!");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            int fixedCount = 0;

            AssetDatabase.StartAssetEditing();
            try {
                for (int i = 0; i < guids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null) continue;

                    if (IsMaterialBroken(mat, true, out _)) {
                        FixSingleMaterial(mat, targetShader);
                        fixedCount++;
                    }
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"<color=green><b>[PSXMaterialFixer]</b> Quick Fix completed: {fixedCount} materials repaired and converted to '{PSX_VERTEX_WARPING_SHADER}'.</color>");
        }

        private void OnGUI() {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("PSX Material & Shader Fixer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Escaneia o projeto em busca de materiais roxos (com shader quebrado, ausente ou Built-in legado) e slots de materiais vazios em Renderers, corrigindo-os para o shader PSX/Vertex_Warping.", MessageType.Info);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Configurações de Varredura", EditorStyles.boldLabel);
            _searchFolder = EditorGUILayout.TextField("Pasta de Busca", _searchFolder);
            _includeBuiltinShaders = EditorGUILayout.Toggle("Incluir Shaders Built-in/Standard", _includeBuiltinShaders);
            _scanPrefabs = EditorGUILayout.Toggle("Escanear Renderers em Prefabs", _scanPrefabs);
            _scanSceneRenderers = EditorGUILayout.Toggle("Escanear Renderers na Cena Aberta", _scanSceneRenderers);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Escanear Projeto", GUILayout.Height(28))) {
                ScanProject();
            }
            if (_brokenMaterials.Count > 0) {
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
                if (GUILayout.Button($"Corrigir Todos ({_brokenMaterials.Count})", GUILayout.Height(28))) {
                    FixAllBrokenMaterials();
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawResultsList();
        }

        private void ScanProject() {
            _brokenMaterials.Clear();
            _brokenRenderers.Clear();

            Shader psxShader = Shader.Find(PSX_VERTEX_WARPING_SHADER);
            if (psxShader == null) {
                EditorUtility.DisplayDialog("Erro", $"Shader '{PSX_VERTEX_WARPING_SHADER}' não encontrado.", "OK");
                return;
            }

            string folder = string.IsNullOrWhiteSpace(_searchFolder) ? "Assets" : _searchFolder;
            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { folder });

            for (int i = 0; i < matGuids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(matGuids[i]);
                EditorUtility.DisplayProgressBar("Escaneando Materiais...", path, (float)i / matGuids.Length);

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                if (IsMaterialBroken(mat, _includeBuiltinShaders, out string reason)) {
                    _brokenMaterials.Add(new BrokenMaterialInfo {
                        Material = mat,
                        AssetPath = path,
                        ShaderName = mat.shader != null ? mat.shader.name : "<NULL>",
                        IssueDescription = reason
                    });
                }
            }

            // Escanear Renderers com slots vazios
            if (_scanPrefabs) {
                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
                for (int i = 0; i < prefabGuids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    EditorUtility.DisplayProgressBar("Escaneando Prefabs...", path, (float)i / prefabGuids.Length);

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;

                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    foreach (var rend in renderers) {
                        Material[] sharedMats = rend.sharedMaterials;
                        for (int s = 0; s < sharedMats.Length; s++) {
                            if (sharedMats[s] == null) {
                                _brokenRenderers.Add(new BrokenRendererInfo {
                                    GameObject = rend.gameObject,
                                    AssetPath = path,
                                    MissingSlotIndex = s,
                                    IssueDescription = "Slot de material vazio (Missing/Null)"
                                });
                            }
                        }
                    }
                }
            }

            if (_scanSceneRenderers) {
                Renderer[] sceneRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var rend in sceneRenderers) {
                    Material[] sharedMats = rend.sharedMaterials;
                    for (int s = 0; s < sharedMats.Length; s++) {
                        if (sharedMats[s] == null) {
                            _brokenRenderers.Add(new BrokenRendererInfo {
                                GameObject = rend.gameObject,
                                AssetPath = "Cena Aberta",
                                MissingSlotIndex = s,
                                IssueDescription = "Slot de material vazio na cena"
                            });
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static bool IsMaterialBroken(Material mat, bool includeBuiltin, out string reason) {
            reason = string.Empty;

            if (mat == null) return false;

            if (mat.shader == null) {
                reason = "Shader é nulo (Missing Shader)";
                return true;
            }

            string shaderName = mat.shader.name;

            if (shaderName == INTERNAL_ERROR_SHADER || shaderName.Contains("InternalErrorShader")) {
                reason = "Shader com erro interno (Hidden/InternalErrorShader - Roxo)";
                return true;
            }

            if (!mat.shader.isSupported) {
                reason = $"Shader '{shaderName}' não é suportado pelo pipeline gráfico atual";
                return true;
            }

            if (includeBuiltin) {
                if (shaderName == "Standard" || 
                    shaderName == "Standard (Specular setup)" || 
                    shaderName.StartsWith("Legacy Shaders/") ||
                    shaderName.StartsWith("Mobile/")) {
                    reason = $"Shader Built-in incompatível com URP ('{shaderName}')";
                    return true;
                }
            }

            return false;
        }

        private static void FixSingleMaterial(Material mat, Shader targetShader) {
            if (mat == null || targetShader == null) return;

            Undo.RecordObject(mat, "Fix Material Shader to PSX");

            // Tenta salvar textura e cor existentes antes de trocar o shader
            Texture mainTex = null;
            if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null) {
                mainTex = mat.GetTexture("_MainTex");
            } else if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null) {
                mainTex = mat.GetTexture("_BaseMap");
            }

            Color mainColor = Color.white;
            if (mat.HasProperty("_Color")) {
                mainColor = mat.GetColor("_Color");
            } else if (mat.HasProperty("_BaseColor")) {
                mainColor = mat.GetColor("_BaseColor");
            }

            // Aplica o novo shader
            mat.shader = targetShader;

            // Restaura as propriedades no shader PSX
            if (mainTex != null) {
                mat.SetTexture("_MainTex", mainTex);
                mat.SetTexture("_BaseMap", mainTex);
            }
            mat.SetColor("_Color", mainColor);
            mat.SetColor("_BaseColor", mainColor);

            // Garante valores padrão apropriados para o PSX Vertex Warping
            if (mat.HasProperty("_SnapResolution") && mat.GetFloat("_SnapResolution") <= 0.001f) {
                mat.SetFloat("_SnapResolution", 240f);
            }
            if (mat.HasProperty("_JitterIntensity") && mat.GetFloat("_JitterIntensity") <= 0.001f) {
                mat.SetFloat("_JitterIntensity", 1f);
            }
            if (mat.HasProperty("_AffineStrength") && mat.GetFloat("_AffineStrength") <= 0.001f) {
                mat.SetFloat("_AffineStrength", 1f);
            }

            EditorUtility.SetDirty(mat);
        }

        private void FixAllBrokenMaterials() {
            Shader targetShader = Shader.Find(PSX_VERTEX_WARPING_SHADER);
            if (targetShader == null) {
                EditorUtility.DisplayDialog("Erro", $"Shader '{PSX_VERTEX_WARPING_SHADER}' não encontrado.", "OK");
                return;
            }

            int count = _brokenMaterials.Count;
            if (!EditorUtility.DisplayDialog("Confirmar Correção", $"Deseja corrigir {count} materiais encontrados para '{PSX_VERTEX_WARPING_SHADER}'?", "Sim, Corrigir", "Cancelar")) {
                return;
            }

            AssetDatabase.StartAssetEditing();
            try {
                for (int i = 0; i < count; i++) {
                    var info = _brokenMaterials[i];
                    EditorUtility.DisplayProgressBar("Corrigindo Materiais...", info.AssetPath, (float)i / count);
                    FixSingleMaterial(info.Material, targetShader);
                }
            } finally {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"<color=green><b>[PSXMaterialFixer]</b> {count} materiais foram corrigidos com sucesso para '{PSX_VERTEX_WARPING_SHADER}'.</color>");
            ScanProject();
        }

        private void DrawResultsList() {
            EditorGUILayout.LabelField($"Materiais com Problemas Encontrados: {_brokenMaterials.Count}", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_brokenMaterials.Count == 0 && _brokenRenderers.Count == 0) {
                EditorGUILayout.HelpBox("Nenhum material quebrado ou slot vazio detectado no escopo configurado.", MessageType.None);
            }

            for (int i = 0; i < _brokenMaterials.Count; i++) {
                var info = _brokenMaterials[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(info.Material != null ? info.Material.name : "<Missing>", EditorStyles.boldLabel, GUILayout.Width(200));
                EditorGUILayout.LabelField(info.IssueDescription, EditorStyles.miniLabel);

                if (GUILayout.Button("Ping", GUILayout.Width(50))) {
                    EditorGUIUtility.PingObject(info.Material);
                    Selection.activeObject = info.Material;
                }

                if (GUILayout.Button("Corrigir", GUILayout.Width(65))) {
                    Shader targetShader = Shader.Find(PSX_VERTEX_WARPING_SHADER);
                    FixSingleMaterial(info.Material, targetShader);
                    AssetDatabase.SaveAssets();
                    _brokenMaterials.RemoveAt(i);
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(info.AssetPath, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }

            if (_brokenRenderers.Count > 0) {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Renderers com Slots de Material Vazios (Null): {_brokenRenderers.Count}", EditorStyles.boldLabel);

                for (int i = 0; i < _brokenRenderers.Count; i++) {
                    var info = _brokenRenderers[i];

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(info.GameObject != null ? info.GameObject.name : "<Missing GO>", EditorStyles.boldLabel, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Slot [{info.MissingSlotIndex}] - {info.IssueDescription}", EditorStyles.miniLabel);

                    if (GUILayout.Button("Ping", GUILayout.Width(50))) {
                        EditorGUIUtility.PingObject(info.GameObject);
                        Selection.activeObject = info.GameObject;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField(info.AssetPath, EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
