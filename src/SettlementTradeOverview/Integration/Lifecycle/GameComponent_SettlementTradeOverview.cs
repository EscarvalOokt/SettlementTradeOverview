using SettlementTradeOverview.Application.Snapshots;
using Verse;

namespace SettlementTradeOverview.Integration.Lifecycle
{
    // ReSharper disable once InconsistentNaming
    internal sealed class GameComponent_SettlementTradeOverview : GameComponent
    {
        // ReSharper disable once UnusedParameter.Local
        public GameComponent_SettlementTradeOverview(Game _)
        {
        }

        public override void StartedNewGame()
        {
            TradeInventorySnapshotService.Invalidate();
        }

        public override void LoadedGame()
        {
            TradeInventorySnapshotService.Invalidate();
        }
    }
}