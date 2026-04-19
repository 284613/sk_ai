using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rewards;
using SkAiRouteAdvisor.DecisionLogging;
using System.Collections;
using System.Reflection;

namespace SkAiRouteAdvisor.RouteAdvisor;

internal sealed class RouteAdvisorRuntime
{
    private static readonly FieldInfo? CardRewardOptionsField =
        typeof(NCardRewardSelectionScreen).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? ChooseRelicOptionsField =
        typeof(NChooseARelicSelection).GetField("_relics", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? RelicHoldersInUseField =
        typeof(NTreasureRoomRelicCollection).GetField("_holdersInUse", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? RewardButtonsField =
        typeof(NRewardsScreen).GetField("_rewardButtons", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? RelicRewardRelicField =
        typeof(RelicReward).GetField("_relic", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo? RelicRewardPredeterminedField =
        typeof(RelicReward).GetField("_predeterminedRelic", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly RouteRecommendationService _service = new();
    private readonly RouteOverlayPresenter _overlayPresenter = new();
    private readonly DecisionLogService _decisionLogService = new();

    private RunState? _runState;
    private NMapScreen? _subscribedMapScreen;
    private Player? _subscribedPlayer;
    private Creature? _subscribedCreature;
    private CardPile? _subscribedDeck;
    private SceneTree? _sceneTree;
    private RouteMode _currentMode = RouteMode.Balanced;
    private bool _modeHotkeyHeld;

    private RouteDecisionLogToken? _activeRouteDecision;
    private string? _lastRouteRecommendationFingerprint;

    private CardDecisionLogToken? _activeCardDecision;
    private string? _lastCardRecommendationFingerprint;
    private int _cardScreenHiddenFrames;

    private RelicDecisionLogToken? _activeRelicDecision;
    private string? _lastRelicRecommendationFingerprint;
    private readonly List<NRewardButton> _subscribedRelicRewardButtons = [];

    public void Initialize()
    {
        var manager = RunManager.Instance;
        manager.RunStarted += OnRunStarted;
        manager.ActEntered += OnActEntered;
        manager.RoomEntered += OnRoomEntered;
        manager.RoomExited += OnRoomExited;

        EnsureMapScreenSubscription();
        EnsurePlayerSubscriptions();

        _sceneTree = (SceneTree)Engine.GetMainLoop();
        _sceneTree.ProcessFrame += OnProcessFrame;

        Log.Info("[SkAiRouteAdvisor] subscribed to run, map, and decision logging events");
    }

    public void Shutdown()
    {
        var manager = RunManager.Instance;
        manager.RunStarted -= OnRunStarted;
        manager.ActEntered -= OnActEntered;
        manager.RoomEntered -= OnRoomEntered;
        manager.RoomExited -= OnRoomExited;

        UnsubscribePlayerEvents();

        if (_sceneTree != null)
        {
            _sceneTree.ProcessFrame -= OnProcessFrame;
            _sceneTree = null;
        }

        if (_subscribedMapScreen != null)
        {
            _subscribedMapScreen.Opened -= OnMapScreenOpened;
            _subscribedMapScreen = null;
        }

        UnsubscribeRelicRewardButtons();
    }

    private void OnRunStarted(RunState runState)
    {
        _runState = runState;
        EnsurePlayerSubscriptions();
        RefreshRecommendations("run_started");
    }

    private void OnActEntered()
    {
        RefreshRecommendations("act_entered");
    }

    private void OnRoomEntered()
    {
        TryLogRouteChoiceResolution();
        RefreshRecommendations("room_entered");
    }

    private void OnRoomExited()
    {
        RefreshRecommendations("room_exited");
    }

    private void OnMapScreenOpened()
    {
        RefreshRecommendations("map_opened");
    }

    private void OnGoldChanged()
    {
        RefreshRecommendations("gold_changed");
    }

    private void OnCurrentHpChanged(int _, int __)
    {
        RefreshRecommendations("hp_changed");
    }

    private void OnMaxHpChanged(int _, int __)
    {
        RefreshRecommendations("max_hp_changed");
    }

    private void OnDeckCardAdded(CardModel card)
    {
        if (_runState == null || _activeCardDecision == null)
        {
            return;
        }

        _decisionLogService.LogCardRewardChoice(_runState, _activeCardDecision, card, skipped: false, choiceSource: "player");
        Log.Info($"[SkAiRouteAdvisor] card reward actual choice logged card={DecisionLogValueFormatter.FormatModelId(card.Id)}");
        _activeCardDecision = null;
        _lastCardRecommendationFingerprint = null;
        _cardScreenHiddenFrames = 0;
    }

    private void OnRelicObtained(RelicModel relic)
    {
        if (_runState == null)
        {
            return;
        }

        if (_activeRelicDecision == null)
        {
            _activeRelicDecision = _decisionLogService.LogRelicChoiceRecommendation(
                _runState,
                [relic],
                recommendedOptionId: DecisionLogValueFormatter.FormatModelId(relic.Id),
                modeTag: _currentMode.ToString().ToLowerInvariant(),
                scorerVersion: null
            );
            Log.Warn($"[SkAiRouteAdvisor] relic recommendation fallback generated for obtained relic={DecisionLogValueFormatter.FormatModelId(relic.Id)}");
        }

        _decisionLogService.LogRelicChoice(_runState, _activeRelicDecision, relic, "player");
        Log.Info($"[SkAiRouteAdvisor] relic actual choice logged relic={DecisionLogValueFormatter.FormatModelId(relic.Id)}");
        _activeRelicDecision = null;
        _lastRelicRecommendationFingerprint = null;
    }

    private void OnRelicRewardClaimed(NRewardButton button)
    {
        if (_runState == null || _activeRelicDecision == null || button.Reward is not RelicReward relicReward)
        {
            return;
        }

        var chosenRelic = ExtractRelicModel(relicReward);
        if (chosenRelic == null)
        {
            return;
        }

        _decisionLogService.LogRelicChoice(_runState, _activeRelicDecision, chosenRelic, "player");
        Log.Info($"[SkAiRouteAdvisor] relic actual choice logged via reward button relic={DecisionLogValueFormatter.FormatModelId(chosenRelic.Id)}");
        _activeRelicDecision = null;
        _lastRelicRecommendationFingerprint = null;
    }

    private void OnProcessFrame()
    {
        var hotkeyDown = Input.IsPhysicalKeyPressed(Key.F6);
        if (hotkeyDown && !_modeHotkeyHeld)
        {
            CycleMode();
        }

        _modeHotkeyHeld = hotkeyDown;
        MonitorDecisionScreens();
    }

    private void RefreshRecommendations(string reason)
    {
        EnsureMapScreenSubscription();
        EnsurePlayerSubscriptions();

        if (_runState == null)
        {
            return;
        }

        var summary = _service.Evaluate(_runState, _currentMode);
        if (summary == null)
        {
            Log.Warn($"[SkAiRouteAdvisor] no route summary available on {reason}");
            return;
        }

        LogSummary(summary, reason);

        var mapScreen = NMapScreen.Instance;
        if (mapScreen != null && mapScreen.IsOpen)
        {
            _overlayPresenter.Present(summary);
            TryLogRouteRecommendation(summary, reason);
        }

        LatestSummary = summary;
    }

    private void MonitorDecisionScreens()
    {
        if (_sceneTree?.Root == null || _runState == null)
        {
            return;
        }

        MonitorCardRewardScreen(_sceneTree.Root);
        MonitorRelicChoiceScreen(_sceneTree.Root);
        MonitorRewardsScreen(_sceneTree.Root);
    }

    private void MonitorCardRewardScreen(Node root)
    {
        var screen = FindVisibleNode<NCardRewardSelectionScreen>(root) ?? FindNode<NCardRewardSelectionScreen>(root);
        if (screen == null)
        {
            if (_activeCardDecision != null)
            {
                _cardScreenHiddenFrames++;
                if (_cardScreenHiddenFrames >= 2 && _activeCardDecision.CanSkip)
                {
                    _decisionLogService.LogCardRewardChoice(_runState!, _activeCardDecision, null, skipped: true, choiceSource: "player");
                    Log.Info("[SkAiRouteAdvisor] card reward skip logged");
                    _activeCardDecision = null;
                    _lastCardRecommendationFingerprint = null;
                    _cardScreenHiddenFrames = 0;
                }
            }

            return;
        }

        _cardScreenHiddenFrames = 0;
        var options = GetCardRewardOptions(screen);
        if (options.Count == 0)
        {
            return;
        }

        var fingerprint = string.Join("|", options.Select(card => DecisionLogValueFormatter.FormatModelId(card.Id)).OrderBy(x => x));
        if (fingerprint == _lastCardRecommendationFingerprint)
        {
            return;
        }

        _lastCardRecommendationFingerprint = fingerprint;
        _activeCardDecision = _decisionLogService.LogCardRewardRecommendation(
            _runState!,
            options,
            canSkip: true,
            modeTag: _currentMode.ToString().ToLowerInvariant(),
            scorerVersion: null
        );
        Log.Info($"[SkAiRouteAdvisor] card reward recommendation logged options={options.Count}");
    }

    private void MonitorRelicChoiceScreen(Node root)
    {
        var relicChoices = GetVisibleRelicChoices(root, out var source);
        if (relicChoices.Count == 0)
        {
            return;
        }

        var fingerprint = string.Join("|", relicChoices.Select(relic => DecisionLogValueFormatter.FormatModelId(relic.Id)).OrderBy(x => x));
        if (fingerprint == _lastRelicRecommendationFingerprint)
        {
            return;
        }

        _lastRelicRecommendationFingerprint = fingerprint;
        _activeRelicDecision = _decisionLogService.LogRelicChoiceRecommendation(
            _runState!,
            relicChoices,
            modeTag: _currentMode.ToString().ToLowerInvariant(),
            scorerVersion: null
        );
        Log.Info($"[SkAiRouteAdvisor] relic choice recommendation logged source={source} options={relicChoices.Count}");
    }

    private void MonitorRewardsScreen(Node root)
    {
        var allRewardButtons = FindNodes<NRewardButton>(root)
            .Where(button => button.Reward is RelicReward)
            .ToList();

        SubscribeRelicRewardButtons(allRewardButtons);

        if (allRewardButtons.Count == 0)
        {
            UnsubscribeRelicRewardButtons();
            return;
        }

        var relicChoices = allRewardButtons
            .Select(button => button.Reward as RelicReward)
            .Where(reward => reward != null)
            .Select(reward => ExtractRelicModel(reward!))
            .OfType<RelicModel>()
            .ToList();

        if (relicChoices.Count == 0)
        {
            return;
        }

        var fingerprint = string.Join("|", relicChoices.Select(relic => DecisionLogValueFormatter.FormatModelId(relic.Id)).OrderBy(x => x));
        if (fingerprint == _lastRelicRecommendationFingerprint)
        {
            return;
        }

        _lastRelicRecommendationFingerprint = fingerprint;
        _activeRelicDecision = _decisionLogService.LogRelicChoiceRecommendation(
            _runState!,
            relicChoices,
            modeTag: _currentMode.ToString().ToLowerInvariant(),
            scorerVersion: null
        );
        Log.Info($"[SkAiRouteAdvisor] relic choice recommendation logged source=reward_button_scan options={relicChoices.Count}");
    }

    private void TryLogRouteRecommendation(RouteRecommendationSummary summary, string reason)
    {
        var fingerprint = BuildRouteRecommendationFingerprint(summary);
        if (fingerprint == _lastRouteRecommendationFingerprint)
        {
            return;
        }

        _lastRouteRecommendationFingerprint = fingerprint;
        _activeRouteDecision = _decisionLogService.LogRouteRecommendation(summary, _runState!, "map_screen");
        Log.Info($"[SkAiRouteAdvisor] route recommendation logged reason={reason} session={_decisionLogService.CurrentSessionId}");
    }

    private void TryLogRouteChoiceResolution()
    {
        if (_runState == null || _activeRouteDecision == null || LatestSummary == null)
        {
            return;
        }

        var currentPoint = _runState.CurrentMapPoint;
        if (currentPoint == null)
        {
            return;
        }

        var currentNodeId = DecisionLogValueFormatter.FormatMapPointId(currentPoint);
        if (string.IsNullOrWhiteSpace(currentNodeId) || currentNodeId == _activeRouteDecision.StartNodeId)
        {
            return;
        }

        _decisionLogService.LogRouteChoice(_activeRouteDecision, LatestSummary, _runState, currentPoint, "player");
        Log.Info($"[SkAiRouteAdvisor] route actual choice logged chosen_next_node={currentNodeId}");
        _activeRouteDecision = null;
        _lastRouteRecommendationFingerprint = null;
    }

    private static string BuildRouteRecommendationFingerprint(RouteRecommendationSummary summary)
    {
        var topPath = summary.RankedRoutes.ElementAtOrDefault(0)?.PathId ?? "none";
        var secondPath = summary.RankedRoutes.ElementAtOrDefault(1)?.PathId ?? "none";
        return string.Join("|",
            summary.Mode.ToString(),
            summary.ActIndex,
            summary.ActFloor,
            summary.CurrentHp,
            summary.MaxHp,
            summary.Gold,
            DecisionLogValueFormatter.FormatMapPointId(summary.StartPoint),
            topPath,
            secondPath
        );
    }

    private void CycleMode()
    {
        _currentMode = _currentMode switch
        {
            RouteMode.Safe => RouteMode.Balanced,
            RouteMode.Balanced => RouteMode.Aggressive,
            _ => RouteMode.Safe,
        };

        Log.Info($"[SkAiRouteAdvisor] mode changed to {_currentMode.ToString().ToLowerInvariant()}");
        RefreshRecommendations("mode_changed");
    }

    private void EnsurePlayerSubscriptions()
    {
        var currentPlayer = _runState?.Players.FirstOrDefault();
        var currentCreature = currentPlayer?.Creature;
        var currentDeck = currentPlayer?.Deck;

        if (ReferenceEquals(currentPlayer, _subscribedPlayer) &&
            ReferenceEquals(currentCreature, _subscribedCreature) &&
            ReferenceEquals(currentDeck, _subscribedDeck))
        {
            return;
        }

        UnsubscribePlayerEvents();

        _subscribedPlayer = currentPlayer;
        _subscribedCreature = currentCreature;
        _subscribedDeck = currentDeck;

        if (_subscribedPlayer != null)
        {
            _subscribedPlayer.GoldChanged += OnGoldChanged;
            _subscribedPlayer.RelicObtained += OnRelicObtained;
        }

        if (_subscribedCreature != null)
        {
            _subscribedCreature.CurrentHpChanged += OnCurrentHpChanged;
            _subscribedCreature.MaxHpChanged += OnMaxHpChanged;
        }

        if (_subscribedDeck != null)
        {
            _subscribedDeck.CardAdded += OnDeckCardAdded;
        }
    }

    private void UnsubscribePlayerEvents()
    {
        if (_subscribedPlayer != null)
        {
            _subscribedPlayer.GoldChanged -= OnGoldChanged;
            _subscribedPlayer.RelicObtained -= OnRelicObtained;
            _subscribedPlayer = null;
        }

        if (_subscribedCreature != null)
        {
            _subscribedCreature.CurrentHpChanged -= OnCurrentHpChanged;
            _subscribedCreature.MaxHpChanged -= OnMaxHpChanged;
            _subscribedCreature = null;
        }

        if (_subscribedDeck != null)
        {
            _subscribedDeck.CardAdded -= OnDeckCardAdded;
            _subscribedDeck = null;
        }
    }

    private void EnsureMapScreenSubscription()
    {
        var currentMapScreen = NMapScreen.Instance;
        if (currentMapScreen == null || ReferenceEquals(currentMapScreen, _subscribedMapScreen))
        {
            return;
        }

        if (_subscribedMapScreen != null)
        {
            _subscribedMapScreen.Opened -= OnMapScreenOpened;
        }

        currentMapScreen.Opened += OnMapScreenOpened;
        _subscribedMapScreen = currentMapScreen;
    }

    private static T? FindVisibleNode<T>(Node root) where T : Node
    {
        if (root is T typedNode)
        {
            if (typedNode is CanvasItem canvasItem)
            {
                if (canvasItem.Visible && canvasItem.IsVisibleInTree())
                {
                    return typedNode;
                }
            }
            else
            {
                return typedNode;
            }
        }

        foreach (var child in root.GetChildren())
        {
            if (child is Node node)
            {
                var found = FindVisibleNode<T>(node);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static T? FindNode<T>(Node root) where T : Node
    {
        if (root is T typedNode)
        {
            return typedNode;
        }

        foreach (var child in root.GetChildren())
        {
            if (child is Node node)
            {
                var found = FindNode<T>(node);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<T> FindNodes<T>(Node root) where T : Node
    {
        var results = new List<T>();
        if (root is T typedNode)
        {
            results.Add(typedNode);
        }

        foreach (var child in root.GetChildren())
        {
            if (child is Node node)
            {
                results.AddRange(FindNodes<T>(node));
            }
        }

        return results;
    }

    private static IReadOnlyList<CardModel> GetCardRewardOptions(NCardRewardSelectionScreen screen)
    {
        if (CardRewardOptionsField?.GetValue(screen) is not IReadOnlyList<CardCreationResult> options)
        {
            return [];
        }

        return options
            .Select(option => option.Card)
            .Where(card => card != null)
            .ToList()!;
    }

    private static IReadOnlyList<RelicModel> GetVisibleRelicChoices(Node root, out string source)
    {
        source = "none";

        var chooseRelicScreen = FindVisibleNode<NChooseARelicSelection>(root) ?? FindNode<NChooseARelicSelection>(root);
        if (chooseRelicScreen != null &&
            ChooseRelicOptionsField?.GetValue(chooseRelicScreen) is IReadOnlyList<RelicModel> chooseRelicOptions &&
            chooseRelicOptions.Count > 0)
        {
            source = "choose_a_relic_screen";
            return chooseRelicOptions.Where(model => model != null).ToList()!;
        }

        var relicCollection = FindVisibleNode<NTreasureRoomRelicCollection>(root) ?? FindNode<NTreasureRoomRelicCollection>(root);
        if (relicCollection == null || RelicHoldersInUseField?.GetValue(relicCollection) is not IEnumerable holders)
        {
            // Fall through to rewards screen.
        }
        else
        {
            var results = new List<RelicModel>();
            foreach (var holderObject in holders)
            {
                if (holderObject is NTreasureRoomRelicHolder holder && holder.Relic?.Model != null)
                {
                    results.Add(holder.Relic.Model);
                }
            }

            if (results.Count > 0)
            {
                source = "treasure_room_relic_collection";
                return results;
            }
        }

        var rewardsScreen = FindVisibleNode<NRewardsScreen>(root) ?? FindNode<NRewardsScreen>(root);
        if (rewardsScreen != null &&
            RewardButtonsField?.GetValue(rewardsScreen) is IEnumerable rewardButtons)
        {
            var results = new List<RelicModel>();
            foreach (var rewardButtonObject in rewardButtons)
            {
                if (rewardButtonObject is not NRewardButton rewardButton)
                {
                    continue;
                }

                if (rewardButton.Reward is not RelicReward relicReward)
                {
                    continue;
                }

                var relicModel = ExtractRelicModel(relicReward);

                if (relicModel != null)
                {
                    results.Add(relicModel);
                }
            }

            if (results.Count > 0)
            {
                source = "rewards_screen_relic_reward";
                return results;
            }
        }

        return [];
    }

    private void SubscribeRelicRewardButtons(IReadOnlyList<NRewardButton> buttons)
    {
        var existing = _subscribedRelicRewardButtons.ToHashSet();
        var current = buttons.ToHashSet();

        foreach (var button in _subscribedRelicRewardButtons.Where(button => !current.Contains(button)).ToList())
        {
            button.RewardClaimed -= OnRelicRewardClaimed;
            _subscribedRelicRewardButtons.Remove(button);
        }

        foreach (var button in buttons.Where(button => !existing.Contains(button)))
        {
            button.RewardClaimed += OnRelicRewardClaimed;
            _subscribedRelicRewardButtons.Add(button);
        }
    }

    private void UnsubscribeRelicRewardButtons()
    {
        foreach (var button in _subscribedRelicRewardButtons)
        {
            button.RewardClaimed -= OnRelicRewardClaimed;
        }

        _subscribedRelicRewardButtons.Clear();
    }

    private static RelicModel? ExtractRelicModel(RelicReward relicReward)
    {
        return relicReward.ClaimedRelic ??
               RelicRewardRelicField?.GetValue(relicReward) as RelicModel ??
               RelicRewardPredeterminedField?.GetValue(relicReward) as RelicModel;
    }

    private static void LogSummary(RouteRecommendationSummary summary, string reason)
    {
        Log.Info(
            $"[SkAiRouteAdvisor] refresh={reason} act={summary.ActIndex} floor={summary.ActFloor}/{summary.TotalFloor} " +
            $"hp={summary.CurrentHp}/{summary.MaxHp} gold={summary.Gold} start={summary.StartPoint.coord}"
        );

        foreach (var pair in summary.RankedRoutes.Select((route, index) => (route, index)))
        {
            var primaryReason = pair.route.Reasons.FirstOrDefault()?.Message ?? "无";
            Log.Info(
                $"[SkAiRouteAdvisor] top{pair.index + 1} score={pair.route.TotalScore:F2} " +
                $"risk={pair.route.RiskScore:F2} reward={pair.route.RewardScore:F2} recovery={pair.route.RecoveryScore:F2} " +
                $"path={pair.route.PathId} reason={primaryReason}"
            );
        }
    }

    internal RouteRecommendationSummary? LatestSummary { get; private set; }
}
