using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MyUtils;
using System.IO;

namespace EvoOptimization
{
    class WeirdCounter
    {
        public int Length{get;private set;}
        public int Sum { get; private set;}
            public WeirdCounter():this(OptoGlobals.n){}
    public WeirdCounter(int p)
    {
        Length = p;
        Sum = -1;
    }
    List<BitArray> results;
    public BitArray Init(int sum)
    {

        results = recurse(new BitArray(Length, false), sum);
        Sum = sum;
        return Next();
    }

    public BitArray Next()
    {
        if (results.Count == 0)
        {
            if (Sum == -1)
                throw new InvalidOperationException("Uninitialized Counter");
            else
                return null;
        }
        BitArray ret = results[0];
        results.RemoveAt(0);
        return ret;
    }

    public bool Good()
    {
        return results.Count > 0;
    }

    List<BitArray> recurse(BitArray reduced, int sum)
    {
        int size = reduced.Length;
        List<BitArray> ret;
        if (sum == 1)
        {
            ret = new List<BitArray>();
            for (int i = size - 1; i > -1; --i)
            {
                BitArray temp = new BitArray(size, false);
                temp[i] = true;
                ret.Add(temp);
            }
            return ret;
        }
        if (sum == size)
        {
            ret = new List<BitArray>(1);
            ret.Add(new BitArray(size, true));
            return ret;
        }
        bool val;
        if (size == 1)
        {
            ret = new List<BitArray>();
            ret.Add(new BitArray(1, true));
            ret.Add(new BitArray(1, false));
            return ret;
        }

        ret = recurse(reduced.Range(0, (uint)size - 1), sum - 1);
        val = true;//Add true to the last

        List<BitArray> ret2 = recurse(reduced.Range(0, (uint)size - 1), sum);
        bool val2 = false;

        for (int i = 0; i < ret.Count; ++i)
        {
            ret[i] = ret[i].Insert(size - 1, val);
        }

        for (int i = 0; i < ret2.Count; ++i)
        {
            ret2[i] = ret2[i].Insert(size - 1, val2);
        }

        ret.AddRange(ret2);

        //To make this work, every  time we recurse we need to freeze another bit and then reconstruct the results with that bit in place
   
        return ret;
    }

        


    public List<BitArray> testRecursion(BitArray r, int sum)
    {
        return recurse(r, sum);
    }

    public List<BitArray> testRecursion(int sum, int length)
    {
        List<BitArray> ret = recurse(new BitArray(length, false), sum);
        return ret;
    }


        public BitArray Increment(){
            //if (Contents.Empty())
            //    throw new InvalidOperationException();
            return null;
        }

        BitArray counter;
        

    }
}
