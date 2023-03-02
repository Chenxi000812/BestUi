using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BestUi.Patch;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace BestUi
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Entrance : BasePlugin
    {
        public const string GUID = "Chenxi.SonsOfForest.BestUi";
        public const string NAME = "BestUi";
        public const string author = "Chenxi";
        public const string VERSION = "0.0.2";
        public static ManualLogSource LOG;
        public override void Load()
        {
            LOG = base.Log;
            Harmony.CreateAndPatchAll(typeof(PlayerInventoryPatch));
            LOG.LogWarning("BestUi is Loaded！");

        }
    }
}