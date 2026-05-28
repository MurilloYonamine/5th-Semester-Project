using System.Collections.Generic;
using FifthSemester.Gameplay.Map2;

namespace FifthSemester.Gameplay.Map2 {
    public interface IMap2KeyService {
        void RegisterKey(Map2KeyItem key);
        void UnregisterKey(Map2KeyItem key);
    }
}
