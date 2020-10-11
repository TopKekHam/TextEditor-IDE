using System;
using System.Numerics;

namespace R
{
    public class UIE_Button : UIElement
    {
        public Action on_click;

        UIE_Text label;

        Vector4 color;
        bool active_last_frame = false;

        public UIE_Button(string text, UIContext context) : this()
        {
            label = new UIE_Text(text);
            label.rect.horizontal_sizing = RectSizing.Strech;
            label.rect.vertical_sizing = RectSizing.Strech;
            label.rect.parent = rect;
            label.rect.transform.position.Z += rect.transform.position.Z + 0.1f;

            rect.width = label.rect.width + 8;
            rect.height = label.rect.height + 8;

            color = UI.state.style.primary_color;

            context.elements.Add(label);  
        }

        public UIE_Button()
        {
            clickable = true;
        }

        public override void Render(Vector2 canvas_size)
        {
            UI.DrawRect(rect, Renderer.CreateColorMaterail(color));
        }

        public override void Update(Vector2 canvas_size)
        {
            if (active_item)
            {
                color = UI.state.style.primary_color * 4f;
            }
            else
            {
                color = UI.state.style.primary_color;
            }

            if (!active_item && active_last_frame && hot_item)
            {
                on_click?.Invoke();
            }

            active_last_frame = active_item;
        }
    }
}
