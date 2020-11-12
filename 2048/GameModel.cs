using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lomont.WPF2048
{
    class GameModel :NotifiableBase
    {

        public GameModel()
        {
            Tiles = new ObservableCollection<Tile>();
            Messages = new ObservableCollection<string>();
            Board = new Board(4,4,1234);
            Board.RaiseMoveEvent += BoardOnRaiseMoveEvent;
            Board.Reset();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += (o,e) => AutoTick();
        }


        DispatcherTimer timer = new DispatcherTimer();

        void BoardOnRaiseMoveEvent(object sender, Board.MoveEventArgs moveEventArgs)
        {
            var r = moveEventArgs.result;
            Tile t;
            switch (r.SpecialAction)
            {
                case MoveResult.Special.Move:
                    t = FindTile(r.srcX,r.srcY);
                    t.PositionX = r.dstX;
                    t.PositionY = r.dstY;
                    break;
                case MoveResult.Special.Merge:
                    t = FindTile(r.dstX,r.dstY);
                    Tiles.Remove(t);
                    t = FindTile(r.srcX,r.srcY);
                    t.PositionX = r.dstX;
                    t.PositionY = r.dstY;
                    t.Value = r.newVal;
                    break;
                case MoveResult.Special.New:
                    Tiles.Add(new Tile(r.newVal,r.srcX,r.srcY));
                    break;
                case MoveResult.Special.Over:
                    break;
                case MoveResult.Special.Reset:
                    Tiles.Clear();
                    break;
            }
        }

        private Tile FindTile(int x, int y)
        {
            foreach (var t in Tiles)
            {
                if (t.PositionX == x && t.PositionY == y)
                    return t;
            }
            throw new Exception("Tile missing");
        }

        public ObservableCollection<Tile> Tiles { get; private set; }

        public Board Board { get; private set; }

        public void MoveToggleAnimation()
        {

        }
        public void MoveToggleSolver()
        {
            timer.IsEnabled = !timer.IsEnabled;
        }

        private void AutoTick()
        {
            AutoMove();
        }

        public ObservableCollection<string> Messages { get; private set; }


        public void AutoMove()
        {
            var searcher = new Searcher();
            var move = searcher.GenerateMove(Board);
            Board.DoMove(move);
            Messages.Add(String.Format("{0} {1} {2}",
                searcher.nodesSearched,
                searcher.depthCutoffs,
                searcher.probabilityCutoffs));
        }
    }
}
