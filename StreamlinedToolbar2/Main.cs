using ICities;
using CitiesHarmony.API;
using UnityEngine.SceneManagement;

namespace StreamlinedToolbar2
{
    public class Mod : IUserMod
    {
        public string Name => "Streamlined Toolbar 2";
        public string Description => "Moves some toolbar items around to make it more usable";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static bool IsInGame()
        {
            return SceneManager.GetActiveScene().name == "Game";
        }

        public static string Identifier = "WQ.ST/";
    }
}
