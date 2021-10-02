//Printer functions for mal
//Created by James Vanderhyde, 30 September 2021

using System;
using System.Text.RegularExpressions;
using System.Text;
using Mal;

namespace Mal
{
    public class printer
    {
        public static string pr_str(types.MalVal tree)
        {
            StringBuilder sb = new StringBuilder();
            pr_form(tree,sb);
            return sb.ToString();
        }

        private static void pr_form(types.MalVal tree, StringBuilder sb)
        {
            if (tree is types.MalList)
                pr_list(tree as types.MalCollection, sb, '(', ')');
            else if (tree is types.MalVector)
                pr_list(tree as types.MalCollection, sb, '[', ']');
            else if (tree is types.MalMap)
                pr_map(tree as types.MalMap, sb);
            else if (tree is types.MalAtom)
                pr_atom(tree as types.MalAtom, sb);
            else
                throw new ArgumentException("Unknown Mal type in the tree");
        }

        private static void pr_list(types.MalCollection tree, StringBuilder sb, char leftBracket, char rightBracket)
        {
            sb.Append(leftBracket);
            bool space = false;
            foreach (types.MalVal child in tree)
            {
                pr_form(child, sb);
                sb.Append(" ");
                space = true;
            }
            if (space) sb.Length = sb.Length - 1;
            sb.Append(rightBracket);
        }

        private static void pr_map(types.MalMap tree, StringBuilder sb)
        {
            sb.Append('{');
            bool space = false;
            foreach (types.MalVal child in tree)
            {
                types.MalVector pair = child as types.MalVector;
                types.MalVal key = pair.nth(0);
                types.MalVal value = pair.nth(1);
                pr_form(key, sb);
                sb.Append(" ");
                pr_form(value, sb);
                sb.Append(", ");
                space = true;
            }
            if (space) sb.Length = sb.Length - 2;
            sb.Append('}');
        }

        private static void pr_atom(types.MalAtom tree, StringBuilder sb)
        {
            sb.Append(tree.value);
        }

    }
}
