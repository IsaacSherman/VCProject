using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections;
using MyUtils;
using System.Threading;
namespace EvoOptimization
{
    public class OptoGlobals
    {
        public enum CrossoverModes { Uniform, SinglePointHunter, TwoPointHunter, RandomHunter, TwoPointChromosome, SinglePointChromosome, RandomChromosome };
        private static int _seed = (int)DateTime.Now.Ticks;
        public static int GetSeed { get { return _seed; } }
        public const int Means = 0, StdDevs = 1, Mins = 2, Maxes = 3, StatSize = 4;
        
        public static CrossoverModes CrossoverMode { get; internal set; }

        public static Boolean IsDebugMode = false;
        public static Random RNG;
        public static double CrossoverChance = .25, ElitismPercent = .20,
            InitialRateOfOnes = .5, MutationChance = .01, MergerPercent = .05;

        public static Dictionary<String, int> ClassDict;
        public static List<String> ClassList, DataHeaders, yHeaders, AllPredictorNames;
        public static int n=-1;



        static OptoGlobals()
        {
#if DEBUG
            IsDebugMode = true;
#endif
            ///This whole thing needs to be moved around
            Console.WriteLine("Entering OptoGlobals STATIC constructor");
            RNG = new Random(_seed);

            Console.WriteLine("Leaving Static Constructor");
        }

        public static void CreateDirectoryAndThenFile(string path)
        {
            char[] tokens = { '/', '\\' };
            string temp = path.Substring(0, path.LastIndexOfAny(tokens));
            if (!Directory.Exists(temp)) Directory.CreateDirectory(temp);
            File.Create(path).Close();

        }

        protected static List<List<int>> adjacency = new List<List<int>>();

        public static void ReadGraph(string path)
        {
            int i, u, v;
            int r1, r2;
            char[] tokens = {' '};
            using (StreamReader fin = new StreamReader(path))
            {
                string line = fin.ReadLine();
                n = Int32.Parse(line);
                for (i = 0; i < n; i++) adjacency.Add(new List<int>());
                
                while (!fin.EndOfStream)
                {
                    

                        line = fin.ReadLine();
                        string[] nums = line.Split(tokens, StringSplitOptions.RemoveEmptyEntries);
                        v = Int32.Parse(nums[0]);
                        u = Int32.Parse(nums[1]);
                        adjacency[u].Add(v);
                        adjacency[v].Add(u);
                    
                }

            }
        }

        
        public static BitArray BitArrayFromString(String bits){
            BitArray ret = new BitArray(bits.Length);
            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = bits[i] == '1';
            }
            return ret;
        }



        public static List<BitArray> BitArraysWithNBits(int bits, int len, List<HashSet<int>> start = null)
        {
            List<HashSet<int>> sets = new List<HashSet<int>>(), working = new List<HashSet<int>>();
            sets = initSets(len, start, sets);
            working = generateSetsAndPrune(bits, len, sets, working);
                sets = null;
                sets = working.DeepCopy();
            
            
           
            //Now we convert sets to bitarrays
            List<BitArray> ret = new List<BitArray>();
            HashSet<String> stringset = new HashSet<string>();
            foreach (HashSet<int> set in sets)
            {
                BitArray temp = new BitArray(len);
                foreach (int x in set)
                {
                    temp[x] = true;
                }
                if(stringset.Add(temp.BitsToString())) ret.Add(temp);
            }
            return ret;
        }

private static List<HashSet<int>> generateSetsAndPrune(int bits, int len, List<HashSet<int>> sets, List<HashSet<int>> working)
{
            working = new List<HashSet<int>>(sets.DeepCopy());
            for (int i = 0; i < len; ++i)
            {
                List<HashSet<int>> scoot = new List<HashSet<int>>(sets.DeepCopy());
                foreach (HashSet<int> set in scoot)
                {
                    set.Add(i);
                }
                working.AddRange(scoot);
            }

              List<int> remove = new List<int>();

                for (int k = working.Count - 1; k > -1; --k)
                {
                    HashSet<int> set = working[k];
                    if (set.Count != bits)
                        remove.Add(k);
                }
                foreach (int pop in remove) working.RemoveAt(pop);
return working;
}

        public static HashSet<int> BitArrayToHashSet(BitArray bits)
        {
            HashSet<int> ret = new HashSet<int>();
            for (int i = 0; i < bits.Length; ++i)
            {
                if (bits[i]) ret.Add(i);
            }
            return ret;
        }



        static public BitArray BruteForce(int start = 1)
        {
            BitArray ret = new BitArray(n);
            WeirdCounter w = new WeirdCounter(n);
            bool found = false;
            while (!found && start < n)
            {
                for (ret = w.Init(start); w.Good(); ret = w.Next())
                {
                    if (CheckIfCover(ret))
                    {
                        found = true; break;
                    }
                }
                ++start;
            }
            if(!found) Console.WriteLine("Somehow, no cover was found- something seriously wrong happened.");
        
            return ret;
        }
        static ArrayList solutionStuff = new ArrayList(2);

