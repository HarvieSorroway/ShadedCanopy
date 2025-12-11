using Fisobs.Core;
using ShadedCanopy.Creatures.Scintivenger.ScintivengerCritobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Creatures
{
    internal static class SCCritobs
    {
        public static void Init()
        {
            Content.Register(new SCScavengerCritob());
        }
    }
}
