//Evaluator functions for mal
//Created by James Vanderhyde, 9 October 2021

using System;
using System.Collections.Generic;
using Mal;

namespace Mal
{
    public class evaluator
    {
        public static readonly int maxIntInFloat = 16777216;

        public static types.MalVal eval_ast(types.MalVal tree, env.Environment env)
        {
            int numIterations = 0;
            while (numIterations < maxIntInFloat)
            {
                numIterations++;

                if (tree is types.MalList)
                {
                    types.MalVal result = apply_list(tree as types.MalList, env);
                    if (!(result is types.TailCall))
                        return result;
                    types.TailCall tailResult = result as types.TailCall;
                    tree = tailResult.bodyTree;
                    env = tailResult.outerEnvironment;
                    continue;
                }
                else if (tree is types.MalVector)
                    return eval_vector(tree as types.MalVector, env);
                else if (tree is types.MalMap)
                    return eval_map(tree as types.MalMap, env);
                else if (tree is types.MalSymbol)
                    return eval_symbol(tree as types.MalSymbol, env);
                else
                    return tree;
            }
            throw new ArgumentException("Possible infinite loop.");
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
                    env.Environment letEnv = new env.Environment(env, true);
                    bind(tree.rest().first(), letEnv);
                    return new types.TailCall(tree.rest().rest().first(), letEnv);
                }
                else if (form.Equals("loop*"))
                {
                    if (tree.rest().isEmpty() || !(tree.rest().first() is types.MalList || tree.rest().first() is types.MalVector))
                        throw new ArgumentException("Loop is missing a list of bindings.");
                    if (tree.rest().rest().isEmpty())
                        throw new ArgumentException("Loop is missing a value.");
                    types.MalVal bodyTree = types.MalNil.malNil;
                    if (!tree.rest().rest().isEmpty())
                        bodyTree = tree.rest().rest().first();
                    types.MalCollection bindingCollection = tree.rest().first() as types.MalCollection;
                    types.MalVector unboundSymbols = new types.MalVector();
                    bool symbol = true;
                    foreach (types.MalVal item in bindingCollection)
                    {
                        if (symbol)
                            unboundSymbols.conj(item);
                        symbol = !symbol;
                    }
                    env.Environment loopEnv = new env.Environment(env, true, new types.FuncClosure(env, unboundSymbols, bodyTree));
                    bind(bindingCollection, loopEnv);
                    return new types.TailCall(bodyTree, loopEnv);
                }
                else if (form.Equals("do"))
                {
                    types.MalList doForms = tree.rest();
                    if (doForms.isEmpty())
                        return types.MalNil.malNil;
                    env.Environment doEnv = new env.Environment(env, false);
                    while (!doForms.rest().isEmpty())
                    {
                        eval_ast(doForms.first(), doEnv);
                        doForms = doForms.rest();
                    }
                    env.Environment doEnvTail = new env.Environment(env, true);
                    return new types.TailCall(doForms.first(), doEnvTail);
                }
                else if (form.Equals("if"))
                {
                    if (tree.rest().isEmpty() || tree.rest().rest().isEmpty())
                        throw new ArgumentException("if is missing a value.");
                    env.Environment ifEnv = new env.Environment(env, false);
                    types.MalVal condition = eval_ast(tree.rest().first(), ifEnv);
                    env.Environment ifEnvTail = new env.Environment(env, true);
                    if ((condition is types.MalNil) ||
                        ((condition is types.MalBoolean) && (condition as types.MalBoolean).value==false))
                    {
                        if (tree.rest().rest().rest().isEmpty())
                            return types.MalNil.malNil;
                        return new types.TailCall(tree.rest().rest().rest().first(), ifEnvTail);
                    }
                    return new types.TailCall(tree.rest().rest().first(), ifEnvTail);
                }
                else if (form.Equals("fn*"))
                {
                    if (tree.rest().isEmpty())
                        throw new ArgumentException("fn is missing binding symbols");
                    if (!(tree.rest().first() is types.MalCollection))
                        throw new ArgumentException("fn parameter must be a list or vector");
                    types.MalCollection bindingList = tree.rest().first() as types.MalCollection;

                    types.MalVal bodyTree = types.MalNil.malNil;
                    if (!tree.rest().rest().isEmpty())
                        bodyTree = tree.rest().rest().first();

                    bool foundAmpersand = false;
                    bool foundSymbolAfterAmpersand = false;
                    foreach (types.MalVal bindingSymbol in bindingList)
                    {
                        if (!(bindingSymbol is types.MalSymbol))
                            throw new ArgumentException("fn has something other than a binding symbol: " + bindingSymbol.GetType());
                        if (foundSymbolAfterAmpersand)
                            throw new ArgumentException("fn has extra parameters after the &.");
                        if (foundAmpersand)
                            foundSymbolAfterAmpersand = true;
                        if (((types.MalSymbol)bindingSymbol).name.Equals("&"))
                            foundAmpersand = true;
                    }
                    return new types.FuncClosure(env, bindingList, bodyTree);
                }
                else if (form.Equals("recur"))
                {
                    if (env.recurPoint==null)
                        throw new ArgumentException("recur must be in tail position inside fn or loop.");
                    env.Environment recurEnv = new env.Environment(env, false);
                    types.MalList recurArgs = eval_list(tree.rest(), recurEnv);
                    return apply_function(env.recurPoint, recurArgs);
                }
            }

            //Assume the form is a function, so evaluate all of the arguments
            env.Environment fEnv = new env.Environment(env, false);
            types.MalVal f = eval_ast(tree.first(), fEnv);
            types.MalList args = eval_list(tree.rest(), fEnv);
            return apply_function(f, args);
        }

        private static void bind(types.MalVal bindingCollection, env.Environment env)
        {
            if (bindingCollection is types.MalList)
            {
                types.MalList bindingList = bindingCollection as types.MalList;
                while (!bindingList.isEmpty() && !bindingList.rest().isEmpty())
                {
                    if (!(bindingList.first() is types.MalSymbol))
                        throw new ArgumentException("Item to bind is not a symbol, it is a " + bindingList.first().GetType());
                    string name = (bindingList.first() as types.MalSymbol).name;
                    types.MalVal value = eval_ast(bindingList.rest().first(), env);
                    env.set(name, value);
                    bindingList = bindingList.rest().rest();
                }
            }
            else
            {
                types.MalVector bindingVector = bindingCollection as types.MalVector;
                int index = 0;
                while (index + 1 < bindingVector.count())
                {
                    if (!(bindingVector.nth(index) is types.MalSymbol))
                        throw new ArgumentException("Item to bind is not a symbol, it is a " + bindingVector.nth(index).GetType());
                    string name = (bindingVector.nth(index) as types.MalSymbol).name;
                    types.MalVal value = eval_ast(bindingVector.nth(index + 1), env);
                    env.set(name, value);
                    index += 2;
                }
            }
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
            else throw new ArgumentException("Item in function position is not a function, it is a " + f.GetType());
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
