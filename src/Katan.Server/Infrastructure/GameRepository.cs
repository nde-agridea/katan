namespace Katan.Server.Infrastructure;

using Katan.Server.Domain.GameFlow;

/// <summary>In-memory repository for active games (one game per lobby for now).</summary>
public class GameRepository
{
    private readonly Dictionary<string, Game> _games = new();

    public void Save(Game game) => _games[game.Id.ToString()] = game;

    public Game Get(string gameId) =>
        _games.TryGetValue(gameId, out var game)
            ? game
            : throw new InvalidOperationException($"Game '{gameId}' not found.");

    public bool TryGet(string gameId, out Game? game) => _games.TryGetValue(gameId, out game);
}
