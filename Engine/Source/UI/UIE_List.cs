using System.Collections.Generic;
using System.Numerics;

namespace R
{

    public enum UIE_List_Oriantation
    {
        Vertical, Horizontal
    }

    public class UIE_List : UIElement
    {

        public List<RectTransform> elements = new List<RectTransform>();
        public UIE_List_Oriantation oriantation = UIE_List_Oriantation.Vertical;

        public void AddElement(UIElement element, UIContext context)
        {
            element.rect.parent = rect;
            elements.Add(element.rect);
            context.elements.Add(element);
        }

        public override void Render(Vector2 canvas_size)
        {
            UI.DrawRect(rect, Renderer.CreateColorMaterail(UI.state.style.primary_color));
        }

        public override void Update(Vector2 canvas_size)
        {
            if (oriantation == UIE_List_Oriantation.Vertical)
            {
                if(elements.Count > 0)
                {
                    elements[0].vertical_alignment = RectAlignment.Start;
                }

                for (int i = 1; i < elements.Count; i++)
                {
                    elements[i].vertical_alignment = RectAlignment.Start;
                    elements[i].transform.position.Y = elements[i - 1].transform.position.Y + elements[i].height;
                }
            }
            else if (oriantation == UIE_List_Oriantation.Horizontal)
            {
                if (elements.Count > 0)
                {
                    elements[0].horizontal_alignment = RectAlignment.Start;
                }

                for (int i = 1; i < elements.Count; i++)
                {
                    elements[i].horizontal_alignment = RectAlignment.Start;
                    elements[i].transform.position.X = elements[i - 1].transform.position.X + elements[i].width;
                }
            }
        }
    }
}
