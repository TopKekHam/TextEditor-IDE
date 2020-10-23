using System.Collections.Generic;
using System.Numerics;

namespace R
{

    public struct UIState
    {
        public UIStyle style;
        public Vector2 canvas_size;
        public Vector2 mouse_position_on_canvas;
    }

    public enum RectStyleType
    {
        Color, Texture
    }

    public struct RectStyle
    {
        public RectStyleType type;
        public Vector4 color;
        public uint texture;
    }

    public struct UIStyle
    {
        public Ascii_Font text_font;
        public Vector4 text_color;
        public int text_size;
        public Vector4 primary_color;
        public uint pointer_texture;
    }

    public abstract class UIElement
    {
        public bool hot_item;
        public bool active_item;
        public bool clickable = false;
        public RectTransform rect = new RectTransform();
        public abstract void Update(Vector2 canvas_size);
        public abstract void Render(Vector2 canvas_size);
    }

    public class UIContext
    {
        public List<UIElement> elements = new List<UIElement>();
        public UIElement HotItem;
        public UIElement ActiveItem;
    }

    public static class UI
    {

        public static void Init()
        {
            state = new UIState
            {
                style = new UIStyle
                {
                    text_color = new Vector4(0.95f, 0.295f, 0.45f, 1),
                    text_size = 16,
                    primary_color = new Vector4(0.15f, 0.15f, 0.15f, 1)
                }
            };
        }

        public static UIState state;

        public static void SetupCamera(float width, float height)
        {
            state.canvas_size = new Vector2(width, height);
            float x = Engine.mouse_state.mouse_position.X / Engine.window_size.X * state.canvas_size.X;
            float y = Engine.mouse_state.mouse_position.Y / Engine.window_size.Y * state.canvas_size.Y;
            state.mouse_position_on_canvas = new Vector2(x - (state.canvas_size.X / 2),  y + (-state.canvas_size.Y / 2));

            Renderer.CameraPosition = Transform.Zero;
            Renderer.SetCameraSize(width / 2, height / 2);
            GFX.DisableDepthTest();
        }

        public static void UpdateAndRender(float width, float height, UIContext context)
        {
            SetupCamera(width, height);
            bool sorted = false;

            while (!sorted)
            {
                sorted = true;

                for (int i = 1; i < context.elements.Count; i++)
                {
                    if (context.elements[i - 1].rect.transform.position.Z < context.elements[i].rect.transform.position.Z)
                    {
                        sorted = false;
                        var temp = context.elements[i - 1];
                        context.elements[i - 1] = context.elements[i];
                        context.elements[i] = temp;
                    }
                }
            }

            if (context.HotItem != null)
            {
                context.HotItem.hot_item = false;
                context.HotItem = null;
            }

            for (int i = 0; i < context.elements.Count; i++)
            {
                if (context.elements[i].clickable &&
                    context.elements[i].rect.PointInside(state.canvas_size, state.mouse_position_on_canvas))
                {
                    context.HotItem = context.elements[i];
                    context.HotItem.hot_item = true;
                    break;
                }
            }

            if (Engine.mouse_state.left.HasFlag(InputState.PRESSED))
            {
                if (context.ActiveItem == null && context.HotItem != null)
                {
                    context.ActiveItem = context.HotItem;
                    context.ActiveItem.active_item = true;
                }
            }
            else
            {
                if (context.ActiveItem != null)
                {
                    context.ActiveItem.active_item = false;
                    context.ActiveItem = null;
                }
            }

            for (int i = context.elements.Count - 1; i >= 0; i--)
            {
                context.elements[i].Update(state.canvas_size);
                context.elements[i].Render(state.canvas_size);
            }

        }

