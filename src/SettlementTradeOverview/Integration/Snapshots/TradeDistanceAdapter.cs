using System;
using SettlementTradeOverview.Domain.Eligibility;
using SettlementTradeOverview.Domain.Snapshots;
using SettlementTradeOverview.Integration.Context;
using SettlementTradeOverview.Integration.Discovery;

namespace SettlementTradeOverview.Integration.Snapshots
{
    internal static class TradeDistanceAdapter
    {
        public static TradeDistance Create(DiscoveredTraderSource source, PlayerTradeContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            int? eligibilityDistance = source.EligibilityFacts.DistanceInTiles;
            int? calculatedDistance = eligibilityDistance.HasValue
                ? null
                : TradeLocationAdapter.TryCalculateDistance(
                    context.WorldGrid,
                    source.Settlement.Tile,
                    context.OriginTile);

            SettlementReachabilityState reachability = source.EligibilityFacts.Reachability;

            if (reachability == SettlementReachabilityState.Unavailable)
            {
                reachability = TradeLocationAdapter.TryCalculateReachability(
                    context.WorldGrid,
                    source.Settlement.Tile,
                    context.OriginTile);
            }

            return TradeDistanceStateResolver.Resolve(eligibilityDistance, calculatedDistance, reachability);
        }
    }

    internal static class TradeDistanceStateResolver
    {
        public static TradeDistance Resolve(
            int? eligibilityDistance,
            int? calculatedDistance,
            SettlementReachabilityState reachability)
        {
            int? distance = IsValidDistance(eligibilityDistance) ? eligibilityDistance :
                IsValidDistance(calculatedDistance) ? calculatedDistance : null;

            if (!distance.HasValue)
            {
                return reachability == SettlementReachabilityState.Unreachable
                    ? TradeDistance.Unreachable(null)
                    : TradeDistance.Unavailable;
            }

            switch (reachability)
            {
                case SettlementReachabilityState.Reachable:
                    return TradeDistance.Reachable(distance.Value);

                case SettlementReachabilityState.Unreachable:
                    return TradeDistance.Unreachable(distance.Value);

                case SettlementReachabilityState.Unavailable:
                    return TradeDistance.WithUnavailableRoute(distance.Value);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reachability));
            }
        }

        private static bool IsValidDistance(int? distance)
        {
            return distance is >= 0;
        }
    }
}