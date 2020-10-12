using System;
using Grasshopper.Kernel;
using Mice.Solvers;

namespace Mice.Components.Util
{
    public class KTtoM : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4830-9cef-5f5647a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.calcM_icon;
        
        public KTtoM()
            : base("Calc M from K & T",                    // 名称
                   "KTtoM",                                // 略称
                   "Calculate Mass from Stiffness and Period",    // コンポーネントの説明
                   "Mice",                                 // カテゴリ(タブの表示名)
                   "Response Analysis"                     // サブカテゴリ(タブ内の表示名)
                  )
        {
        }

        /// <summary>
        /// インプットパラメータの登録
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Stiffness", "K", "Spring Stiffness(N/m)", GH_ParamAccess.item);
            pManager.AddNumberParameter("NaturalPeriod", "T", "Natural Period(sec)", GH_ParamAccess.item);
        }

        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Mass", "M", "Lumped Mass(kg)", GH_ParamAccess.item);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double M = double.NaN;        // 質量
            double K = double.NaN;        // 剛性
            double T = double.NaN;        // 固有周期

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref K)) { return; }
            if (!DA.GetData(1, ref T)) { return; }

            // 各値の計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            M = ResponseAnalysis.KT2M(K, T);

            // grasshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetData(0, M);
        }
    }
}