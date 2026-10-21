using System;
using SettlementTradeOverview.Domain.Eligibility;

namespace SettlementTradeOverview.Integration.Context
{
    internal static class TradeLocationPolicy
    {
        public static int? CalculateDistance(
            int settlementTile,
            int settlementLayerId,
            int originTile,
            int originLayerId,
            Func<float> calculateSameLayerDistance)
        {
            if (calculateSameLayerDistance == null)
                throw new ArgumentNullException(nameof(calculateSameLayerDistance));

            if (!AreValidLocations(settlementTile, settlementLayerId, originTile, originLayerId) ||
                settlementLayerId != originLayerId)
            {
                return null;
            }

            float distance = calculateSameLayerDistance();

            // int.MaxValue rounds up when converted to float, so equality is also outside the valid range.
            if (float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0f || distance >= int.MaxValue)
                return null;

            return (int)distance;
        }

        public static SettlementReachabilityState CalculateReachability(
            int settlementTile,
            int settlementLayerId,
            int originTile,
            int originLayerId,
            Func<int> calculateSameLayerTraversalDistance)
        {
            if (calculateSameLayerTraversalDistance == null)
                throw new ArgumentNullException(nameof(calculateSameLayerTraversalDistance));

            if (!AreValidLocations(settlementTile, settlementLayerId, originTile, originLayerId))
                return SettlementReachabilityState.Unavailable;

            if (settlementLayerId != originLayerId)
                return SettlementReachabilityState.Unreachable;

            int distance = calculateSameLayerTraversalDistance();

            if (distance == int.MaxValue)
                return SettlementReachabilityState.Unreachable;

            return distance < 0 ? SettlementReachabilityState.Unavailable : SettlementReachabilityState.Reachable;
        }

        private static bool AreValidLocations(
            int settlementTile,
            int settlementLayerId,
            int originTile,
            int originLayerId)
        {
            return settlementTile >= 0 && settlementLayerId >= 0 && originTile >= 0 && originLayerId >= 0;
        }
    }
}