//Evaluator functions for mal
//Created by James Vanderhyde, 9 October 2021

using System;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class evaluator
    {
        public static types.MalVal eval_ast(types.MalVal tree, env.Environment env)
        {
            if (tree is types.MalList)
                return apply_list(tree as types.MalList, env);
            else if (tree is types.MalVector)
                return eval_vector(tree as types.MalVector, env);
            else if (tree is types.MalMap)
                return eval_map(tree as types.MalMap, env);
            else if (tree is types.MalSymbol)
                return eval_symbol(tree as types.MalSymbol, env);
            else
                return tree;
        }

        public static types.MalVal apply_list(types.MalList tree, env.Environment env)
        {
            //Empty list: return the empty list
            if (tree.isEmpty())
                return tree;

            //Check for special forms first
            if (tree.first() is types.MalSymbol)
            {
                string form = (tree.first() as types.MalSymbol).name;
                if (form.Equals("def!"))
                {
                    if (tree.rest().isEmpty() || !(tree.rest().first() is types.MalSymbol))
                        throw new ArgumentException("Item to define is not a symbol.");
                    if (tree.rest().rest().isEmpty())
                        throw new ArgumentException("There is no value to define the symbol to.");
                    string name = (tree.rest().first() as types.MalSymbol).name;
                    types.MalVal value = eval_ast(tree.rest().rest().first(), env);
                    env.set(name, value);
                    return value;
                }
                else if (form.Equals("let*"))
                {
                    if (tree.rest().isEmpty() || !(tree.rest().first() is types.MalList || tree.rest().first() is types.MalVector))
                        throw new ArgumentException("Let is missing a list of bindings.");
                    if (tree.rest().rest().isEmpty())
                        throw new ArgumentException("Let is missing a value.");
                    env.Environment letEnv = new env.Environment(env);
                    if (tree.rest().first() is types.MalList)
                    {
                        types.MalList bindingList = tree.rest().first() as types.MalList;
                        while (!bindingList.isEmpty() && !bindingList.rest().isEmpty())
                        {
                            if (!(bindingList.first() is types.MalSymbol))
                                throw new ArgumentException("Item to bind is not a symbol.");
                            string name = (bindingList.first() as types.MalSymbol).name;
                            types.MalVal value = eval_ast(bindingList.rest().first(), letEnv);
                            letEnv.set(name, value);
                            bindingList = bindingList.rest().rest();
                        }
                    }
                    else
                    {
                        types.MalVector bindingVector = tree.rest().first() as types.MalVector;
                        int index = 0;
                        while (index+1 < bindingVector.count())
                        {
                            if (!(bindingVector.nth(index) is types.MalSymbol))
                                throw new ArgumentException("Item to bind is not a symbol.");
                            string name = (bindingVector.nth(index) as types.MalSymbol).name;
                            types.MalVal value = eval_ast(bindingVector.nth(index+1), letEnv);
                            letEnv.set(name, value);
                            index += 2;
                        }
                    }
                    return eval_ast(tree.rest().rest().first(), letEnv);
                }
            }

            //Assume the form is a function, so evaluate all of the arguments
            types.MalVal f = eval_ast(tree.first(), env);
            types.MalList args = eval_list(tree.rest(), env);
            return apply_function(f, args);
        }

        public static types.MalList eval_list(types.MalList tree, env.Environment env)
        {
            //Empty list: return the empty list
            if (tree.isEmpty())
                return tree;

            //Recursively evaluate all the items in the list
            types.MalList evaluatedList = eval_list(tree.rest(), env);
            evaluatedList.cons(eval_ast(tree.first(), env));

            return evaluatedList;
        }

        public static types.MalVal apply_function(types.MalVal f, types.MalList args)
        {
            if (f is types.MalFunc)
                return (f as types.MalFunc).apply(args);
            else throw new ArgumentException("Item in function position is not a function.");
        }

        public static types.MalVal eval_vector(types.MalVector tree, env.Environment env)
        {
            types.MalVector evaluatedVector = new types.MalVector();
            foreach (types.MalVal child in tree)
            {
                evaluatedVector.conj(eval_ast(child, env));
            }
            return evaluatedVector;
        }

        public static types.MalVal eval_map(types.MalMap tree, env.Environment env)
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

        public static types.MalVal eval_symbol(types.MalSymbol tree, env.Environment env)
        {
            return env.get(tree.name);
        }
    }
}
