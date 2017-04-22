﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
namespace EvoOptimization
{
    class EvoOptimizerProgram<T> where T: Optimizer, new()
    {
        private int _maxGen,_popSize = 50, _saveAfterGens=10;
        private bool _validate = false, _noload = false, _multiThread = false, _running = false, _outputBaseline = true, _suppressMessages = false;
        
        private List<T> best;

        OptimoEvolver<T> D;
     
        private OptimoEvolver<T>.CrossoverType _crossOverType = OptimoEvolver<T>.CrossoverType.Uniform;
        private string _bestFilePath, _outputFileStem;

        public int SaveAfterGens
        {
            get
            {
                return _saveAfterGens;
            }

            set
            {
                _saveAfterGens = value;
            }
        }
        public bool SuppressMessages { get { return _suppressMessages; } set { _suppressMessages = value; } }


        public int MaxGen
        {
            get
            {
                return _maxGen;
            }

            set
            {
                _maxGen = value;
            }
        }

        public bool Validate
        {
            get
            {
                return _validate;
            }

            set
            {
                _validate = value;
            }
        }

        public bool Noload
        {
            get
            {
                return _noload;
            }

            set
            {
                _noload = value;
            }
        }

        public bool MultiThread
        {
            get
            {
                return _multiThread;
            }

            set
            {
                _multiThread = value;
            }
        }

        public bool Running
        {
            get
            {
                return _running;
            }
        }

        public int PopSize
        {
            get
            {
                return _popSize;
            }

            set
            {
                _popSize = value;
            }
        }

        public OptimoEvolver<T>.CrossoverType CrossOverType
        {
            get
            {
                return _crossOverType;
            }

            set
            {
                _crossOverType = value;
            }
        }

        public string BestFilePath
        {
            get
            {
                return _bestFilePath;
            }

            set
            {
                _bestFilePath = value;
            }
        }

        public bool OutputBaseline
        {
            get
            {
                return _outputBaseline;
            }

            set
            {
                _outputBaseline = value;
            }
        }

        public bool IncludeAllFeatures { get; private set; }

        public string OutputFileStem
        {
            get
            {
                return _outputFileStem;
            }
            private set
            {
                if (_outputFileStem != null)
                {
                    char []tokens = { '/', '\\' };
                    string old = OutputFileStem.Substring(0, OutputFileStem.LastIndexOfAny(tokens));
                    int count = 0;
                    if (Directory.Exists(old))
                    {
                        foreach (string x in Directory.EnumerateFileSystemEntries(old))
                        {
                            ++count;
                            break;
                        }
                        if (count == 0) Directory.Delete(old);
                    }
                }
                _outputFileStem = value;
                 
            }
        }


        /// <summary>
        /// Convenience function, Calls ConfigureEvolver() then Run();
        /// </summary>
        public void ConfigureAndRun()
        {
            ConfigureEvolver();
            Run();
        }

        public EvoOptimizerProgram()
        {
            checkDirectoryExists("./" + OptoGlobals.EnvironmentTag);
            T bob = new T();

        }

        /// <summary>
        /// Run this to set up the evolver- to apply changes made to PopSize or other behavior (MultiThread, Validate, NoLoad, etc).
        /// Once set up, run Run() to begin the program.
        /// </summary>
        public void ConfigureEvolver()
        {
            T temp = new T();
            if (OutputFileStem == null)  OutputFileStem = OptoGlobals.EnvironmentTag +"/" + OptoGlobals.DataSetName + "/" + temp.GetToken;
            _bestFilePath = OutputFileStem + "best.csv";
            checkDirectoryExists(OptoGlobals.EnvironmentTag + "/");
            checkDirectoryExists(OptoGlobals.EnvironmentTag + "/" + OptoGlobals.DataSetName + "/");
            D = new OptimoEvolver<T>(_popSize, _crossOverType, OutputFileStem ,!_noload);
            LoadBestFromFile(_bestFilePath);
            int q = 1;
            Stopwatch sw = new Stopwatch();
            foreach (T o in best)
            {
                D.AddToPopulation(o, q++);
            }
            D.MultiThread = _multiThread;
            if (IncludeAllFeatures) D.SetPopToAllCols();
            
        }

        private static void checkDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path + "/");
        }

        private void LoadBestFromFile(String path)
        {
            best = new List<T>();
            using (StreamReader fin = new StreamReader(new FileStream(path, FileMode.OpenOrCreate)))
            {
                String line = fin.ReadLine();
                while (!fin.EndOfStream)
                {
                    if (line[0] == '0' || line[0] == '1')
                    {
                        T temp = new T();
                        temp.SetBitsToString(line);
                        temp.Prepare();
                        best.Add(temp);
                    }
                    else continue;
                }

            }
        }

        public void Run()
        {
            if (D == null)
                throw new InvalidOperationException("Run ConfigureEvolver() first");
            if (_validate) { D.VerifyOutput(); }


            if (_outputBaseline)
            {
                //Run a simulation including all features to find the best combination of parameters for the particular classifier
                T baseline = new T();
                string basePath = OptoGlobals.EnvironmentTag + "/" + OptoGlobals.DataSetName + "/" + baseline.GetToken + "Baseline";
                EvoOptimizerProgram<T> baseProgram = new EvoOptimizerProgram<T>();
                baseProgram.MaxGen = 100;
                baseProgram.SaveAfterGens = 25;
                baseProgram.PopSize = 50;
                baseProgram.IncludeAllFeatures = true;
                baseProgram.OutputFileStem = basePath;
                baseProgram.Noload = true;
                baseProgram._outputBaseline = false;
                baseProgram.ConfigureAndRun();
            }

            Stopwatch sw = new Stopwatch();
            _running = true;
            for (int x = 0; x < _maxGen; ++x)
            {
                sw.Start();
                D.AdvanceGeneration();
                
                double sum = 0, count = 0;
                foreach(T t in D.Population)
                {
                    sum += t.Fitness;
                    count += 1;
                }
                string msg = "Elapsed time for generation " + x + " = " + sw.ElapsedMilliseconds + "  ms" + "\nBest Fitness = " + D.Population[0].Fitness + " \n Average fitness = " + D.AverageFitness;
                if (!SuppressMessages) blurt(msg);

                if (x % _saveAfterGens == 0)
                {
                    D.DumpLookupToFile(OutputFileStem);
                    if (SuppressMessages) blurt(msg);
                }
                sw.Reset();
            }
            D.DumpLookupToFile(OutputFileStem);
            OutputFileStem = null;
            //Console.ReadLine();
        }

        private void blurt(string x)
        {
                Debug.WriteLine(x);
                Console.WriteLine(x);
            
        }

        internal T YieldBestOptimizer()
        {
            return D.Population[0];
        }

        internal void AddToPopulation(T p)
        {
            D.AddToPopulation(p, PopSize - 2);
        }

    }
}
