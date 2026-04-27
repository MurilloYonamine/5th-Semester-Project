using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Menu {
    public class GameplayService : IGameplayService {
        private readonly ISettingsService _settingsService;

        public GameplayService(ISettingsService settingsService) {
            _settingsService = settingsService;
            ServiceLocator.Register<IGameplayService>(this);
        }

        public float Sensibility {
            get => _settingsService.Sensibility;
            set => _settingsService.Sensibility = value;
        }

        public bool InvertYAxis {
            get => _settingsService.InvertYAxis;
            set => _settingsService.InvertYAxis = value;
        }

        public Language Language {
            get => _settingsService.Language;
            set => _settingsService.Language = value;
        }
    }
}
