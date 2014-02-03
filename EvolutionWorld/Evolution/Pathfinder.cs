using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Evolution
{
    class Pathfinder
    {
		class CompleteSquare
		{
			public int DistanceSteps { get; set; }

			public bool IsPath { get; set; }
		}
        /*
         * 
         * Movements is an array of various directions.
         * 
         * */
        Location[] _movements;

        /*
         * 
         * Squares is an array of square objects.
         * 
         * */
		int[,] _contentSquares;
		CompleteSquare[,] _completeSquares;
		public CompleteSquare[,] CompleteSquares
        {
            get { return _completeSquares; }
            set { _completeSquares = value; }
        }

        public Pathfinder(int mapWidth, int mapHeigth, int[,] map)
        {
			_completeSquares = new CompleteSquare[mapWidth, mapHeigth];
			_contentSquares = map;
            InitMovements(4);
            ClearSquares();
        }

        public void InitMovements(int movementCount)
        {
            /*
             * 
             * Just do some initializations.
             * 
             * */
            if (movementCount == 4)
            {
                _movements = new Location[]
                {
                    new Location(0, -1),
                    new Location(1, 0),
                    new Location(0, 1),
                    new Location(-1, 0)
                };
            }
            else
            {
                _movements = new Location[]
                {
                    new Location(-1, -1),
                    new Location(0, -1),
                    new Location(1, -1),
                    new Location(1, 0),
                    new Location(1, 1),
                    new Location(0, 1),
                    new Location(-1, 1),
                    new Location(-1, 0)
                };
            }
        }

        public void ClearSquares()
        {
            /*
             * 
             * Reset every square.
             * 
             * */
            foreach (Location point in AllSquares())
            {
                _completeSquares[point.X, point.Y] = new CompleteSquare();
            }
        }

        public void ClearLogic()
        {
            /*
             * 
             * Reset some information about the squares.
             * 
             * */
            foreach (Location point in AllSquares())
            {
                int x = point.X;
                int y = point.Y;
                _completeSquares[x, y].DistanceSteps = 10000;
                _completeSquares[x, y].IsPath = false;
            }
        }

        public void Pathfind(Location startingLocation, Location targetLocation)
        {
			int heroX = startingLocation.X;
			int heroY = startingLocation.Y;
            if (heroX == -1 || heroY == -1)
            {
                return;
            }
            /*
             * 
             * Hero starts at distance of 0.
             * 
             * */
            _completeSquares[heroX, heroY].DistanceSteps = 0;

            while (true)
            {
                bool madeProgress = false;

                /*
                 * 
                 * Look at each square on the board.
                 * 
                 * */
                foreach (Location mainPoint in AllSquares())
                {
                    int x = mainPoint.X;
                    int y = mainPoint.Y;

                    /*
                     * 
                     * If the square is open, look through valid moves given
                     * the coordinates of that square.
                     * 
                     * */
                    if (SquareOpen(x, y))
                    {
                        int passHere = _completeSquares[x, y].DistanceSteps;

                        foreach (Location movePoint in ValidMoves(x, y))
                        {
                            int newX = movePoint.X;
                            int newY = movePoint.Y;
                            int newPass = passHere + 1;

							if (_completeSquares[newX, newY].DistanceSteps > newPass)
                            {
								_completeSquares[newX, newY].DistanceSteps = newPass;
                                madeProgress = true;
                            }
                        }
                    }
                }
                if (!madeProgress)
                {
                    break;
                }
            }
        }

        static private bool ValidCoordinates(int x, int y)
        {
            /*
             * 
             * Our coordinates are constrained between 0 and 14.
             * 
             * */
            if (x < 0)
            {
                return false;
            }
            if (y < 0)
            {
                return false;
            }
            if (x > 14)
            {
                return false;
            }
            if (y > 14)
            {
                return false;
            }
            return true;
        }

        private bool SquareOpen(int x, int y)
        {
			return _contentSquares[x, y] == 0;
        }


		public void HighlightPath()
		{
			/*
			 * 
			 * Mark the path from monster to hero.
			 * 
			 * */
			int heroX = startingLocation.X;
			int heroY = startingLocation.Y;
			if (pointX == -1 && pointY == -1)
			{
				return;
			}

			while (true)
			{
				/*
				 * 
				 * Look through each direction and find the square
				 * with the lowest number of steps marked.
				 * 
				 * */
				Point lowestPoint = Point.Empty;
				int lowest = 10000;

				foreach (Point movePoint in ValidMoves(pointX, pointY))
				{
					int count = _squares[movePoint.X, movePoint.Y].DistanceSteps;
					if (count < lowest)
					{
						lowest = count;
						lowestPoint.X = movePoint.X;
						lowestPoint.Y = movePoint.Y;
					}
				}
				if (lowest != 10000)
				{
					/*
					 * 
					 * Mark the square as part of the path if it is the lowest
					 * number. Set the current position as the square with
					 * that number of steps.
					 * 
					 * */
					_squares[lowestPoint.X, lowestPoint.Y].IsPath = true;
					pointX = lowestPoint.X;
					pointY = lowestPoint.Y;
				}
				else
				{
					break;
				}

				if (_squares[pointX, pointY].ContentCode == SquareContent.Hero)
				{
					/*
					 * 
					 * We went from monster to hero, so we're finished.
					 * 
					 * */
					break;
				}
			}
		}

        private static IEnumerable<Location> AllSquares()
        {
            /*
             * 
             * Return every point on the board in order.
             * 
             * */
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    yield return new Location(x, y);
                }
            }
        }

        private IEnumerable<Location> ValidMoves(int x, int y)
        {
            /*
             * 
             * Return each valid square we can move to.
             * 
             * */
            foreach (Location movePoint in _movements)
            {
                int newX = x + movePoint.X;
                int newY = y + movePoint.Y;

                if (ValidCoordinates(newX, newY) &&
                    SquareOpen(newX, newY))
                {
                    yield return new Location(newX, newY);
                }
            }
        }
    }
}
