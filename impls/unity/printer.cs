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
            {
                sb.Append("( ");
                foreach (types.MalVal child in (tree as types.MalList).value)
                {
                    pr_form(child, sb);
                    sb.Append(" ");
                }
                sb.Append(")");
            }
            else if (tree is types.MalAtom)
            {
                sb.Append((tree as types.MalAtom).value);
            }
            else
                throw new ArgumentException("Unknown Mal type in the tree");
        }

    }
}
