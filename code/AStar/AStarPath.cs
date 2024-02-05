namespace Deathcard;

public struct AStarPath
{
	public List<AStarNode> Nodes { get; set; } = new();
	public int Count => Nodes?.Count() ?? 0;
	public bool IsEmpty => Nodes == null || Count == 0;

	public AStarPath() { }

	public AStarPath( List<AStarNode> nodes ) : this()
	{
		Nodes = nodes;
	}

	/// <summary>
	/// Return the total length of the path
	/// </summary>
	/// <returns></returns>
	public float GetLength()
	{
		var length = 0f;

		for ( int i = 0; i < Nodes.Count - 1; i++ )
			length += Nodes[i].Position.Distance( Nodes[i + 1].Position );

		return length;
	}

	/// <summary>
	/// Simplify the path by iterating over line of sights between the given segment size, joining them if valid
	/// </summary>
	/// <param name="segmentAmounts"></param>
	/// <param name="iterations"></param>
	/// <param name="tagsToExclude">Tags where it won't merge if they're present</param>
	/// <returns></returns>
	public void Simplify( int segmentAmounts = 2, int iterations = 8, params string[] tagsToExclude )
	{
		for ( int iteration = 0; iteration < iterations; iteration++ )
		{
			var segmentStart = 0;
			var segmentEnd = Math.Min( segmentAmounts, Count - 1 );

			while ( Count > 2 && segmentEnd < Count - 1 )
			{
				var currentNode = Nodes[segmentStart];
				var nextNode = Nodes[segmentStart + 1];
				var furtherNode = Nodes[segmentEnd];

				// TODO: Line of sight for this

				//if ( Settings.Grid.LineOfSight( currentNode.Current, furtherNode.Current, Settings.PathCreator ) )
				//	for ( int toDelete = segmentStart + 1; toDelete < segmentEnd; toDelete++ )
				//		Nodes.RemoveAt( toDelete );


				if ( segmentEnd == Count - 1 )
					break;

				segmentStart++;
				segmentEnd = Math.Min( segmentStart + segmentAmounts, Count - 1 );
			}
		}
	}
}

