using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SimpleInjector;
using System.Text;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Character;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine.AddressableAssets;

namespace CultOfSpikedrivers.Plugin
{

    class ConfigDescriptionBuilder
    {
        public string English { get; set; } = "";
        public string French { get; set; } = "";
        public string German { get; set; } = "";
        public string Russian { get; set; } = "";
        public string Portuguese { get; set; } = "";
        public string Chinese { get; set; } = "";
        public string Spanish { get; set; } = "";
        public string ChineseTraditional { get; set; } = "";
        public string Korean { get; set; } = "";
        public string Japanese { get; set; } = "";

        public override string ToString()
        {
            StringBuilder builder = new();
            if (!string.IsNullOrEmpty(English)) builder.AppendLine(English);
            if (!string.IsNullOrEmpty(French)) builder.AppendLine(French);
            if (!string.IsNullOrEmpty(German)) builder.AppendLine(German);
            if (!string.IsNullOrEmpty(Russian)) builder.AppendLine(Russian);
            if (!string.IsNullOrEmpty(Portuguese)) builder.AppendLine(Portuguese);
            if (!string.IsNullOrEmpty(Chinese)) builder.AppendLine(Chinese);
            if (!string.IsNullOrEmpty(Spanish)) builder.AppendLine(Spanish);
            if (!string.IsNullOrEmpty(ChineseTraditional)) builder.AppendLine(ChineseTraditional);
            if (!string.IsNullOrEmpty(Korean)) builder.AppendLine(Korean);
            if (!string.IsNullOrEmpty(Japanese)) builder.AppendLine(Japanese);
            return builder.ToString().TrimEnd();
        }
    }

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);
        
        public void Awake()
        {
            Logger = base.Logger;

            List<string> files = ["json/localizations.json", "json/spikedriver.json"];

            var builder = Railhead.GetBuilder();
            builder.Configure(
                MyPluginInfo.PLUGIN_GUID,
                c =>
                {
                    c.AddMergedJsonFile(
                        files
                    );
                }
            );

            Railend.ConfigurePostAction(
                c =>
                {
                    PostInitialize(c);
                }
            );
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void PostInitialize(Container c)
        {
            System.Reflection.FieldInfo characterPrefabVariantRefField = AccessTools.Field(typeof(CharacterData), "characterPrefabVariantRef");

            var client = c.GetInstance<GameDataClient>();
            if (!client.TryGetProvider<SaveManager>(out var saveManager))
            {
                Logger.LogError("Could not initialize");
                return;
            }

            var gameObjectRegister = c.GetInstance<IRegister<AssetReferenceGameObject>>();

            var allGameData = saveManager!.GetAllGameData();
            var monsterFollower = allGameData.FindCharacterData("e8b1f88f-8628-48ea-84fd-0191ce2e88a0");

            if (!gameObjectRegister.TryLookupName(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.GameObject, "SpikedriverColonyCharacterArt"), out var spikedriverArt, out var _))
            {
                Logger.LogError("Did not find spikedriver colony character art");
                return;
            }

            // Replace the character skeleton
            // The card art is already replaced.
            characterPrefabVariantRefField.SetValue(monsterFollower, spikedriverArt);

            ConfigEntry<bool> ClassicSpikedrivers = Config.Bind<bool>("Options", "Enable Classic Spikedriver Colony", false,
                new ConfigDescriptionBuilder
                {
                    English = "Spikedriver Colony retains its original effect from MT1 (Multistrike 1 and Extinguish: Add a permanent copy of this card to the discard pile.). Warning enabling this makes the mod non-cosmetic, do not play daily challenges with this enabled.",
                }.ToString());

            if (ClassicSpikedrivers.Value)
            {
                var followerCard = allGameData.FindCardData("3d32e031-9796-4eaf-84c3-a30b48db22d4");
                var spawn = followerCard!.GetEffects()[0];
                var characterRegister = c.GetInstance<CharacterDataRegister>();
                if (!characterRegister.TryLookupName(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.Character, "SpikedriverColonyCharacter"), out var classicSpikedriverCharacter, out var _))
                {
                    Logger.LogError("Did not find spikedriver colony character");
                    return;
                }
                AccessTools.Field(typeof(CardEffectData), "paramCharacterData").SetValue(spawn, classicSpikedriverCharacter);
            }
        }
    }
}
