using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Grasshopper.Kernel;
using Mice.Solvers;
using Rhino.Geometry;

namespace Mice.Components.Analysis
{
    public class ResponseSpectrum : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("45F26C32-6EEA-450D-8B2B-647CFCC8AF9F");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.icon;

        public ResponseSpectrum()
            : base("Response Spectrum", "Spectrum", "Response spectrum of the Single dof",
                "Mice", "Response Analysis")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Range", "Rng", "Range of Target Periods(s)", GH_ParamAccess.item, new Interval(0.1, 10));
            pManager.AddIntegerParameter("Division", "div", "Time Increment(sec)", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Damping ratio", "h", "Damping ratio", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Time increment", "dt", "Time Increment(sec)", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Beta", "Beta", "Parameters of Newmark β ", GH_ParamAccess.item, 0.25);
            pManager.AddIntegerParameter("N", "N", "Length of the wave to be analyzed", GH_ParamAccess.item, 1000);
            pManager.AddTextParameter("Wave", "Wave", "Acceleration Wave(m/s^2)", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Acceleration", "Acc", "output Acceleration(m/s^2)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Velocity", "Vel", "output Velocity(m/s)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacement", "Disp", "output Displacement(m)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Energy", "EN", "output Energy(kNm)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            var h = 0d; // 減衰定数
            var g = 9.80665; // 重力加速度 m/s^2
            var dt = 0d; // 時間刻み sec
            var beta = 0d; // 解析パラメータ
            var N = 0; // 波形データ数
            var waveStr = string.Empty;
            var periodInterval = new Interval();
            var division = 0;

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref periodInterval)){ return; }
            if (!DA.GetData(1, ref division)){ return; }
            if (!DA.GetData(2, ref h)) { return; }
            if (!DA.GetData(3, ref dt)) { return; }
            if (!DA.GetData(4, ref beta)) { return; }
            if (!DA.GetData(5, ref N)) { return; }
            if (!DA.GetData(6, ref waveStr)) { return; }

            //　応答解析　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            
            var wave = ResponseAnalysis.Csv2Wave(waveStr, N);
            var accSpectrum = new List<double>();
            var velSpectrum = new List<double>();
            var dispSpectrum = new List<double>();
            var energySpectrum = new List<double>();
            
            //TODO: error出力するようにする
            if (periodInterval.T0 > 0 & periodInterval.IsIncreasing)
            {
                const double mass = 10d; // 質量 ton
                var p0 = periodInterval.T0;
                var p1 = periodInterval.T1;
                var pInc = (p1 - p0) / division;
                var period = p0;
                for (int i = 0; i < division + 1; i++)
                {
                    var stiffness = ResponseAnalysis.MT2K(mass, period);
                    ResponseAnalysis.NewmarkBeta(mass / g, stiffness, h, dt, beta, N, wave,
                        out double[] outAcc, out double[] outVel, out double[] outDisp,
                        out double[] outEo, out double[] outEi, out double[] outEk, out double[] outEp
                    );
                    
                    accSpectrum.Add(outAcc.Max());
                    velSpectrum.Add(outVel.Max());
                    dispSpectrum.Add(outDisp.Max());
                    energySpectrum.Add(outEo.Max());
                    period = period + pInc;
                }
            }

            // grasshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, accSpectrum);
            DA.SetDataList(1, velSpectrum);
            DA.SetDataList(2, dispSpectrum);
            DA.SetDataList(3, energySpectrum);
        }
    }
}