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
	/// <param name="reversed">Whether or not to reverse the resulting path.</param>
	/// <returns>An <see cref="List{AStarNode}"/> that contains the computed path.</returns>
	internal AStarPath ComputePathInternal( Vector3B start, Vector3B end, CancellationToken token, float maxDistance = 2048f, bool reversed = false )
	{
		// Setup.
		var path = new AStarPath();

		var maxCells = 8192; // Placeholder until I have something to calculate this (Max voxels in a VoxelWorld?)
		var openSet = new Heap<AStarNode>( maxCells );
		var closedSet = new HashSet<AStarNode>();

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

			foreach ( var neighbour in withCellConnections ? currentNode.Current.GetNeighbourAndConnections() : currentNode.Current.GetNeighbourConnections() )
			{
				if ( pathBuilder.HasOccupiedTagToExclude && !pathBuilder.HasPathCreator && neighbour.Occupied ) continue;
				if ( pathBuilder.HasOccupiedTagToExclude && pathBuilder.HasPathCreator && neighbour.Occupied && neighbour.OccupyingEntity != pathBuilder.PathCreator ) continue;
				if ( pathBuilder.HasTagsToExlude && neighbour.Tags.Has( pathBuilder.TagsToExclude ) ) continue;
				if ( pathBuilder.HasTagsToInclude && !neighbour.Tags.Has( pathBuilder.TagsToInclude ) ) continue;
				if ( neighbour.MovementTag == "drop" && currentNode.Current.Bottom.z - neighbour.Current.Position.z > pathBuilder.MaxDropHeight ) continue;
				if ( closedSet.Contains( neighbour ) ) continue;

				var isInOpenSet = openSetReference.ContainsKey( neighbour.GetHashCode() );
				var currentNeighbour = isInOpenSet ? openSetReference[neighbour.GetHashCode()] : neighbour;

				var malus = 0f;

				if ( pathBuilder.HasTagsToAvoid && currentNeighbour.Tags.Has( pathBuilder.TagsToAvoid.Keys ) )
					foreach ( var tag in currentNeighbour.Tags.All )
						if ( pathBuilder.TagsToAvoid.TryGetValue( tag, out float tagMalus ) )
							malus += tagMalus;

				var newMovementCostToNeighbour = currentNode.gCost + currentNode.Distance( currentNeighbour ) + malus / 2f;
				var distanceToTarget = currentNeighbour.Distance( targetNode ) + malus / 2f;

				if ( distanceToTarget > maxDistance ) continue;

				if ( newMovementCostToNeighbour < currentNeighbour.gCost || !isInOpenSet )
				{
					currentNeighbour.gCost = newMovementCostToNeighbour;
					currentNeighbour.hCost = distanceToTarget;
					currentNeighbour.Parent = currentNode;

					if ( !isInOpenSet )
					{
						openSet.Add( currentNeighbour );
						openSetReference.Add( currentNeighbour.GetHashCode(), currentNeighbour );
					}
				}
			}

			cellsChecked++;
		}

		if ( token.IsCancellationRequested )
			return path;

		if ( path.Count == 0 && pathBuilder.AcceptsPartial )
		{
			var closestNode = closedSet.OrderBy( x => x.hCost )
				.Where( x => x.gCost != 0f )
				.First();

			RetracePath( ref path, startingNode, closestNode );
		}

		if ( reversed )
			path.Reverse();

		return path;
	}

	private static void RetracePath( ref List<AStarNode> pathList, AStarNode startNode, AStarNode targetNode )
	{
		var currentNode = targetNode;

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

		pathList = fixedList;
		//pathList = pathList.Select( node => new AStarNode( node.Parent.Current, node, node.MovementTag ) ).ToList(); // Cell connections are flipped when we reversed earlier
	}
}
