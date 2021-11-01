//Core functions for mal
//Created by James Vanderhyde, 29 October 2021

using System;
using System.Text;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class core
    {
        public static readonly Dictionary<string, types.MalVal> ns = new Dictionary<string, types.MalVal>();
        static core()
        {
            ns.Add("+", new types.MalBinaryOperator((a, b) => a + b));
            ns.Add("-", new types.MalBinaryOperator((a, b) => a - b));
            ns.Add("*", new types.MalBinaryOperator((a, b) => a * b));
            ns.Add("/", new types.MalBinaryOperator((a, b) => a / b));
            ns.Add("π", new types.MalNumber(3.141592653589793f));
            ns.Add("prn", new prn());
            ns.Add("list", new list());
            ns.Add("list?", new list_QMARK_());
            ns.Add("empty?", new empty_QMARK_());
            ns.Add("count", new count());
            ns.Add("=", new _EQ_());
            ns.Add("<", new types.MalLogicalOperator((a, b) => a < b));
            ns.Add("<=", new types.MalLogicalOperator((a, b) => a <= b));
            ns.Add(">", new types.MalLogicalOperator((a, b) => a > b));
            ns.Add(">=", new types.MalLogicalOperator((a, b) => a >= b));
        }

        private class prn : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                StringBuilder sb = new StringBuilder();
                foreach (types.MalVal arg in arguments)
                    sb.Append(printer.pr_str(arg)+" ");
                if (sb.Length == 0)
                    Console.WriteLine();
                else
                    Console.WriteLine(sb.ToString().Substring(0,sb.Length-1));
                return types.MalNil.malNil;
            }
        }

        private class list : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                return arguments;
            }
        }

        private class list_QMARK_ : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                if (arguments.isEmpty())
                    throw new ArgumentException("Missing argument to list? function.");
                return new types.MalBoolean(arguments.first() is types.MalList);
            }
        }

        private class empty_QMARK_ : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                if (arguments.isEmpty())
                    throw new ArgumentException("Missing argument to empty? function.");
                if (!(arguments.first() is types.MalList))
                    throw new ArgumentException("Must specify a list.");
                return new types.MalBoolean((arguments.first() as types.MalList).isEmpty());
            }
        }

        private class count : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                if (arguments.isEmpty())
                    throw new ArgumentException("Missing argument to count function.");
                if (arguments.first() is types.MalNil)
                    return types.MalNumber.zero;
                if (!(arguments.first() is types.MalCollection))
                    throw new ArgumentException("Must specify a list or other collection.");
                return new types.MalNumber((arguments.first() as types.MalCollection).count());
            }
        }

        private class _EQ_ : types.MalFunc
        {
            public override types.MalVal apply(types.MalList arguments)
            {
                if (arguments.isEmpty())
                    throw new ArgumentException("Missing arguments to = (equal) function.");
                if (arguments.rest().isEmpty())
                    return types.MalBoolean.malTrue;
                return new types.MalBoolean(arguments.first().Equals(arguments.rest().first()));
            }
        }
    }
}