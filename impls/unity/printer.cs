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
            else if (tree is types.MalSymbol)
                pr_symbol(tree as types.MalSymbol, sb);
            else if (tree is types.MalBoolean)
                pr_boolean(tree as types.MalBoolean, sb);
            else if (tree is types.MalNil)
                pr_nil(tree as types.MalNil, sb);
            else if (tree is types.MalNumber)
                pr_number(tree as types.MalNumber, sb);
            else if (tree is types.MalString)
                pr_string(tree as types.MalString, sb);
            else if (tree is types.MalKeyword)
                pr_keyword(tree as types.MalKeyword, sb);
            else if (tree is types.MalFunc)
                pr_func(tree as types.MalFunc, sb);
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

        private static void pr_number(types.MalNumber tree, StringBuilder sb)
        {
            sb.Append(tree.value);
        }

        private static void pr_boolean(types.MalBoolean tree, StringBuilder sb)
        {
            if (tree.value)
                sb.Append("true");
            else
                sb.Append("false");
        }

        private static void pr_nil(types.MalNil tree, StringBuilder sb)
        {
            sb.Append("nil");
        }

        private static void pr_symbol(types.MalSymbol tree, StringBuilder sb)
        {
            sb.Append(tree.name);
        }

        private static void pr_string(types.MalString tree, StringBuilder sb)
        {
            sb.Append("\"");
            sb.Append(tree.value);
            sb.Append("\"");
        }

        private static void pr_keyword(types.MalKeyword tree, StringBuilder sb)
        {
            sb.Append(":");
            sb.Append(tree.name.Substring(1));
        }

        private static void pr_func(types.MalFunc tree, StringBuilder sb)
        {
            sb.Append("#<function>");
            //sb.Append(tree.value);
        }

    }
}
