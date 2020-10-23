using FreeImageAPI;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using static StbTrueTypeSharp.StbTrueType;

namespace R
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Audio_WAV
    {
        public byte* buffer;
        public uint length;
        public SDL_AudioSpec spec;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Key_Value
    {
        public string key;
        public string value;
    }

    public unsafe struct RamTexture
    {
        public byte* data;
        public int width, height;
    }

    public static unsafe class AssetsLoader
    {

        public static void Init()
        {

        }

        public unsafe static bool LoadAudioWAV(string path, out Audio_WAV audio)
        {
            audio = new Audio_WAV();
            var _audio = new Audio_WAV();

            void* ptr = SDL.LoadWAV(path, &_audio.spec, &_audio.buffer, &_audio.length);

            if (ptr != null)
            {
                audio = _audio;
                return true;
            }

            return false;
        }

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

        public static bool LoadAsciiFont(string path, out Ascii_Font font)
        {
            font = new Ascii_Font();

            font.glyphs = new Ascii_Glyph[256];

            font = LoadFontData(path, font);

            bool loaded = LoadTexture(font.texture_path, out RamTexture ram_texture);

            if (loaded)
            {
                font = GenerateAsciiFontTexture(font, ram_texture);
                font.d_pixel_between_characters = 2;
                font.d_pixel_line_height = (int)(font.glyph_size_in_pixels * 1.25f);

                return true;
            }

            return false;
        }

        //texture format 16 x 16 tilemap.
        public static Ascii_Font GenerateAsciiFontTexture(Ascii_Font font, RamTexture texture, uint transperent_mask = 0xFF000000)
        {
            uint* pixel = (uint*)texture.data;

            int glyph_size = texture.width / 16;
            int width = texture.width / glyph_size;
            int height = texture.height / glyph_size;

            var glyphs = font.glyphs;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = x + (y * height);

                    glyphs[offset].min_x = -1;
                    glyphs[offset].max_x = -1;
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

        public static Ascii_Font LoadFontData(string path, Ascii_Font font)
        {
            var doc = LoadKeyValueDoc(path);

            for (int i = 0; i < doc.count; i++)
            {
                if (doc[i].key.Length == 1)
                {
                    font.glyphs[doc[i].key[0]].base_line_offset = int.Parse(doc[i].value);
                }
                else
                {
                    switch (doc[i].key)
                    {
                        case "texture_path":
                            font.texture_path = doc[i].value;
                            break;
                    }
                }
            }

            return font;
        }

        public static ArrayList<Key_Value> LoadKeyValueDoc(string path)
        {
            string src = File.ReadAllText(path).Replace("\r", "");

            ArrayList<Key_Value> data = new ArrayList<Key_Value>(128);

            var lines = src.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var values = lines[i].Split(' ');

                if (values.Length == 2)
                {
                    if (values[0].StartsWith("//"))
                    {
                        continue;
                    }

                    data.AddItem(new Key_Value()
                    {
                        key = values[0],
                        value = values[1],
                    });
                }
            }

            return data;
        }

        public static TTF_Font LoadTTFFontAsciiChars(string path)
        {
            char[] chars = new char[256];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[0] = (char)i;
            }

            return LoadTTFFont(path, chars);
        }

        public static TTF_Font LoadTTFFont(string path, char[] chars_to_load, int line_height = 128)
        {
            int texture_size = 2048;

            byte[] file = File.ReadAllBytes(path);
            TTF_Font font = new TTF_Font();
            font.info = new stbtt_fontinfo();
            font.texture_size = texture_size;
            font.line_height = line_height;

            fixed (byte* file_ptr = file)
            {
                stbtt_InitFont(font.info, file_ptr, stbtt_GetFontOffsetForIndex(file_ptr, 0));
            }

            int[] texture = new int[texture_size * texture_size];

            int xc = 0, yc = 0; // current x and current y
            int current_hightest_height = 0;

            font.glyphs = new TTF_Glyph[chars_to_load.Length];

            for (int i = 0; i < chars_to_load.Length; i++)
            {

                int glyph_width, glyph_height;
                int offset_x, offset_y;
                byte* char_bitmap = stbtt_GetCodepointBitmap(font.info, 0, stbtt_ScaleForPixelHeight(font.info, font.line_height), (byte)chars_to_load[i], &glyph_width, &glyph_height, &offset_x, &offset_y);

                if (xc + glyph_width >= texture_size)
                {
                    yc += current_hightest_height;
                    current_hightest_height = 0;
                    xc = 0;
                    Debug.Assert(yc + glyph_height <= texture_size, "no all glyphs cant fit in the texture!");
                }

                var char_bitmap_ptr = char_bitmap;

                for (int y = 0; y < glyph_height; y++)
                {
                    for (int x = 0; x < glyph_width; x++)
                    {
                        byte val = *char_bitmap_ptr;
                        char_bitmap_ptr++;

                        texture[(x + xc) + ((y + yc) * texture_size)] = val + (val << 8) + (val << 16) + (val << 24);
                    }
                }

                xc += glyph_width;
                current_hightest_height = glyph_height > current_hightest_height ? glyph_height : current_hightest_height;

                stbtt_FreeBitmap(char_bitmap, (void*)0);

                TTF_Glyph glyph = new TTF_Glyph();
                glyph.width = glyph_width;
                glyph.height = glyph_height;
                glyph.offset = new Vector2((float)offset_x / (float)texture_size, (float)offset_y / (float)texture_size);
                glyph.uv_top_right = new Vector2((float)xc / (float)texture_size, (float)yc / (float)texture_size);
                glyph.uv_top_right = new Vector2((float)(xc + glyph_width) / (float)texture_size, (float)(yc + glyph_height) / (float)texture_size);
            }

            fixed (int* texture_ptr = texture)
            {
                font.texture = GFX.BufferTexture(texture_ptr, texture_size, texture_size, TextureType.BIT_32);
            }

            font.glyph_lookup_table = new char[chars_to_load.Length];
            chars_to_load.CopyTo(font.glyph_lookup_table, 0);

            return font;
        }

    }
}
