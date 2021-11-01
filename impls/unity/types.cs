//Types in MAL
//Created by James Vanderhyde, 22 September 2021

using System;
using System.Collections.Generic;
using System.Collections;
using Mal;

namespace Mal
{
    public class types
    {
        public abstract class MalVal
        {
        }

        public abstract class MalAtom : MalVal
        {
        }

        public class MalSymbol : MalAtom
        {
            public readonly string name;

            public MalSymbol(string name)
            {
                this.name = name;
            }
        }

        public class MalNumber : MalAtom
        {
            public static readonly MalNumber zero = new MalNumber(0);
            public readonly float value;

            public MalNumber(float value)
            {
                this.value = value;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;
                MalNumber a = (MalNumber)obj;
                return this.value == a.value;
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }

        public class MalBoolean : MalAtom
        {
            public static readonly MalBoolean malTrue = new MalBoolean(true);
            public static readonly MalBoolean malFalse = new MalBoolean(false);
            public readonly bool value;

            public MalBoolean(bool value)
            {
                this.value = value;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;
                MalBoolean a = (MalBoolean)obj;
                return this.value == a.value;
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }

        public class MalNil : MalAtom
        {
            public static readonly MalNil malNil = new MalNil();
            public readonly Object value = null;

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;
                return true;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        public class MalString : MalAtom
        {
            public readonly string value;

            public MalString(string value)
            {
                this.value = value;
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;
                MalString a = (MalString)obj;
                return this.value == a.value;
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }
        }

        public class MalKeyword : MalAtom
        {
            public readonly string name;

            public MalKeyword(string name)
            {
                //using Unicode "Interlinear Annotation Anchor" to mark keywords
                // internally instead of ':', since strings may contain colons.
                this.name = '\ufff9' + name.Substring(1);
            }

            public override bool Equals(Object obj)
            {
                if (obj == null || this.GetType() != obj.GetType()) return false;
                MalKeyword a = (MalKeyword)obj;
                return this.name == a.name;
            }

            public override int GetHashCode()
            {
                return name.GetHashCode();
            }
        }

        public class TailCall : MalVal
        {
            public readonly MalVal bodyTree;
            public readonly env.Environment outerEnvironment;

            public TailCall(MalVal bodyTree, env.Environment outerEnvironment)
            {
                this.outerEnvironment = outerEnvironment;
                this.bodyTree = bodyTree;
            }
        }

        public abstract class MalFunc : MalAtom
        {
            public abstract MalVal apply(MalList arguments);
        }

        public class FuncClosure : types.MalFunc
        {
            private readonly env.Environment outerEnvironment;
            private readonly MalCollection unboundSymbols;
            private readonly MalVal bodyTree;

            public FuncClosure(env.Environment outerEnvironment, MalCollection unboundSymbols, MalVal bodyTree)
            {
                this.outerEnvironment = outerEnvironment;
                this.unboundSymbols = unboundSymbols;
                this.bodyTree = bodyTree;
            }

            public override MalVal apply(MalList arguments)
            {
                env.Environment inner = new env.Environment(outerEnvironment);
                bool foundAmpersand = false;
                foreach (MalSymbol symbol in this.unboundSymbols)
                {
                    if (foundAmpersand)
                        inner.set(symbol.name, arguments);
                    else if (symbol.name.Equals("&"))
                        foundAmpersand = true;
                    else
                    {
                        if (arguments.isEmpty())
                            throw new ArgumentException("Not enough arguments passed to function.");
                        inner.set(symbol.name, arguments.first());
                        arguments = arguments.rest();
                    }
                }
                if (!foundAmpersand && !arguments.isEmpty())
                    throw new ArgumentException("Too many arguments passed to function.");

                return evaluator.eval_ast(this.bodyTree, inner);
            }
        }

        public class MalBinaryOperator : MalFunc
        {
            public readonly Func<float, float, float> value;

            public MalBinaryOperator(Func<float, float, float> value)
            {
                this.value = value;
            }

            public override MalVal apply(MalList arguments)
            {
                float a, b;
                try
                {
                    a = ((MalNumber)arguments.first()).value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("First item is not a number.");
                }
                try
                {
                    b = ((MalNumber)arguments.rest().first()).value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Second item is not a number.");
                }
                return new MalNumber(value(a, b));
            }
        }

        public class MalLogicalOperator : MalFunc
        {
            public readonly Func<float, float, bool> value;

            public MalLogicalOperator(Func<float, float, bool> value)
            {
                this.value = value;
            }

            public override MalVal apply(MalList arguments)
            {
                float a, b;
                try
                {
                    a = ((MalNumber)arguments.first()).value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("First item is not a number.");
                }
                try
                {
                    b = ((MalNumber)arguments.rest().first()).value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Second item is not a number.");
                }
                return new MalBoolean(value(a, b));
            }
        }

