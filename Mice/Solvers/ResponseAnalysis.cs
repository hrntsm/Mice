using System;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;

namespace Mice.Solvers
{
    public class ResponseAnalysis
    {
        /// <summary>
        /// ここを参考に実装
        /// NewmarkBeta: http://kentiku-kouzou.jp/fortran-7.html
        /// エネルギー： https://www.jstage.jst.go.jp/article/jscej1984/2001/676/2001_676_1/_pdf
        /// </summary>
        public static void NewmarkBeta(double mass, double k, double h, double dt, double beta, int N, double[] accInput,
                                       out double[] outAcc, out double[] outVel, out double[] outDisp,
                                       out double[] outEo, out double[] outEi, out double[] outEk, out double[] outEp
                                       )
        {
            // 解析関連パラメータ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double acc = 0d, vel = 0d, disp = 0d, prevAcc = 0d, prevVel = 0d, prevDisp = 0d;
            double accIni = accInput[0];
            const double velIni = 0d;
            const double dispIni = 0d;
            double c = 2 * h * Math.Sqrt(mass * k); // 粘性減衰定数 (kN s/m)
            
            outAcc = new double[N];
            outVel = new double[N];
            outDisp = new double[N];
            outEo = new double[N]; // 総入力エネルギー
            outEi = new double[N]; // 内部粘性減衰エネルギー
            outEk = new double[N]; // 運動エネルギー
            outEp = new double[N]; // 弾性ひずみエネルギー

            // 解析個所 ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            for (int n = 0; n < N; n++)
            {
                if (n == 0)
                {
                    acc = accIni;
                    vel = velIni;
                    disp = dispIni;
                    outEp[n] = 0;
                    outEk[n] = 0;
                    outEi[n] = 0;
                    outEo[n] = 0;
                }
                else
                {
                    var accU = accInput[n] +
                                      c / mass * (prevVel + 0.5 * prevAcc * dt) +
                                      k / mass * (prevDisp + prevVel * dt + (0.5 - beta) * prevAcc * Math.Pow(dt, 2));
                    var accL = 1d + (0.5 * c / mass * dt) + (beta * k / mass * Math.Pow(dt, 2));
                    acc = -1d * accU / accL;
                    vel = vel + 0.5 * dt * (acc + prevAcc);
                    disp = disp + prevVel * dt + ((0.5 - beta) * prevAcc * Math.Pow(dt, 2) + beta * acc * Math.Pow(dt, 2));

                    // 各エネルギー結果--------------------------------------------
                    outEp[n] = outEp[n - 1] + (k * disp) * vel;   // 弾性ひずみエネルギー
                    outEk[n] = outEk[n - 1] + (mass * acc) * vel; // 運動エネルギー
                    outEi[n] = outEi[n - 1] + (c * vel) * vel;             // 内部粘性減衰エネルギー
                    outEo[n] = outEo[n - 1] - (mass * accInput[n]) * vel;  // 波の入力エネルギー
                }

                // 結果を出力配列に格納＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
                outAcc[n] = acc;
                outVel[n] = vel;
                outDisp[n] = disp;
                prevAcc = acc;
                prevVel = vel;
                prevDisp = disp;
            }
        }

        public static double[] Csv2Wave(string waveStr, int N)
        {
            char[] delimiter = {','}; //分割文字
            double[] wave = new double[N];
            var wk = waveStr.Split(delimiter);
            for (int i = 0; i < N; i++)
            {
                wave[i] = double.Parse(wk[i]);
            }

            return wave;
        }

        public static double MT2K(double mass, double period)
        {
            var stiffness = (4.0 * mass * Math.PI * Math.PI) / (period * period);
            return stiffness;
        }

        public static double KT2M(double stiffness, double period)
        {
            var mass = stiffness * ( period * period ) / (4.0 * Math.PI * Math.PI);
            return mass;
        }
        
        public static double MK2T(double mass, double stiffness)
        {
            var omega = Math.Sqrt(stiffness / mass);
            var T = 2.0 * Math.PI / omega;
            return T;
        }
    }
}