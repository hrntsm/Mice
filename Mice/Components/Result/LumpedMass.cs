using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Mice.Components.Result
{
    public class LumpedMass : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4701-9cef-5f5648a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.ResultViewicon;

        public LumpedMass()
            : base("Result View",                           // 名称
                   "Result View",                           // 略称
                   "Response Analysis of the Single dof",   // コンポーネントの説明
                   "Mice",                                  // カテゴリ(タブの表示名)
                   "Result"                                 // サブカテゴリ(タブ内の表示名)
                  )
        {
        }

        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Model", "Model", "Model Data", GH_ParamAccess.list);
            pManager.AddNumberParameter("Result", "Result", "Analysis Result", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Output Number", "N", "Output Result Number", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Model Scale", "MSc", "Scale Model", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Result Scale", "RSc", "Scale Model", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("High", "H", "High Model", GH_ParamAccess.item, 3000);
        }

        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Srf", "Output Model Surface", GH_ParamAccess.list);
        }

        /// <summary>
        /// 解析を実行する箇所
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            List<double> Model = new List<double>();
            List<double> Rslt = new List<double>();
            double MSc = double.NaN;
            double RSc = double.NaN;
            double H = double.NaN;
            double M = double.NaN;
            double K = double.NaN;
            int N = 100;

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Model)) { return; }
            if (!DA.GetDataList(1, Rslt)) { return; }
            if (!DA.GetData(2, ref N)) { return; }
            if (!DA.GetData(3, ref MSc)) { return; }
            if (!DA.GetData(4, ref RSc)) { return; }
            if (!DA.GetData(5, ref H)) { return; }

            // 質点の作成＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Point3d FMorigin = new Point3d(RSc * Rslt[N], 0, H);
            Point3d FMp1 = new Point3d(RSc * Rslt[N]+1, 0, H);
            Point3d FMp2 = new Point3d(RSc * Rslt[N], 1, H);
            Plane FMplane = new Plane(FMorigin, FMp1, FMp2);
            M = Model[0];
            Sphere FirstMass = new Sphere(FMplane, MSc * M);

            // バネの作成＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Point3d FSPGorigin = new Point3d(0, 0, 0);
            Point3d FSPGp1 = new Point3d(H, 0, RSc * -Rslt[N]);
            Point3d FSPGp2 = new Point3d(0, 1, 0);
            Plane FSPGplane = new Plane(FSPGorigin, FSPGp1, FSPGp2);
            K = Model[1];
            Cylinder FirstSpring = new Cylinder(new Circle(FSPGplane, MSc * M / 10), Math.Sqrt(RSc*RSc*Rslt[N] * Rslt[N] + H*H ) - MSc * M);

            // モデルのrhino上への出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            var Srf = new Surface[2];
            Srf[0] = FirstSpring.ToRevSurface();
            Srf[1] = FirstMass.ToRevSurface();

            DA.SetDataList(0, Srf);
        }
    }
}