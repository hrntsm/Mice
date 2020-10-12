using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Mice.Properties;
using Mice.Solvers;
using Rhino.Geometry;

namespace Mice.Components.Analysis
{
    /// <summary>
    ///     台形分布荷重の梁の計算
    /// </summary>
    public class TLoad : GH_Component
    {
        private readonly Color _loadArrowColour = Color.FromName("Green");

        private readonly Color _rfArrowColour = Color.FromName("Chocolate");
        private Color _textColour = Color.FromName("Black");
        private readonly double C = 1.0;
        private double fb;

        //
        private double L, Iy, Zy, Ra, Mx;

        // output
        private double M, Sig, D;
        private readonly List<double> M_out = new List<double>();

        // input
        private readonly List<double> Param = new List<double>();

        private double W, DW, Lb, E;

        public TLoad()
            : base("Trapezoid Load", "Trapezoid L", "Analysis of the beam of Trapezoid Load", "Mice", "Beam Analysis")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override bool IsPreviewCapable => true;
        protected override Bitmap Icon => Resource.UL_icon;
        public override Guid ComponentGuid => new Guid("621eac11-23fb-445c-9430-44ce37bf9020");

        public override void ClearData()
        {
            base.ClearData();
            Param.Clear();
            M_out.Clear();
            W = double.NaN;
            Ra = double.NaN;
            L = double.NaN;
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Trapezoid Load", "W", "Trapezoid Load (kN/m^2)", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("D Width", "DW", "Domination Width (mm)", GH_ParamAccess.item, 1800);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item,
                205000);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment (kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress (N/mm^2)",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending Stress", "fb", "Output Allowable Bending Stress (N/mm^2)",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) return;

            if (!DA.GetData(1, ref W)) return;

            if (!DA.GetData(2, ref DW)) return;

            if (!DA.GetData(3, ref Lb)) return;

            if (!DA.GetData(4, ref E)) return;

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];
            Iy = Param[3];
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M = W * (DW / 1000) / 24 * (3 * L * L - 4 * DW * DW) / 1000000;
            Sig = M * 1000000 / Zy;
            D = W * (DW / 1000) / (1920 * E * Iy) * (5 * L * L - 4 * DW * DW) * (5 * L * L - 4 * DW * DW);
            Ra = W * (DW / 1000) * (L - DW) / 2; // 反力
            Mx = (Ra * L / 4 - W * (DW / 1000) * Math.Pow(L / 4, 3) / (6 * DW)) / 1000000; // 1/4点のモーメント計算

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
            if (double.IsNaN(W))
                return;
            var loadArrowCenter = new Point3d(0, L / 2, DW / 2);
            //
            var rfArrowStart1 = new Point3d(0, 0, -L / 10);
            var rfArrowEnd1 = new Point3d(0, 0, 0);
            var rfArrow1 = new Line(rfArrowStart1, rfArrowEnd1);
            //
            var rfArrowStart2 = new Point3d(0, L, -L / 10);
            var rfArrowEnd2 = new Point3d(0, L, 0);
            var rfArrow2 = new Line(rfArrowStart2, rfArrowEnd2);

            if (D != 0)
            {
                for (var i = 0; i < 10; i++)
                {
                    var loadPosition = L / 11 * (i + 1);
                    Point3d loadArrowStart;
                    Point3d loadArrowEnd;
                    Line loadArrow;
                    if (loadPosition < DW)
                    {
                        loadArrowStart = new Point3d(0, loadPosition, loadPosition / 2);
                        loadArrowEnd = new Point3d(0, loadPosition, 0);
                        loadArrow = new Line(loadArrowStart, loadArrowEnd);
                    }
                    else if (loadPosition > L - DW)
                    {
                        loadArrowStart = new Point3d(0, loadPosition, (L - loadPosition) / 2);
                        loadArrowEnd = new Point3d(0, loadPosition, 0);
                        loadArrow = new Line(loadArrowStart, loadArrowEnd);
                    }
                    else
                    {
                        loadArrowStart = new Point3d(0, loadPosition, DW / 2);
                        loadArrowEnd = new Point3d(0, loadPosition, 0);
                        loadArrow = new Line(loadArrowStart, loadArrowEnd);
                    }

                    args.Display.DrawArrow(loadArrow, _loadArrowColour);
                }

                //
                args.Display.DrawLine(new Point3d(0, 0, 0), new Point3d(0, DW, DW / 2), _loadArrowColour);
                args.Display.DrawLine(new Point3d(0, DW, DW / 2), new Point3d(0, L - DW, DW / 2), _loadArrowColour);
                args.Display.DrawLine(new Point3d(0, L - DW, DW / 2), new Point3d(0, L, 0), _loadArrowColour);
                args.Display.Draw2dText(W.ToString("F1"), _loadArrowColour, loadArrowCenter, true, 22);
                //
                args.Display.DrawArrow(rfArrow1, _rfArrowColour);
                args.Display.Draw2dText((Ra / 1000).ToString("F1"), _rfArrowColour, rfArrowStart1, false, 22);
                //
                args.Display.DrawArrow(rfArrow2, _rfArrowColour);
                args.Display.Draw2dText((Ra / 1000).ToString("F1"), _rfArrowColour, rfArrowStart2, false, 22);
            }
        }
    }
}