using System;
using Grasshopper.Kernel;
using Mice.Solvers;

namespace Mice.Components.Util
{
    public class MKtoT : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4823-9cef-5f5647a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.calcT_icon;
        
        public MKtoT()
            : base("Calc Natural Period", "Calc T", "Calculation of the Natural Period",
                 "Mice", "Response Analysis")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Mass", "M", "Lumped Mass(ton)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stiffness", "K", "Spring Stiffness(kN/m)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("NaturalPeriod", "T", "output Natural Period(sec)", GH_ParamAccess.item);
            pManager.AddNumberParameter("NaturalFrequency", "f", "output Natural Frequency(Hz)", GH_ParamAccess.item);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var mass = 0d;
            var K = 0d;
                        
            if (!DA.GetData(0, ref mass)) { return; }
            if (!DA.GetData(1, ref K)) { return; }

            var T = ResponseAnalysis.MK2T(mass, K);
            var f = 1.0 / T;

            DA.SetData(0, T);
            DA.SetData(1, f);
        }
    }
}