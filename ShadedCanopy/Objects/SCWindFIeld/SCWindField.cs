using BepInEx;
using IL.RWCustom;
using SCUtils.SCDevTools.NodeTreeManager;
using ShadedCanopy.Objects.SCBlinkingLawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace ShadedCanopy.Objects.SCWindFIeld
{
    internal class SCWindField: UpdatableAndDeletable
    {
        
        public SCWindField(Room room, int collisionLayer) : base()
        {
            
        }

       
        public override void Update(bool eu)
        {
            base.Update(eu);
            
        }

        public virtual Vector2 GetWind(Vector2 pos, int latency)
        {
            throw new NotImplementedException();
        }
    }

}
