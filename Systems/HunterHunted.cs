using ProjectM;
using ProjectM.Network;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Wetstone.API;

namespace RPGMods.Systems
{
    public class HunterHunted
    {
        private static EntityManager entityManager = VWorld.Server.EntityManager;

        public static bool isActive = true;
        public static int heat_cooldown = 35;
        public static int bandit_heat_cooldown = 35;
        public static int cooldown_timer = 60;
        public static int ambush_interval = 300;
        public static int ambush_chance = 50;
        public static float ambush_despawn_timer = 300;

        private static Random rand = new Random();
        public static void PlayerUpdateHeat(Entity killerEntity, Entity victimEntity)
        {
            var player = entityManager.GetComponentData<PlayerCharacter>(killerEntity);
            var userEntity = player.UserEntity._Entity;
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            var victim = entityManager.GetComponentData<FactionReference>(victimEntity);
            var victim_faction = victim.FactionGuid._Value;
            if (Database.faction_heatvalue.TryGetValue(victim_faction, out int heatValue))
            {
                if (victim_faction.GetHashCode() == -413163549) //-- Separate bandit heat level
                {
                    int bandit_heatvalue = rand.Next(1, 10);
                    bool isBanditExist = Cache.bandit_heatlevel.TryGetValue(SteamID, out int player_banditheat);
                    if (isBanditExist) bandit_heatvalue = player_banditheat + bandit_heatvalue;
                    Cache.bandit_heatlevel[SteamID] = bandit_heatvalue;
                }
                bool isExist = Cache.heatlevel.TryGetValue(SteamID, out int player_heat);
                if (isExist) heatValue = player_heat + heatValue;
                Cache.heatlevel[SteamID] = heatValue;
            }
        }

        public static void HeatManager(Entity userEntity, Entity playerEntity, bool InCombat)
        {
            var user = entityManager.GetComponentData<User>(userEntity);
            var SteamID = user.PlatformId;

            DateTime last_update;
            DateTime last_ambushed;
            DateTime bandit_last_ambush;
            Cache.player_heat_timestamp.TryGetValue(SteamID, out last_update);
            Cache.player_last_ambushed.TryGetValue(SteamID, out last_ambushed);
            Cache.bandit_last_ambushed.TryGetValue(SteamID, out bandit_last_ambush);

            TimeSpan elapsed_time = DateTime.Now - last_update;
            if (elapsed_time.TotalSeconds > cooldown_timer)
            {
                int heat_ticks = (int)elapsed_time.TotalSeconds / cooldown_timer;
                if (heat_ticks < 0) heat_ticks = 0;

                int player_heat;
                Cache.heatlevel.TryGetValue(SteamID, out player_heat);
                if (player_heat > 0)
                {
                    player_heat = player_heat - heat_cooldown * heat_ticks;
                    if (player_heat < 0) player_heat = 0;
                    Cache.heatlevel[SteamID] = player_heat;

                    TimeSpan since_ambush = DateTime.Now - last_ambushed;
                    if (since_ambush.TotalSeconds > ambush_interval)
                    {
                        if (rand.Next(0, 100) < ambush_chance && player_heat >= 250 && InCombat)
                        {
                            if (player_heat >= 3000)
                            {
                                SquadList.SpawnSquad(playerEntity, 4, rand.Next(10, 20));
                                SquadList.SpawnSquad(playerEntity, 5, 2);
                                Output.SendLore(userEntity,"<color=#c90e21ff>An extermination squad has found you and wants you DEAD.</color>");
                            }
                            else if (player_heat >= 2000)
                            {
                                if (rand.Next(0, 100) < 50)
                                {
                                    SquadList.SpawnSquad(playerEntity, 5, 1);
                                    SquadList.SpawnSquad(playerEntity, 4, 9);
                                }
                                else
                                {
                                    SquadList.SpawnSquad(playerEntity, 4, rand.Next(15, 20));
                                }
                                Output.SendLore(userEntity, "<color=#c90e21ff>教会求助了盟友，吸血鬼猎人加入了战斗，他们是你永恒的宿敌</color>");
                            }
                            else if (player_heat >= 1000)
                            {
                                SquadList.SpawnSquad(playerEntity, 3, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c90e21ff>教会下达了最高指令，人类的顶尖战力们正在追捕你</color>");
                            }
                            else if (player_heat >= 500)
                            {
                                SquadList.SpawnSquad(playerEntity, 2, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c4515cff>教会下达追捕令，一队精英士兵埋伏了你</color>");
                            }
                            else if (player_heat >= 250)
                            {
                                SquadList.SpawnSquad(playerEntity, 1, rand.Next(5, 10));
                                Output.SendLore(userEntity, "<color=#c9999eff>在赏金的诱惑下，一只小队埋伏了你</color>");
                            }
                            Cache.player_last_ambushed[SteamID] = DateTime.Now;
                        }
                    }
                }

                int player_banditheat;
                Cache.bandit_heatlevel.TryGetValue(SteamID, out player_banditheat);
                if (player_banditheat > 0)
                {
                    player_banditheat = player_banditheat - bandit_heat_cooldown * heat_ticks;
                    if (player_banditheat < 0) player_banditheat = 0;
                    Cache.bandit_heatlevel[SteamID] = player_banditheat;

                    TimeSpan since_ambush = DateTime.Now - bandit_last_ambush;
                    if (since_ambush.TotalSeconds > ambush_interval)
                    {
                        if (rand.Next(0, 100) < ambush_chance && player_banditheat >= 250 && InCombat)
                        {
                            if (player_banditheat >= 2000)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, rand.Next(20, 25));
                                Output.SendLore(userEntity, "<color=#c90e21ff>恼羞成怒的强盗们派出了他们最强的战力来埋伏你</color>");
                            }
                            else if (player_banditheat >= 1000)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, rand.Next(10, 15));
                                Output.SendLore(userEntity, "<color=#c90e21ff>强盗们认识到了你的威胁，一队强大的强盗小队埋伏了你！</color>");
                            }
                            else if (player_banditheat >= 500)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, 5);
                                Output.SendLore(userEntity, "<color=#c4515cff>强盗们愤怒了，一队精英强盗小队埋伏了你！</color>");
                            }
                            else if (player_banditheat >= 250)
                            {
                                SquadList.SpawnSquad(playerEntity, 0, 3);
                                Output.SendLore(userEntity, "<color=#c9999eff>强盗们开始了报复，一队强盗小队埋伏了你！</color>");
                            }
                            Cache.bandit_last_ambushed[SteamID] = DateTime.Now;
                        }
                    }
                }
                Cache.player_heat_timestamp[SteamID] = DateTime.Now;
            }
        }
    }
}
