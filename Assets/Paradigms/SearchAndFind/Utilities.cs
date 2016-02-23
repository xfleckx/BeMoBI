using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.BeMoBI.Paradigms.SearchAndFind
{
    public static class Utilities
    {
        public static Dictionary<int, List<PathInMaze>> GetPathsGroupedByDifficulty(List<PathInMaze> paths)
        {
           var pathsAsSetOfPathElements = paths.GroupBy(p => p.GetDifficulty());

            return pathsAsSetOfPathElements.ToDictionary(g => g.Key, g => g.ToList());
        }

        private static int GetDifficulty(this PathInMaze path)
        {
            var pathElements = path.PathAsLinkedList.ToList();

            return pathElements.Count(e => e.Type == UnitType.T);
        }
    }
}
