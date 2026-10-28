namespace SettlementTradeOverview.Settings
{
    internal sealed class SettlementTradeOverviewSettingsValues
    {
        public const bool DefaultShowGlobalOverviewTab = true;
        public const bool DefaultRequirePoweredCommsConsole = true;
        public const bool DefaultRequireIndustrialTechnology = true;
        public const bool DefaultLimitMaximumDistance = true;
        public const int DefaultMaximumDistanceInTiles = 40;
        public const int MaximumAllowedDistanceInTiles = 3000;
        public const bool DefaultRequireReachable = true;
        public const bool DefaultRequireRoyaltyTradePermission = false;

        public bool ShowGlobalOverviewTab { get; init; } = DefaultShowGlobalOverviewTab;

        public bool RequirePoweredCommsConsole { get; init; } = DefaultRequirePoweredCommsConsole;

        public bool RequireIndustrialTechnology { get; init; } = DefaultRequireIndustrialTechnology;

        public bool LimitMaximumDistance { get; init; } = DefaultLimitMaximumDistance;

        public int MaximumDistanceInTiles { get; set; } = DefaultMaximumDistanceInTiles;

        public bool RequireReachable { get; init; } = DefaultRequireReachable;

        public bool RequireRoyaltyTradePermission { get; init; } = DefaultRequireRoyaltyTradePermission;
    }
}