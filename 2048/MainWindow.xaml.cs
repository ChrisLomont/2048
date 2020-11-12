using System.Windows;
using System.Windows.Input;

namespace Lomont.WPF2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            var vm = DataContext as GameModel;
            if (vm == null) return;
            if (e.Key == Key.Up)
                vm.Board.DoMove(Move.Up);
            if (e.Key == Key.Down)
                vm.Board.DoMove(Move.Down);
            if (e.Key == Key.Left)
                vm.Board.DoMove(Move.Left);
            if (e.Key == Key.Right)
                vm.Board.DoMove(Move.Right);
            if (e.Key == Key.N)
                vm.Board.Reset();
            if (e.Key == Key.A)
                vm.MoveToggleAnimation();
            if (e.Key == Key.S)
                vm.MoveToggleSolver();
            if (e.Key == Key.H)
                vm.AutoMove();
        }
    }
}
