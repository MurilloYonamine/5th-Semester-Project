using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Menu {
    public class GameplayService : IGameplayService {
        private readonly ISettingsService _settingsService;

        public GameplayService(ISettingsService settingsService) {
            _settingsService = settingsService ?? ServiceLocator.Get<ISettingsService>();
            ServiceLocator.Register<IGameplayService>(this);
        }

        public float Sensibility {
            get => _settingsService != null ? _settingsService.Sensibility : 1f;
            set {
                if (_settingsService != null) {
                    _settingsService.Sensibility = value;
                }
            }
        }

        public bool InvertYAxis {
            get => _settingsService != null && _settingsService.InvertYAxis;
            set {
                if (_settingsService != null) {
                    _settingsService.InvertYAxis = value;
                }
            }
        }

        public Language Language {
            get => _settingsService != null ? _settingsService.Language : Language.English;
            set {
                if (_settingsService != null) {
                    _settingsService.Language = value;
                }
            }
        }
    }
}
