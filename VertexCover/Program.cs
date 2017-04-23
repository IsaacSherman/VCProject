using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using MyUtils;
using System.IO;

namespace EvoOptimization
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> sizes = new List<int>(), sols = new List<int>(), evoSols = new List<int>(), approxSols=new List<int>();
            List<double> bfTimes = new List<double>(), bfpTimes = new List<double>(), approxTimes = new List<double>(), evoTimes = new List<Double>();
            Stopwatch sw = new Stopwatch();
            for (int i = 10; i < 41; ++i)
            {
                sizes.Add(i);
                string graphName = "../../graphs/density=15/graph" + i + ".txt";
                OptoGlobals.ReadGraph(graphName);
                BitArray BruteForceBits, ApproxBits;
                //Brute Force
                sw.Start();
                //BruteForceBits = OptoGlobals.BruteForce();
                sw.Stop();
                bfTimes.Add(sw.ElapsedMilliseconds);
                //sols.Add(BruteForceBits.SumBitArray());
                sw.Reset();

                //Parallel Brute Force
                sw.Start();
                    BruteForceBits = OptoGlobals.ParallelBruteForce();
                sw.Stop();
                    bfpTimes.Add(sw.ElapsedMilliseconds);
                sols.Add(BruteForceBits.SumBitArray());
                sw.Reset();
                sw.Stop();
                //Approximate
                sw.Start();
                //ApproxBits = OptoGlobals.Approximation();
                sw.Stop();
                //approxSols.Add(ApproxBits.SumBitArray());
                approxTimes.Add(sw.ElapsedMilliseconds);
                sw.Reset();

                //Evolutionary Algorithm

                EvoOptimizerProgram<VertexOptimizer> P = new EvoOptimizerProgram<VertexOptimizer>();
                P.MaxGen = 5*i;
                P.MultiThread = true;
                P.Noload = true;
                P.OutputBaseline = false;
                P.PopSize = 100;
                P.SuppressMessages = true;
                P.SaveAfterGens = 25;
                P.ConfigureEvolver();
                sw.Start();
                P.Run();
                sw.Stop();
                evoTimes.Add(sw.ElapsedMilliseconds);

                VertexOptimizer best = P.YieldBestOptimizer();
                evoSols.Add(best.Bits.SumBitArray());
                Console.WriteLine("Evolution yielded: " + best.Bits.BitsToString() + "\n And Brute Force yielded: "
                    + BruteForceBits.BitsToString());// + "\n And an approximation yielded: " + ApproxBits.BitsToString());
            }

            using (StreamWriter fout = new StreamWriter("results.csv")){
                StringBuilder line = writeLineToSB(sizes, "Size,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(bfTimes, "Brute Force Serial,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(bfpTimes, "Brute Force Parallel,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(approxTimes, "Approximate,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(evoTimes, "EvoAlg,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(sols, "Optimal Solution");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(approxSols, "Approximate Solution,");
                fout.WriteLine(line.ToString());
                line = writeLineToSB(evoSols, "EA Solution,");
                fout.WriteLine(line.ToString());

            }
/*                for (BitArray x = w.Init(7); w.Good(); x = w.Next())
                {
                    Console.WriteLine(x.BitsToString());
                }
            List<Double> times = new List<double>();
            Stopwatch sw = new Stopwatch();
            for (int i = 6; i < 10; i += 5)
            {

                sw.Start();
                OptoGlobals.n = i;
                OptoGlobals.BruteForce();

                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
                Console.WriteLine("Elapsed time = " + sw.ElapsedMilliseconds + "ms");
                sw.Reset();
            }
            int count = 5;
                
            foreach(double x in times){
                Console.WriteLine("Elapsed time for n = " + count + " = " + x + "ms.");
                count += 5;
            }
*/
                Console.ReadKey();
            EvoOptimization.EvoOptimizer<VertexOptimizer> D = new EvoOptimization.EvoOptimizer<VertexOptimizer>();

        }

        private static StringBuilder writeLineToSB<T>(List<T> sizes, string Header)
        {
            StringBuilder line = new StringBuilder(Header);
            for (int i = 0; i < sizes.Count; ++i)
            {
                line.Append(sizes[i] + ",");
            }
            line.Remove(line.Length - 1, 1);
            return line;
        }


        
    }
}
