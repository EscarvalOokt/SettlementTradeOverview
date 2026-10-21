using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RimWorld.Planet;
using Verse;

namespace SettlementTradeOverview.Integration.Context
{
    internal sealed class PlayerTradeReuseContext
    {
        private readonly ReadOnlyCollection<Pawn> _colonists;

        public PlayerTradeReuseContext(
            Map originMap,
            PlanetTile originTile,
            int originLayerId,
            WorldGrid worldGrid,
            IReadOnlyList<Pawn> colonists,
            bool hasPoweredCommsConsole,
            bool isRoyaltyActive)
        {
            if (originMap == null)
                throw new ArgumentNullException(nameof(originMap));

            if (!originTile.Valid)
                throw new ArgumentOutOfRangeException(nameof(originTile));

            if (originLayerId < 0)
                throw new ArgumentOutOfRangeException(nameof(originLayerId));

            if (worldGrid == null)
                throw new ArgumentNullException(nameof(worldGrid));

            if (colonists == null)
                throw new ArgumentNullException(nameof(colonists));

            var copiedColonists = new Pawn[colonists.Count];

            for (var index = 0; index < colonists.Count; index++)
                copiedColonists[index] = colonists[index];

            OriginMap = originMap;
            OriginTile = originTile;
            OriginLayerId = originLayerId;
            WorldGrid = worldGrid;
            _colonists = Array.AsReadOnly(copiedColonists);
            HasPoweredCommsConsole = hasPoweredCommsConsole;
            IsRoyaltyActive = isRoyaltyActive;
        }

        public Map OriginMap { get; }

        public PlanetTile OriginTile { get; }

        public int OriginLayerId { get; }

        public WorldGrid WorldGrid { get; }

        public IReadOnlyList<Pawn> Colonists => _colonists;

        public bool HasPoweredCommsConsole { get; }

        public bool IsRoyaltyActive { get; }
    }
}