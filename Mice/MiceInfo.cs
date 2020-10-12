using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Mice
{
    public class MiceInfo : GH_AssemblyInfo
    {
        public override string Name => "Mice";
        public override Bitmap Icon => null;
        public override string Description => "Response analysis for 1DOF and stress analysis for simple beams component";
        public override Guid Id => new Guid("8b3cd60f-4aaa-4723-a285-ce5590a58adc");
        public override string AuthorName => "hrntsm";
        public override string AuthorContact => "contact@hrntsm.com";
    }
}
