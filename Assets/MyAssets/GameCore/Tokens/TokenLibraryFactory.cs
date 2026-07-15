using System;
using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public static class TokenLibraryFactory
    {
        private static int _nextTokenId;

        public static TokenLibraryData CreateDefault(PlayerCountMode mode)
        {
            _nextTokenId = 1;

            var library = new TokenLibraryData();

            AddStarterPools(library, mode);
            AddOneStarPools(library, mode);
            AddTwoStarPools(library, mode);

            return library;
        }

        private static void AddStarterPools(TokenLibraryData library, PlayerCountMode mode)
        {
            if (mode == PlayerCountMode.FiveSix)
            {
                // 5-6 rule: switch out one starter token in each starter pool.
                AddPool(library, TokenPoolId.StarterA,
                    Number(4), Number(8), Number(10, 11));

                AddPool(library, TokenPoolId.StarterB,
                    Number(8), Number(10), Number(9, 12));

                AddPool(library, TokenPoolId.StarterC,
                    Number(3, 4), Number(6), Number(5));

                AddPool(library, TokenPoolId.StarterD,
                    Number(2, 5), Number(6), Number(9));
            }
            else
            {
                AddPool(library, TokenPoolId.StarterA,
                    Number(4), Number(8), Number(11));

                AddPool(library, TokenPoolId.StarterB,
                    Number(8), Number(10), Number(3, 12));

                AddPool(library, TokenPoolId.StarterC,
                    Number(3), Number(6), Number(5));

                AddPool(library, TokenPoolId.StarterD,
                    Number(2, 11), Number(6), Number(9));
            }
        }

        private static void AddOneStarPools(TokenLibraryData library, PlayerCountMode mode)
        {
            var triangle = new List<RollTokenDef>
            {
                Number(3), Number(4), Number(4), Number(11), Number(12)
            };

            var square = new List<RollTokenDef>
            {
                Number(2), Number(5), Number(5), Number(6), Number(9)
            };

            var hexagon = new List<RollTokenDef>
            {
                Number(10), Number(10), Upgrade(4), Upgrade(5), Upgrade(3)
            };

            if (mode == PlayerCountMode.FiveSix)
            {
                triangle.Add(Number(3));
                square.Add(Number(8));
                hexagon.Add(Upgrade(2));
            }

            AddPool(library, TokenPoolId.OneStarTriangle, triangle);
            AddPool(library, TokenPoolId.OneStarSquare, square);
            AddPool(library, TokenPoolId.OneStarHexagon, hexagon);
        }

        private static void AddTwoStarPools(TokenLibraryData library, PlayerCountMode mode)
        {
            var triangle = new List<RollTokenDef>
            {
                Number(3), Number(4), Number(11)
            };

            var square = new List<RollTokenDef>
            {
                Number(5), Number(8), Number(9)
            };

            var hexagon = new List<RollTokenDef>
            {
                Number(10), Upgrade(6), Upgrade(4)
            };

            if (mode == PlayerCountMode.FiveSix)
            {
                triangle.Add(Number(4));
                square.Add(Number(12));
                hexagon.Add(Upgrade(3));
            }

            AddPool(library, TokenPoolId.TwoStarTriangle, triangle);
            AddPool(library, TokenPoolId.TwoStarSquare, square);
            AddPool(library, TokenPoolId.TwoStarHexagon, hexagon);
        }

        private static RollTokenDef Number(int rollA)
        {
            return new RollTokenDef(
                TokenId: _nextTokenId++,
                RollA: rollA,
                RollB: null,
                RequiresUpgrade: false,
                IsReserveCandidate: false);
        }

        private static RollTokenDef Number(int rollA, int rollB)
        {
            return new RollTokenDef(
                TokenId: _nextTokenId++,
                RollA: rollA,
                RollB: rollB,
                RequiresUpgrade: false,
                IsReserveCandidate: false);
        }

        private static RollTokenDef Upgrade(int requiredUpgradeCount)
        {
            // The current RollTokenDef model only stores that an upgrade is required,
            // not which upgrade type. RollA carries the required count for display/debug.
            return new RollTokenDef(
                TokenId: _nextTokenId++,
                RollA: requiredUpgradeCount,
                RollB: null,
                RequiresUpgrade: true,
                IsReserveCandidate: false);
        }

        private static void AddPool(TokenLibraryData library, TokenPoolId poolId, params RollTokenDef[] tokens)
        {
            AddPool(library, poolId, (IEnumerable<RollTokenDef>)tokens);
        }

        private static void AddPool(TokenLibraryData library, TokenPoolId poolId, IEnumerable<RollTokenDef> tokens)
        {
            if (library.Pools.ContainsKey(poolId))
                throw new InvalidOperationException($"Duplicate token pool definition: {poolId}");

            library.Pools[poolId] = new List<RollTokenDef>(tokens);
        }
    }
}
