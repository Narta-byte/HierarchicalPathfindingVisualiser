namespace HPF.model {
    public class GridMapBuilder {
        private int _n, _m, _gridSize;
        private string _mapStr = "";
        private bool _oneGatePerEdge = false;

        public GridMapBuilder WithMapSize(int n, int m) {
            _n = n;
            _m = m;
            return this;
        }
        public GridMapBuilder WithGridSize(int gridSize) {
            _gridSize = gridSize;
            return this;
        }
        public GridMapBuilder WithMap(string mapStr) {
            _mapStr = mapStr;
            return this;
        }

        public GridMapBuilder WithOneGatePerEdge(bool value = true) {
            _oneGatePerEdge = value;
            return this;
        }

        public GridMap Build() {
            if (string.IsNullOrEmpty(_mapStr))
                throw new InvalidOperationException("Map string must be set before building.");

            var grid = new GridMap(_n, _m, _gridSize);
            grid.MapFromStr(_n, _m, _mapStr);
            grid.InitComponents();
            grid.SetIsUsingOneGatePerEdge(_oneGatePerEdge)
                .InitChunks()
                .InitGates()
                .InitConnections()
                .ConnectStartGate()
                .ConnectGoalGate();
            return grid;
        }
    }

}