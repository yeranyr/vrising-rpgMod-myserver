using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("heat, 通缉", Usage = "heat", Description = "显示你的猎杀通缉信息.")]
    public static class Heat
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            var user = ctx.Event.User;
            var SteamID = user.PlatformId;
            var userEntity = ctx.Event.SenderUserEntity;
            var charEntity = ctx.Event.SenderCharacterEntity;

            if (!HunterHunted.isActive)
            {
                Output.CustomErrorMessage(ctx, "HunterHunted system is not enabled.");
                return;
            }

            bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "heat_args");
            if (ctx.Args.Length >= 2 && isAllowed)
            {
                string CharName = ctx.Event.User.CharacterName.ToString();
                if (ctx.Args.Length == 3)
                {
                    string name = ctx.Args[2];
                    if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                    {
                        SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                        CharName = name;
                        userEntity = targetUserEntity;
                        charEntity = targetEntity;
                    }
                    else
                    {
                        Output.CustomErrorMessage(ctx, $"Could not find specified player \"{name}\".");
                        return;
                    }
                }
                if (int.TryParse(ctx.Args[0], out var n)) Cache.heatlevel[SteamID] = n;
                if (int.TryParse(ctx.Args[1], out var nm)) Cache.bandit_heatlevel[SteamID] = nm;
                user.SendSystemMessage($"Player \"{CharName}\" heat value changed.");
                user.SendSystemMessage($"Human: <color=#ffff00ff>{Cache.heatlevel[SteamID]}</color> | Bandit: <color=#ffff00ff>{Cache.bandit_heatlevel[SteamID]}</color>");
                HunterHunted.HeatManager(userEntity, charEntity, false);
                return;
            }

            HunterHunted.HeatManager(userEntity, charEntity, false);

            Cache.heatlevel.TryGetValue(SteamID, out var human_heatlevel);
            if (human_heatlevel >= 3000) Output.SendLore(userEntity,$"<color=#0048ffff>[光明教会]</color> <color=#c90e21ff>教会已经无法阻止你了，你就是恐惧的化身...</color>");
            else if (human_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#0048ffff>[光明教会]</color> <color=#c90e21ff>吸血鬼猎人加入了猎杀，他们是你永恒的宿敌...</color>");
            else if (human_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#0048ffff>[光明教会]</color> <color=#c90e21ff>教会重整了猎杀队，人类的顶尖战力们正在追捕你...</color>");
            else if (human_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#0048ffff>[光明教会]</color> <color=#c4515cff>教会军队开始了对你的猎杀，只有精英士兵才能加入战斗...</color>");
            else if (human_heatlevel >= 250) Output.SendLore(userEntity, $"<color=#0048ffff>[光明教会]</color> <color=#c9999eff>教会张贴了对你的悬赏，普通的战士跃跃欲试...</color>");
            else Output.SendLore(userEntity, $"<color=#0048ffff>[光明教会]</color> <color=#ffffffff>你现在对教会来说是无名小卒...</color>");

            Cache.bandit_heatlevel.TryGetValue(SteamID, out var bandit_heatlevel);
            if (bandit_heatlevel >= 2000) Output.SendLore(userEntity, $"<color=#ff0000ff>[强盗团]</color> <color=#c90e21ff>强盗大王也想杀死你，但他无能为力...</color>");
            else if (bandit_heatlevel >= 1000) Output.SendLore(userEntity, $"<color=#ff0000ff>[强盗团]</color> <color=#c90e21ff>强盗团全体都对你恨之入骨...</color>");
            else if (bandit_heatlevel >= 500) Output.SendLore(userEntity, $"<color=#ff0000ff>[强盗团]</color> <color=#c4515cff>强盗精英们加入了埋伏你的队伍...</color>");
            else if (bandit_heatlevel >= 250) Output.SendLore(userEntity,$"<color=#ff0000ff>[强盗团]</color> <color=#c9999eff>强盗小队开始埋伏你...</color>");
            else Output.SendLore(userEntity, $"<color=#ff0000ff>[强盗团]</color> <color=#ffffffff>强盗们从未听说过你...</color>");

            if (ctx.Args.Length == 1 && user.IsAdmin)
            {
                if (!ctx.Args[0].Equals("debug") && ctx.Args.Length != 2) return;
                user.SendSystemMessage($"Heat Cooldown: {HunterHunted.heat_cooldown}");
                user.SendSystemMessage($"Bandit Heat Cooldown: {HunterHunted.bandit_heat_cooldown}");
                user.SendSystemMessage($"Cooldown Interval: {HunterHunted.cooldown_timer}");
                user.SendSystemMessage($"Ambush Interval: {HunterHunted.ambush_interval}");
                user.SendSystemMessage($"Ambush Chance: {HunterHunted.ambush_chance}");
                user.SendSystemMessage($"Human: <color=#ffff00ff>{human_heatlevel}</color> | Bandit: <color=#ffff00ff>{bandit_heatlevel}</color>");
            }
        }
    }
}
