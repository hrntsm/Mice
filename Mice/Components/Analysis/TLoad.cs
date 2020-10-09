using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Mice.Solvers;
using Rhino.Geometry;

namespace Mice.Components.Analysis
{
    /// <summary>
    /// 台形分布荷重の梁の計算
    /// </summary>
    public class TLoad : GH_Component
    {
        private Color _textColour = Color.FromName("Black");
        private readonly Color _loadArrowColour = Color.FromName("Green");
        private readonly Color _rfArrowColour = Color.FromName("Chocolate");
        // input
        List<double> Param = new List<double>();
        List<double> M_out = new List<double>();
        double W, DW, Lb, E;
        // output
        double M, Sig, D;
        //
        double L, Iy, Zy, Ra, Mx;
        double C = 1.0;
        double fb = 0.0;

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override bool IsPreviewCapable => true;
        protected override Bitmap Icon => Properties.Resource.UL_icon;
        public override Guid ComponentGuid => new Guid("621eac11-23fb-445c-9430-44ce37bf9020");

        public TLoad()
            : base("Trapezoid Load", "Trapezoid L", "Analysis of the beam of Trapezoid Load", "Mice", "Beam Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Trapezoid Load", "W", "Trapezoid Load (kN/m^2)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("D Width", "DW", "Domination Width (mm)", GH_ParamAccess.item, 1800);
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
            if (!DA.GetData(1, ref W)) { return; }
            if (!DA.GetData(2, ref DW)) { return; }
            if (!DA.GetData(3, ref Lb)) { return; }
            if (!DA.GetData(4, ref E)) { return; }
        
            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M = ((W * (DW / 1000)) / 24 * (3 * L * L - 4 * DW * DW))/1000000;
            Sig = M * 1000000 / Zy;
            D = (W*(DW/1000)) / (1920 * E * Iy) * (5 * L * L - 4 * DW * DW) * (5 * L * L - 4 * DW * DW);
            Ra = (W* (DW / 1000)) * (L - DW) / 2; // 反力
            Mx = ((Ra * L / 4) - ((W * (DW / 1000)) * Math.Pow(L / 4, 3) / (6 * DW)))/1000000; // 1/4点のモーメント計算

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(0);
            M_out.Add(Mx);
            M_out.Add(M);
            M_out.Add(Mx);
            M_out.Add(0);
            M_out.Add(L);
     
            // 許容曲げの計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
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
            Point3d LoadArrowStart, LoadArrowEnd, LoadArrowCenter = new Point3d(0, L /2, DW / 2);
            Line LoadArrow; 
            //
            Point3d RFArrowStart1 = new Point3d(0, 0, -L / 10);
            Point3d RFArrowEnd1 = new Point3d(0, 0, 0);
            Line RFArrow1 = new Line(RFArrowStart1, RFArrowEnd1);
            //
            Point3d RFArrowStart2 = new Point3d(0, L, -L / 10);
            Point3d RFArrowEnd2 = new Point3d(0, L, 0);
            Line RFArrow2 = new Line(RFArrowStart2, RFArrowEnd2);

            if (D != 0) {
                for (int i = 0; i < 10; i++) {
                    double LoadPosition = L / 11 * ( i + 1 );
                    if (LoadPosition < DW) {
                        LoadArrowStart = new Point3d(0, LoadPosition, LoadPosition / 2);
                        LoadArrowEnd = new Point3d(0, LoadPosition, 0);
                        LoadArrow = new Line(LoadArrowStart, LoadArrowEnd);
                    }
                    else if (LoadPosition > L - DW) {
                        LoadArrowStart = new Point3d(0, LoadPosition, (L - LoadPosition) / 2);
                        LoadArrowEnd = new Point3d(0, LoadPosition, 0);
                        LoadArrow = new Line(LoadArrowStart, LoadArrowEnd);
                    }
                    else {
                        LoadArrowStart = new Point3d(0, LoadPosition, DW / 2);
                        LoadArrowEnd = new Point3d(0, LoadPosition, 0);
                        LoadArrow = new Line(LoadArrowStart, LoadArrowEnd);
                    }
                    args.Display.DrawArrow(LoadArrow, _loadArrowColour);
                }
                //
                args.Display.DrawLine(new Point3d(0, 0, 0), new Point3d(0, DW, DW / 2), _loadArrowColour);
                args.Display.DrawLine(new Point3d(0, DW, DW / 2), new Point3d(0, L - DW, DW / 2), _loadArrowColour);
                args.Display.DrawLine(new Point3d(0, L - DW, DW / 2), new Point3d(0, L, 0), _loadArrowColour);
                args.Display.Draw2dText(W.ToString("F1"), _loadArrowColour, LoadArrowCenter, true, 22);
                //
                args.Display.DrawArrow(RFArrow1, _rfArrowColour);
                args.Display.Draw2dText((Ra / 1000).ToString("F1"), _rfArrowColour, RFArrowStart1, false, 22);
                //
                args.Display.DrawArrow(RFArrow2, _rfArrowColour);
                args.Display.Draw2dText((Ra / 1000).ToString("F1"), _rfArrowColour, RFArrowStart2, false, 22);
            }
        }
    }
}