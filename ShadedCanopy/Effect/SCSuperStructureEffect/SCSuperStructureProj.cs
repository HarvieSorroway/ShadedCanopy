using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Effect.SCSuperStructureEffect
{
    internal class SCSuperStructureProj : UpdatableAndDeletable
    {
        public List<SCSuperStructureProjPart> parts = new List<SCSuperStructureProjPart>();
        public bool visible;

        public float projActivation;

        public SCSuperStructureProj(Room room)
        {
            this.room = room;

            var boids = new SCBoids(this, 1000);
            room.AddObject(boids);

            for(int i = 0;i < 10;i++)
            {
                room.AddObject(new SCBoidCursor(this, boids));
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            projActivation = Mathf.Clamp01(projActivation + Random.value * 2f - 1f);

            if (Random.value < 0.1f)
            {
                visible = Random.value < room.ElectricPower;
            }
        }

        public override void Destroy()
        {
            for (int i = 0; i < this.parts.Count; i++)
            {
                this.parts[i].Destroy();
            }
            base.Destroy();
        }

    }
}