        public static void Update(float width, float height, UIContext context)
        {
            state.canvas_size = new Vector2(width, height);
            float x = Engine.mouse_state.mouse_position.X / Engine.window_size.X * state.canvas_size.X;
            float y = Engine.mouse_state.mouse_position.Y / Engine.window_size.Y * state.canvas_size.Y;
            state.mouse_position_on_canvas = new Vector2(x - (state.canvas_size.X / 2), y + (-state.canvas_size.Y / 2));

            bool sorted = false;

            while (!sorted)
            {
                sorted = true;

                for (int i = 1; i < context.elements.Count; i++)
                {
                    if (context.elements[i - 1].rect.transform.position.Z < context.elements[i].rect.transform.position.Z)
                    {
                        sorted = false;
                        var temp = context.elements[i - 1];
                        context.elements[i - 1] = context.elements[i];
                        context.elements[i] = temp;
                    }
                }
            }

            if (context.HotItem != null)
            {
                context.HotItem.hot_item = false;
                context.HotItem = null;
            }

            for (int i = 0; i < context.elements.Count; i++)
            {
                if (context.elements[i].clickable &&
                    context.elements[i].rect.PointInside(state.canvas_size, state.mouse_position_on_canvas))
                {
                    context.HotItem = context.elements[i];
                    context.HotItem.hot_item = true;
                    break;
                }
            }

            if (Engine.mouse_state.left.HasFlag(InputState.PRESSED))
            {
                if (context.ActiveItem == null && context.HotItem != null)
                {
                    context.ActiveItem = context.HotItem;
                    context.ActiveItem.active_item = true;
                }
            }
            else
            {
                if (context.ActiveItem != null)
                {
                    context.ActiveItem.active_item = false;
                    context.ActiveItem = null;
                }
            }

            for (int i = context.elements.Count - 1; i >= 0; i--)
            {
                context.elements[i].Update(state.canvas_size);
            }


        }

        public static void Render(float width, float height, UIContext context)
        {
            Renderer.CameraPosition = Transform.Zero;
            Renderer.SetCameraSize(width / 2, height / 2);
            GFX.DisableDepthTest();

            for (int i = context.elements.Count - 1; i >= 0; i--)
            {
                context.elements[i].Render(state.canvas_size);
            }
        }

        public static Material CreateMaterialFromRectStyle(RectStyle style)
        {
            if (style.type == RectStyleType.Color)
            {
                return Renderer.CreateColorMaterail(style.color);
            }
            else if (style.type == RectStyleType.Texture)
            {
                return Renderer.CreateImageMaterail(style.texture);
            }

            return Renderer.CreateColorMaterail(Vector4.Zero);
        }

        public static void Text(Vector3 position, string text)
        {
            Transform tran = Transform.Zero;
            tran.position = position;

            Renderer.DrawTextAscii(tran, state.style.text_font, text, state.style.text_color, state.style.text_size);
        }

        public static void TextCursor(Vector3 position, string text, int char_idx)
        {
            Transform tran = Transform.Zero;
            tran.position = position + Ascii_Font_Utils.GetCharPosition(state.style.text_font, text, state.style.text_size, char_idx);
            tran.position.Y += -state.style.text_size + 2;

            Renderer.DrawTextAscii(tran, state.style.text_font, "_", state.style.text_color, state.style.text_size);
        }

        public static void DrawCursor(uint texture, float size = 16)
        {
            Transform tran = Transform.Zero;

            tran.position.X = state.mouse_position_on_canvas.X + (size / 2);
            tran.position.Y = state.mouse_position_on_canvas.Y + (size / 2);

            Renderer.FlipY = true;
            Renderer.DrawQuad(tran, new Vector2(size, size), Renderer.CreateImageMaterail(texture));
            Renderer.FlipY = false;
        }

        public static void DrawRect(RectTransform transform, Material material)
        {
            Transform tran = Transform.Zero;
            tran.position = transform.CalcPosition(state.canvas_size);
            var size = transform.CalcRectSize(state.canvas_size);

            Renderer.DrawQuad(tran, size, material);
        }

    }

    public enum RectAlignment
    {
        Start, Center, End
    }

    public enum RectSizing
    {
        OneToOne, Strech, Percent
    }

    public class RectTransform
    {
        public Transform transform = Transform.Zero;
        public float width = 100, height = 100;
        public RectTransform parent = null;
        public RectAlignment horizontal_alignment = RectAlignment.Center, vertical_alignment = RectAlignment.Center;
        public RectSizing horizontal_sizing = RectSizing.OneToOne, vertical_sizing = RectSizing.OneToOne;

        public Vector2 CalcRectSize(Vector2 canvas)
        {
            Vector2 size = Vector2.Zero;

            Vector2 parent_size = canvas;

            if (parent != null)
            {
                parent_size = parent.CalcRectSize(canvas);
            }

            size.X = CalcSize(horizontal_sizing, width, parent_size.X);
            size.Y = CalcSize(vertical_sizing, height, parent_size.Y);

            return size;
        }

        public float CalcSize(RectSizing sizing, float value, float parent_size)
        {
            float end_value = 0;

            if (sizing == RectSizing.OneToOne)
            {
                return value;
            }
            else if (sizing == RectSizing.Strech)
            {
                return parent_size;
            }
            else if (sizing == RectSizing.Percent)
            {
                return parent_size * value;
            }

            return end_value;
        }

