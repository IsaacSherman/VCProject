using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using MyUtils;
namespace EvoOptimization
{
    public abstract class Optimizer : IComparable<Optimizer>, IHazFitness
    {
        
        protected static int firstFeature = 0;// OptoGlobals.NumberOfFeatures;
        public virtual string GetToken { get { return _optimizerToken; } }
        protected void PrepForSave(string path, System.Security.AccessControl.DirectorySecurity temp)
        {

            Directory.CreateDirectory(path, temp);

        }

        public static Boolean Multiclass = false;
        protected void PrepForSave(string path)
        {
            System.Security.AccessControl.DirectorySecurity activeDirSec = Directory.GetAccessControl(@".\");
            PrepForSave(path, activeDirSec);
        }

        protected abstract string getFunctionString();
        public List<int> GeneratedLabels = null, CVGeneratedLabels = null, PredictorLabels = null;
        protected BitArray _bits;
        protected double _nLearners = 1;


        protected void NullGeneratedLabels()
        {
            GeneratedLabels = null;
            CVGeneratedLabels = null;
        }

        public void CompactMemory()
        {
            NullGeneratedLabels();
        }


        public int Length { get { return _bits.Length; } }
        public BitArray Bits { get { return _bits; } }
        public Optimizer() : this(OptoGlobals.n) { }
        public Optimizer(int stringLength)
        {
            _bits = new BitArray(stringLength);
            for (int i = 0; i < stringLength; ++i)
                _bits[i] = OptoGlobals.RNG.NextDouble() < OptoGlobals.InitialRateOfOnes;
            errorCheck();
        }
        public virtual void AllColumns()
        {

        }

        public virtual double FirstMetric { get { return Fitness; } set { Fitness = value; } }
        public virtual double SecondMetric{get;set;}

        protected static string _optimizerToken;
        
        

        public static String Token { get { return _optimizerToken; } }
        public Optimizer(String bitString)
        {
            _bits = new BitArray(bitString.Length);
            SetBitsToString(bitString);

            Prepare();
        }
        protected double _fitness = -1;
        public double Fitness { get { return _fitness; } set { _fitness = value; } }
        public int BitLength { get; protected set; } 


        public void SetBitsToString(String bitString)
        {
            System.Diagnostics.Debug.Assert(bitString.Length == Bits.Length);
            for (int i = 0; i < bitString.Length; ++i)
            {
                _bits[i] = bitString[i] == '1';
            }
        }

        /// <summary>
        /// Invokes errorCheck() and interpretVals(), and anything else to make eval a legitimate call.  Publically accessible.
        /// </summary>
        public virtual void Prepare()
        {
            errorCheck();
        }

        /// <summary>
        /// Corrects internal errors in the optimizer
        /// </summary>
        protected abstract void errorCheck();
        /// <summary>
        /// Evaluates the bitstring in matlab
        /// </summary>
        public abstract void Eval();
        public override String ToString() { return _bits.BitsToString(); }
        public void SetFitness()
        {
            this.setFitness();
        }

        public static T And<T>(T a, T b) where T : Optimizer, new()
        {
            T ret = new T();
            BitArray tempBits = (BitArray)a._bits.Clone();
            ret._bits = tempBits.And(b.Bits);
            return ret;
        }

        public static T Or<T>(T a, T b) where T : Optimizer, new()
        {
            T ret = new T();
            BitArray tempBits = (BitArray)a._bits.Clone();
            ret._bits = tempBits.Or(b.Bits);
            return ret;
        }

        public static T Xor<T>(T a, T b) where T : Optimizer, new()
        {
            T ret = new T();
            BitArray tempBits = (BitArray)a._bits.Clone();
            ret._bits = tempBits.Xor(b.Bits);
            return ret;
        }


        int IComparable<Optimizer>.CompareTo(Optimizer other)
        {
            int ret = FirstMetric.CompareTo(other.FirstMetric);
            if (ret == 0) ret = SecondMetric.CompareTo(other.SecondMetric);
            return ret;
        }

        protected abstract void setFitness();


        protected void rerollBits(int start, int end)
        {
            for (int i = start; i < end; ++i)
            {
                _bits[i] = OptoGlobals.RNG.NextDouble() < .5;
            }
        }

    }
}
