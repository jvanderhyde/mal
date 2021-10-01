//Reader functions for mal
//Created by James Vanderhyde, 22 September 2021

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mal;

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
            if (token[0] == ')')
                throw new ArgumentException("Unexpected ) in input");
            else if (token[0] == '(')
                return read_list(en);
            else
                return read_atom(en);
        }

        private static types.MalList read_list(IEnumerator<Match> en)
        {
            types.MalList l = new types.MalList();
            bool hasNext = en.MoveNext(); //consume left paren
            while (hasNext)
            {
                string token = en.Current.Value.Trim(charsToTrim);
                if (token[0] == ')')
                    return l;
                types.MalVal value = read_form(en);
                l.cons(value);
                hasNext = en.MoveNext(); //consume right paren or atom
            }
            throw new ArgumentException("Missing matching ) in input");
        }

        private static types.MalAtom read_atom(IEnumerator<Match> en)
        {
            return new types.MalAtom(en.Current.Value.Trim(charsToTrim));
        }
    }
}
