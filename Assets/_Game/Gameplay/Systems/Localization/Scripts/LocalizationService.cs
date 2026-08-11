// Autor: Murillo Gomes Yonamine
// Data: 17/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class LocalizationService : MonoBehaviour, ILocalizationService {
        private const string TAG = "<color=magenta>[LocalizationService]</color>";
        private const string _csvPath = "Assets/_Game/Data/Localization/LocalizedText.csv";

        [Header("Ficheiro de Textos")]
        [Tooltip("Arraste o ficheiro LocalizedText.csv aqui")]
        [SerializeField] private TextAsset _csvFile;

        [Header("Configuração")]
        [SerializeField] private Language _defaultLanguage = Language.Portuguese;

        private Dictionary<string, string> _localizedTexts = new Dictionary<string, string>();
        private IEventBus _eventBus;
        private Language? _currentLanguage = null;

        private void Awake() {
            ServiceLocator.Register<ILocalizationService>(this);
        }

        private void Start() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            var settingsService = ServiceLocator.Get<ISettingsService>();

            if (settingsService != null) {
                SetLanguage(settingsService.Language);
            }
            else {
                SetLanguage(_defaultLanguage);
            }
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<ILocalizationService>();
        }

        public void SetLanguage(Language language) {
            if (_currentLanguage == language) return;

            _currentLanguage = language;

            string langCode = GetLangCode(language);
            LoadLanguageFromCSV(langCode);
        }

        public string GetText(string key) {
            if (_localizedTexts.TryGetValue(key, out string translatedText)) {
                return translatedText;
            }
            Debug.LogWarning($"{TAG} Chave não encontrada: {key}");
            return $"[{key}]"; 
        }

        private void LoadLanguageFromCSV(string targetLanguage) {
            _localizedTexts.Clear();

            if (_csvFile == null) {
                Debug.LogError($"{TAG} Ficheiro CSV não atribuído no Inspector!");
                return;
            }

            string[] rows = _csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (rows.Length == 0) return;

            string[] headers = rows[0].Split(',');
            int languageIndex = -1;

            for (int i = 1; i < headers.Length; i++) {
                if (headers[i].Trim() == targetLanguage) {
                    languageIndex = i;
                    break;
                }
            }

            if (languageIndex == -1) {
                Debug.LogError($"{TAG} Idioma '{targetLanguage}' não encontrado no cabeçalho do CSV!");
                return;
            }

            for (int i = 1; i < rows.Length; i++) {
                string[] columns = rows[i].Split(',');

                if (columns.Length > languageIndex) {
                    string key = columns[0].Trim();
                    string value = columns[languageIndex].Trim();

                    if (!string.IsNullOrEmpty(key)) {
                        _localizedTexts[key] = value;
                    }
                }
            }

            Debug.Log($"{TAG} Idioma carregado com sucesso: {targetLanguage} ({_localizedTexts.Count} textos).");
        }
        private string GetLangCode(Language lang) {
            return lang switch {
                Language.Portuguese => "pt-BR",
                Language.English => "en-US",
                _ => "pt-BR"
            };
        }
    }
}
