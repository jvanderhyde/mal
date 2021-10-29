//Environent functions for mal
//Created by James Vanderhyde, 11 October 2021

using System;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class env
    {
        public static readonly Environment baseEnvironment = new Environment(null);
        static env()
        {
            baseEnvironment.set("+", new types.MalBinaryOperator((a, b) => a + b));
            baseEnvironment.set("-", new types.MalBinaryOperator((a, b) => a - b));
            baseEnvironment.set("*", new types.MalBinaryOperator((a, b) => a * b));
            baseEnvironment.set("/", new types.MalBinaryOperator((a, b) => a / b));
            baseEnvironment.set("π", new types.MalNumber(3.141592653589793f));
        }

        public class Environment
        {
            private Environment outer;
            private Dictionary<string, types.MalVal> dict;

            public Environment(Environment outer)
            {
                this.outer = outer;
                this.dict = new Dictionary<string, types.MalVal>();
            }

            public void set(string key, types.MalVal value)
            {
                this.dict.Remove(key);
                this.dict.Add(key, value);
            }

            public Environment find(string key)
            {
                if (this.dict.ContainsKey(key))
                    return this;
                else
                    return this.outer?.find(key);
            }

            public types.MalVal get(string key)
            {
                Environment e = this.find(key);
                if (e != null) return e.dict[key];
                else throw new ArgumentException("Symbol " + key + " not found.");
            }
        }
    }
}