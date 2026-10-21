using NUnit.Framework;
using SettlementTradeOverview.Domain.Eligibility;
using SettlementTradeOverview.Domain.Snapshots;
using SettlementTradeOverview.Integration.Context;
using SettlementTradeOverview.Integration.Snapshots;

namespace SettlementTradeOverview.Tests.Integration.Context
{
    [TestFixture]
    public sealed class TradeLocationPolicyTests
    {
        [TestCase(42, 42)]
        [TestCase(42, 99)]
        public void DifferentLayers_DoNotCallSameLayerCalculatorsOrInventDistance(int settlementTile, int originTile)
        {
            var calculationCount = 0;
            int? distance = TradeLocationPolicy.CalculateDistance(
                settlementTile,
                0,
                originTile,
                1,
                () =>
                {
                    calculationCount++;
                    return 0f;
                });
            SettlementReachabilityState reachability = TradeLocationPolicy.CalculateReachability(
                settlementTile,
                0,
                originTile,
                1,
                () =>
                {
                    calculationCount++;
                    return 0;
                });
            TradeDistance presentation = TradeDistanceStateResolver.Resolve(distance, null, reachability);

            Assert.That(calculationCount, Is.Zero);
            Assert.That(distance, Is.Null);
            Assert.That(reachability, Is.EqualTo(SettlementReachabilityState.Unreachable));
            Assert.That(presentation.Tiles, Is.Null);
            Assert.That(presentation.RouteState, Is.EqualTo(TradeRouteState.Unreachable));
        }

        [TestCase(0)]
        [TestCase(7)]
        public void SameLayer_UsesVanillaDistanceAndTraversalValues(int layerId)
        {
            var calculationCount = 0;
            int? distance = TradeLocationPolicy.CalculateDistance(
                42,
                layerId,
                99,
                layerId,
                () =>
                {
                    calculationCount++;
                    return 17.8f;
                });
            SettlementReachabilityState reachability = TradeLocationPolicy.CalculateReachability(
                42,
                layerId,
                99,
                layerId,
                () =>
                {
                    calculationCount++;
                    return 24;
                });

            Assert.That(calculationCount, Is.EqualTo(2));
            Assert.That(distance, Is.EqualTo(17));
            Assert.That(reachability, Is.EqualTo(SettlementReachabilityState.Reachable));
        }

        [Test]
        public void SameTileOnSameLayer_CanHaveZeroDistanceAndReachableRoute()
        {
            int? distance = TradeLocationPolicy.CalculateDistance(42, 3, 42, 3, () => 0f);
            SettlementReachabilityState reachability = TradeLocationPolicy.CalculateReachability(42, 3, 42, 3, () => 0);

            Assert.That(distance, Is.Zero);
            Assert.That(reachability, Is.EqualTo(SettlementReachabilityState.Reachable));
        }

        [TestCase(-1, 0, 42, 0)]
        [TestCase(42, -1, 42, 0)]
        [TestCase(42, 0, -1, 1)]
        [TestCase(42, 0, 42, -1)]
        public void InvalidLocation_StaysUnavailableWithoutCallingCalculators(
            int settlementTile,
            int settlementLayerId,
            int originTile,
            int originLayerId)
        {
            var calculationCount = 0;
            int? distance = TradeLocationPolicy.CalculateDistance(
                settlementTile,
                settlementLayerId,
                originTile,
                originLayerId,
                () =>
                {
                    calculationCount++;
                    return 0f;
                });
            SettlementReachabilityState reachability = TradeLocationPolicy.CalculateReachability(
                settlementTile,
                settlementLayerId,
                originTile,
                originLayerId,
                () =>
                {
                    calculationCount++;
                    return 0;
                });

            Assert.That(calculationCount, Is.Zero);
            Assert.That(distance, Is.Null);
            Assert.That(reachability, Is.EqualTo(SettlementReachabilityState.Unavailable));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        [TestCase((float)int.MaxValue)]
        public void InvalidCalculatedDistance_DoesNotBecomeATileCount(float calculatedDistance)
        {
            int? distance = TradeLocationPolicy.CalculateDistance(42, 0, 99, 0, () => calculatedDistance);

            Assert.That(distance, Is.Null);
        }

        [TestCase(int.MaxValue, SettlementReachabilityState.Unreachable)]
        [TestCase(-1, SettlementReachabilityState.Unavailable)]
        public void SameLayer_PreservesVanillaRouteFailureStates(
            int traversalDistance,
            SettlementReachabilityState expected)
        {
            SettlementReachabilityState result =
                TradeLocationPolicy.CalculateReachability(42, 0, 99, 0, () => traversalDistance);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(true, null, SettlementEligibilityFailureReason.Unreachable)]
        [TestCase(false, 40, SettlementEligibilityFailureReason.DistanceUnavailable)]
        [TestCase(false, null, SettlementEligibilityFailureReason.None)]
        public void CrossLayerFacts_RespectTheConfiguredEligibilityFilters(
            bool requireReachable,
            int? maximumDistance,
            SettlementEligibilityFailureReason expected)
        {
            int? distance = TradeLocationPolicy.CalculateDistance(42, 0, 42, 1, () => 0f);
            SettlementReachabilityState reachability = TradeLocationPolicy.CalculateReachability(42, 0, 42, 1, () => 0);
            var facts = new SettlementEligibilityFacts(
                isPlayerOwned: false,
                isTradeAvailable: true,
                isHostileToPlayer: false,
                technologyLevel: SettlementTechnologyLevel.Industrial,
                hasPoweredCommsConsole: true,
                distanceInTiles: distance,
                reachability: reachability,
                isRoyaltyActive: false,
                royaltyTradePermission: SettlementRoyaltyTradePermissionState.NotApplicable);
            var criteria = new SettlementEligibilityCriteria(
                maximumDistanceInTiles: maximumDistance,
                requireReachable: requireReachable);

            SettlementEligibilityResult result = SettlementEligibilityPolicy.Evaluate(facts, criteria);

            Assert.That(result.FailureReason, Is.EqualTo(expected));
            Assert.That(result.IsEligible, Is.EqualTo(expected == SettlementEligibilityFailureReason.None));
        }
    }
}