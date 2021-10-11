//Evaluator functions for mal
//Created by James Vanderhyde, 9 October 2021

using System;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class evaluator
    {
        public static readonly Environment baseEnvironment = new Environment();
        static evaluator()
        {
            baseEnvironment.Add("+", new types.MalBinaryOperator((a, b) => a + b));
            baseEnvironment.Add("-", new types.MalBinaryOperator((a, b) => a - b));
            baseEnvironment.Add("*", new types.MalBinaryOperator((a, b) => a * b));
            baseEnvironment.Add("/", new types.MalBinaryOperator((a, b) => a / b));
        }

        public class Environment : Dictionary<string, types.MalVal> { }

        public static types.MalVal eval_ast(types.MalVal tree, evaluator.Environment env)
        {
            if (tree is types.MalList)
                return apply_function(eval_list(tree as types.MalList, env));
            else if (tree is types.MalVector)
                return eval_vector(tree as types.MalVector, env);
            else if (tree is types.MalMap)
                return eval_map(tree as types.MalMap, env);
            else if (tree is types.MalSymbol)
                return eval_symbol(tree as types.MalSymbol, env);
            else
                return tree;
        }

        public static types.MalList eval_list(types.MalList tree, evaluator.Environment env)
        {
            //Empty list: return the empty list
            if (tree.isEmpty())
                return tree;

            //Recursively evaluate all the items in the list
            types.MalList evaluatedList = eval_list(tree.rest(), env);
            evaluatedList.cons(eval_ast(tree.first(), env));

            return evaluatedList;
        }

        public static types.MalVal apply_function(types.MalList tree)
        {
            if (tree.isEmpty())
                return tree;
            if (tree.first() is types.MalFunc)
                return (tree.first() as types.MalFunc).apply(tree.rest());
            else throw new ArgumentException("Item in function position is not a function.");
        }

        public static types.MalVal eval_vector(types.MalVector tree, evaluator.Environment env)
        {
            types.MalVector evaluatedVector = new types.MalVector();
            foreach (types.MalVal child in tree)
            {
                evaluatedVector.conj(eval_ast(child, env));
            }
            return evaluatedVector;
        }

        public static types.MalVal eval_map(types.MalMap tree, evaluator.Environment env)
        {
            types.MalMap evaluatedMap = new types.MalMap();
            foreach (types.MalVal child in tree)
            {
                types.MalVector pair = child as types.MalVector;
                types.MalVal key = pair.nth(0);
                types.MalVal value = pair.nth(1);
                evaluatedMap.assoc(eval_ast(key,env), eval_ast(value,env));
            }
            return evaluatedMap;
        }

        public static types.MalVal eval_symbol(types.MalSymbol tree, evaluator.Environment env)
        {
            if (env.ContainsKey(tree.name))
                return env[tree.name];
            else throw new ArgumentException("Symbol " + tree.name + " not defined.");
        }
    }
}
