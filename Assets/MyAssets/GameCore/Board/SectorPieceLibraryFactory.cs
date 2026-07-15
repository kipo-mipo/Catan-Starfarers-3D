namespace MyAssets.GameCore
{
    public static class SectorPieceLibraryFactory
    {
        public static SectorPieceLibraryData CreateDefault()
        {
            var library = new SectorPieceLibraryData();

            AddPlanetaryPieces(library);
            AddEmptyPieces(library);
            AddTradePieces(library);
            AddStarterPieces(library);

            return library;
        }

        private static void AddPlanetaryPieces(SectorPieceLibraryData library)
        {
            // Four-player pool needs: 5 one-star + 3 two-star.
            // Five/six-player pool needs: 6 one-star + 4 two-star.

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(1),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Carbon, StarRating.One, TokenPoolId.OneStarSquare),
                    new PlanetOnPiece(1, ResourceType.Food,   StarRating.One, TokenPoolId.OneStarTriangle),
                    new PlanetOnPiece(2, ResourceType.Fuel,   StarRating.One, TokenPoolId.OneStarHexagon),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(2),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Carbon, StarRating.One, TokenPoolId.OneStarTriangle),
                    new PlanetOnPiece(1, ResourceType.Ore,    StarRating.One, TokenPoolId.OneStarHexagon),
                    new PlanetOnPiece(2, ResourceType.Goods,  StarRating.One, TokenPoolId.OneStarSquare),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(3),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Fuel,  StarRating.One, TokenPoolId.OneStarSquare),
                    new PlanetOnPiece(1, ResourceType.Ore,   StarRating.One, TokenPoolId.OneStarTriangle),
                    new PlanetOnPiece(2, ResourceType.Goods, StarRating.One, TokenPoolId.OneStarHexagon),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(4),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Fuel, StarRating.One, TokenPoolId.OneStarTriangle),
                    new PlanetOnPiece(1, ResourceType.Food, StarRating.One, TokenPoolId.OneStarHexagon),
                    new PlanetOnPiece(2, ResourceType.Ore,  StarRating.One, TokenPoolId.OneStarSquare),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(5),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Food,   StarRating.One, TokenPoolId.OneStarSquare),
                    new PlanetOnPiece(1, ResourceType.Carbon, StarRating.One, TokenPoolId.OneStarHexagon),
                    new PlanetOnPiece(2, ResourceType.Goods,  StarRating.One, TokenPoolId.OneStarTriangle),
                }
            });

            // Extra one-star planetary sector for five/six-player mode.
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(9),
                Type = SectorPieceType.Planetary,
                Star = StarRating.One,
                FiveSixOnly = true,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Food,  StarRating.One, TokenPoolId.OneStarTriangle),
                    new PlanetOnPiece(1, ResourceType.Fuel,  StarRating.One, TokenPoolId.OneStarHexagon),
                    new PlanetOnPiece(2, ResourceType.Goods, StarRating.One, TokenPoolId.OneStarSquare),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(6),
                Type = SectorPieceType.Planetary,
                Star = StarRating.Two,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Carbon, StarRating.Two, TokenPoolId.TwoStarSquare),
                    new PlanetOnPiece(1, ResourceType.Fuel,   StarRating.Two, TokenPoolId.TwoStarTriangle),
                    new PlanetOnPiece(2, ResourceType.Goods,  StarRating.Two, TokenPoolId.TwoStarHexagon),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(7),
                Type = SectorPieceType.Planetary,
                Star = StarRating.Two,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Goods, StarRating.Two, TokenPoolId.TwoStarTriangle),
                    new PlanetOnPiece(1, ResourceType.Food,  StarRating.Two, TokenPoolId.TwoStarSquare),
                    new PlanetOnPiece(2, ResourceType.Ore,   StarRating.Two, TokenPoolId.TwoStarHexagon),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(8),
                Type = SectorPieceType.Planetary,
                Star = StarRating.Two,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Ore,    StarRating.Two, TokenPoolId.TwoStarHexagon),
                    new PlanetOnPiece(1, ResourceType.Carbon, StarRating.Two, TokenPoolId.TwoStarTriangle),
                    new PlanetOnPiece(2, ResourceType.Goods,  StarRating.Two, TokenPoolId.TwoStarSquare),
                }
            });

            // Extra two-star planetary sector for five/six-player mode.
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(10),
                Type = SectorPieceType.Planetary,
                Star = StarRating.Two,
                FiveSixOnly = true,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Carbon, StarRating.Two, TokenPoolId.TwoStarHexagon),
                    new PlanetOnPiece(1, ResourceType.Ore,    StarRating.Two, TokenPoolId.TwoStarSquare),
                    new PlanetOnPiece(2, ResourceType.Fuel,   StarRating.Two, TokenPoolId.TwoStarTriangle),
                }
            });
        }

        private static void AddEmptyPieces(SectorPieceLibraryData library)
        {
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(100),
                Type = SectorPieceType.Empty,
                Star = StarRating.One,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(101),
                Type = SectorPieceType.Empty,
                Star = StarRating.One,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(102),
                Type = SectorPieceType.Empty,
                Star = StarRating.Two,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(103),
                Type = SectorPieceType.Empty,
                Star = StarRating.Two,
            });
        }

        private static void AddTradePieces(SectorPieceLibraryData library)
        {
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(200),
                Type = SectorPieceType.Trade,
                Star = StarRating.One,
                TradeSpecies = TradeSpecies.SpeciesMerchants,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(201),
                Type = SectorPieceType.Trade,
                Star = StarRating.One,
                TradeSpecies = TradeSpecies.SpeciesGreenfolk,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(202),
                Type = SectorPieceType.Trade,
                Star = StarRating.Two,
                TradeSpecies = TradeSpecies.SpeciesScientists,
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(203),
                Type = SectorPieceType.Trade,
                Star = StarRating.Two,
                TradeSpecies = TradeSpecies.SpeciesDiplomats,
            });

            // Extra two-star trade sector for five/six-player mode.
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(204),
                Type = SectorPieceType.Trade,
                Star = StarRating.Two,
                FiveSixOnly = true,
                TradeSpecies = TradeSpecies.SpeciesTravelers,
            });
        }

        private static void AddStarterPieces(SectorPieceLibraryData library)
        {
            // Starter sectors are fixed/revealed starting sectors.
            // Edit these planet/resource/token entries later to match the rulebook exactly.
            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(300),
                Type = SectorPieceType.StarterPlanetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Fuel,  StarRating.One, TokenPoolId.StarterD),
                    new PlanetOnPiece(1, ResourceType.Goods, StarRating.One, TokenPoolId.StarterD),
                    new PlanetOnPiece(2, ResourceType.Ore,   StarRating.One, TokenPoolId.StarterD),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(301),
                Type = SectorPieceType.StarterPlanetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Carbon, StarRating.One, TokenPoolId.StarterC),
                    new PlanetOnPiece(1, ResourceType.Food,   StarRating.One, TokenPoolId.StarterC),
                    new PlanetOnPiece(2, ResourceType.Ore,    StarRating.One, TokenPoolId.StarterC),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(302),
                Type = SectorPieceType.StarterPlanetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Ore,   StarRating.One, TokenPoolId.StarterB),
                    new PlanetOnPiece(1, ResourceType.Fuel,  StarRating.One, TokenPoolId.StarterB),
                    new PlanetOnPiece(2, ResourceType.Goods, StarRating.One, TokenPoolId.StarterB),
                }
            });

            Add(library, new SectorPieceDef
            {
                Id = new SectorPieceId(303),
                Type = SectorPieceType.StarterPlanetary,
                Star = StarRating.One,
                Planets =
                {
                    new PlanetOnPiece(0, ResourceType.Fuel,   StarRating.One, TokenPoolId.StarterA),
                    new PlanetOnPiece(1, ResourceType.Carbon, StarRating.One, TokenPoolId.StarterA),
                    new PlanetOnPiece(2, ResourceType.Food,   StarRating.One, TokenPoolId.StarterA),
                }
            });
        }

        private static void Add(SectorPieceLibraryData library, SectorPieceDef def)
        {
            Validate(def);
            library.Pieces.Add(def.Id, def);
        }

        private static void Validate(SectorPieceDef def)
        {
            if (def.Type == SectorPieceType.Trade && def.TradeSpecies == TradeSpecies.None)
                throw new System.InvalidOperationException($"Trade sector {def.Id.Value} is missing TradeSpecies.");

            if (def.Type != SectorPieceType.Trade && def.TradeSpecies != TradeSpecies.None)
                throw new System.InvalidOperationException($"Non-trade sector {def.Id.Value} has TradeSpecies set.");

            if (def.Type is SectorPieceType.Empty or SectorPieceType.Trade)
            {
                if (def.Planets.Count != 0)
                    throw new System.InvalidOperationException($"{def.Type} sector {def.Id.Value} must not contain planets.");
            }
            else
            {
                if (def.Planets.Count == 0)
                    throw new System.InvalidOperationException($"{def.Type} sector {def.Id.Value} must contain planets.");
            }
        }
    }
}
