using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Mice.Solvers;

namespace Mice.Components.Analysis
{
    public class NewmarkBeta : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4717-9cef-5f5647a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.icon;
        
        public NewmarkBeta()
            : base("1dof Response Analysis", "1dof RA", "**Experimental Function**\nResponse Analysis of the Single dof",
                 "Mice", "Response Analysis")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Mass", "M", "Lumped Mass(kg)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Stiffness", "K", "Spring Stiffness(N/m)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Damping ratio", "h", "Damping ratio", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Time Increment", "dt", "Time Increment(sec)", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Beta", "Beta", "Parameters of Newmark β ", GH_ParamAccess.item, 0.25);
            pManager.AddIntegerParameter("N", "N", "Parameters of Newmark β ", GH_ParamAccess.item,1000);
            pManager.AddTextParameter("Wave", "Wave", "Acceleration Wave(m/s^2)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Model", "Model", "output Model Data", GH_ParamAccess.item);
            pManager.AddNumberParameter("Acceleration", "Acc", "output Acceleration(m/s^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Velocity", "Vel", "output Velocity(m/s)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Displacement", "Disp", "output Displacement(m)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Total E", "Eo", "output Total Input Energy(Nm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Internal E", "Ei", "output Internal Viscous Damping Energy(Nm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Kinetic E", "Ek", "output Kinetic Energy(Nm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Potential E", "Ep", "output Potential Energy(Nm)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            var model = new List<double>();
            var mass = 0d;    // 質量 kg
            var K = 0d;    // 剛性 N/m
            var h = 0d;    // 減衰定数
            var dt = 0d;   // 時間刻み sec
            var beta = 0d; // 解析パラメータ
            var N = 0;                // 波形データ数
            var waveStr = string.Empty;

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref mass)) { return; }
            if (!DA.GetData(1, ref K)) { return; }
            if (!DA.GetData(2, ref h)) { return; }
            if (!DA.GetData(3, ref dt)) { return; }
            if (!DA.GetData(4, ref beta)) { return; }
            if (!DA.GetData(5, ref N)) { return; }
            if (!DA.GetData(6, ref waveStr)) { return; }

            // モデルデータ出力用＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            model.Add(mass);
            model.Add(K);

            //　応答解析　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            var wave = ResponseAnalysis.Csv2Wave(waveStr, N);
            ResponseAnalysis.NewmarkBeta(mass, K, h, dt, beta, N, wave,
                out double[] outAcc, out double[] outVel, out double[] outDisp,
                out double[] outEo, out double[] outEi, out double[] outEk, out double[] outEp
            );
            
            // grasshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, model);
            DA.SetDataList(1, outAcc);
            DA.SetDataList(2, outVel);
            DA.SetDataList(3, outDisp);
            DA.SetDataList(4, outEo);
            DA.SetDataList(5, outEi);
            DA.SetDataList(6, outEk);
            DA.SetDataList(7, outEp);
        }
    }
}