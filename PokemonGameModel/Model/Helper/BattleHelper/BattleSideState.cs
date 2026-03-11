using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleSideState
    {
        // ── Side State (screens, hazards, etc.) ───────────────────────────────────
        private readonly Dictionary<Screen, int> _screens = new();
        private readonly Dictionary<Hazard, int> _hazards = new();
        public bool IsSafeguardActive { get; private set; }
        public int SafeguardTurns { get; private set; }
        public bool IsMistActive { get; private set; }
        public int MistTurns { get; private set; }

        public void ActivateScreen(Screen screen, int turns) => _screens[screen] = turns;
        public bool IsScreenActive(Screen screen) => _screens.TryGetValue(screen, out int t) && t > 0;

        public void AddHazard(Hazard hazard)
        {
            _hazards.TryGetValue(hazard, out int layers);
            _hazards[hazard] = layers + 1;
        }

        public int GetHazardLayers(Hazard hazard) => _hazards.TryGetValue(hazard, out int layers) ? layers : 0;
        public void RemoveHazard(Hazard hazard) => _hazards.Remove(hazard);
        public void ActivateSafeguard(int turns = 5) { IsSafeguardActive = true; SafeguardTurns = turns; }
        public void ActivateMist(int turns = 5) { IsMistActive = true; MistTurns = turns; }

        public void Tick()
        {
            foreach (var key in _screens.Keys.ToList())
            {
                _screens[key]--;
                if (_screens[key] <= 0)
                {
                    _screens.Remove(key);
                }
            }
            if (IsSafeguardActive && --SafeguardTurns <= 0)
            {
                IsSafeguardActive = false;
            }

            if (IsMistActive && --MistTurns <= 0)
            {
                IsMistActive = false;
            }
        }
    }

}
