﻿using System;
using Server;

namespace Server.Misc
{
    public static class TileDataOverride
    {
        public static void Initialize()
        {
            // Missing NoShoot Flags
            TileData.ItemTable[0x2A0].Flags |= TileFlag.NoShoot; // Marble Wall
            TileData.ItemTable[0x3E0].Flags |= TileFlag.NoShoot; // Stone Wall
            TileData.ItemTable[0x3E1].Flags |= TileFlag.NoShoot; // Stone Wall

            // Incorrect Item Height
            TileData.ItemTable[0x34D2].Height = 0; // Water Tile
        }
    }
}