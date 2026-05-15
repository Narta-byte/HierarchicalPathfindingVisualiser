namespace HPF.model {
    class UnionFind {
        private int[] parent;

        public UnionFind(int size) {
            parent = new int[size];

            for (int i = 0; i < size; i++)
                parent[i] = i;
        }

        public int Find(int x) {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);

            return parent[x];
        }

        public void Union(int a, int b) {
            int rootA = Find(a);
            int rootB = Find(b);

            if (rootA != rootB) {
                parent[rootB] = rootA;
            }
        }
    }
}
