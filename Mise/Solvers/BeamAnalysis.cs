using System;
using System.Collections.Generic;

namespace Mise.Solvers
{
    /// <summary>
    /// 許容曲げを計算するクラス
    /// </summary>
    public class BeamAnalysis {
        public static double CalcFb(List<double> param, double lb,  double C) {
            // 解析関連パラメータ ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double H, L, F, fb_calc, i_t, lamda, Af, fb1, fb2, fb;
            H = param[0];
            L = param[1];
            F = param[2];
            fb_calc = param[5];
            i_t = param[6];
            lamda = param[7];
            Af = param[8];

            switch (fb_calc)
            {
                case 0:  // H強軸回りの場合
                    fb1 = (1.0 - 0.4 * (lb / i_t) * (lb / i_t) / (C * lamda * lamda)) * F / 1.5;
                    fb2 = 89000.0 / (lb * H / Af);
                    fb = Math.Min(Math.Max(fb1, fb2), F / 1.5);
                    break;
                case 1:  // 箱型丸型の場合
                    fb = F / 1.5;
                    break;
                case 2: // L型等非対称断面の場合
                    fb2 = 89000.0 / (lb * H / Af);
                    fb = Math.Min(fb2, F / 1.5);
                    break;
                default:  // エラー用　sig/fb が inf になるように 0指定
                    fb = 0.0;
                    break;
            }

            return fb;
        }
    }
}