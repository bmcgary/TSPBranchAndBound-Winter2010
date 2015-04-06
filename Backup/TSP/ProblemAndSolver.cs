using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Timers;
using C5;
//BLAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAINE
namespace TSP
{
    class ProblemAndSolver
    {
        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// you are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }


            /// <summary>
            ///  compute the cost of the current route.  does not check that the route is complete, btw.
            /// assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }
                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region private members
        private const int DEFAULT_SIZE = 25;

        private const int CITY_ICON_SIZE = 5;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region public members.
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        public const int DEFAULT_SEED = -1;

        #region Constructors
        public ProblemAndSolver()
        {
            initialize(DEFAULT_SEED, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed)
        {
            initialize(seed, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed, int size)
        {
            initialize(seed, size);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// reset the problem instance. 
        /// </summary>
        private void resetData()
        {
            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            for (int i = 0; i < _size; i++)
                Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.LightGray, 1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        private void initialize(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            if (seed != DEFAULT_SEED)
                this.rnd = new Random(seed);
            else
                this.rnd = new Random();
            this.resetData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size)
        {
            this._size = size;
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width = g.VisibleClipBounds.Width - 45F;
            float height = g.VisibleClipBounds.Height - 15F;
            Font labelFont = new Font("Arial", 10);

            g.DrawString("n(c) means this node is the nth node in the current solution and incurs cost c to travel to the next node.", labelFont, cityBrushStartStyle, new PointF(0F, 0F));

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count - 1)
                        g.DrawString(" " + index + "(" + c.costToGetTo(bssf.Route[index + 1] as City) + ")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else
                        g.DrawString(" " + index + "(" + c.costToGetTo(bssf.Route[0] as City) + ")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D;
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        /// 

        Timer timer = new Timer(60000); //set the interval for 60 seconds

        public void solveProblem()
        {

            timer.AutoReset = true;
            timer.Start();
            //branch and bound
            //init the state
            TSPSolution initSolution;

            initSolution = getInitialSolution();        //here is your initial solution...fool.

            State initState = new State();

            //MAY BE ABLKE TO USER OTHER HELPER FUNCTIONS==============================
            int minValue = int.MaxValue;
            double value = int.MaxValue;

            for (int i = 0; i < Cities.Length; i++)
            {
                initState.costMatrix.Add(new List<double>());
                for (int j = 0; j < Cities.Length; j++)
                {
                    value = Cities[i].costToGetTo(Cities[j]);
                    //load up the cost matrix with the values
                    initState.costMatrix[i].Add(value);

                    //also look for the smallest value
                    if (value < minValue)
                        value = minValue;
                }
            }

            //go reduce the matrix
            reduceMatrix(initState.costMatrix, minValue);
            
            initState.cost += minValue;
            //======================================================================
            initState.cost += checkFor0(initState.costMatrix);

            
            for (int i = 0; i < initState.costMatrix.Count; i++)
            {
                //give the matrix and the row to investigate
                int next = findNextCity(initState.costMatrix, i);
                if (next != -1)
                {
                    //we need to add the beggining city
                    initState.cityIndexes.Add(i);
                    //and the next city that was just found
                    initState.cityIndexes.Add(next);
                    break;
                }
            }

            State bssf = BandB(initState);

            //throw the bssf on the screen







            //int x;
            //Route = new ArrayList();
            // this is the trivial solution. 
            //for (x = 0; x < Cities.Length; x++)
            //{
            //    Route.Add(Cities[Cities.Length - x - 1]);
            //}
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
           // bssf = new TSPSolution(Route);
            // update the cost of the tour. 
            Program.MainForm.tbCostOfTour.Text = " " + bssf.cost;
            // do a refresh. 
            Program.MainForm.Invalidate();
        }

        private TSPSolution getInitialSolution()
        {
            List<City> thePath = new List<City>();
            List<City> gonnaDoThat = new List<City>();

            for (int i = 0; i < Cities.Length; i++) //put the cities into a list that is more easily manipulated
            {
                gonnaDoThat.Add(Cities[i]);
            }

            thePath.Add(gonnaDoThat[0]);    //start at the first city
            gonnaDoThat.Remove(gonnaDoThat[0]);     //remove first city from cities to be searched

            while (gonnaDoThat.Count != 0)
            {
                int minCostIndex = findMinIndex(gonnaDoThat, thePath[thePath.Count - 1]); //find the index of the closest city
                thePath.Add(gonnaDoThat[minCostIndex]);                                     //add closest city to path
                gonnaDoThat.Remove(gonnaDoThat[minCostIndex]);                              //remove that city from cities still to be searched
            }

            ArrayList path = new ArrayList();
            for (int i = 0; i < thePath.Count; i++)
            {
                path.Add(thePath[i]);
            }
            TSPSolution solution = new TSPSolution(path);

            return solution;
        }

        private int findMinIndex(List<City> cities, City currentCity)
        {
            int min = int.MaxValue;
            int minIndex=0;
            for (int i = 0; i < cities.Count; i++)
            {
                if (currentCity.costToGetTo(cities[i]) < min)
                    minIndex = i;
            }

            return minIndex;
        }

        private int findNextCity(List<List<double>> list, int i)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void reduceMatrix(List<List<double>> matrix, double minValue)
        {
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    if(matrix[i][j] != int.MaxValue)
                        matrix[i][j] -= minValue;
                }
            }
        }

        
        private int checkFor0(List<List<double>> matrix)
        {
            //check each col and each row if there is a 0 in it. if not we need to reduce that col or row 
            //by that smallest number return it?

            double rowMin;
            double colMin;
            int runningTotal = 0;
            for (int i = 0; i < matrix.Count; i++)
            {
                rowMin = int.MaxValue;
                colMin = int.MaxValue;
                for (int j = 0; j < matrix[i].Count; j++)
                {
                    if (matrix[i][j] < rowMin)
                        rowMin = matrix[i][j];

                    if (matrix[j][i] < colMin)
                        colMin = matrix[j][i];
                }

                //since the intersection is always int.max we dont have to worry about any changes in a row or col 
                //affecting the col or row respectivly.
                if (rowMin <= 0)
                {
                    for (int j = 0; j < matrix[i].Count; j++)
                    {
                        if (matrix[i][j] <= int.MaxValue)
                            matrix[i][j] -= rowMin;
                    }
                }
                if (colMin <= 0)
                {
                    for (int j = 0; j < matrix[i].Count; j++)
                    {
                        if (matrix[j][i] <= int.MaxValue)
                            matrix[j][i] -= colMin;
                    }
                }

                runningTotal += (int)rowMin + (int)colMin;
            }

            return runningTotal;
        }

        private State BandB(State input)
        {

            State state = new State(input);

            State bssf = quickSolution(input);

           //myCompare compare = new myCompare(State);
            IntervalHeap<State> agenda = new IntervalHeap<State>(new myCompare());

            agenda.Add(input);

            while (!agenda.IsEmpty && timer.Enabled && bssf.cost != agenda.FindMin().bound)
            {
                State temp = agenda.FindMin();
                agenda.DeleteMin();

                if (temp.bound < bssf.cost)
                {
                    List<State> children = findSuccessors(temp,bssf.cost);

                    foreach (State child in children)
                    {
                        if (timer.Enabled)
                            break; //were done return the BSSF
                        if (child.bound < bssf.cost)
                            bssf = child;
                        else
                            agenda.Add(child);
                    }
                }
            }

            return bssf;

        }

        private List<State> findSuccessors(State temp, int bssfCost)
        {
            
            List<State> children = new List<State>();

            int rowToTry = temp.cityIndexes[temp.cityIndexes.Count - 1];
            //get the last aded city. it is here we will look for the next 0
            for (int i = 0; i < temp.costMatrix.Count; i++)
            {                
                if(temp.costMatrix[rowToTry][i] < int.MaxValue)
                {
                    State newChild = new State(temp);
                    newChild.cityIndexes.Add(i);
                    newChild.cost += checkFor0(newChild.costMatrix);

                    if (newChild.cost <= bssfCost) //THIS SHOULD BE THE BOUND I THINK!!!!!!!!!!!!!!!!!!!
                       children.Add(newChild);
                }
                else
                    throw new Exception("Can't find a next city to go to");
            }

            return children;
        }

        private State quickSolution(State input)
        {
            throw new NotImplementedException();
        }

        private class myCompare : System.Collections.Generic.Comparer<State>
        {

            //this favors states that are further along in their journey. AKA they have fewer cities left to visit
            public override int Compare(State x, State y)
            {
                if (x.cityIndexes.Count == y.cityIndexes.Count)
                {
                    if (x.cost < y.cost)
                        return -1;
                    else if (x.cost > y.cost)
                        return 1;
                    else //equal
                        return 0;
                }
                else if (x.cityIndexes.Count < y.cityIndexes.Count)
                    return -1;
                else
                    return 1;
            }
        }





        private class State
        {
            public List<List<double>> costMatrix;
            public int cost;
            public double bound;
            public List<int> cityIndexes;


            public State()
            {
                costMatrix = new List<List<double>>();
                cityIndexes = new List<int>();
            }

            public State(State state)
                : this()
            {
                this.cost = state.cost;
                this.bound = state.bound;

                //do a deep copy of the cost matrix
                for (int i = 0; i < state.costMatrix.Count; i++)
                {
                    this.costMatrix[i] = new List<double>();
                    for (int j = 0; j < state.costMatrix[i].Count; j++)
                    {
                        this.costMatrix[i].Add(state.costMatrix[i][j]);
                    }
                }

                for (int i = 0; i < state.cityIndexes.Count; i++)
                {
                    cityIndexes.Add(state.cityIndexes[i]);
                }

            }
        }
    }
}
