using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mice.Components.Result
{
    public class MomentViewer : GH_Component {
        private List<double> M = new List<double>();
        private double M1 = 0, M2 = 0, M3 = 0, M4 = 0, M5 = 0, L = 0;
        private Point3d M_point1, M_point2, M_point3, M_point4, M_point5;
        private List<Brep> MomentBrep = new List<Brep> ();
        private Rhino.Display.DisplayMaterial MomentMaterial;
        private Color TextColour = Color.FromName("Black");
        private Color MomentColour = Color.FromName("SkyBlue");

        public MomentViewer()
            //     名称           略称      ｺﾝﾎﾟｰﾈﾝﾄの説明    ｶﾃｺﾞﾘ   ｻﾌﾞｶﾃｺﾞﾘ
            : base("Moment View", "Moment", "Display Moment", "Mice", "Result") {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Moment", "M", "Input Moment", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scale", "Sc", "Input Output Scale", GH_ParamAccess.item, 10);
            pManager[0].Optional = true;
        }
        public override void ClearData() {
            base.ClearData();
            M.Clear();
            MomentBrep.Clear();
        }
        protected override void BeforeSolveInstance() {
            M = new List<double>();
            MomentBrep = new List<Brep>();
            M1 = 0; M2 = 0; M3 = 0; M4 = 0; M5 = 0; L = 0;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddBrepParameter("View Moment Surface", "Srf", "output Moment Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double Sc = double.NaN;

            // 入力設定
            if (!DA.GetDataList(0, M)) { return; }
            if (!DA.GetData(1, ref Sc)) { return; }

            // Scが0だとbrepが作れずエラーが出るので、Scの最小値を1に設定
            if (Sc < 1) {
                Sc = 1;
            }

            M1 = M[0];
            M2 = M[1];
            M3 = M[2];
            M4 = M[3];
            M5 = M[4];
            L = M[5];

            // モーメント図の作成
            var M12_P1 = new Point3d(0, 0, 0);
            var M12_P2 = new Point3d(0, 0, Sc * -M1);
            var M12_P3 = new Point3d(0, L / 4, Sc * -M2);
            var M12_P4 = new Point3d(0, L / 4, 0);
            M_point1 = new Point3d(0, 0, Sc * -M1);
            //
            var M23_P1 = new Point3d(0, L / 4, 0);
            var M23_P2 = new Point3d(0, L / 4, Sc * -M2);
            var M23_P3 = new Point3d(0, L / 2, Sc * -M3);
            var M23_P4 = new Point3d(0, L / 2, 0);
            M_point2 = new Point3d(0, L / 4, Sc * -M2);
            //
            var M34_P1 = new Point3d(0, L / 2, 0);
            var M34_P2 = new Point3d(0, L / 2, Sc * -M3);
            var M34_P3 = new Point3d(0, 3 * L / 4, Sc * -M4);
            var M34_P4 = new Point3d(0, 3 * L / 4, 0);
            M_point3 = new Point3d(0, L / 2, Sc * -M3);
            //
            var M45_P1 = new Point3d(0, 3 * L / 4, 0);
            var M45_P2 = new Point3d(0, 3 * L / 4, Sc * -M4);
            var M45_P3 = new Point3d(0, L, Sc * -M5);
            var M45_P4 = new Point3d(0, L, 0);
            M_point4 = new Point3d(0, 3 * L / 4, Sc * -M4);
            M_point5 = new Point3d(0, L, Sc * -M5);

            Brep M12_brep = Brep.CreateFromCornerPoints(M12_P1, M12_P2, M12_P3, M12_P4, GH_Component.DocumentTolerance());
            Brep M23_brep = Brep.CreateFromCornerPoints(M23_P1, M23_P2, M23_P3, M23_P4, GH_Component.DocumentTolerance());
            Brep M34_brep = Brep.CreateFromCornerPoints(M34_P1, M34_P2, M34_P3, M34_P4, GH_Component.DocumentTolerance());
            Brep M45_brep = Brep.CreateFromCornerPoints(M45_P1, M45_P2, M45_P3, M45_P4, GH_Component.DocumentTolerance());

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            MomentBrep.Add(M12_brep);
            MomentBrep.Add(M23_brep);
            MomentBrep.Add(M34_brep);
            MomentBrep.Add(M45_brep);

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, MomentBrep);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            if (M2 != 0) {
                args.Display.Draw2dText(M1.ToString("F1"), TextColour, M_point1, true, 22);
                args.Display.Draw2dText(M2.ToString("F1"), TextColour, M_point2, true, 22);
                args.Display.Draw2dText(M3.ToString("F1"), TextColour, M_point3, true, 22);
                args.Display.Draw2dText(M4.ToString("F1"), TextColour, M_point4, true, 22);
                args.Display.Draw2dText(M5.ToString("F1"), TextColour, M_point5, true, 22);
            }
            // sureface 色付け
            MomentMaterial = new Rhino.Display.DisplayMaterial(MomentColour);

            if (M2 != 0) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepShaded(MomentBrep[i], MomentMaterial);
                }
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            MomentMaterial = new Rhino.Display.DisplayMaterial(MomentColour);

            if (M2 != 0) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepWires(MomentBrep[i], MomentMaterial.Diffuse, 0);
                }
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resource.Result_M_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a3a-cc48-4717-8cef-5f5647a5dcAa");
            }
        }
    }
}