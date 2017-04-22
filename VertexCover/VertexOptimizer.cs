using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyUtils;

namespace EvoOptimization
{
    class VertexOptimizer:Optimizer
    {
        static int _bitLength;

        public static void ResetBitLength()
        {
            _bitLength = OptoGlobals.n;
        }

        static VertexOptimizer() { ResetBitLength(); }

        protected override string getFunctionString()
        {
            return null;
        }

        protected override void errorCheck()
        {
            return;
        }

        public override void Eval()
        {
            double bestFitness =  fitnessfunc();
            int bestIndex = -1;
            for (int i = 0; i < Bits.Length; ++i) {
                if (Bits[i])
                {
                    Bits[i] = false;
                    double newFitness = fitnessfunc();
                    Bits[i] = true;
                    if (newFitness > bestFitness)
                    {
                        bestFitness = newFitness;
                        bestIndex = i;
                    }
                }
             
            }
            _fitness = bestFitness;
            if (bestIndex != -1) { Bits[bestIndex] = false; }
           
            
            
        }

        private double fitnessfunc()
        {
            BitArray coverage;
            if (OptoGlobals.CheckIfCover(Bits, out coverage))
            {
                _fitness = (double)Bits.Length / Bits.SumBitArray();
            }
            else
            {
                _fitness = (double)coverage.SumBitArray() / (Bits.SumBitArray() + (2 * Bits.Length));
            }
            return _fitness;
        }

        public override double FirstMetric
        {
            get
            {
                return Fitness;
            }
            set
            {
                Fitness = value;
            }
        }

        public override double SecondMetric
        {
            get
            {
                return 0;
            }
            set
            {
                return;
            }
        }

        protected override void setFitness()
        {
            Eval();
        }
    }
}
