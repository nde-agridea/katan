namespace Katan.Server.Infrastructure;

using Grpc.Core;
using Katan.Server.Domain.Board;
using Katan.Server.Domain.Development;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Scoring;
using Katan.Server.Domain.Structures;
using Katan.Shared.Protos;

public class GameServiceImpl : GameService.GameServiceBase
{
    private readonly GameRepository _repository;

    public GameServiceImpl(GameRepository repository)
    {
        _repository = repository;
    }

    public override Task<GameState> CreateGame(CreateGameRequest request, ServerCallContext context)
    {
        var game = new Game();
        foreach (var name in request.PlayerNames)
            game.AddPlayer(new Player(Guid.NewGuid().ToString(), name));

        var generator = new MapGenerator();
        var board = generator.GenerateMap();
        game.SetBoard(board);
        game.SetGamePhase(GamePhase.MapVoting);
        game.InitializeVoting();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> VoteMap(VoteMapRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var session = game.VotingSession ?? throw new InvalidOperationException("No voting session active.");
        session.CastVote(request.PlayerId, request.Approve);

        if (session.IsComplete)
        {
            if (session.NeedsRegeneration)
            {
                var generator = new MapGenerator();
                game.SetBoard(generator.GenerateMap());
                game.StartNewVotingRound(session.NextGenerationAttempt);
            }
            else
            {
                game.StartGame(game.Players[0].Id);
                var placement = new InitialPlacementService(game);
                placement.BuildPlacementOrder();
            }
        }

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlaceInitialSettlement(PlaceInitialSettlementRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var placement = new InitialPlacementService(game);
        placement.PlaceSettlement(request.PlayerId, GameStateMapper.ToIntersection(request.Location));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlaceInitialRoad(PlaceInitialRoadRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var placement = new InitialPlacementService(game);
        placement.PlaceRoad(request.PlayerId, GameStateMapper.ToEdge(request.Location));
        placement.AdvancePlacement();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> RollDice(RollDiceRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var sm = new TurnStateMachine(game);
        sm.RollDice();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> DiscardResources(DiscardResourcesRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var player = game.Players.First(p => p.Id == request.PlayerId);
        var discards = request.Discards.ToDictionary(
            d => GameStateMapper.ToResourceType(d.Type),
            d => d.Amount);
        player.Hand.Pay(discards);
        game.AddEvent(new Domain.Resources.Events.ResourcesDiscarded(request.PlayerId, discards));

        var sm = new TurnStateMachine(game);
        sm.FinishDiscard();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> MoveRobber(MoveRobberRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var pos = GameStateMapper.ToTilePosition(request.Position);
        game.Board.PlaceRobber(pos);
        game.AddEvent(new Domain.Board.Events.RobberMoved(request.PlayerId, pos));

        if (!string.IsNullOrEmpty(request.StealFromPlayerId))
        {
            var robberSvc = new RobberService(game);
            robberSvc.StealFromOpponent(request.PlayerId, request.StealFromPlayerId);
        }

        var sm = new TurnStateMachine(game);
        sm.FinishRobberMovement();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PayTribute(PayTributeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var robberSvc = new RobberService(game);
        robberSvc.PayTribute(request.PlayerId, GameStateMapper.ToResourceType(request.Resource));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> BankTrade(BankTradeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var tradeSvc = new TradeService(game);
        tradeSvc.BankTrade(request.PlayerId,
            GameStateMapper.ToResourceType(request.Give),
            request.Amount,
            GameStateMapper.ToResourceType(request.Receive));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PortTrade(PortTradeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var tradeSvc = new TradeService(game);
        tradeSvc.PortTrade(request.PlayerId,
            GameStateMapper.ToResourceType(request.Give),
            request.Amount,
            GameStateMapper.ToResourceType(request.Receive));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> OfferTrade(OfferTradeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var tradeSvc = new TradeService(game);
        tradeSvc.OfferTrade(
            request.OfferingPlayerId,
            request.TargetPlayerId,
            request.Offering.ToDictionary(r => GameStateMapper.ToResourceType(r.Type), r => r.Amount),
            request.Requesting.ToDictionary(r => GameStateMapper.ToResourceType(r.Type), r => r.Amount));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> RespondTrade(RespondTradeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var tradeSvc = new TradeService(game);
        if (request.Accept)
            tradeSvc.AcceptTrade(request.OfferingPlayerId, request.RespondingPlayerId);
        else
            tradeSvc.DeclineTrade(request.OfferingPlayerId, request.RespondingPlayerId);

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> EndTrade(EndTradeRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var sm = new TurnStateMachine(game);
        sm.EndTrade();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> BuildRoad(BuildRoadRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var buildSvc = new BuildService(game);
        buildSvc.BuildRoad(request.PlayerId, GameStateMapper.ToEdge(request.Location));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> BuildSettlement(BuildSettlementRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var buildSvc = new BuildService(game);
        buildSvc.BuildSettlement(request.PlayerId, GameStateMapper.ToIntersection(request.Location));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> UpgradeCity(UpgradeCityRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var buildSvc = new BuildService(game);
        buildSvc.UpgradeCity(request.PlayerId, GameStateMapper.ToIntersection(request.Location));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> BuildArmy(BuildArmyRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var buildSvc = new BuildService(game);
        buildSvc.BuildArmy(request.PlayerId, GameStateMapper.ToTilePosition(request.Position));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> MoveArmy(MoveArmyRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var player = game.Players.First(p => p.Id == request.PlayerId);
        var from = GameStateMapper.ToTilePosition(request.From);
        var to = GameStateMapper.ToTilePosition(request.To);
        var army = player.Armies.First(a => a.Position == from);

        var militarySvc = new MilitaryService(game);
        militarySvc.MoveArmy(request.PlayerId, army, to);

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> Attack(AttackRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var militarySvc = new MilitaryService(game);
        militarySvc.Attack(request.PlayerId,
            GameStateMapper.ToTilePosition(request.FromTile),
            GameStateMapper.ToTilePosition(request.TargetTile));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> BuyDevelopmentCard(BuyDevelopmentCardRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PurchaseAndPlay(request.PlayerId);

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayKnight(PlayKnightRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayKnight(request.PlayerId,
            GameStateMapper.ToTilePosition(request.NewRobberPosition),
            string.IsNullOrEmpty(request.StealFromPlayerId) ? null : request.StealFromPlayerId);

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayVictoryPoint(PlayVictoryPointRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayVictoryPoint(request.PlayerId);

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayRoadBuilding(PlayRoadBuildingRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayRoadBuilding(request.PlayerId, GameStateMapper.ToEdge(request.RoadLocation));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayMonopoly(PlayMonopolyRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayMonopoly(request.PlayerId, GameStateMapper.ToResourceType(request.Resource));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayExcess(PlayExcessRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayExcess(request.PlayerId,
            GameStateMapper.ToResourceType(request.Resource1),
            GameStateMapper.ToResourceType(request.Resource2));

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> PlayDisaster(PlayDisasterRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var devSvc = new DevelopmentCardService(game);
        devSvc.PlayDisaster(request.PlayerId, GameStateMapper.ToTilePosition(request.Tile));

        var scoring = new ScoringService(game);
        scoring.CheckWinCondition();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> EndBuild(EndBuildRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var sm = new TurnStateMachine(game);
        sm.EndBuild();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }

    public override Task<GameState> EndTurn(EndTurnRequest request, ServerCallContext context)
    {
        var game = _repository.Get(request.GameId);
        var sm = new TurnStateMachine(game);
        sm.EndTurn();

        _repository.Save(game);
        return Task.FromResult(GameStateMapper.Map(game));
    }
}
