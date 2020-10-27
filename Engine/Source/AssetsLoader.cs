using FreeImageAPI;
using System;
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

        public static bool LoadAsciiFont(string path, ref Ascii_Font font)
        {
            font = new Ascii_Font();

            font.glyphs = new Ascii_Glyph[256];

            LoadFontData(path, ref font);

            bool loaded = LoadTexture(font.texture_path, out RamTexture ram_texture);

            if (loaded)
            {
                font.texture_width = ram_texture.width;
                font.texture_height = ram_texture.height;
                int glyph_size = font.texture_width / 16;
                font.pixel_size = 1.0f / (16.0f * glyph_size);
                InitAsciiFontSizes(ref font, glyph_size);

                GenerateAsciiFontTexture(ref font, ram_texture);

                font.texture = GFX.BufferTexture(ram_texture.data, ram_texture.width, ram_texture.height, TextureType.BIT_32);

                return true;
            }

            return false;
        }

        public static bool LoadAsciiFontFromTTFFile(string path, ref Ascii_Font font, int glyph_size)
        {
            font.glyphs = new Ascii_Glyph[256];
            float padding_ratio = 1.25f;
            int glyph_cell_size = (int)(glyph_size * padding_ratio);
            int texture_size = 16 * glyph_cell_size;
            
            InitAsciiFontSizes(ref font, glyph_cell_size);

            font.natural_offset = glyph_cell_size - glyph_size;
            font.texture_width = texture_size;
            font.texture_height = texture_size;
            font.pixel_size = 1.0f / (float)(glyph_cell_size * 16);

            stbtt_fontinfo info = new stbtt_fontinfo();
            try
            {
                byte[] file = File.ReadAllBytes(path);
                fixed (byte* file_ptr = file)
                {
                    stbtt_InitFont(info, file_ptr, stbtt_GetFontOffsetForIndex(file_ptr, 0));

                }
            }
            catch (Exception) { Console.WriteLine($"Couldn't load font: {path}"); return false; }

            int[] texture = new int[texture_size * texture_size];

            int xc = 0, yc = font.texture_height - 1; // current x and current y

            int x0, x1, y0, y1;
            stbtt_GetFontBoundingBox(info, &x0, &y0, &x1, &y1);

            for (int glyph_idx = 0; glyph_idx < 256; glyph_idx++)
            {


                int glyph_width, glyph_height;
                int offset_x, offset_y;
                byte* char_bitmap = stbtt_GetCodepointBitmap(info, 0, stbtt_ScaleForPixelHeight(info, glyph_size), (byte)glyph_idx, &glyph_width, &glyph_height, &offset_x, &offset_y);

                if (xc + glyph_width >= texture_size)
                {
                    yc -= glyph_cell_size;
                    xc = 0;
                }

                var char_bitmap_ptr = char_bitmap;
                var y_start = glyph_cell_size - glyph_height;

                //int draw_color = ((glyph_idx + (glyph_idx / 16) % 2) % 2 == 0) ? (255 + (255 << 8) + (255 << 24)) : ((255 << 8) + (255 << 16) + (255 << 24));
                //for (int y = 0; y < glyph_cell_size; y++)
                //{
                //    for (int x = 0; x < glyph_width; x++)
                //    {
                //        texture[(x + xc) + ((yc - y) * texture_size)] = draw_color;
                //    }
                //}

                for (int y = 0; y < glyph_height; y++)
                {
                    for (int x = 0; x < glyph_width; x++)
                    {
                        byte val = *char_bitmap_ptr;
                        char_bitmap_ptr++;

                        texture[(x + xc) + ((yc - y - y_start) * texture_size)] = val + (val << 8) + (val << 16) + (val << 24);
                    }
                }

                //float glyph_start_xf = (float)xc / (float)texture_size;
                //float glyph_widthf = (float)glyph_width / (float)texture_size;
                //float glyph_start_yf = (float)(yc - y_start) / (float)texture_size;
                //float glyph_heightf = (float)glyph_height / (float)texture_size;

                //if ((char)glyph_idx == ' ')
                //{
                //    glyph_widthf = ((float)glyph_size / 2.5f) / (float)texture_size;
                //}

                //font.glyphs[glyph_idx].uv_top_right = new Vector2(glyph_start_xf + glyph_widthf, glyph_start_yf);
                //font.glyphs[glyph_idx].uv_bottom_left = new Vector2(glyph_start_xf, glyph_start_yf - glyph_heightf);
                //font.glyphs[glyph_idx].width = glyph_widthf;
                //font.glyphs[glyph_idx].height = glyph_heightf;



                font.glyphs[glyph_idx].base_line_offset = (glyph_cell_size - glyph_height) - offset_y - glyph_cell_size;
                xc += glyph_cell_size;
                stbtt_FreeBitmap(char_bitmap, (void*)0);
            }

            fixed (void* ptr = texture)
            {
                RamTexture tex = new RamTexture()
                {
                    data = (byte*)ptr,
                    width = texture_size,
                    height = texture_size
                };

                GenerateAsciiFontTexture(ref font, tex);
            }

            fixed (int* texture_ptr = texture)
            {
                font.texture = GFX.BufferTexture(texture_ptr, texture_size, texture_size, TextureType.BIT_32);
            }

            font.d_ratio_between_characters /= padding_ratio;
            font.d_ratio_line_height /= padding_ratio;

            return true;
        }

        static void InitAsciiFontSizes(ref Ascii_Font font, int glyph_size)
        {
            font.glyph_size_in_pixels = glyph_size;
            font.d_ratio_between_characters = 1f;
            font.d_ratio_line_height = 1.125f;
        }

        //texture format 16 x 16 tilemap.
        static void GenerateAsciiFontTexture(ref Ascii_Font font, RamTexture texture, uint transperent_mask = 0xFF000000)
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
                    glyphs[offset].min_y = -1;
                    glyphs[offset].max_y = -1;
                }
            }

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    int glyph_x = x / glyph_size;
                    int glyph_y = y / glyph_size;
                    int offset = glyph_x + ((15 - glyph_y) * 16);

                    if ((*pixel & transperent_mask) > 0)
                    {
                        if (glyphs[offset].min_x > x || glyphs[offset].min_x == -1) glyphs[offset].min_x = x;
                        if (glyphs[offset].max_x < x || glyphs[offset].max_x == -1) glyphs[offset].max_x = x;

                        if (glyphs[offset].min_y > y || glyphs[offset].min_y == -1) glyphs[offset].min_y = y;
                        if (glyphs[offset].max_y < y || glyphs[offset].max_y == -1) glyphs[offset].max_y = y;
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
                                                              (float)(glyphs[offset].max_y + 1) / (float)texture.height);

                    glyphs[offset].uv_bottom_left = new Vector2((float)(glyphs[offset].min_x) / (float)texture.width,
                                                                (float)(glyphs[offset].min_y) / (float)texture.height);

                    if (offset == (int)' ')
                    {
                        glyphs[offset].width = ((float)glyph_size / 2.0f) / (float)texture.width;
                    }
                    else
                    {
                        glyphs[offset].width = (float)((glyphs[offset].max_x - glyphs[offset].min_x) + 1) / (float)texture.width;
                    }

                    glyphs[offset].height = (float)((glyphs[offset].max_y - glyphs[offset].min_y) + 1) / (float)texture.height;
                }
            }
        }

        public static void LoadFontData(string path,ref Ascii_Font font)
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

    }
}
