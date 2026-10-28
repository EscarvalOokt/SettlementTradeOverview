using System;
using NUnit.Framework;
using SettlementTradeOverview.Domain.Snapshots;
using SettlementTradeOverview.Integration.Planner;

namespace SettlementTradeOverview.Tests.Integration.Planner
{
    [TestFixture]
    public sealed class PlannerGenepackRelevanceAdapterTests
    {
        [Test]
        public void Query_NullInput_Throws()
        {
            Assert.That(
                (Action)(() => PlannerGenepackRelevanceAdapter.Query(null)),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Query_NullComposition_Throws()
        {
            Assert.That(
                (Action)(() => PlannerGenepackRelevanceAdapter.Query([null])),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Query_EmptyInput_ReturnsSuccessfulEmptyResult()
        {
            PlannerGenepackRelevanceBatchResult result = PlannerGenepackRelevanceAdapter.Query(
                []);

            Assert.That(result.Status, Is.EqualTo(PlannerGenepackRelevanceBatchStatus.Success));
            Assert.That(result.Results, Is.Empty);
        }

        [Test]
        public void Query_OptionalPlannerUnavailable_ReturnsNeutralResult()
        {
            var composition = new GenepackCompositionSnapshot(["GeneA"]);

            PlannerGenepackRelevanceBatchResult result = PlannerGenepackRelevanceAdapter.Query([composition]);

            Assert.That(result.Status, Is.EqualTo(PlannerGenepackRelevanceBatchStatus.Unavailable));
            Assert.That(result.Results, Is.Empty);
        }

        [Test]
        public void Query_RepeatedUnavailableCalls_RemainSafe()
        {
            var composition = new GenepackCompositionSnapshot(["GeneA"]);

            PlannerGenepackRelevanceBatchResult first = PlannerGenepackRelevanceAdapter.Query([composition]);

            PlannerGenepackRelevanceBatchResult second = PlannerGenepackRelevanceAdapter.Query([composition]);

            Assert.That(first.Status, Is.EqualTo(PlannerGenepackRelevanceBatchStatus.Unavailable));
            Assert.That(second.Status, Is.EqualTo(PlannerGenepackRelevanceBatchStatus.Unavailable));
            Assert.That(first.Results, Is.Empty);
            Assert.That(second.Results, Is.Empty);
        }
    }
}