using System;
using Button = UnityEngine.Dt.App.UI.Button;

namespace UnityEngine.Dt.App.Editor
{
    public class ButtonPage : StoryBookPage
    {
        public override string displayName => "Button";

        public override Type componentType => typeof(ButtonComponent);

        public ButtonPage()
        {
            m_Stories.Add(new StoryBookStory("Primary", () => new Button { primary = true, title = "Primary Style Button" }));
        }
    }

    public class ButtonComponent : StoryBookComponent
    {
        public override Type uiElementType => typeof(Button);

        public ButtonComponent()
        {
            m_Properties.Add(new StoryBookBooleanProperty(
                nameof(Button.primary),
                (btn) => ((Button)btn).primary,
                (btn, val) => ((Button)btn).primary = val));

            m_Properties.Add(new StoryBookStringProperty(
                nameof(Button.title),
                (btn) => ((Button)btn).title,
                (btn, val) => ((Button)btn).title = val));
        }
    }
}