        public abstract class MalCollection : MalVal, IEnumerable<MalVal>
        {
            public abstract IEnumerator<MalVal> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public abstract long count();

            public override bool Equals(Object obj)
            {
                if (obj == null || (!(obj is MalCollection))) return false;
                MalCollection a = (MalCollection)obj;
                if (a.count() != this.count()) return false;
                IEnumerator<MalVal> it = a.GetEnumerator();
                foreach (MalVal item in this)
                {
                    it.MoveNext();
                    if (!item.Equals(it.Current)) return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 0;
                foreach (MalVal item in this)
                {
                    hash ^= item.GetHashCode();
                }
                return hash;
            }
        }

        public class MalList : MalCollection
        {
            private class Node
            {
                public readonly MalVal data;
                public readonly Node link;

                public Node(MalVal data, Node link)
                {
                    this.data = data;
                    this.link = link;
                }
            }

            private class NodeEnumerator : IEnumerator<MalVal>
            {
                private Node head;
                private Node current;
                private bool done;

                public NodeEnumerator(Node head)
                {
                    this.head = head;
                    this.current = null;
                    this.done = false;
                }

                public MalVal Current
                {
                    get
                    {
                        if (this.current == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return this.current.data;
                    }
                }

                public bool MoveNext()
                {
                    if (current == null && !done)
                        current = head;
                    else if (current != null)
                        current = current.link;

                    if (current == null)
                    {
                        done = true;
                        return false;
                    }
                    return true;
                }

                public void Reset()
                {
                    this.current = null;
                    this.done = false;
                }

                void IDisposable.Dispose() { }

                object IEnumerator.Current
                {
                    get { return Current; }
                }
            }

            private Node head;
            private long numElements;

            public MalList()
            {
                this.head = null;
                this.numElements = 0;
            }

            private MalList(Node head, long numElements)
            {
                this.head = head;
                this.numElements = numElements;
            }

            public override IEnumerator<MalVal> GetEnumerator()
            {
                return new NodeEnumerator(this.head);
            }

            public void cons(MalVal item)
            {
                this.head = new Node(item, this.head);
                this.numElements++;
            }

            public MalVal first()
            {
                if (head == null)
                    throw new InvalidOperationException("The list is empty; cannot get the first item.");
                return head.data;
            }

            public MalList rest()
            {
                if (head == null)
                    throw new InvalidOperationException("The list is empty; cannot get the rest.");
                return new MalList(this.head.link, this.numElements-1);
            }

            public bool isEmpty()
            {
                return (head == null);
            }

            public override long count()
            {
                return this.numElements;
            }
        }

        public class MalVector : MalCollection
        {
            private List<MalVal> value;

            public MalVector(List<MalVal> list)
            {
                this.value = list;
            }

            public MalVector()
            {
                value = new List<MalVal>();
            }

            public override IEnumerator<MalVal> GetEnumerator()
            {
                return value.GetEnumerator();
            }

            public void conj(MalVal item)
            {
                value.Add(item);
            }

            public MalVal nth(int index)
            {
                return value[index];
            }

            public bool isEmpty()
            {
                return (value.Count == 0);
            }

            public override long count()
            {
                return value.Count;
            }
        }

        public class MalMap : MalCollection
        {
            private Dictionary<MalVal, MalVal> dict;

            public MalMap()
            {
                dict = new Dictionary<MalVal, MalVal>();
            }

            private class PairEnumerator : IEnumerator<MalVal>
            {
                private IEnumerator<KeyValuePair<MalVal, MalVal>> en;

                public PairEnumerator(IEnumerator<KeyValuePair<MalVal, MalVal>> en)
                {
                    this.en = en;
                }

                public MalVal Current
                {
                    get
                    {
                        KeyValuePair<MalVal, MalVal> p = en.Current;
                        MalVector v = new MalVector();
                        v.conj(p.Key);
                        v.conj(p.Value);
                        return v;
                    }
                }

                public bool MoveNext()
                {
                    return en.MoveNext();
                }

                public void Reset()
                {
                    en.Reset();
                }

                void IDisposable.Dispose() { }

                object IEnumerator.Current
                {
                    get { return Current; }
                }
            }

            public override IEnumerator<MalVal> GetEnumerator()
            {
                return new PairEnumerator(dict.GetEnumerator());
            }

            public void assoc(MalVal key, MalVal value)
            {
                dict.Add(key, value);
            }

            public MalVal get(MalVal key)
            {
                return dict[key];
            }

            public override long count()
            {
                return dict.Count;
            }
        }
    }
}
