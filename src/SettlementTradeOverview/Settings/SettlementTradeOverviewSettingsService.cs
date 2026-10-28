using System;
using SettlementTradeOverview.Domain.Eligibility;

namespace SettlementTradeOverview.Settings
{
    internal readonly struct SettlementTradeOverviewSettingsApplyResult(
        bool eligibilityChanged,
        bool globalOverviewTabVisibilityChanged)
    {
        public bool EligibilityChanged { get; } = eligibilityChanged;

        public bool GlobalOverviewTabVisibilityChanged { get; } = globalOverviewTabVisibilityChanged;
    }

    internal static class SettlementTradeOverviewSettingsService
    {
        private static SettlementEligibilityCriteria _currentEligibilityCriteria =
            SettlementEligibilityCriteria.Default;

        private static bool _showGlobalOverviewTab = SettlementTradeOverviewSettingsValues.DefaultShowGlobalOverviewTab;

        private static int _eligibilityRevision;

        public static SettlementEligibilityCriteria CurrentEligibilityCriteria => _currentEligibilityCriteria;

        public static bool ShowGlobalOverviewTab => _showGlobalOverviewTab;

        public static int EligibilityRevision => _eligibilityRevision;

        public static SettlementTradeOverviewSettingsApplyResult Apply(SettlementTradeOverviewSettingsValues values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            SettlementEligibilityCriteria nextCriteria =
                SettlementTradeOverviewSettingsPolicy.CreateEligibilityCriteria(values);

            bool eligibilityChanged = !SettlementTradeOverviewSettingsPolicy.AreEquivalent(
                _currentEligibilityCriteria,
                nextCriteria);

            bool globalOverviewTabVisibilityChanged = _showGlobalOverviewTab != values.ShowGlobalOverviewTab;

            _currentEligibilityCriteria = nextCriteria;
            _showGlobalOverviewTab = values.ShowGlobalOverviewTab;

            if (eligibilityChanged)
                _eligibilityRevision++;

            return new SettlementTradeOverviewSettingsApplyResult(
                eligibilityChanged,
                globalOverviewTabVisibilityChanged);
        }
    }
}