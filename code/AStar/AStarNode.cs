namespace Deathcard;

public struct AStarNode : IHeapItem<AStarNode>, IEquatable<AStarNode>, IValid
{
	public VoxelQueryData Data { get; internal set; }
	public VoxelWorld World { get; internal set; }
	public float gCost { get; internal set; } = 0f;
	public float hCost { get; internal set; } = 0f;
	public float fCost => gCost + hCost;
	public int HeapIndex { get; set; }
	public bool IsValid = false;

	public AStarNode() { }

	public AStarNode( VoxelWorld world, VoxelQueryData data ) : this()
	{
		Data = data;
		World = world;
		IsValid = true;
	}

	public AStarNode( VoxelWorld world, Vector3B offset ) : this()
	{
		var queryData = World?.GetByOffset( offset );

		if ( queryData != null )
		{
			Data = queryData.Value;
			World = world;
			IsValid = true;
		}
	}

	/// <summary>
	/// Get the neighbouring voxel
	/// </summary>
	/// <param name="offset"></param>
	/// <returns></returns>
	public AStarNode? GetNeighbour( Vector3B offset )
	{
		var checkPosition = Data.Position + offset;

		var queryData = World?.GetByOffset( checkPosition );

		if ( queryData != null )
		{
			var newNode = new AStarNode( World, queryData.Value );
			return newNode;
		}

		return null;
	}

	/// <summary>
	/// Get all neighbouring voxels
	/// </summary>
	/// <returns></returns>
	public IEnumerable<AStarNode> GetNeighbours()
	{
		for ( int x = -1; x < 1; x++ )
		{
			for ( int y = -1; y < 1; y++ )
			{
				for ( int z = -1; z < 1; z++ )
				{
					var offset = new Vector3( x, y, z );
					if ( offset == Vector3.Zero ) continue;

					var neighbourFound = GetNeighbour( offset );

					if ( neighbourFound != null )
						yield return neighbourFound.Value;
				}
			}
		}
	}


	public int CompareTo( AStarNode other )
	{
		var compare = fCost.CompareTo( other.fCost );
		if ( compare == 0 )
			compare = hCost.CompareTo( other.hCost );
		return -compare;
	}

	public override int GetHashCode()
	{
		var currentHash = Data.GetHashCode();
		var gCostHash = gCost.GetHashCode();
		var hCostHash = hCost.GetHashCode();

		return HashCode.Combine( currentHash, gCostHash, hCostHash );
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
