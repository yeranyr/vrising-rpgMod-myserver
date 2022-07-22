using ProjectM.Network;
using RPGMods.Utils;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("ping, p, 延迟", Usage = "ping", Description = "显示你的延迟.")]
    public static class Ping
    {
        public static void Initialize(Context ctx)
        {
            var ping = ctx.EntityManager.GetComponentData<Latency>(ctx.Event.SenderCharacterEntity).Value;
            ctx.Event.User.SendSystemMessage($"你的延迟是 <color=#ffff00ff>{ping}</color>ms");
        }
    }
}
