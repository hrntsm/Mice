using System;
using Grasshopper.Kernel;

namespace Mice.Components.Util
{
    public class SinWave : GH_Component
    {
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override Guid ComponentGuid => new Guid("419c3a3a-cc48-4835-9cef-5f5647a5ecfc");
        protected override System.Drawing.Bitmap Icon => Properties.Resource.MakeSinWaveicon;
        
        public SinWave()
            : base("Make Sin Wave ",                    // 名称
                   "SinWave",                                // 略称
                   "Make Sin Wave",    // コンポーネントの説明
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
            pManager.AddNumberParameter("Amplitude", "A", "Amplitude", GH_ParamAccess.item,100);
            pManager.AddNumberParameter("Period", "T", "Period(sec)", GH_ParamAccess.item,0.5);
            pManager.AddNumberParameter("Time Increment", "dt", "Time Increment(sec)", GH_ParamAccess.item,0.02);
            pManager.AddIntegerParameter("Data Length", "N", "Data Length", GH_ParamAccess.item,1000);
        }

        /// <summary>
        /// アウトプットパラメータの登録
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Wave", "Wave", "Acceleration Wave(cm/s^2)", GH_ParamAccess.item);
        }

        /// <summary>
        /// 解析を実行する箇所
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // パラメータの定義 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double A = double.NaN;    
            double T = double.NaN;
            double dt = double.NaN;
            int N = 0;

            // grasshopper からデータ取得　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            if (!DA.GetData(0, ref A)) { return; }
            if (!DA.GetData(1, ref T)) { return; }
            if (!DA.GetData(2, ref dt)) { return; }
            if (!DA.GetData(3, ref N)) { return; }

            // 各値の計算 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double[] wave = new double[N];
            for (int i = 0; i<=N-1; ++i)
            {
                wave[i] = A * Math.Sin((2 * Math.PI) * (dt / T) * i);
            }
            // カンマ区切りのテキストで出力------------------------------
            String wave_csv = string.Join(",",wave);

            // grasshopper へのデータ出力　＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            DA.SetData(0, wave_csv);
        }
    }
}