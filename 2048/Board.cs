using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// see discussion at http://stackoverflow.com/questions/22342854/what-is-the-optimal-algorithm-for-the-game-2048

namespace Lomont.WPF2048
{

    struct MoveResult
    {
        public int srcX, srcY; // 
        public int dstX, dstY; // 
        public int newVal;
        public Special SpecialAction;
        public enum Special
        {
            Move,
            Merge,
            Reset,
            Over,
            New // piece placed on src of newVal
        }
    }

    public enum Move
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Store a board, apply moves, output results
    /// </summary>
    class Board
    {
        public int width { get; private set; }
        public int height { get; private set; }

        private bool[,] mergeArray;

        //private int tagVal = 1 << 7;
        private int bitsPerCell;

        /// <summary>
        /// Create sizex by sizey board
        /// </summary>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        public Board(int sizex, int sizey, int seed = 0)
        {
            if (seed != 0)
                rand = new Random(seed);
            else
                rand = new Random();
            width = sizex;
            height = sizey;
            bitsPerCell = 8; // max size 8
            mergeArray = new bool[width,height];
            Reset();
        }


        // pack state in two 64 bit ints as:
        // 128 bits/16 cells = 8 bits per cell
        // value 0 = empty, else value n = tile 2^n in cell.
        ulong boardState1, boardState2;

        public void GetState(out ulong s1, out ulong s2)
        {
            s1 = boardState1;
            s2 = boardState2;
        }
        public void SetState(ulong s1, ulong s2)
        {
            boardState1 = s1;
            boardState2 = s2;
        }

        public void SetCell(int i, int j, int val)
        {
            var index = i + j*width;
            if (index >= 8)
            {
                index -= 8;
                index *= 8;
                boardState2 &= ~(0xFFUL<<index);
                boardState2 |= ((ulong)val & 255) << index;
            }
            else
            {
                index *= 8;
                boardState1 &= ~(0xFFUL << index);
                boardState1 |= ((ulong)val & 255) << index;
            }
        }

        public int GetCell(int i, int j)
        {
            var index = i + j*width;
            var val2 = 0;
            if (index >= 8)
            {
                index -= 8;
                index *= 8;
                val2 = (int)(boardState2>> index)&255;
            }
            else
            {
                index *= 8;
                val2 = (int)(boardState1 >> index) & 255;
            }
            return val2;
        }

        /// <summary>
        /// Reset board
        /// </summary>
        public void Reset()
        {
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                    SetCell(i, j, 0);
            OnMoveEvent(0, 0, 0, 0, 0, MoveResult.Special.Reset);
            AddRandomTile();
        }

        Random rand;

        int EmptyCellCount()
        {
            var count = 0;
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                    if (GetCell(i, j) == 0)
                        count++;
            return count;
        }
        
        /// <summary>
        /// Add the random tile
        /// </summary>
        public void AddRandomTile()
        {
            var emptyCellCount = EmptyCellCount();
            if (emptyCellCount == 0)
            {   
                OnMoveEvent(0,0,0,0,0,MoveResult.Special.Over);
                return;
            }
            var entry = rand.Next(emptyCellCount);
            var i = 0;
            while (entry >= 0)
            {
                if (GetCell(i % width, i / width) == 0)
                    entry--;
                if (entry >= 0)
                    i++;
            }


            // original uses this distribution
            var value = rand.NextDouble() < 0.9 ? 1 : 2; // 4 10%, 2 90%
            SetCell(i % width, i / width, value);
            OnMoveEvent(i%width,i/width,0,0,value,MoveResult.Special.New);
        }

        // shift all pieces in dx,dy direction
        // exactly one is 0, other is either +-1
        // return true if anything moved
        // does not add new random tile
        bool DoMove(int dx, int dy)
        {
            var moved = false;
            if (dy == 0 && (dx==-1 || dx==1))
            {
                // x range, inclusive
                var xmin = dx == 1 ? width - 2 : 1;
                var xmax = dx == 1 ? 0 : width - 1;
                for (var y = 0; y < height; ++y)
                {
                    var merged = false;
                    for (var x = xmin; x != xmax - dx; x -= dx)
                        if (GetCell(x, y) != 0)
                            moved |= SlideTile(x, y, dx, dy, ref merged);
                }
            }
            else if (dx ==0 && (dy == 1 || dy==-1))
            {
                // y range, inclusive
                var ymin = dy == 1 ? height - 2 : 1;
                var ymax = dy == 1 ? 0 : height - 1;
                for (var x = 0; x < width; ++x)
                {
                    var merged = false;
                    for (var y = ymin; y != ymax - dy; y -= dy)
                        if (GetCell(x, y) != 0)
                            moved |= SlideTile(x, y, dx, dy, ref merged);
                }
            }
            Untag();
            return moved;
        }

        /// <summary>
        /// Try to slide the tile in the given direction
        /// Return true if moved
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        private bool SlideTile(int x, int y, int dx, int dy, ref bool merged)
        {
            var dstx = x;
            var dsty = y;
            var val = GetCell(x, y);

            var moved = false;

            do
            {
                moved = false; // assume this
                var nx = dstx + dx;
                var ny = dsty + dy;
                if (0 <= nx && 0 <= ny && nx < width && ny < height)
                {
                    var nbrVal = GetCell(nx, ny);
                    if (nbrVal == 0)
                    {
                        dstx = nx;
                        dsty = ny;
                        moved = true;
                    }
                    else if (val == nbrVal && mergeArray[nx,ny] == false)
                    {
                        // merge and return 
                        merged = true;
                        dstx = nx;
                        dsty = ny;
                        SetCell(x,y,0);
                        SetCell(dstx, dsty, val + 1);
                        mergeArray[dstx, dsty] = true;
                        OnMoveEvent(x, y, dstx, dsty, val + 1, MoveResult.Special.Merge);
                        return true;
                    }
                }

            } while (moved);


            if (dstx != x || dsty != y)
            {
                SetCell(x, y, 0);
                SetCell(dstx, dsty, val);
                OnMoveEvent(x, y, dstx, dsty, val, MoveResult.Special.Move);
                return true;
            }

        return false;
        }

        private void Untag()
        {
            for (var i = 0; i < width; ++i)
                for (var j = 0; j < height; ++j)
                    mergeArray[i, j] = false;
        }

        /// <summary>
        /// Apply move and add random tile if move allowed (optional)
        /// Return true if move done
        /// </summary>
        /// <param name="move"></param>
        public bool DoMove(Move move, bool addRandomTile = true)
        {
            var moved = false;

            switch (move)
            {
                case Move.Up:
                    moved = DoMove(0, -1);
                    break;
                case Move.Down:
                    moved = DoMove(0,  1);
                    break;
                case Move.Right:
                    moved = DoMove(1, 0);
                    break;
                case Move.Left:
                    moved = DoMove(-1, 0);
                    break;
            }

            if (moved && addRandomTile)
                AddRandomTile();
            return moved;
        }

        public class MoveEventArgs : EventArgs
        {
            public MoveResult result;

        }


        public event EventHandler<MoveEventArgs> RaiseMoveEvent;

        protected void OnMoveEvent(int srcx, int srcy, int dstx, int dsty, int logVal, MoveResult.Special special)
        {
            var e = RaiseMoveEvent;
            if (e != null)
            {
                e(this,new MoveEventArgs
                {
                    result = new MoveResult
                    {
                        srcX=srcx,srcY=srcy,
                        dstX = dstx, dstY = dsty,
                        newVal = 1<<logVal,
                        SpecialAction = special
                    }
                });
            }
        }


    }
}
