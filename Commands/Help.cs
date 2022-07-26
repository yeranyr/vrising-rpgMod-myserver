using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("help, h, 帮助", Usage = "help [<command>]" , Description = "显示命令列表和帮助", ReqPermission = 0)]
    public static class Help
    {
        public static void Initialize(Context ctx)
        {
            List<string> commands = new List<string>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();
            try
            {
                if (types.Any(x => x.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases.First() == ctx.Args[0].ToLower())))
                {
                    var type = types.First(x => x.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases.First() == ctx.Args[0].ToLower()));

                    List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
                    if (ctx.DisabledCommands.Any(x => x.ToLower() == aliases.First().ToLower())) return;
                    string usage = type.GetAttributeValue((CommandAttribute cmd) => cmd.Usage);
                    string description = type.GetAttributeValue((CommandAttribute cmd) => cmd.Description);
                    if (!Database.command_permission.TryGetValue(aliases[0], out var reqPermission)) reqPermission = 100;
                    if (!Database.user_permission.TryGetValue(ctx.Event.User.PlatformId, out var userPermission)) userPermission = 0;

                    if (userPermission < reqPermission && !ctx.Event.User.IsAdmin)
                    {
                        ctx.Event.User.SendSystemMessage($"Specified command not found.");
                        return;
                    }
                    ctx.Event.User.SendSystemMessage($"帮助信息： <color=#00ff00ff>{ctx.Prefix}{aliases.First()}</color>");
                    ctx.Event.User.SendSystemMessage($"<color=#ffffffff>别名: {string.Join(", ", aliases)}</color>");
                    ctx.Event.User.SendSystemMessage($"<color=#ffffffff>描述: {description}</color>");
                    ctx.Event.User.SendSystemMessage($"<color=#ffffffff>用法: {ctx.Prefix}{usage}</color>");
                    return;
                }
                else
                {
                    ctx.Event.User.SendSystemMessage($"Specified command not found.");
                    return;
                }
            }
            catch
            {
                ctx.Event.User.SendSystemMessage("所有指令列表:");
                foreach (Type type in types)
                {
                    List<string> aliases = type.GetAttributeValue((CommandAttribute cmd) => cmd.Aliases);
                    if (ctx.DisabledCommands.Any(x => x.ToLower() == aliases.First().ToLower())) continue;
                    string description = type.GetAttributeValue((CommandAttribute cmd) => cmd.Description);
                    if (!Database.command_permission.TryGetValue(aliases[0], out var reqPermission)) reqPermission = 100;
                    if (!Database.user_permission.TryGetValue(ctx.Event.User.PlatformId, out var userPermission)) userPermission = 0;

                    string s = "";
                    bool send = false;
                    if (userPermission < reqPermission && ctx.Event.User.IsAdmin)
                    {
                        s = $"<color=#00ff00ff>{ctx.Prefix}{string.Join("/@", aliases)}</color> - <color=#ff0000ff>[{reqPermission}]</color> <color=#ffffffff>{description}</color>";
                        //s = $"<color=#00ff00ff>{ctx.Prefix}{aliases.First()}/{string.Join(", ", aliases)}</color> - <color=#ff0000ff>[ADMIN]</color> <color=#ffffffff>{description}</color>";
                        send = true;
                    }
                    else if (userPermission >= reqPermission)
                    {
                        s = $"<color=#00ff00ff>{ctx.Prefix}{string.Join("/@", aliases)}</color> - <color=#ffffffff>{description}</color>";
                        //s = $"<color=#00ff00ff>{ctx.Prefix}{aliases.First()}/{string.Join(", ", aliases)}</color> - <color=#ffffffff>{description}</color>";
                        send = true;
                    }
                    if (send) ctx.Event.User.SendSystemMessage(s);
                }
            }
        }

        public static void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/RPGMods/permissions.json")) File.Create("BepInEx/config/RPGMods/permissions.json");
            string json = File.ReadAllText("BepInEx/config/RPGMods/permissions.json");
            try
            {
                CommandHandler.Permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            }
            catch
            {
                CommandHandler.Permissions = new Dictionary<string, bool>();
            }
        }
    }
}
