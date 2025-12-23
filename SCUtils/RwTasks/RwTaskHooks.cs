using RWCustom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public static class RwTaskHooks
    {
        public static readonly ConcurrentQueue<Action> PendingActions = new();

        public static void Init()
        {
            On.RainWorld.Update += RainWorld_Update;
            On.MainLoopProcess.Update += MainLoopProcess_Update;
            On.UpdatableAndDeletable.Destroy += UpdatableAndDeletable_Destroy;
        }

        private static void UpdatableAndDeletable_Destroy(On.UpdatableAndDeletable.orig_Destroy orig, UpdatableAndDeletable self)
        {
            orig(self);
            if(self.TryGetDestroyTokenSource(out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private static void MainLoopProcess_Update(On.MainLoopProcess.orig_Update orig, MainLoopProcess self)
        {
            //if(Custom.rainWorld.processManager.currentMainLoop != null)
                SCUtils.Log($"[RwTaskScope] MainLoopProcess_Update self : {self.ID}");
            if (Custom.rainWorld.processManager.currentMainLoop == self)
            {
                SCUtils.Log("[RwTaskScope] MainLoopProcess_Update ticking 1");
                using (new RwTaskScope(RwLoopRunner.EarlyUpdateRunner))
                {
                    RwLoopRunner.EarlyUpdateRunner.Tick();
                }
            }
            orig(self);
            if (Custom.rainWorld.processManager.currentMainLoop == self)
            {
                SCUtils.Log("[RwTaskScope] MainLoopProcess_Update ticking 2");
                using (new RwTaskScope(RwLoopRunner.LateUpdateRunner))
                {
                    RwLoopRunner.LateUpdateRunner.Tick();
                }
            }
            
            while (PendingActions.TryDequeue(out var action))
                action?.Invoke();
            
        }

        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            using (new RwTaskScope(RwLoopRunner.EarlyRawUpdateRunner))
            {
                RwLoopRunner.EarlyRawUpdateRunner.Tick();
            }
            SCUtils.Log("[RwTaskScope] RainWorld_Update ticking");
            orig(self);
            using (new RwTaskScope(RwLoopRunner.LateRawUpdateRunner))
            {
                RwLoopRunner.LateRawUpdateRunner.Tick();
            }
        }
    }
}
