#if DEBUG
#define EnableLogging
#endif

using Kitchen_InfiniteReroll;
using Unity.Entities;

public static class Logger
{
    public static void Log(string message)
    {
#if EnableLogging
        UnityEngine.Debug.Log($"[DKatGames] -- {message}");
#endif
    }

    public static void LogEntityComponents(Entity entity, string name)
    {
        var components = Main.instance.EntityManager.GetComponentTypes(entity, Unity.Collections.Allocator.Temp);
        string componentsString = "";
        foreach (var comp in components)
        {
            componentsString += $"({comp.TypeIndex}, {comp.GetManagedType()}), ";
        }

        Logger.Log($"Components on {name} entity: {componentsString}");
        components.Dispose();
    }
}