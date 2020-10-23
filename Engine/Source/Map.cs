using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace R
{

    [Flags]
    public enum TileType
    {
        Single = 1, 
        NineSlice = 2,
        Animated = 4, 
    }

    public struct TileDefinition
    {
        public TileType type;
        public int index; // top left corner is [0,0] or index 0.
        public int[] animation_frames; // offset from the index in atlas.
    }

    public struct Tileset
    {
        public int tile_size; // size of tile in pixels.
        public string texture; // path to textures ("/Assets/<path>").
        public TileDefinition[] tile_definitions; // definitions of all tiles
    }

    public struct TileAtlas
    {
        public Tileset[] tilesets;
        public uint tileatlas_texture;
    }

    public struct Tilemap
    {
        public TileAtlas tileset;
        public int[,,] tiles; // layer, x, y
    }

    public struct TilemapRenderer
    {
        public Tilemap tilemap;
        public Mesh tilemap_mesh;
        public TileAnimation[] tile_animations;
    }

    public struct TileAnimation
    {
        float time;
        int current_frame;
    }
}
