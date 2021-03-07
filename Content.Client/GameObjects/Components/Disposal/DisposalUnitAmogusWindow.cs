#nullable enable
using System;
using Content.Client.Utility;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;

namespace Content.Client.GameObjects.Components.Disposal
{
    public class DisposalUnitAmogusWindow : SS14Window
    {
        private TextureRect Image { get; }

        public Action? OnEngaged;

        public DisposalUnitAmogusWindow()
        {
            AddChild(new VBoxContainer
            {
                Children =
                {
                    (Image = new TextureRect
                    {
                        Texture = IoCManager.Resolve<IResourceCache>()
                            .GetTexture("/Textures/Constructible/disposalup.png"),
                        MouseFilter = MouseFilterMode.Stop,
                    })
                }
            });

            Title = "Disposal Unit";
            Image.OnKeyBindDown += ImagePressed;
        }

        private void ImagePressed(GUIBoundKeyEventArgs obj)
        {
            Image.Texture = IoCManager.Resolve<IResourceCache>().GetTexture("/Textures/Constructible/disposaldown.png");

            OnEngaged?.Invoke();
        }
    }
}
