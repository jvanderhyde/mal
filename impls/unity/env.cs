//Environment functions for mal
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
            baseEnvironment.setAll(core.ns);
        }

        public class Environment
        {
            public readonly Environment outer;
            private Dictionary<string, types.MalVal> dict;
            public readonly types.FuncClosure recurPoint;

            public Environment(Environment outer, types.FuncClosure recurPoint = null)
            {
                this.outer = outer;
                this.dict = new Dictionary<string, types.MalVal>();
                this.recurPoint = recurPoint;
            }

            public void setAll(Dictionary<string, types.MalVal> ns)
            {
                foreach (KeyValuePair<string, types.MalVal> item in ns)
                    this.set(item.Key, item.Value);
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