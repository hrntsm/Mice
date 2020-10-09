using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mise.Components.Model
{
    /// <summary>
    /// H型断面の計算、出力
    /// </summary>
    public class HShape : GH_Component {
        private List<Brep> ModelBrep = new List<Brep>();
        private Rhino.Display.DisplayMaterial ModelMaterial;
        private Color ModelColour = Color.FromName("LightCoral");

        public HShape()
            //     名称                  略称       ｺﾝﾎﾟｰﾈﾝﾄの説明           ｶﾃｺﾞﾘ   ｻﾌﾞｶﾃｺﾞﾘ
            : base("Make H Shape Model", "H Shape", "Display H Shape Model", "Mice", "CrossSection") {
        }
        public override void ClearData() {
            base.ClearData();
            ModelBrep.Clear();
        }
        protected override void BeforeSolveInstance() {
            ModelBrep = new List<Brep>();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 200.0);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 400.0);
            pManager.AddNumberParameter("Web Thickness", "tw", "Web Thickness (mm)", GH_ParamAccess.item, 8.0);
            pManager.AddNumberParameter("Flange Thickness", "tf", "Flange Thickness (mm)", GH_ParamAccess.item, 13.0);
            pManager.AddNumberParameter("F", "F", "F (N/mm2)", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 6300.0);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "output Analysis Parameter", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("View Model Surface", "Srf", "output Model Surface", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 引数設定
            double B = double.NaN;
            double H = double.NaN;
            double L = double.NaN;
            double tw = double.NaN;
            double tf = double.NaN;
            double F = double.NaN;
            double Iy, Zy, i_t, lamda, Af;
            int fb_calc = 0; // 0:H強軸　1:箱型、丸形　2:L形

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref tw)) { return; }
            if (!DA.GetData(3, ref tf)) { return; }
            if (!DA.GetData(4, ref F)) { return; }
            if (!DA.GetData(5, ref L)) { return; }

            // 原点の作成
            var Ori = new Point3d(0, 0, 0);

            // 上フランジのサーフェス作成
            var UFp1 = new Point3d(0, 0, H / 2);
            var UFp2 = new Point3d(1, 0, H / 2);
            var UFp3 = new Point3d(0, 1, H / 2);
            var UFplane = new Plane(UFp1, UFp2, UFp3);
            var upper_flange = new PlaneSurface(UFplane, new Interval(-B / 2, B / 2), new Interval(0, L));

            // 下フランジのサーフェス作成
            var BFp1 = new Point3d(0, 0, -H / 2);
            var BFp2 = new Point3d(1, 0, -H / 2);
            var BFp3 = new Point3d(0, 1, -H / 2);
            var BFplane = new Plane(BFp1, BFp2, BFp3);
            var bottom_flange = new PlaneSurface(BFplane, new Interval(-B / 2, B / 2), new Interval(0, L));

            // ウェブのサーフェス作成
            var Wp1 = new Point3d(0, 0, 0);
            var Wp2 = new Point3d(0, 0, -1);
            var Wp3 = new Point3d(0, 1, 0);
            var Wplane = new Plane(Wp1, Wp2, Wp3);
            var web = new PlaneSurface(Wplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // 解析用パラメータの計算
            Iy = (1.0 / 12.0 * B * H * H * H) - (1.0 / 12.0 * (B - tw) * (H - 2 * tf) * (H - 2 * tf) * (H - 2 * tf));
            Zy = Iy / (H / 2);

            // 許容曲げ関連の計算
            i_t = Math.Sqrt((tf * B * B * B + (H / 6.0 - tf) * tw * tw * tw) / (12 * (tf * B + (H / 6.0 - tf) * tw)));
            lamda = 1500 / Math.Sqrt(F / 1.5);
            Af = B * tf;

            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(H);
            Params.Add(L);  //  部材長さ
            Params.Add(F);
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数
            Params.Add(fb_calc); 
            Params.Add(i_t);
            Params.Add(lamda);
            Params.Add(Af);

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[3];
            model[0] = upper_flange;
            model[1] = bottom_flange;
            model[2] = web;
            ModelBrep.Add(upper_flange.ToBrep());
            ModelBrep.Add(bottom_flange.ToBrep());
            ModelBrep.Add(web.ToBrep());

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(1, model);
            DA.SetDataList(0, Params);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 3; i++) {
                    args.Display.DrawBrepShaded(ModelBrep[i], ModelMaterial);
                }
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 3; i++) {
                    args.Display.DrawBrepWires(ModelBrep[i], ModelMaterial.Diffuse, 0);
                }
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return Properties.Resource.H_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a3a-cc48-4717-8cef-5f5647a5ecAa");
            }
        }
    }
}