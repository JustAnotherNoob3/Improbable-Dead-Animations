using SML;
using UnityEngine;
using HarmonyLib;
using Game.Simulation;
using Server.Shared.Info;
using Server.Shared.State;
using Services;
using System.Globalization;
using Game.Services;
using Server.Shared.Messages;
using Server.Shared.Cinematics.Data;
using Server.Shared.Cinematics;
using Game.DevMenu;
using Utils;

namespace Main
{
    [Mod.SalemMod]
    public class Main
    {
        static public Role[] rolesWithDeathAnim =
        {
            Role.ARSONIST,
            Role.SERIALKILLER,
            Role.WEREWOLF,
            Role.COVENLEADER
        };
        public static void Start()
        {
            Debug.Log("Working?");
        }
        public static Role GetRandomRole()
        {
            System.Random r = new();
            return rolesWithDeathAnim[r.Next(0, rolesWithDeathAnim.Length)];
        }
    }
    [HarmonyPatch(typeof(GameSimulation), "HandlePlayCinematic")]
    class RandomizeInGameDeathAnimations
    {
        static public void Prefix(ref PlayCinematicMessage message)
        {

            if (!ModSettings.GetBool("Random Death Animations", "JAN.improbabledeadanims")) return;
            var data = message.cinematicEntry.GetData();
            if (data.cinematicType == CinematicType.Attacked)
            {
                (data as AttackedCinematicData).attackerRole = Main.GetRandomRole();
            }
        }
    }
    [HarmonyPatch(typeof(GameSimulation), "HandleOnGameInfoChanged")]
    class ImprobableDeadAnims
    {
        public static bool smart = false;
        public static float sprobability = 0;
        public static float probability = 0;
        [HarmonyPostfix]
        public static void AddMessage(GameSimulation __instance, GameInfo gameInfo)
        {
            if (!ModSettings.GetBool("Improbable Death Animations", "JAN.improbabledeadanims")) return;
            if (gameInfo.gamePhase != GamePhase.PLAY) return;
            if (!Pepper.AmIAlive()) return;
            PlayPhase playPhase = __instance.playPhaseState.Get().playPhase;
            if (playPhase == PlayPhase.FIRST_DISCUSSION)
            {
                probability = float.Parse(ModSettings.GetString("Probability of the Death Anims", "JAN.improbabledeadanims"), CultureInfo.InvariantCulture.NumberFormat) * 1000;
                sprobability = float.Parse(ModSettings.GetString("Probability added per night", "JAN.improbabledeadanims"), CultureInfo.InvariantCulture.NumberFormat) * 1000;
                smart = ModSettings.GetBool("Use smart probabilities", "JAN.improbabledeadanims");
                return;
            }
            if (playPhase != PlayPhase.NIGHT) return;
            if (smart) probability = Mathf.Clamp(probability + sprobability, 0, 100000);
            System.Random r = new();
            Service.Home.UserService.Settings.SetShowCinematics(r.Next(0, 100000) < probability);
        }
    }


}