using System;
using RimWorld;
using RimWorld.Planet;
using SettlementTradeOverview.Domain.Eligibility;
using SettlementTradeOverview.Integration.Context;

namespace SettlementTradeOverview.Integration.Eligibility
{
    internal static class SettlementEligibilityFactsAdapter
    {
        public static SettlementEligibilityFacts Create(
            Settlement settlement,
            PlayerTradeContext context,
            SettlementEligibilityCriteria criteria)
        {
            if (settlement == null)
                throw new ArgumentNullException(nameof(settlement));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            Faction settlementFaction = settlement.Faction;
            Faction playerFaction = Faction.OfPlayer;

            bool isPlayerOwned = playerFaction != null && settlementFaction == playerFaction;

            bool isTradeAvailable =
                settlementFaction != null && settlement.TraderKind != null && settlement.CanTradeNow;

            bool isHostileToPlayer = settlementFaction == null ||
                                     playerFaction == null ||
                                     settlementFaction.HostileTo(playerFaction);

            SettlementTechnologyLevel technologyLevel = settlementFaction?.def == null
                ? SettlementTechnologyLevel.Unavailable
                : SettlementTechnologyLevelAdapter.Convert(settlementFaction.def.techLevel);

            int? distanceInTiles = CalculateDistance(settlement, context, criteria);

            SettlementReachabilityState reachability = CalculateReachability(settlement, context, criteria);

            SettlementRoyaltyTradePermissionState royaltyTradePermission = criteria.RequireRoyaltyTradePermission
                ? SettlementRoyaltyTradePermissionAdapter.Evaluate(
                    settlement,
                    context.Colonists,
                    context.IsRoyaltyActive)
                : SettlementRoyaltyTradePermissionState.NotApplicable;

            return new SettlementEligibilityFacts(
                isPlayerOwned,
                isTradeAvailable,
                isHostileToPlayer,
                technologyLevel,
                context.HasPoweredCommsConsole,
                distanceInTiles,
                reachability,
                context.IsRoyaltyActive,
                royaltyTradePermission);
        }

        private static int? CalculateDistance(
            Settlement settlement,
            PlayerTradeContext context,
            SettlementEligibilityCriteria criteria)
        {
            if (!criteria.MaximumDistanceInTiles.HasValue)
                return null;

            return TradeLocationAdapter.TryCalculateDistance(context.WorldGrid, settlement.Tile, context.OriginTile);
        }

        private static SettlementReachabilityState CalculateReachability(
            Settlement settlement,
            PlayerTradeContext context,
            SettlementEligibilityCriteria criteria)
        {
            if (!criteria.RequireReachable)
                return SettlementReachabilityState.Unavailable;

            return TradeLocationAdapter.TryCalculateReachability(
                context.WorldGrid,
                settlement.Tile,
                context.OriginTile);
        }
    }
}