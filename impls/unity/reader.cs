//Reader functions for mal
//Created by James Vanderhyde, 22 September 2021

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mal;
using System.IO;

namespace Mal
{
    public class reader
    {
        static char[] charsToTrim = { ' ', ',', '\t', '\n', '\r' };

        public static types.MalVal read_str(string input)
        {
            if (input.Length == 0)
                throw new ArgumentException("Input string must not be empty", nameof(input));

            string expression = @"[\s,]*(~@|[\[\]{}()'`~^@]|""(?:\\.|[^\\""])*"" ?|;.*|[^\s\[\]{ } ('""`,;)]*)";
            MatchCollection mc = Regex.Matches(input, expression);
            IEnumerable<Match> enumerable = mc;
            IEnumerator<Match> en = enumerable.GetEnumerator();
            en.MoveNext(); //get ready for first read
            return read_form(en);
        }

        private static types.MalVal read_form(IEnumerator<Match> en)
        {
            string token = en.Current.Value.Trim(charsToTrim);
            //Console.WriteLine("token: ***" + token + "***");
            if (token.Length == 0)
                throw new ArgumentException("unbalanced: Missing matching quote in input");
            else if (token[0] == ')')
                throw new ArgumentException("unbalanced: Unexpected ) in input");
            else if (token[0] == ']')
                throw new ArgumentException("unbalanced: Unexpected ] in input");
            else if (token[0] == '}')
                throw new ArgumentException("unbalanced: Unexpected } in input");
            else if (token[0] == '(')
                return listToMalList(read_list(en,')'));
            else if (token[0] == '[')
                return listToMalVector(read_list(en, ']'));
            else if (token[0] == '{')
                return listToMalMap(read_list(en, '}'));
            else
                return read_atom(en);
        }

        private static List<types.MalVal> read_list(IEnumerator<Match> en, char bracket)
        {
            List<types.MalVal> l = new List<types.MalVal>();
            bool hasNext = en.MoveNext(); //consume left paren
            while (hasNext)
            {
                string token = en.Current.Value.Trim(charsToTrim);
                if (token.Length==0)
                    throw new ArgumentException("unbalanced: Missing matching "+bracket+" in input");
                if (token[0] == bracket)
                    return l;
                types.MalVal value = read_form(en);
                l.Add(value);
                hasNext = en.MoveNext(); //consume right paren or atom
            }
            throw new ArgumentException("unbalanced: Missing matching "+bracket+" in input");
        }

        private static types.MalList listToMalList(List<types.MalVal> list)
        {
            types.MalList l = new types.MalList();
            for (int i=list.Count-1; i>=0; i--)
            {
                l.cons(list[i]);
            }
            return l;
        }

        private static types.MalVector listToMalVector(List<types.MalVal> list)
        {
            return new types.MalVector(list);
        }

        private static types.MalMap listToMalMap(List<types.MalVal> list)
        {
            types.MalMap m = new types.MalMap();
            for (int i=0; i+1<list.Count; i+=2)
            {
                m.assoc(list[i], list[i+1]);
            }
            return m;
        }

        private static types.MalAtom read_atom(IEnumerator<Match> en)
        {
            return new types.MalAtom(en.Current.Value.Trim(charsToTrim));
        }
    }
}
