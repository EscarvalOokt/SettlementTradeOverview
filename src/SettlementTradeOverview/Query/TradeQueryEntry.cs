using System;
using SettlementTradeOverview.Domain.Snapshots;

namespace SettlementTradeOverview.Query
{
    public sealed class TradeQueryEntry(TraderSnapshot trader, TradeEntrySnapshot entry)
    {
        public TraderSnapshot Trader { get; } = trader ?? throw new ArgumentNullException(nameof(trader));

        public TradeEntrySnapshot Entry { get; } = entry ?? throw new ArgumentNullException(nameof(entry));
    }
}