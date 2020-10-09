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
        public override Guid Id => new Guid("d6aa9b29-0ad2-4639-ab6f-37c55475b8f6");
        public override string AuthorName => "hrntsm";
        public override string AuthorContact => "contact@hrntsm.com";
    }
}
