using System.Runtime.CompilerServices;
using System.Threading;

namespace Deathcard;


partial class VoxelWorld
{
	/// <summary>
	/// Computes a path from the starting point to a target point. Reversing the path if needed.
	/// </summary>
	/// <param name="start">The starting point of the path.</param>
	/// <param name="end">The desired destination point of the path.</param>
	/// <param name="token">A cancellation token used to cancel computing the path.</param>
	/// <param name="maxDistance">Max distance it will search for, then will just approach the closest point</param>
	/// <param name="acceptPartial">If it doesn't find the target, return the closest node found</param>
	/// <param name="reversed">Whether or not to reverse the resulting path.</param>
	/// <returns>An <see cref="List{AStarNode}"/> that contains the computed path.</returns>
	internal List<AStarNode> ComputePathInternal( Vector3B start, Vector3B end, CancellationToken token, float maxDistance = 2048f, bool acceptPartial = true, bool reversed = false )
	{
		// Setup.
		var path = new List<AStarNode>();

		var maxCells = 8192; // Placeholder until I have something to calculate this (Max voxels in a VoxelWorld?)
		var openSet = new Heap<AStarNode>( maxCells );
		var closedSet = new HashSet<AStarNode>();
		var openSetReference = new Dictionary<int, AStarNode>(); // We need this because we create AStarNode down the line for each neighbour and we need a way to reference these

		var startNode = new AStarNode( this, start );
		var endNode = new AStarNode( this, end );

		if ( !startNode.IsValid || !startNode.IsValid )
			return path;

		openSet.Add( startNode );

		var cellsChecked = 0;

		while ( openSet.Count > 0 && cellsChecked < maxCells && !token.IsCancellationRequested )
		{
			var currentNode = openSet.RemoveFirst();
			closedSet.Add( currentNode );

			if ( currentNode.Data.Position == endNode.Data.Position )
			{
				RetracePath( ref path, startNode, currentNode );
				break;
			}

			foreach ( var neighbour in currentNode.GetNeighbours() )
			{
				if ( !neighbour.IsValid || neighbour.Data.Voxel == null || closedSet.Contains( neighbour ) )
					continue;

				var isInOpenSet = openSet.Contains( neighbour );
				var currentNeighbour = isInOpenSet ? openSetReference[neighbour.GetHashCode()] : neighbour;
				var newMovementCostToNeighbour = currentNode.gCost + currentNode.Distance( currentNeighbour );
				var distanceToTarget = currentNeighbour.Distance( endNode );

				if ( distanceToTarget > maxDistance ) continue;

				if ( newMovementCostToNeighbour < currentNeighbour.gCost || !isInOpenSet )
				{
					currentNeighbour.gCost = newMovementCostToNeighbour;
					currentNeighbour.hCost = distanceToTarget;
					currentNeighbour.Parent = currentNode.GetHashCode();

					if ( !isInOpenSet )
					{
						openSet.Add( currentNeighbour );
						openSetReference[currentNeighbour.GetHashCode()] = currentNeighbour;
					}
				}
			}

			cellsChecked++;
		}

		if ( token.IsCancellationRequested )
			return path;

		if ( path.Count == 0 && acceptPartial )
		{
			var closestNode = closedSet.OrderBy( x => x.hCost )
				.Where( x => x.gCost != 0f )
				.First();

			RetracePath( ref path, startNode, closestNode );
		}

		if ( reversed )
			path.Reverse();

		return path;
	}

	private static void RetracePath( ref List<AStarNode> pathList, AStarNode startNode, AStarNode targetNode )
	{
		/*var currentNode = targetNode;

		while ( currentNode != startNode )
		{
			pathList.Add( currentNode );
			currentNode = currentNode.Parent;
		}
		pathList.Reverse();

		var fixedList = new List<AStarNode>();

		foreach ( var node in pathList )
		{
			if ( node.Parent?.Current == null )
				continue;

			var newNode = new AStarNode( node.Parent.Current, node, node.MovementTag );
			fixedList.Add( newNode );
		}

		pathList = fixedList;*/
		//pathList = pathList.Select( node => new AStarNode( node.Parent.Current, node, node.MovementTag ) ).ToList(); // Cell connections are flipped when we reversed earlier
	}
}
