using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMZTestMod
{
    public class ExampleMod : ModBase
    {
        public ExampleMod(CastleMinerZGame game) : base(game, "ExampleMod", "com.YourName.ExampleMod")
        {

        }

        public override void LoadMain()
        {
            Log("YEA!!!!");
        }
    }
}
