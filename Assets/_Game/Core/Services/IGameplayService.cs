using FifthSemester.Core.Enums;

namespace FifthSemester.Core.Services {
    public interface IGameplayService {
        float Sensibility { get; set; }
        bool InvertYAxis { get; set; }
        Language Language { get; set; }

    }
}
