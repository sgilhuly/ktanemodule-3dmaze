namespace ThreeDMaze
{
    sealed class MapNode
    {
        public char label;
        public bool n_wall;
        public bool w_wall;

        public MapNode(char label, bool n_wall, bool w_wall)
        {
            this.label = label;
            this.n_wall = n_wall;
            this.w_wall = w_wall;
        }
    }
}