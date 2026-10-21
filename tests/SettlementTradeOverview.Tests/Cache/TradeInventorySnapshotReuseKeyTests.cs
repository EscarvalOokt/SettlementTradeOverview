using System;
using NUnit.Framework;
using SettlementTradeOverview.Cache;
using SettlementTradeOverview.Domain.Eligibility;
using SettlementTradeOverview.Domain.Snapshots;

namespace SettlementTradeOverview.Tests.Cache
{
    [TestFixture]
    public sealed class TradeInventorySnapshotReuseKeyTests
    {
        [Test]
        public void EquivalentInputs_ProduceEqualKeysAndHashCodes()
        {
            TradeInventorySnapshotReuseKey first = CreateAvailableKey();
            TradeInventorySnapshotReuseKey second = CreateAvailableKey();

            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void MapOrOriginChange_ChangesKey()
        {
            TradeInventorySnapshotReuseKey baseline = CreateAvailableKey();
            TradeInventorySnapshotReuseKey differentMap = CreateAvailableKey(mapId: 2);
            TradeInventorySnapshotReuseKey differentOrigin = CreateAvailableKey(originTile: 11);

            Assert.That(baseline.Equals(differentMap), Is.False);
            Assert.That(baseline.Equals(differentOrigin), Is.False);
        }

        [Test]
        public void SameMapAndTileOnDifferentLayers_AreNotReusable()
        {
            TradeInventorySnapshotReuseKey surface = CreateAvailableKey(originLayerId: 0);
            TradeInventorySnapshotReuseKey orbit = CreateAvailableKey(originLayerId: 1);

            Assert.That(surface.MapId, Is.EqualTo(orbit.MapId));
            Assert.That(surface.OriginTile, Is.EqualTo(orbit.OriginTile));
            Assert.That(surface.Equals(orbit), Is.False);
            Assert.That(orbit.OriginLayerId, Is.EqualTo(1));
            Assert.That(orbit.Equals(CreateAvailableKey(originLayerId: 1)), Is.True);
            Assert.That(orbit.GetHashCode(), Is.EqualTo(CreateAvailableKey(originLayerId: 1).GetHashCode()));
        }

        [Test]
        public void NegotiatorPresenceIdentityLabelOrModifierChange_ChangesKey()
        {
            TradeInventorySnapshotReuseKey withoutNegotiator = CreateAvailableKey();

            TradeInventorySnapshotReuseKey baseline = CreateAvailableKey(
                negotiator: new TradeNegotiatorSnapshot("Pawn:1", "Alex", 0.1f));

            TradeInventorySnapshotReuseKey differentIdentity = CreateAvailableKey(
                negotiator: new TradeNegotiatorSnapshot("Pawn:2", "Alex", 0.1f));

            TradeInventorySnapshotReuseKey differentLabel = CreateAvailableKey(
                negotiator: new TradeNegotiatorSnapshot("Pawn:1", "Morgan", 0.1f));

            TradeInventorySnapshotReuseKey differentModifier = CreateAvailableKey(
                negotiator: new TradeNegotiatorSnapshot("Pawn:1", "Alex", 0.2f));

            Assert.That(withoutNegotiator.Equals(baseline), Is.False);
            Assert.That(baseline.Equals(differentIdentity), Is.False);
            Assert.That(baseline.Equals(differentLabel), Is.False);
            Assert.That(baseline.Equals(differentModifier), Is.False);
        }

        [Test]
        public void EligibilityCriteriaChanges_ChangeKey()
        {
            TradeInventorySnapshotReuseKey baseline = CreateAvailableKey();

            TradeInventorySnapshotReuseKey consoleDisabled = CreateAvailableKey(
                criteria: new SettlementEligibilityCriteria(requirePoweredCommsConsole: false));

            TradeInventorySnapshotReuseKey technologyDisabled = CreateAvailableKey(
                criteria: new SettlementEligibilityCriteria(minimumTechnologyLevel: null));

            TradeInventorySnapshotReuseKey distanceChanged = CreateAvailableKey(
                criteria: new SettlementEligibilityCriteria(maximumDistanceInTiles: 20));

            TradeInventorySnapshotReuseKey reachabilityDisabled = CreateAvailableKey(
                criteria: new SettlementEligibilityCriteria(requireReachable: false));

            TradeInventorySnapshotReuseKey royaltyEnabled = CreateAvailableKey(
                criteria: new SettlementEligibilityCriteria(requireRoyaltyTradePermission: true));

            Assert.That(baseline.Equals(consoleDisabled), Is.False);
            Assert.That(baseline.Equals(technologyDisabled), Is.False);
            Assert.That(baseline.Equals(distanceChanged), Is.False);
            Assert.That(baseline.Equals(reachabilityDisabled), Is.False);
            Assert.That(baseline.Equals(royaltyEnabled), Is.False);
        }

        [Test]
        public void PoweredCommsState_ChangesKeyOnlyWhenFilterIsEnabled()
        {
            TradeInventorySnapshotReuseKey enabledWithConsole = CreateAvailableKey(hasPoweredCommsConsole: true);
            TradeInventorySnapshotReuseKey enabledWithoutConsole = CreateAvailableKey(hasPoweredCommsConsole: false);

            var disabledCriteria = new SettlementEligibilityCriteria(requirePoweredCommsConsole: false);

            TradeInventorySnapshotReuseKey disabledWithConsole = CreateAvailableKey(
                criteria: disabledCriteria,
                hasPoweredCommsConsole: true);

            TradeInventorySnapshotReuseKey disabledWithoutConsole = CreateAvailableKey(
                criteria: disabledCriteria,
                hasPoweredCommsConsole: false);

            Assert.That(enabledWithConsole.Equals(enabledWithoutConsole), Is.False);
            Assert.That(disabledWithConsole.Equals(disabledWithoutConsole), Is.True);
        }

        [Test]
        public void RoyaltyState_ChangesKeyOnlyWhenFilterIsEnabled()
        {
            var enabledCriteria = new SettlementEligibilityCriteria(requireRoyaltyTradePermission: true);

            TradeInventorySnapshotReuseKey enabledWithRoyalty = CreateAvailableKey(
                criteria: enabledCriteria,
                isRoyaltyActive: true);

            TradeInventorySnapshotReuseKey enabledWithoutRoyalty = CreateAvailableKey(
                criteria: enabledCriteria,
                isRoyaltyActive: false);

            TradeInventorySnapshotReuseKey disabledWithRoyalty = CreateAvailableKey(isRoyaltyActive: true);
            TradeInventorySnapshotReuseKey disabledWithoutRoyalty = CreateAvailableKey(isRoyaltyActive: false);

            Assert.That(enabledWithRoyalty.Equals(enabledWithoutRoyalty), Is.False);
            Assert.That(disabledWithRoyalty.Equals(disabledWithoutRoyalty), Is.True);
        }

        [Test]
        public void AvailableAndUnavailableContexts_AreDifferent()
        {
            var unavailable =
                TradeInventorySnapshotReuseKey.CreateUnavailableContext(SettlementEligibilityCriteria.Default);

            TradeInventorySnapshotReuseKey available = CreateAvailableKey();

            Assert.That(unavailable.IsContextAvailable, Is.False);
            Assert.That(available.IsContextAvailable, Is.True);
            Assert.That(unavailable.Equals(available), Is.False);
        }

        [Test]
        public void ReuseKey_DoesNotExposeTraderIdentityCollection()
        {
            Assert.That(typeof(TradeInventorySnapshotReuseKey).GetProperty("TraderIdentities"), Is.Null);
            Assert.That(typeof(TradeInventorySnapshotReuseKey).GetProperty("DiscoveryFailureCount"), Is.Null);
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            Assert.That(
                (Action)(() => TradeInventorySnapshotReuseKey.CreateUnavailableContext(null)),
                Throws.TypeOf<ArgumentNullException>());

            Assert.That(
                (Action)(() =>
                {
                    _ = TradeInventorySnapshotReuseKey.CreateAvailableContext(
                        -1,
                        10,
                        0,
                        null,
                        SettlementEligibilityCriteria.Default,
                        hasPoweredCommsConsole: true,
                        isRoyaltyActive: false);
                }),
                Throws.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(
                (Action)(() =>
                {
                    _ = TradeInventorySnapshotReuseKey.CreateAvailableContext(
                        1,
                        -1,
                        0,
                        null,
                        SettlementEligibilityCriteria.Default,
                        hasPoweredCommsConsole: true,
                        isRoyaltyActive: false);
                }),
                Throws.TypeOf<ArgumentOutOfRangeException>());

            Assert.That(
                (Action)(() => CreateAvailableKey(originLayerId: -1)),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        private static TradeInventorySnapshotReuseKey CreateAvailableKey(
            int mapId = 1,
            int originTile = 10,
            int originLayerId = 0,
            TradeNegotiatorSnapshot negotiator = null,
            SettlementEligibilityCriteria criteria = null,
            bool hasPoweredCommsConsole = true,
            bool isRoyaltyActive = false)
        {
            return TradeInventorySnapshotReuseKey.CreateAvailableContext(
                mapId,
                originTile,
                originLayerId,
                negotiator,
                criteria ?? SettlementEligibilityCriteria.Default,
                hasPoweredCommsConsole,
                isRoyaltyActive);
        }
    }
}