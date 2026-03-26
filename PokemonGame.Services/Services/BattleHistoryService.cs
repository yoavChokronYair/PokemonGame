using PokemonGame.Services.Factory;
using PokemonGame.Services.Data.GameData.OnlineBattleData;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryService
    {
        public List<BattleDisplayData> GetBattleHistoryDisplay(int battlePlayerID, string username)
        {
            var displayList = new List<BattleDisplayData>();

            // 1. Get all battles this player participated in
            var records = ServiceFactory.Instance.BattleRepository.GetPlayerBattleHistory(battlePlayerID);

            foreach (var record in records)
            {
                // 2. Get both participants for this battle
                var participants = ServiceFactory.Instance.ParticipantRepository.GetParticipantsForBattle(record.BattleID);

                var playerPart = participants.FirstOrDefault(p => p.BattlePlayerID == battlePlayerID);
                var opponentPart = participants.FirstOrDefault(p => p.BattlePlayerID != battlePlayerID);

                // 3. Construct the display object
                displayList.Add(new BattleDisplayData
                {
                    BattleID = record.BattleID,
                    PlayerName = username,
                    OpponentName = opponentPart?.BattlePlayerID.ToString() ?? "Unknown", // Replace with real name lookup if available
                    IsPlayerWinner = (record.WinnerBattlePlayerID == battlePlayerID),
                    PlayerPokemon = GetPokemonNames(playerPart?.BattlePlayerID), // You'll need to link Team to Participant
                    OpponentPokemon = GetPokemonNames(opponentPart?.BattlePlayerID)
                });
            }
            return displayList;
        }

        private List<string> GetPokemonNames(int? battlePlayerID)
        {
            if (battlePlayerID == null) return new List<string>();

            // Logic: Find the Team linked to this BattlePlayerID, then get members
            var team = ServiceFactory.Instance.TeamRepository.GetTeamByBattlePlayer(battlePlayerID.Value);
            if (team == null) return new List<string>();

            var members = ServiceFactory.Instance.TeamMemberRepository.GetTeamMembers(team.Id);

            // Return Pokedex IDs (or names if you fetch species data)
            return members.Select(m => $"Pokemon #{m.PokemonID}").ToList();
        }
    }
}