        public Vector3 CalcPosition(Vector2 canvas)
        {
            Vector3 pos = transform.position;
            Vector2 size = CalcRectSize(canvas);

            Vector2 parent_size = canvas;
            Vector3 parent_position = Vector3.Zero;

            if (parent != null)
            {
                parent_size = parent.CalcRectSize(canvas);
                parent_position = parent.CalcPosition(canvas);
            }

            pos.X = CalcAlignment(horizontal_alignment, transform.position.X, size.X / 2, parent_position.X, parent_size.X / 2);
            pos.Y = CalcAlignment(vertical_alignment, transform.position.Y, size.Y / 2, parent_position.Y, parent_size.Y / 2);

            return pos;
        }

        public float CalcAlignment(RectAlignment alignment, float value, float half_size, float parent_value, float parent_half_size)
        {
            float end_value = 0;

            if (alignment == RectAlignment.Start)
            {
                end_value += (-parent_half_size + parent_value) + (half_size + value);
            }
            else if (alignment == RectAlignment.Center)
            {
                end_value += (parent_value) + (value);
            }
            else if (alignment == RectAlignment.End)
            {
                end_value += (parent_half_size + parent_value) + (-half_size + value);
            }

            return end_value;
        }
        
        public bool PointInside(Vector2 canvas, Vector2 point)
        {
            var pos = CalcPosition(canvas);
            var half_size = CalcRectSize(canvas) / 2;

            return (pos.X - half_size.X < point.X &&
                    pos.X + half_size.X > point.X &&
                    pos.Y - half_size.Y < point.Y &&
                    pos.Y + half_size.Y > point.Y);
        }
    }

/**
    public class Button_Old : UIElement
    {
        public string text;
        public float padding_in_pixels;
        public Action on_click;

        public Button_Old(Vector3 position, string _text, float _padding_in_pixels, Action _on_click)
        {
            text = _text;
            padding_in_pixels = _padding_in_pixels;
            on_click = _on_click;

            rect = new Rect();
            rect.position = position;
            rect.alignment = Alignment.Left | Alignment.Top;

            float padding = padding_in_pixels;
            float width = MeshGenerator.GetTextWidth(UI.state.style.text_font, text, UI.state.style.text_size) + (padding * 2);
            float height = MeshGenerator.GetTextHeight(UI.state.style.text_font, text, UI.state.style.text_size) + (padding * 2);

            rect.half_width = new SizeFloat() { type = SizeType.Pixels, value = width / 2 };
            rect.half_height = new SizeFloat() { type = SizeType.Pixels, value = height / 2 };

            style = new RectStyle();
            style.type = RectStyleType.Color;
            style.color = UI.state.style.primary_color;
        }

        RectStyle style;

        bool active_last_frame = false;

        public override void Render()
        {
            UI.DrawRect(rect, style);
            UI.DrawTextInRect(rect, text);
        }

        public override void Update()
        {
            if (active_item)
            {
                style.color = UI.state.style.primary_color * 4f;
            }
            else
            {
                style.color = UI.state.style.primary_color;
            }

            if (!active_item && active_last_frame && hot_item)
            {
                on_click();
            }

            active_last_frame = active_item;
        }
    }

    public class FPSCounter_Old : UIElement
    {

        public FPSCounter_Old()
        {
            rect = new Rect();
            rect.alignment = Alignment.Bottom | Alignment.Right;
        }

        List<float> fpses = new List<float>();
        float fps = 0;

        public override void Render()
        {
            string text = $"{(1 / fps):n0}";
            float width = MeshGenerator.GetTextWidth(UI.state.style.text_font, text, UI.state.style.text_size);
            float height = MeshGenerator.GetTextHeight(UI.state.style.text_font, text, UI.state.style.text_size);

            rect.half_width = new SizeFloat() { type = SizeType.Pixels, value = width / 2 + 2 };
            rect.half_height = new SizeFloat() { type = SizeType.Pixels, value = height / 2 + 2 };
            rect.position = new Vector3(UI.state.canvas_size.X - (2 * UI.ToPixels(rect.half_width)), UI.state.canvas_size.Y, 1);

            UI.DrawTextInRect(rect, text);
        }

        public override void Update()
        {
            if (fpses.Count > 32)
            {
                fpses.RemoveAt(0);
            }

            fpses.Add(Engine.delta_time);

            for (int i = 0; i < fpses.Count; i++)
            {
                fps += fpses[i];
            }

            fps /= fpses.Count;
        }

    }
**/
}
