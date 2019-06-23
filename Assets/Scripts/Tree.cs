//representation of a phylogenetic tree, has a root node and keeps track of the total number of nodes in the tree 
public class Tree {

    private Node root;
    private int numNodes = 0;
    private int numLeafNodes = 0;

    //sets the trees root to the passed node
    //PARAMETERS: Node n - node to be used as the tree's root 
    public void setRoot(Node n) {
        root = n;
    }

    public Node getRoot()
    {
        return root;
    }

    public void newNodeConfirmed(Node n) {
        numNodes++;
        if (n.isLeafNode()) {
            numLeafNodes++;
        }
        n.calculateTotalDescendants();
    }

    public int getNumberOfBranches() {
        return numNodes - 1;
    }

    public double totalBranchLength() {
        return root.sumWeightsOfDescendants();
    }

    public int getNumLeafNodes() {
        return numLeafNodes;
    }    
}

