namespace HPF.model {
    public class GridMapBuilder {
        private readonly int _n, _m, _gridSize;
        private string _mapStr = "";
        private bool _oneGatePerEdge = false;

        public GridMapBuilder(int n, int m, int gridSize) {
            _n = n;
            _m = m;
            _gridSize = gridSize;
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
                .InitGatesV2()
                .InitConnections()
                .ConnectStartGate()
                .ConnectGoalGate();
            return grid;
        }
    }

}
