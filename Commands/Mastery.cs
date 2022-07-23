﻿using ProjectM;
using ProjectM.Network;
using RPGMods.Systems;
using RPGMods.Utils;
using System;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Commands
{
    [Command("mastery, m, 精通", Usage = "mastery [<log> <on>|<off>]", Description = "显示你的当前精通进度，或切换通知.")]
    public static class Mastery
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;
        public static void Initialize(Context ctx)
        {
            if (!WeaponMasterSystem.isMasteryEnabled)
            {
                Output.CustomErrorMessage(ctx, "Weapon Mastery system is not enabled.");
                return;
            }
            var SteamID = ctx.Event.User.PlatformId;

            if (ctx.Args.Length > 1)
            {
                if (ctx.Args[0].ToLower().Equals("set") && ctx.Args.Length >= 3)
                {
                    bool isAllowed = ctx.Event.User.IsAdmin || PermissionSystem.PermissionCheck(ctx.Event.User.PlatformId, "mastery_args");
                    if (!isAllowed) return;
                    if (int.TryParse(ctx.Args[2], out int value))
                    {
                        string CharName = ctx.Event.User.CharacterName.ToString();
                        var UserEntity = ctx.Event.SenderUserEntity;
                        var CharEntity = ctx.Event.SenderCharacterEntity;
                        if (ctx.Args.Length == 4)
                        {
                            string name = ctx.Args[3];
                            if (Helper.FindPlayer(name, true, out var targetEntity, out var targetUserEntity))
                            {
                                SteamID = entityManager.GetComponentData<User>(targetUserEntity).PlatformId;
                                CharName = name;
                                UserEntity = targetUserEntity;
                                CharEntity = targetEntity;
                            }
                            else
                            {
                                Output.CustomErrorMessage(ctx, $"没有找到特定的玩家 \"{name}\".");
                                return;
                            }
                        }
                        string MasteryType = ctx.Args[1].ToLower();
                        if (MasteryType.Equals("sword")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Sword, value);
                        else if (MasteryType.Equals("none")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.None, value);
                        else if (MasteryType.Equals("spear")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Spear, value);
                        else if (MasteryType.Equals("crossbow")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Crossbow, value);
                        else if (MasteryType.Equals("slashers")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Slashers, value);
                        else if (MasteryType.Equals("scythe")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Scythe, value);
                        else if (MasteryType.Equals("fishingpole")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.FishingPole, value);
                        else if (MasteryType.Equals("mace")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Mace, value);
                        else if (MasteryType.Equals("axes")) WeaponMasterSystem.SetMastery(SteamID, WeaponType.Axes, value);
                        else 
                        {
                            Output.InvalidArguments(ctx);
                            return;
                        }
                        ctx.Event.User.SendSystemMessage($"{ctx.Args[1].ToUpper()} Mastery for \"{CharName}\" is now set as<color=#ffffffff>  {value * 0.001}%</color>");
                        Helper.ApplyBuff(UserEntity, CharEntity, Database.buff.Buff_VBlood_Perk_Moose);
                        return;
                        
                    }
                    else
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }
                if (ctx.Args[0].ToLower().Equals("log"))
                {
                    if (ctx.Args[1].ToLower().Equals("on"))
                    {
                        Database.player_log_mastery[SteamID] = true;
                        ctx.Event.User.SendSystemMessage($"Mastery gain is now logged.");
                        return;
                    }
                    else if (ctx.Args[1].ToLower().Equals("off"))
                    {
                        Database.player_log_mastery[SteamID] = false;
                        ctx.Event.User.SendSystemMessage($"精通增益不再被记录.");
                        return;
                    }
                    else
                    {
                        Output.InvalidArguments(ctx);
                        return;
                    }
                }
            }
            else
            {
                bool isDataExist = Database.player_weaponmastery.TryGetValue(SteamID, out var MasteryData);
                if (!isDataExist)
                {
                    Output.CustomErrorMessage(ctx, "你还没有尝试过任何武器...");
                    return;
                }


                ctx.Event.User.SendSystemMessage("================= <color=#ffffffff>武器精通</color>==================");
                ctx.Event.User.SendSystemMessage($"单手剑:<color=#ffffffff> {(double)MasteryData.Sword * 0.001}%</color> (攻击力 <color=#75FF33FF>↑</color>, 攻击速度 <color=#75FF33FF>↑</color>)");
                ctx.Event.User.SendSystemMessage($"长矛:<color=#ffffffff> {(double)MasteryData.Spear * 0.001}%</color> (攻击力 <color=#75FF33FF>↑↑</color>)");
                ctx.Event.User.SendSystemMessage($"斧头:<color=#ffffffff> {(double)MasteryData.Axes * 0.001}%</color> (攻击力 <color=#75FF33FF>↑</color>, 生命值 <color=#75FF33FF>↑</color>)");
                ctx.Event.User.SendSystemMessage($"死神镰刀:<color=#ffffffff> {(double)MasteryData.Scythe * 0.001}%</color> (攻击力 <color=#75FF33FF>↑</color>, 暴击 <color=#75FF33FF>↑</color>)");
                ctx.Event.User.SendSystemMessage($"反手刃:<color=#ffffffff> {(double)MasteryData.Slashers * 0.001}%</color> (暴击 <color=#75FF33FF>↑</color>, 移动速度 <color=#75FF33FF>↑</color>)");
                ctx.Event.User.SendSystemMessage($"锤杖:<color=#ffffffff> {(double)MasteryData.Mace * 0.001}%</color> (生命值 <color=#75FF33FF>↑↑</color>)");
                ctx.Event.User.SendSystemMessage($"空手:<color=#ffffffff> {(double)MasteryData.None * 0.001}%</color> (攻击力 <color=#75FF33FF>↑↑</color>, 移动速度 <color=#75FF33FF>↑↑</color>)");
                ctx.Event.User.SendSystemMessage($"技能:<color=#ffffffff> {(double)MasteryData.Spell * 0.001}%</color> (冷却 <color=#75FF33FF>↓↓</color>)");
                ctx.Event.User.SendSystemMessage($"十字弩:<color=#ffffffff> {(double)MasteryData.Crossbow * 0.001}%</color> (暴击 <color=#75FF33FF>↑↑</color>)");
                ctx.Event.User.SendSystemMessage("=========================================");
                //ctx.Event.User.SendSystemMessage($"Fishing Pole: <color=#ffffffff>{(double)MasteryData.FishingPole * 0.001}%</color> (??? ↑↑)");
            }
        }
    }
}