        static public BitArray ParallelBruteForce(int start = 1)
        {
            int chunksize = 40;
            WeirdCounter w = new WeirdCounter(n);
            BitArray ret = null;
            bool found = false;
            solutionStuff = new ArrayList(2);
            solutionStuff.Add(null);
            solutionStuff.Add((Object)false);

                while (!found && start < n)
                {
                    for (ret = w.Init(start); w.Good(); ret = w.Next())
                    {
                        List<Thread> threadPool = new List<Thread>();
                        
                       for(int i = 0; w.Good() && i < chunksize; ++i){
                           threadPool.Add(new Thread(
                               () =>
                               {
                                   BitArray bits, coverage;
                                   lock (w)
                                   {
                                       bits = w.Next();
                                   }
                                   if (bits == null) return;
                                   if(CheckIfCover(bits, out coverage))
                                   {
                                       lock (solutionStuff)
                                       {
                                           solutionStuff[0] = bits;
                                           solutionStuff[1] = (Object)true;
                                       }

                                   }
                               }));
                       }
                       foreach (Thread t in threadPool)    t.Start();
                       while ((bool)solutionStuff[1] && threadPool.Any(t => t.IsAlive)) Thread.Sleep(100);
                    if ((bool)solutionStuff[1])
                    {
                        foreach (Thread t in threadPool) t.Abort();
                        return (BitArray)solutionStuff[0];
                    }


            
                       
                    }
                    ++start;
                }
                if (!found) Console.WriteLine("Somehow, no cover was found- something seriously wrong happened.");

                return ret;

            
        }

        private static List<HashSet<int>> initSets(int len, List<HashSet<int>> start, List<HashSet<int>> sets)
        {
            if (start == null)
            {
                for (int i = 0; i < len; ++i)
                {
                    HashSet<int> temp = new HashSet<int>();
                    temp.Add(i);
                    sets.Add(temp);
                }
            }
            else { sets = start.DeepCopy(); }
            return sets;
        }

        /*
         * std::vector<int> graph::approximateVertexCover(){
            std::vector<int> cover;
            int* adjCopy = new int[numVert*numVert];
            memcpy(adjCopy,adjMat, sizeof(int)*numVert*numVert);
            for(int i = 0; i < numVert; i++){
                for(int j = i+1; j < numVert; j++){
                    if(adjCopy[i*numVert + j]){
                        cover.push_back(i);
                        cover.push_back(j);

                for(int k = 0; k < numVert; k++){
                    if(adjCopy[i*numVert + k]){
                        adjCopy[i*numVert + k] = 0;
                        adjCopy[k*numVert + i] = 0;
                    }
                }

                for(int k = 0; k < numVert; k++){
                    if(adjCopy[j*numVert + k]){
                    adjCopy[j*numVert + k] = 0;
                    adjCopy[k*numVert + j] = 0;
                }
                    }
            }
        }
    }
    return cover;
}*/

        public static BitArray Approximation()
        {
            List<List<int>> adjPrime = new List<List<int>>(adjacency);
            BitArray ret = new BitArray(n);
            while (true)
            {
                for (int i = 0; i < n; ++i)
                {
                    if (adjPrime[i].Count != 0)
                    {
                        int target = adjPrime[i][0];
                        ret[i] = ret[target] = true;
                        removeEdgesAdjacent(adjPrime, i);
                        removeEdgesAdjacent(adjPrime, target);
                    }
                }

            }

            return ret;
        }

        private static void removeEdgesAdjacent(List<List<int>> adj, int u)
        {
            List<int> remFromV= new List<int>(), remFromU = new List<int>();
            foreach (int vertex in adj[u])
            {
                remFromV.Add(u);
                remFromU.Add(vertex);
            }
            for (int i = 0; i < remFromU.Count; ++i)
            {
                adj[remFromU[i]].Remove(remFromV[i]);

                adj[remFromV[i]].Remove(remFromU[i]);
            }
        }

        public static bool CheckIfCover(BitArray vertexes, out BitArray covered)
            {
            covered = new BitArray(vertexes);//Same length, and any vertex which is a part of the cover is by definition covered
            for(int i = 0; i < vertexes.Length; ++i){
                if (vertexes[i]) foreach(int edge in adjacency[i]) covered[edge] = true; 
            }
            bool ret = true;
            string coveredString=covered.BitsToString(), vertexesString=vertexes.BitsToString();
            foreach (bool vertex in covered) ret &= vertex;
            return ret;
                
        }

        public static bool CheckIfCover(BitArray vertexes)
        {
            BitArray plop;
            return CheckIfCover(vertexes, out plop);
        }

        public static string EnvironmentTag = "Laptop";
        public static string DataSetName = "VertexCover";
    }
}
