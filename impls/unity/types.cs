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

        public class MalAtom : MalVal
        {
            public readonly string value;

            public MalAtom(string value)
            {
                this.value = value;
            }
        }

        public abstract class MalCollection : MalVal, IEnumerable<MalVal>
        {
            public abstract IEnumerator<MalVal> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
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

            public MalList()
            {
                this.head = null;
            }

            private MalList(Node head)
            {
                this.head = head;
            }

            public override IEnumerator<MalVal> GetEnumerator()
            {
                return new NodeEnumerator(this.head);
            }

            public void cons(MalVal item)
            {
                head = new Node(item, head);
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
                return new MalList(head.link);
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
        }
    }
}
