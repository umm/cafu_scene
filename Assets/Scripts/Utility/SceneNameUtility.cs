using System;

namespace Modules.Scripts.Utility
{
    public static class SceneNameUtility
    {
        public static TSceneName Parse<TSceneName>(string value)
            where TSceneName : struct
        {
            TSceneName result;
            return !Enum.TryParse(value, out result) ? default(TSceneName) : result;
        }
    }
}