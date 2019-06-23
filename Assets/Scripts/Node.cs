using System.Collections.Generic;

public class Node {

    List<Node> children;
    int totalDescendants = 0;
    public int nodeDepth { get; private set; } = 0;
    Node parent = null;
    string label = "";
    double weightToParent;
    string metaData = "";
    bool weightSpecified = false;

    public static int maxDepth { get; private set; } = 0;

    public Node()
    {
        children = new List<Node>();
    }

    public static void clearMaxDepth() {
        maxDepth = 0;
    }

    public void setParent(Node p) {
        parent = p;
    }

    public Node getParent()
    {
        return parent;
    }

    public void addChild(Node child)
    {
        children.Add(child);
    }

    public void setWeightToParent(double d)
    {
        weightToParent = d;
        weightSpecified = true;
    }

    public void setLabel(string label) {
        this.label = label;
    }

    public string getLabel() {
        return label;
    }

    public List<Node> getChildren() {
        return children;
    }

    public int getNumberOfChildren() {
        return children.Count;
    }

    public double getWeightToParent() {
        if (weightSpecified)
            return weightToParent;
        return 1d;
    }

    public string GetMetaData() {
        if (metaData != null)
        {
            return metaData;
        }
        else
        {
            return "";
        }
    }

    public void SetMetaData(string s) {
        metaData = s;
    }

    public bool weightToParentSet() {
        return weightSpecified;
    }

    public double sumWeightsOfDescendants() {
        double w = getWeightToParent();
        foreach(Node child in children)
        {
            w += child.sumWeightsOfDescendants();
        }
        return w;
    }

    public bool isLeafNode() {
        return children.Count == 0;
    }

    public int getTotalDescendants() {
        return totalDescendants;
    }

    public void calculateTotalDescendants() {
        totalDescendants = children.Count;
        foreach (Node c in children) {
            totalDescendants += c.getTotalDescendants();
        }
    }

    public void setDepth(int depth) {
        nodeDepth = depth;
        maxDepth = depth > maxDepth ? depth : maxDepth;
    }

}
