using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Mice.Solvers;
using Rhino.Geometry;

namespace Mice.Components.Analysis
{/// <summary>
    /// 任意荷重のかかった梁の計算
    /// </summary>
    public class AnyM : GH_Component {
        private Color TextColour = Color.FromName("Black");
        private Color LoadArrowColour = Color.FromName("Green");
        private Color RFArrowColour = Color.FromName("Chocolate");
        // input
        List<double> Param = new List<double>();
        List<double> M_out = new List<double>();
        double Lb, E;
        // output
        double M, Sig, D;
        //
        double L, Zy;
        double C = 1.0;
        double fb = 0.0;
        // サブカテゴリ内の配置
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public AnyM()
            //     名称          略称     ｺﾝﾎﾟｰﾈﾝﾄの説明                 ｶﾃｺﾞﾘ   ｻﾌﾞｶﾃｺﾞﾘ
            : base("Any Moment", "Any M", "Analysis of the beam of Any Moment", "Mice", "Beam Analysis") {
        }
        
        public override void ClearData() {
            base.ClearData();
            Param.Clear();
            M_out.Clear();
            
        }
        
        // ジオメトリなどを出力しなくてもPreviewを有効にする。
        public override bool IsPreviewCapable {
            get {
                return true;
            }
        }
        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddNumberParameter("Analysis Parametar", "Param", "Input Analysis Parameter", GH_ParamAccess.list);
            pManager.AddNumberParameter("Any Moment", "AnyM", "Any Moment (kNm)", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Lb", "Lb", "Buckling Length (mm)", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's Modulus (N/mm^2)", GH_ParamAccess.item, 205000);
            pManager[0].Optional = true;
        }
        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddNumberParameter("Bending Moment", "M", "Output Max Bending Moment(kNm)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Stress", "Sig", "Output Max Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Allowable Bending stress", "fb", "Output Allowable Bending Stress(N/mm^2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Examination Result", "Sig/fb", "Output Max Examination Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Deformation", "D", "Output Max Deformation(mm)", GH_ParamAccess.item);
        }
        /// <summary>
        /// 計算部分
        /// </summary>
        /// <param name="DA">インプットパラメータからデータを取得し、出力用のデータを保持するオブジェクト</param>
        protected override void SolveInstance(IGH_DataAccess DA) {
            // 入力設定＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetDataList(0, Param)) { return; }
            if (!DA.GetData(1, ref M)) { return; }
            if (!DA.GetData(2, ref Lb)) { return; }
            if (!DA.GetData(3, ref E)) { return; }

            // 必要な引数の割り当て＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            L = Param[1];;
            Zy = Param[4];

            // 梁の計算箇所＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            Sig = M * 1000000 / Zy;
            D = 0;

            // モーメントの出力＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
            M_out.Add(M);
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
        /// <summary>
        /// Rhino の viewport への出力
        /// </summary>
        public override void DrawViewportWires(IGH_PreviewArgs args) {
            // 荷重出力
            Point3d LoadArrowStart = new Point3d(0, L / 2, L / 5);
            if (fb != 0) {
                args.Display.Draw2dText(("AnyMoment "+ M_out[0].ToString("F1")), LoadArrowColour, LoadArrowStart, true, 22);
            }
        }
        /// <summary>
        /// アイコンの設定。24x24 pixelsが推奨
        /// </summary>
        protected override Bitmap Icon {
            get {
                return Properties.Resource.AnyM_icon;
            }
        }
        /// <summary>
        /// GUIDの設定
        /// </summary>
        public override Guid ComponentGuid {
            get {
                return new Guid("621eac11-23fb-445c-9430-44ce37ba9020");
            }
        }
    }
}