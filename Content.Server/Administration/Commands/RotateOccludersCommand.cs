#nullable enable
using System;
using Content.Shared.Administration;
using Robust.Server.Interfaces.Console;
using Robust.Server.Interfaces.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Mapping)]
    public class RotateOccludersCommand : IClientCommand
    {
        public string Command => "rotateoccluders";
        public string Description => "Makes all occluders rotation match the input. If no input is supplied, it points them south.";
        public string Help => $"Usage: {Command} / {Command} <rotation>\nExample: {Command} east";

        public void Execute(IConsoleShell shell, IPlayerSession? player, string[] args)
        {
            Direction direction;

            switch (args.Length)
            {
                case 0:
                    direction = Direction.South;
                    break;
                case 1:
                    if (!Enum.TryParse(args[0], true, out direction))
                    {
                        shell.SendText(player, $"No direction found with name {args[0]}.\nValid directions: {Enum.GetValues<Direction>()}");
                        return;
                    }

                    break;
                default:
                    shell.SendText(player, $"Invalid amount of arguments: {args.Length}.\n{Help}");
                    return;
            }

            var angle = direction.ToAngle();
            var componentManager = IoCManager.Resolve<IComponentManager>();
            var occluders = componentManager.EntityQuery<OccluderComponent>();
            var i = 0;

            foreach (var occluder in occluders)
            {
                occluder.Owner.Transform.WorldRotation = angle;
                i++;
            }

            shell.SendText(player, $"Changed the rotation of {i} occluders to {direction}.");
        }
    }
}
