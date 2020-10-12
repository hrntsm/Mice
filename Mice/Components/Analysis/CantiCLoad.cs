using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Mice.Solvers;
using Rhino.Geometry;

namespace Mice.Components.Analysis
{
    /// <summary>
    /// 先端集中荷重の片持ち梁の計算
    /// </summary>
    public class CantiCLoad : GH_Component {
        private Color _textColour = Color.FromName("Black");
        private readonly Color _loadArrowColour = Color.FromName("Green");
        private readonly Color _rfArrowColour = Color.FromName("Chocolate");
        // input
        List<double> Param = new List<double>();
        List<double> M_out = new List<double>();
        double P, Lb, E;
        // output
        double M, Sig, D;
        //
        double L, Iy, Zy;
        double C = 1.0;
        double fb = 0.0;

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override bool IsPreviewCapable => true;
        protected override Bitmap Icon => Properties.Resource.CantiPL_icon;
        public override Guid ComponentGuid => new Guid("621eac11-23fb-445c-9430-44ce37bf9031");

        public CantiCLoad()
            : base("Cantilever Point Load", "Canti PL", "Analysis of the beam of Cantilever Point Load", "Mice", "Beam Analysis")
        {
        }
        
        public override void ClearData() {
            base.ClearData();
            Param.Clear();
            M_out.Clear();
            P = double.NaN;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Point Load", "P", "Point Load (kN)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment (kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress (N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref P)) { return; }
            if (!DA.GetData(2, ref Lb)) { return; }
            if (!DA.GetData(3, ref E)) { return; }

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M =  P * L/ 1000;
            Sig = M * 1000000 / Zy;
            D = (P /1000) * L * L * L / (3.0 * E * Iy); 

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(-M);
            M_out.Add(-M * 3/4);
            M_out.Add(-M * 1/2);
            M_out.Add(-M * 1/4);
            M_out.Add(0);
            M_out.Add(L);

            // 許容曲げの計算＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            fb = BeamAnalysis.CalcFb(Param, Lb, C);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig / fb);
            DA.SetData(4, D);
        }
        
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (double.IsNaN(P))
                return;

            // 荷重出力
            var loadArrowStart = new Point3d(0, L, L / 5);
            var loadArrowEnd = new Point3d(0, L, 0);
            var loadArrow = new Line(loadArrowStart, loadArrowEnd);
            // 反力出力
            var rfArrowStart1 = new Point3d(0, 0, -L / 5);
            var rfArrowEnd1 = new Point3d(0, 0, 0);
            var rfArrow1 = new Line(rfArrowStart1, rfArrowEnd1);
            //
            if (D != 0) {
                args.Display.DrawArrow(loadArrow, _loadArrowColour);
                args.Display.Draw2dText(P.ToString("F1"), _loadArrowColour, loadArrowStart, false, 22);
                //
                args.Display.DrawArrow(rfArrow1, _rfArrowColour);
                args.Display.Draw2dText(P.ToString("F1"), _rfArrowColour, rfArrowStart1, false, 22);
            }
        }
    }
}