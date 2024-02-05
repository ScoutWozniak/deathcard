namespace Deathcard;

public struct AStarNode : IHeapItem<AStarNode>, IEquatable<AStarNode>
{
	public IVoxel Current { get; internal set; } = null;
	public Vector3S Position { get; internal set; }
	public float gCost { get; internal set; } = 0f;
	public float hCost { get; internal set; } = 0f;
	public float fCost => gCost + hCost;
	public int HeapIndex { get; set; }

	public AStarNode() { }

	public int CompareTo( AStarNode other )
	{
		var compare = fCost.CompareTo( other.fCost );
		if ( compare == 0 )
			compare = hCost.CompareTo( other.hCost );
		return -compare;
	}

	public override int GetHashCode()
	{
		var currentHash = Current.GetHashCode();
		var positionHash = Position.GetHashCode();
		var gCostHash = gCost.GetHashCode();
		var hCostHash = hCost.GetHashCode();

		return HashCode.Combine( currentHash, positionHash, gCostHash, hCostHash );
	}

	public static bool operator ==( AStarNode a, AStarNode b ) => a.Equals( b );
	public static bool operator !=( AStarNode a, AStarNode b ) => !a.Equals( b );

	public override bool Equals( object obj )
	{
		if ( obj is not AStarNode node ) return false;
		if ( node.GetHashCode() != GetHashCode() ) return false;

		return true;
	}

	public bool Equals( AStarNode other )
		=> GetHashCode() == other.GetHashCode();
}
