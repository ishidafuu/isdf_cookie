using Unity.Entities;
using UnityEngine;

static class Preload
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize() => PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);

    static void DomainUnloadShutdown()
    {
        World.DisposeAllWorlds();
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
    }
}
