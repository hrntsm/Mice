using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mice.Components.Model
{
    public class LumpedMass : GH_Component
    {
        private readonly List<Surface> _srf = new List<Surface>();
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4715-9cef-5f5648a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.ModelViewicon;

        public LumpedMass()
            : base("Model View",                           // 名称
                   "Model View",                           // 略称
                   "Response Analysis of the Single dof",   // コンポーネントの説明
                   "Mice",                                  // カテゴリ(タブの表示名)
                   "Result"                                 // サブカテゴリ(タブ内の表示名)
                  )
        {
        }
        
        public override void ClearData() {
            base.ClearData();
            _srf.Clear();
        }

        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Model", "Model", "Model Data", GH_ParamAccess.list);
            pManager.AddNumberParameter("Scale", "Sc", "Scale Model", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("High", "H", "High Model", GH_ParamAccess.item, 3500);
        }

        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Srf", "Output Model Surface", GH_ParamAccess.item);
        }

        /// <summary>
        /// 解析を実行する箇所
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            List<double> Model = new List<double>();
            double Sc = double.NaN;
            double H = double.NaN;
            double M = double.NaN;
            double K = double.NaN;

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Model)) { return; }
            if (!DA.GetData(1, ref Sc)) { return; }
            if (!DA.GetData(2, ref H)) { return; }

            // 質点の作成＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Point3d FMorigin = new Point3d(0, 0, H);
            Point3d FMp1 = new Point3d(1, 0, H);
            Point3d FMp2 = new Point3d(0, 1, H);
            Plane FMplane = new Plane(FMorigin, FMp1, FMp2);
            M = Model[0];
            Sphere FirstMass = new Sphere(FMplane, Sc * M);

            // バネの作成＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Point3d FSPGorigin = new Point3d(0, 0, 0);
            Point3d FSPGp1 = new Point3d(1, 0, 0);
            Point3d FSPGp2 = new Point3d(0, 1, 0);
            Plane FSPGplane = new Plane(FSPGorigin, FSPGp1, FSPGp2);
            K = Model[1];
            Cylinder FirstSpring = new Cylinder(new Circle(FSPGplane, Sc * K / 50), H-Sc*M);

            // モデルのrhino上への出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            _srf[0] = FirstSpring.ToRevSurface();
            _srf[1] = FirstMass.ToRevSurface();

            DA.SetDataList(0, _srf);
        }
    }
}