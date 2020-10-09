using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Mise.Solvers;
using Rhino.Geometry;

namespace Mise.Components.Load
{
    /// <summary>
    /// 中央集中荷重の梁の計算
    /// </summary>
    public class Cload : GH_Component {
        private Color _textColour = Color.FromName("Black");
        private readonly Color _loadArrowColour = Color.FromName("Green");
        private readonly Color _rfArrowColour = Color.FromName("Chocolate");
        // input
        private List<double> Param = new List<double>();
        private List<double> M_out = new List<double>();
        private double P, Lb, E;
        // output
        private double M, Sig, D;
        //
        private double L, Iy, Zy;
        private double C = 1.0;
        private double fb = 0.0;

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override bool IsPreviewCapable => true;
        protected override Bitmap Icon => Properties.Resource.CL_icon;
        public override Guid ComponentGuid => new Guid("621eac03-23fb-445c-9430-44ce37bf9020");

        public Cload()
            : base("Centralized Load", "Centralized L", "Analysis of the beam of Centralized Load", "Mice",
                "Beam Analysis")
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Load", "Load", "Centralized Load (kN)", GH_ParamAccess.item,100);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
            pManager[0].Optional = true;
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment(kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("examination result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
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
            M = P * (L / 1000) / 4;
            Sig = M * 1000000 / Zy;
            D = P * 1000 * L * L * L / (48 * E * Iy);

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(0);
            M_out.Add(M / 2);
            M_out.Add(M);
            M_out.Add(M / 2);
            M_out.Add(0);
            M_out.Add(L);

            // 許容曲げの計算＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            fb = BeamAnalysis.CalcFb(Param, Lb, C);

            // 出力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetDataList(0, M_out);
            DA.SetData(1, Sig);
            DA.SetData(2, fb);
            DA.SetData(3, Sig/fb);
            DA.SetData(4, D);
        }
        
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            // 荷重出力
            var LoadArrowStart = new Point3d(0, L / 2, L / 5);
            var LoadArrowEnd = new Point3d(0, L / 2, 0);
            var LoadArrow = new Line(LoadArrowStart, LoadArrowEnd);
            // 反力出力
            var RFArrowStart1 = new Point3d(0, 0, -L / 10);
            var RFArrowEnd1 = new Point3d(0, 0, 0);
            var RFArrow1 = new Line(RFArrowStart1, RFArrowEnd1);
            //
            var RFArrowStart2 = new Point3d(0, L, -L/10);
            var RFArrowEnd2 = new Point3d(0, L, 0);
            var RFArrow2 = new Line(RFArrowStart2, RFArrowEnd2);
            
            if (D == 0) return;
            args.Display.DrawArrow(LoadArrow, _loadArrowColour);
            args.Display.Draw2dText(P.ToString("F1"), _loadArrowColour, LoadArrowStart, false, 22);
            //
            args.Display.DrawArrow(RFArrow1, _rfArrowColour);
            args.Display.Draw2dText((P / 2.0).ToString("F1"), _rfArrowColour, RFArrowStart1, false, 22);
            //
            args.Display.DrawArrow(RFArrow2, _rfArrowColour);
            args.Display.Draw2dText((P / 2.0).ToString("F1"), _rfArrowColour, RFArrowStart2, false, 22);
        }
    }
}