using System;

namespace Mice.Solvers
{
    public class ResponseAnalysis
    {
        public static void NewmarkBeta(double m, double k, double h, double dt, double beta, int N, double[] accInput,
                                       out double[] outAcc, out double[] outVel, out double[] outDisp,
                                       out double[] outEo, out double[] outEi, out double[] outEk, out double[] outEp
                                       )
        {
            // 解析関連パラメータ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
            double acc = 0d, vel = 0d, disp = 0d, an = 0d, vn = 0d, xn = 0d;
            double accIni = accInput[0];
            const double velIni = 0d;
            const double dispIni = 0d;
            double c = 2 * h * Math.Sqrt(m * k); // 粘性減衰定数 (kN s/m)
            
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
                    acc = -(c * (vel + acc * dt / 2.0) + k * (disp + vel * dt + acc * (dt * dt) * (0.5 - beta)) + m * accInput[n])
                        / (m + c * dt / 2.0 + k * (dt * dt) * beta);
                    vel = vel + (1.0 / 2.0) * (acc + an) * dt;
                    disp = disp + vn * dt + beta * (acc + 2.0 * an) * (dt * dt);

                    // 各エネルギー結果--------------------------------------------
                    outEp[n] = 1.0 / 2.0 * k * (disp * disp);         // 弾性ひずみエネルギー
                    outEk[n] = 1.0 / 2.0 * (m) * (vel * vel);       // 運動エネルギー
                    outEi[n] = c * (vel * vel) + outEi[n - 1];      // 内部粘性減衰エネルギー
                    outEo[n] = outEp[n] + outEk[n] + outEi[n];  // 総入力エネルギー
                }

                // 結果を出力配列に格納＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
                outAcc[n] = acc;
                outVel[n] = vel;
                outDisp[n] = disp;
                an = acc;
                vn = vel;
                xn = disp;
            }
        }
    }
}