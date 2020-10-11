using FreeImageAPI;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace R
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Glyph
    {
        public Vector2 uv_bottom_left;
        public Vector2 uv_top_right;
        public float width, height;
        public int min_x, max_x;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FontAscii
    {
        public Glyph[] glyphs;
        public uint texture;
        public int texture_width, texture_height, glyph_size_in_pixels;
        public float pixel_size;
        public int d_pixel_between_characters;
        public int d_pixel_line_height;
    }

    public unsafe struct RamTexture
    {
        public byte* data;
        public int width, height;
    }

    public static unsafe class AssetsLoader
    {

        public static bool LoadTexture(string path, out RamTexture texture)
        {
            texture = new RamTexture();

            FIBITMAP image;

            image = FreeImage.LoadEx(path);

            if (image.IsNull) return false;

            //Console.WriteLine($"loaded image width bpp: {FreeImage.GetBPP(image)}");

            texture.width = (int)FreeImage.GetWidth(image);
            texture.height = (int)FreeImage.GetHeight(image);
            texture.data = (byte*)FreeImage.GetBits(image);

            return true;
        }

        public static bool LoadTexture(string path, out uint texture)
        {
            texture = 0;

            var loaded = LoadTexture(path, out RamTexture tex);

            if (loaded)
            {
                texture = GFX.BufferTexture(tex.data, tex.width, tex.height, TextureType.BIT_32);
                return true;
            }

            return false;
        }

        public static bool LoadAsciiFont(string path, out FontAscii font)
        {
            font = new FontAscii();
            bool loaded = LoadTexture(path, out RamTexture ram_texture);

            if (loaded)
            {
                font = GenerateAsciiFont(ram_texture);
                font.d_pixel_between_characters = 2;
                font.d_pixel_line_height = (int)(font.glyph_size_in_pixels * 1.25f);
                return true;
            }

            return false;
        }

        //texture format 16 x 16 tilemap.
        public static FontAscii GenerateAsciiFont(RamTexture texture, uint transperent_mask = 0xFF000000)
        {
            uint* pixel = (uint*)texture.data;

            int glyph_size = texture.width / 16;
            int width = texture.width / glyph_size;
            int height = texture.height / glyph_size;

            FontAscii font = new FontAscii();
            Glyph[] glyphs = new Glyph[256]; // Asci table.

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = x + (y * height);

                    glyphs[offset] = new Glyph()
                    {
                        min_x = -1, max_x = -1
                    };
                }
            }

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    int glyph_x = x / glyph_size;
                    int glyph_y = y / glyph_size;
                    int offset = glyph_x + ((15 - glyph_y) * 16);

                    if ((*pixel & transperent_mask) == transperent_mask)
                    {
                        if (glyphs[offset].min_x > x || glyphs[offset].min_x == -1) glyphs[offset].min_x = x;
                        if (glyphs[offset].max_x < x || glyphs[offset].max_x == -1) glyphs[offset].max_x = x;
                        //if (glyphs[offset].min_y > y || glyphs[offset].min_y == -1) glyphs[offset].min_y = y;
                        //if (glyphs[offset].max_y < y || glyphs[offset].max_y == -1) glyphs[offset].max_y = y;
                    }

                    pixel += 1;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = x + (y * height);

                    glyphs[offset].uv_top_right = new Vector2((float)(glyphs[offset].max_x + 1) / (float)texture.width,
                                                              (float)((16 - y) * glyph_size) / (float)texture.height);

                    glyphs[offset].uv_bottom_left = new Vector2((float)(glyphs[offset].min_x) / (float)texture.width,
                                                                (float)((15 - y) * glyph_size) / (float)texture.height); 

                    if (offset == (int)' ')
                    {
                        glyphs[offset].width = ((float)glyph_size / 2.0f) / (float)texture.width;
                    }
                    else
                    {
                        glyphs[offset].width = (float)((glyphs[offset].max_x - glyphs[offset].min_x) + 1) / (float)texture.width;
                    }

                    glyphs[offset].height = (float)glyph_size / (float)texture.height;
                }
            }

            font.glyphs = glyphs;
            font.glyph_size_in_pixels = glyph_size;
            font.pixel_size = 1.0f / (16.0f * glyph_size);
            font.texture = GFX.BufferTexture(texture.data, texture.width, texture.height, TextureType.BIT_32);

            return font;
        }

    }
}
