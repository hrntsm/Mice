using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mise.Components.Model
{
    /// <summary>
    /// 箱型断面の計算、出力
    /// </summary>
    public class BOX_Shape_Model : GH_Component
    {
        private List<Brep> ModelBrep = new List<Brep>();
        private Rhino.Display.DisplayMaterial ModelMaterial;
        private Color ModelColour = Color.FromName("LightCoral");

        public BOX_Shape_Model()
            //     名称                    略称         ｺﾝﾎﾟｰﾈﾝﾄの説明             ｶﾃｺﾞﾘ   ｻﾌﾞｶﾃｺﾞﾘ
            : base("Make BOX Shape Model", "BOX Shape", "Display BOX Shape Model", "Mice", "CrossSection") {
        }
        public override void ClearData() {
            base.ClearData();
            ModelBrep.Clear();
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Width", "B", "Model Width (mm)", GH_ParamAccess.item, 150.0);
            pManager.AddNumberParameter("Height", "H", "Model High (mm)", GH_ParamAccess.item, 150.0);
            pManager.AddNumberParameter("Thickness", "t", "Thickness (mm)", GH_ParamAccess.item, 6.0);
            pManager.AddNumberParameter("F", "F", "F (N/mm2)", GH_ParamAccess.item, 235);
            pManager.AddNumberParameter("Length", "L", "Model Length (mm)", GH_ParamAccess.item, 3200.0);
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
            double t = double.NaN;
            double F = double.NaN;
            double Iy, Zy;
            int fb_calc = 1; // 0:H強軸　1:箱型、丸形　2:L形

            // 入力設定
            if (!DA.GetData(0, ref B)) { return; }
            if (!DA.GetData(1, ref H)) { return; }
            if (!DA.GetData(2, ref t)) { return; }
            if (!DA.GetData(3, ref F)) { return; }
            if (!DA.GetData(4, ref L)) { return; }

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
            var LWp1 = new Point3d(-B / 2, 0, 0);
            var LWp2 = new Point3d(-B / 2, 0, -1);
            var LWp3 = new Point3d(-B / 2, 1, 0);
            var LWplane = new Plane(LWp1, LWp2, LWp3);
            var Lweb = new PlaneSurface(LWplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // ウェブのサーフェス作成
            var RWp1 = new Point3d(B / 2, 0, 0);
            var RWp2 = new Point3d(B / 2, 0, -1);
            var RWp3 = new Point3d(B / 2, 1, 0);
            var RWplane = new Plane(RWp1, RWp2, RWp3);
            var Rweb = new PlaneSurface(RWplane, new Interval(-H / 2, H / 2), new Interval(0, L));

            // 解析用パラメータの計算
            Iy = 1.0 / 12.0 * ((B * H * H * H) - ((B - t) * (H - t) * (H - t) * (H - t)));
            Zy = Iy / (H / 2);

            // ひとまとめにするため List で作成
            List<double> Params = new List<double>();
            Params.Add(H);
            Params.Add(L);  //  部材長さ
            Params.Add(F);
            Params.Add(Iy); //  断面二次モーメント
            Params.Add(Zy); //  断面係数
            Params.Add(fb_calc);
            Params.Add(0);
            Params.Add(0);
            Params.Add(0);

            // モデルはRhino上に出力するだけなので、とりあえず配列でまとめる
            var model = new PlaneSurface[4];
            model[0] = upper_flange;
            model[1] = bottom_flange;
            model[2] = Rweb;
            model[3] = Lweb;
            ModelBrep.Add(upper_flange.ToBrep());
            ModelBrep.Add(bottom_flange.ToBrep());
            ModelBrep.Add(Rweb.ToBrep());
            ModelBrep.Add(Lweb.ToBrep());

            // まとめての出力なので、SetDataList で出力
            DA.SetDataList(0, Params);
            DA.SetDataList(1, model);
        }
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepShaded(ModelBrep[i], ModelMaterial);
                }
            }
        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {
            ModelMaterial = new Rhino.Display.DisplayMaterial(ModelColour);
            if (ModelBrep != null) {
                for (int i = 0; i < 4; i++) {
                    args.Display.DrawBrepWires(ModelBrep[i], ModelMaterial.Diffuse, 0);
                }
            }
        }
        protected override Bitmap Icon {
            get {
                return Properties.Resource.BOX_icon;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("419c3a44-cc48-4717-8fdf-5f5647a5ecAa");
            }
        }
    }
}