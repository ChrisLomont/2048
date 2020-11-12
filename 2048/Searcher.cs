using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Lomont.WPF2048
{
    /// <summary>
    /// Perform expectimax on the 2048 game.
    /// Expectimax is like minimax, except it tracks probability on each node. 
    /// </summary>
    class Searcher
    {
        Board board;
        public Move GenerateMove(Board board)
        {
            this.board = new Board(board.width, board.height);
            ulong s1, s2;
            board.GetState(out s1, out s2);
            this.board.SetState(s1,s2);

            var maxDepth = 3;
            var minProbability = 1e-6;

            nodesSearched = 0;
            depthCutoffs = 0;
            probabilityCutoffs = 0;

            Move bestMove;
            Recurse(0, maxDepth, 1.0, minProbability, out bestMove);
            return bestMove;
        }

        public ulong nodesSearched;
        public ulong depthCutoffs;
        public ulong probabilityCutoffs;

        // 
        private List<Move> moves = new List<Move> {Move.Up, Move.Down, Move.Left, Move.Right};
        

        // return best score
        double Recurse(
            int curDepth, int maxDepth, 
            double curProbability, double minProbability, 
            out Move bestMove
            )
        {
            ++nodesSearched;
            if (curDepth >= maxDepth || curProbability < minProbability)
            {
                if (curDepth >= maxDepth)
                    depthCutoffs ++;
                else
                    probabilityCutoffs++;
                bestMove = Move.Down; // does not matter
                return ScoreBoard();
            }

            ulong state1, state2; // back current board state here
            board.GetState(out state1, out state2);

            // these two variables used to score this node
            var directChildrenCount = 0;
            var weightedScore = 0.0;

            // track best seen
            bestMove = moves[0];
            var bestScore = -1.0;

            // space to track empty cell coordinates
            var empty = new List<int>();

            Move tempMove;

            foreach (var move in moves)
            {
                if (board.DoMove(move,false))
                {
                    GetEmptyLocations(empty);

                    if (empty.Count == 0)
                    {
                        weightedScore = ScoreBoard();
                        directChildrenCount = 1;
                    }
                    else
                    {
                        // foreach empty location, compute the best score
                        for (var j = 0; j < empty.Count; j += 2)
                        {
                            var x = empty[j];
                            var y = empty[j + 1];
                            board.SetCell(x, y, 2); // 90% chance of 2
                            weightedScore += 0.90 * Recurse(curDepth + 1, maxDepth, curProbability * 0.90, minProbability, out tempMove);
                            board.SetCell(x, y, 4); // 10% chance of 4
                            weightedScore += 0.10*Recurse(curDepth+1, maxDepth, curProbability*0.10, minProbability, out tempMove);
                            board.SetCell(x, y, 0); // restore as empty
                            directChildrenCount += 1; // 
                        }
                    }
                    board.SetState(state1,state2); // undo move

                    weightedScore /= directChildrenCount;
                    if (weightedScore > bestScore)
                    {
                        bestScore = weightedScore;
                        bestMove = move;
                    }
                }
            }
            return bestScore;
        }

        private List<int> emptyScore = new List<int>(); 
        private void GetEmptyLocations(List<int> empty )
        {
            empty.Clear();
            for (var i =0 ; i < 4; ++i)
                for (var j = 0; j < 4; ++j)
                    if (board.GetCell(i, j) == 0)
                    {
                        empty.Add(i);
                        empty.Add(j);
                    }
        }

        /// <summary>
        /// A score is a non-negative number representing the quality of 
        /// the board position. Larger is better.
        /// 
        /// </summary>
        /// <returns></returns>
        double ScoreBoard()
        {
            var w = board.width;
            var h = board.height;

            GetEmptyLocations(emptyScore);
            var emptyCountPartial = (double)emptyScore.Count;

            var ptPartial = 0.0;
            var chainPartial = 0.0;
            var maxPiece = 0;
            for (var i = 0; i < w; ++i)
                for (var j = 0; j < h; ++j)
                {
                    var val = board.GetCell(i, j);
                    maxPiece = Math.Max(val, maxPiece);
                    ptPartial += 1<<val;
                    if (i > 0 && board.GetCell(i - 1, j) == val + 1)
                        chainPartial += val;
                    if (i < w-1 && board.GetCell(i + 1, j) == val + 1)
                        chainPartial += val;
                    if (j > 0 && board.GetCell(i, j - 1) == val + 1)
                        chainPartial += val;
                    if (j < h - 1 && board.GetCell(i, j+1) == val + 1)
                        chainPartial += val;
                }

            var maxCornerPartial = (
                board.GetCell(0, 0) == maxPiece || board.GetCell(0, w - 1) == maxPiece ||
                board.GetCell(h - 1, 0) == maxPiece || board.GetCell(w - 1, h - 1) == maxPiece
                )
                ? 50.0
                : 0.0;


            var score = emptyCountPartial + ptPartial + 40.0*chainPartial + maxCornerPartial;
            return score;
        }


    }
}
