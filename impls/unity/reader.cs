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
            IEnumerable<Match> enumerable = (IEnumerable<Match>)mc;
            IEnumerator<Match> en = enumerable.GetEnumerator();
            en.MoveNext(); //get ready for first read
            types.MalVal val = read_form(en);
            //Console.WriteLine(printer.pr_str(val));
            return val;
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
                return listToMalList(read_list(en, ')'));
            else if (token[0] == '[')
                return listToMalVector(read_list(en, ']'));
            else if (token[0] == '{')
                return listToMalMap(read_list(en, '}'));
            else if (token[0] == '\'')
                return listToMalList(read_quote(en, "quote"));
            else if (token[0] == '`')
                return listToMalList(read_quote(en, "quasiquote"));
            else if (token[0] == '~')
                return listToMalList(read_quote(en, "unquote"));
            else if (token[0] == '@')
                return listToMalList(read_quote(en, "deref"));
            else if (token[0] == '^')
                return listToMalList(read_meta(en));
            else if (token[0] == '#' && token.Length==1)
                return read_hash_form(en);
            else
                return read_atom(en);
        }

        private static types.MalVal read_hash_form(IEnumerator<Match> en)
        {
            en.MoveNext(); //consume hash
            types.MalVal hashvalue = read_form(en);
            if (hashvalue is types.MalString)
                return new types.MalSymbol((hashvalue as types.MalString).value);
            else
                throw new ArgumentException("Unknown hash form: #"+hashvalue);
        }

        private static List<types.MalVal> read_meta(IEnumerator<Match> en)
        {
            List<types.MalVal> l = new List<types.MalVal>();
            en.MoveNext(); //consume quote
            l.Add(new types.MalSymbol("with-meta"));
            types.MalVal metavalue = read_form(en);
            en.MoveNext(); //consume right paren or atom
            types.MalVal value = read_form(en);
            l.Add(value);
            l.Add(metavalue);
            return l;
        }

        private static List<types.MalVal> read_quote(IEnumerator<Match> en, string formName)
        {
            List<types.MalVal> l = new List<types.MalVal>();
            en.MoveNext(); //consume quote
            l.Add(new types.MalSymbol(formName));
            types.MalVal value = read_form(en);
            l.Add(value);
            return l;
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
            string token = en.Current.Value.Trim(charsToTrim);
            if (token[0] == '\"')
                return new types.MalString(token.Substring(1, token.Length - 2));
            else if (token[0] == ':')
                return new types.MalKeyword(token);
            else if (float.TryParse(token, out float floatValue))
                return new types.MalNumber(floatValue);
            else if (token.Equals("true"))
                return new types.MalBoolean(true);
            else if (token.Equals("false"))
                return new types.MalBoolean(false);
            else if (token.Equals("nil"))
                return new types.MalNil();
            else
                return new types.MalSymbol(token);
        }
    }
}
