using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.GameCommands;

namespace KunalsDiscordBot.Services.Games
{
    public class GameService : IGameService
    {
        public List<BattleShip> BattleShipMatches { get; private set; } = new List<BattleShip>();
        public List<UNOGame> UNOMatches { get; private set; } = new List<UNOGame>();

        public Dictionary<ulong, TicTacToe> TicTacToeMatches { get; private set; } = new Dictionary<ulong, TicTacToe>();
        public Dictionary<ulong, ConnectFour> Connect4Matches { get; private set; } = new Dictionary<ulong, ConnectFour>();

        public GameService()
        {

        }

        public Task<Game> StartGame<T>(ulong guildid, List<DiscordMember> players, DiscordClient client) where T : Game
        {
            switch(typeof(T))
            {
                case var x when x == typeof(BattleShip):
                    var battleShipMatch = new BattleShip(client, players);

                    BattleShipMatches.Add(battleShipMatch);
                    battleShipMatch.OnGameOver += () => BattleShipMatches.Remove(battleShipMatch);

                    return Task.FromResult((Game)battleShipMatch);
                case var x when x == typeof(UNOGame):
                    var unoMatch = new UNOGame(client, players);

                    UNOMatches.Add(unoMatch);
                    unoMatch.OnGameOver += () => UNOMatches.Remove(unoMatch);

                    return Task.FromResult((Game)unoMatch);
                default:
                    return Task.FromResult<Game>(null);
            }
        }

        public async Task<Game> StartGame<T>(ulong guildid, List<DiscordMember> players, DiscordClient client, DiscordChannel channel, int cellCount) where T : Game
        {
            switch(typeof(T))
            {
                case var x when x == typeof(ConnectFour):
                    if (await GetServerGame<T>(guildid) != null)
                        return null;

                    var connect = new ConnectFour(client, players, channel, cellCount);

                    Connect4Matches.Add(guildid, connect);
                    connect.OnGameOver += () => Connect4Matches.Remove(guildid);
                    return connect;

                case var x when x == typeof(TicTacToe):
                    if (await GetServerGame<T>(guildid) != null)
                        return null;

                    var tictactoe = new TicTacToe(client, players, channel);

                    TicTacToeMatches.Add(guildid, tictactoe);
                    tictactoe.OnGameOver += () => TicTacToeMatches.Remove(guildid);
                    return tictactoe;
                default:
                    return null;
            }
        }
            
        public async Task<bool> AddSpectator<T>(ulong id, DiscordMember specator) where T : ISpectatorGame
        {
            switch (typeof(T))
            {
                case var x when x == typeof(BattleShip):
                    var battleShipMatch = BattleShipMatches.FirstOrDefault(x => x.Players.FirstOrDefault(x => x.member.Id == id) != null);
                    if (battleShipMatch == null)
                        return false;

                    return await battleShipMatch.AddSpectator(specator); 
                case var x when x == typeof(UNOGame):
                    var unoMatch = UNOMatches.FirstOrDefault(x => x.Players.FirstOrDefault(x => x.member.Id == id) != null);
                    if (unoMatch == null)
                        return false;

                    return await unoMatch.AddSpectator(specator);
                default:
                    return false;
            }
        }

        public Task<Game> GetServerGame<T>(ulong id) where T : Game
        {
            switch(typeof(T))
            {
                case var x when x == typeof(ConnectFour):
                    if (!Connect4Matches.ContainsKey(id))
                        return Task.FromResult<Game>(null);

                    return Task.FromResult((Game)Connect4Matches[id]);
                case var x when x == typeof(TicTacToe):
                    if (!TicTacToeMatches.ContainsKey(id))
                        return Task.FromResult<Game>(null);

                    return Task.FromResult((Game)TicTacToeMatches[id]);
                default:
                    return Task.FromResult<Game>(null);
            }
        }


        public Task<Game> GetDMGame<T>(ulong playerId) where T : Game => typeof(T) switch
        {
            var x when x == typeof(UNOGame) => Task.FromResult((Game)UNOMatches.FirstOrDefault(x => x.Players.FirstOrDefault(x => x.member.Id == playerId) != null)),
            var x when x == typeof(BattleShip) => Task.FromResult((Game)BattleShipMatches.FirstOrDefault(x => x.Players.FirstOrDefault(x => x.member.Id == playerId) != null)),
            _ => null
        };
    }
}
