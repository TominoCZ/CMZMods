using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Modding;
using ResourcePacks.Gui;
using System.Collections.Generic;

namespace ResourcePacks.Packs
{
    class MyGuiHandler : GuiHandler
    {
        Queue<OptionsScreen> _queue = new Queue<OptionsScreen>();

        protected override void OnCreated(Screen screen)
        {
            if (screen is OptionsScreen os)
            {
                _queue.Enqueue(os);
            }
        }

        protected override void OnShown(Screen screen)
        {
            if (screen is OptionsScreen)
            {
                while (_queue.Count > 0)
                {
                    var control = _queue.Dequeue().GetValue<TabControl>("tabControl");

                    control.Tabs.Add(new TexturesTab());
                }
            }
        }
    }
}
