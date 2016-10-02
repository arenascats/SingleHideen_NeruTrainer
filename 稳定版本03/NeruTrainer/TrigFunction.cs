using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeruTrainer
{
    class TrigFunction
    {
        public double DataNormalize(double x, double Min, double Max)
        {
            double Output = 0;
            Output = (x - Min) / (Max - Min);
            return Output;
        }
        public double DataNormalizeS(double x, double Min, double Max)
        {
            double Output = 0;
            Output = 2 * (x - Min) / (Max - Min) - 1;
            return Output;
        }
        public double Liner(double k, double x, double c)
        {
            double Output;
            Output = k * x + c;
            return Output;
        }
        public double Ramp(double T, double k, double x, double c)
        {
            if (x > c) return T;
            else if (x * x <= c) return k * x;
            else if (x < -c) return -T;
            else return -1;
        }
        public double Threshold(double x, double c)
        {
            if (x >= c) return 1;
            else if (x < c) return 0;
            else return -1;
        }
        public double Threshold(double x, double c, double min, double max)
        {
            if (x >= c) return max;
            else if (x < c) return min;
            else return -1;
        }
        public double Sigmoid(double x, double a)
        {
            double Output = 0;
            Output = 1.0 / (1.0 + Math.Exp(-x -a));
            return Output;
        }
        public double Sigmoidnob(double x)
        {
            double Output = 0;
            Output = 1.0 / (1.0 + Math.Exp(-x));
            return Output;
        }
        public double DoubleSigmoid(double x, double a)
        {
            double Output = 0;
            Output = 2 / (1 + Math.Pow(Math.E, -2 * a* x));
            return Output;
        }
    }
}
