using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class NewickParser{

    static TextAsset textTreeFile;

    static private int depth = 0;

    static public string ErrorMessage { private set; get; } = null;
    static private string errorMessageStart = "";

    static public Tree Parse(string sFileName)
    {
        errorMessageStart = "error while generating:\n" + TreeLoader.CurrentTreeFileName;
        Tree createdTree = new Tree();
        textTreeFile = Resources.Load<TextAsset>(sFileName);
        string textTree = textTreeFile.ToString();
        textTree = Regex.Replace(textTree, @"\s+", "");

        List<string> tokens = Tokeniser.Tokenise(textTree);

        Stack<Node> nodeStack = new Stack<Node>();
        createdTree.setRoot(new Node());
        nodeStack.Push(createdTree.getRoot());

        bool bExpectName = true;
        Node lastNamed = null;
        bool isLabel = false;

        foreach (string token in tokens)
        {
            if (token.Length == 1)
            { // not a name - basically this is just a character token like '(' or '"'
                switch (token[0])
                {
                    case Tokeniser.openBracket:
                        {
                            depth++;
                            nodeStack.Push(new Node());
                            bExpectName = true;
                            break;
                        }
                    case Tokeniser.closeBracket:
                        {
                            depth--;
                            if (bExpectName)
                            {
                                lastNamed = popAndName(createdTree, null, nodeStack);
                                if (lastNamed == null) //error in naming because the stack was empty but we didn't name the root
                                    return null;
                            }
                            bExpectName = true;
                            break;
                        }
                    case Tokeniser.childSeparator:
                        {
                            if (bExpectName)
                            {
                                lastNamed = popAndName(createdTree, null, nodeStack);
                                if (lastNamed == null) //error in naming because the stack was empty but we didn't name the root
                                    return null;
                            }
                            nodeStack.Push(new Node());
                            bExpectName = true;
                            break;
                        }
                    case Tokeniser.treeTerminator:
                        {
                            if (bExpectName)
                            {
                                lastNamed = popAndName(createdTree, null, nodeStack);
                                if (lastNamed == null) //error in naming because the stack was empty but we didn't name the root
                                    return null;
                            }
                            break;
                        }
                    case Tokeniser.quote:
                        {
                            break;
                        }
                    case Tokeniser.doubleQuote:
                        {
                            break;
                        }
                    case Tokeniser.infoSeparator:
                        {
                            if (bExpectName)
                            {
                                lastNamed = popAndName(createdTree, null, nodeStack);
                                if (lastNamed == null) //error in naming because the stack was empty but we didn't name the root
                                    return null;
                            }
                            bExpectName = false;
                            break;
                        }
                    default:
                        {
                            isLabel = true;
                            break;
                        }
                }
            }
            if (token.Length > 1 || isLabel)
            { // this is likely a name or an error
                if (!bExpectName)
                {
                    try
                    {
                        double d = double.Parse(token);
                        if (lastNamed != null) lastNamed.setWeightToParent(d);
                        else
                        {
                            ErrorMessage = "Syntax " + errorMessageStart  + "\nEdge weight: " + d + " not expected here";
                            GameManager.DebugLog(ErrorMessage);
                            return null;
                        }
                    }
                    catch (FormatException nfe)
                    {
                        ErrorMessage = "Syntax " + errorMessageStart + "\nName: " + token + " not expected here"; 
                        GameManager.DebugLog(ErrorMessage);
                        return null;
                    }
                }
                else
                {
                    lastNamed = popAndName(createdTree, token, nodeStack);
                    if (lastNamed == null) //error in naming because the stack was empty but we didn't name the root
                        return null;
                    bExpectName = false;
                }
                isLabel = false;
            }
            //Debug.Log("Just read: " + token + " - Current stack depth: " + nodeStack.Count);
        }

        if (!(nodeStack.Count == 0))
        {
            ErrorMessage = "Syntax " + errorMessageStart + " \nNode stack still has: " + nodeStack.Count + " elements\nMissing/Imbalanced parentheses in file";
            GameManager.DebugLog(ErrorMessage);
            return null;            
        }

        GameManager.DebugLog("Successfully Created Tree");
        return createdTree;
    }

    //pops a node from the node stack, names it, and assigns it to a parent if it is a child node
    static Node popAndName(Tree currentTree, string name, Stack<Node> nodeStack)
    {
        Node top = nodeStack.Pop();
        currentTree.newNodeConfirmed(top);
        if (name == null)
            top.setLabel("");
        else
            top.setLabel(name);

        top.setDepth(depth);
        
        if (nodeStack.Count > 0)
        {
            Node parent = nodeStack.Peek();
            if (parent != null)
            {
                parent.addChild(top);
                top.setParent(parent);
            }
        }
        else
        {
            if (top != currentTree.getRoot())
            {
                ErrorMessage = "Really weird "+ errorMessageStart + " \nOn node: " + top + " node stack is empty\nBut this node is not the root!";
                GameManager.DebugLog(ErrorMessage);
                return null; //if we return null then the main function will also return null
            }
        }
        return top;
    }
}
