//Types in MAL
//Created by James Vanderhyde, 22 September 2021

using System;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class types
    {
        public abstract class MalVal
        {
        }

        public class MalAtom : MalVal
        {
            public readonly string value;

            public MalAtom(string value)
            {
                this.value = value;
            }
        }

        public class MalList : MalVal
        {
            public readonly List<MalVal> value;

            public MalList()
            {
                value = new List<MalVal>();
            }

            public void cons(MalVal item)
            {
                value.Add(item);
            }
        }
    }
}
