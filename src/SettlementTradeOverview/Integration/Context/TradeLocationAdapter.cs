using RimWorld.Planet;
using SettlementTradeOverview.Domain.Eligibility;

namespace SettlementTradeOverview.Integration.Context
{
    internal static class TradeLocationAdapter
    {
        public static int? TryCalculateDistance(WorldGrid worldGrid, PlanetTile settlementTile, PlanetTile originTile)
        {
            if (worldGrid == null || !settlementTile.Valid || !originTile.Valid)
                return null;

            try
            {
                return TradeLocationPolicy.CalculateDistance(
                    settlementTile.tileId,
                    settlementTile.Layer.LayerID,
                    originTile.tileId,
                    originTile.Layer.LayerID,
                    () => worldGrid.ApproxDistanceInTiles(settlementTile, originTile));
            }
            catch
            {
                return null;
            }
        }

        public static SettlementReachabilityState TryCalculateReachability(
            WorldGrid worldGrid,
            PlanetTile settlementTile,
            PlanetTile originTile)
        {
            if (worldGrid == null || !settlementTile.Valid || !originTile.Valid)
                return SettlementReachabilityState.Unavailable;

            try
            {
                return TradeLocationPolicy.CalculateReachability(
                    settlementTile.tileId,
                    settlementTile.Layer.LayerID,
                    originTile.tileId,
                    originTile.Layer.LayerID,
                    () => worldGrid.TraversalDistanceBetween(settlementTile, originTile, false));
            }
            catch
            {
                return SettlementReachabilityState.Unavailable;
            }
        }
    }
}