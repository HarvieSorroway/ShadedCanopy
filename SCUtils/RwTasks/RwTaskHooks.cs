using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.RwTasks
{
    public static class RwTaskHooks
    {

        private class RwTaskProcess : MainLoopProcess
        {
            public RwTaskProcess(ProcessManager manager) : base(manager, RwTask)
            {
            }

            public override void Update()
            {
                base.Update();
                if (Custom.rainWorld.processManager.currentMainLoop == null)
                {
                    using (new RwTaskScope(RwLoopRunner.EarlyUpdateRunner))
                    {
                        RwLoopRunner.EarlyUpdateRunner.Tick();
                    }
                    using (new RwTaskScope(RwLoopRunner.LateUpdateRunner))
                    {
                        RwLoopRunner.LateUpdateRunner.Tick();
                    }
                }
            }

            public static readonly ProcessManager.ProcessID RwTask = new ProcessManager.ProcessID("SC.RwTask", true);
        }

        public static readonly ConcurrentQueue<Action> PendingActions = new();

        public static void Init()
        {
            On.RainWorld.Update += RainWorld_Update;

            On.RainWorldGame.Update += RainWorldGame_Update;
            On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
            Custom.rainWorld.processManager.sideProcesses.Add(new RwTaskProcess(Custom.rainWorld.processManager));

            On.UpdatableAndDeletable.Destroy += UpdatableAndDeletable_Destroy;
        }

        private static void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
        {
            if (self is not RainWorldGame && self == Custom.rainWorld.processManager.currentMainLoop)
            {
                var value = self.myTimeStacker + dt * self.framesPerSecond;
                var v2 = value;
                while (value > 1f)
                {
                    using (new RwTaskScope(RwLoopRunner.EarlyUpdateRunner))
                    {
                        RwLoopRunner.EarlyUpdateRunner.Tick();
                    }
                    value -= 1f;
                }
                orig(self, dt);
                while (v2 > 1f)
                {
                    using (new RwTaskScope(RwLoopRunner.LateUpdateRunner))
                    {
                        RwLoopRunner.LateUpdateRunner.Tick();
                    }
                    v2 -= 1f;
                }
            }
            else
            {
                orig(self, dt);
            }
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

        private static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            if (Custom.rainWorld.processManager.currentMainLoop == self)
            { 
                using (new RwTaskScope(RwLoopRunner.EarlyUpdateRunner))
                {
                    RwLoopRunner.EarlyUpdateRunner.Tick();
                }
                orig(self);
                using (new RwTaskScope(RwLoopRunner.LateUpdateRunner))
                {
                    RwLoopRunner.LateUpdateRunner.Tick();
                }
            }
            else
            {
                orig(self);
            }
            
        }



        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            using (new RwTaskScope(RwLoopRunner.EarlyRawUpdateRunner))
            {
                RwLoopRunner.EarlyRawUpdateRunner.Tick();
            }
            orig(self);
            using (new RwTaskScope(RwLoopRunner.LateRawUpdateRunner))
            {
                RwLoopRunner.LateRawUpdateRunner.Tick();
            }

            while (PendingActions.TryDequeue(out var action))
                action?.Invoke();
        }
    }
}
