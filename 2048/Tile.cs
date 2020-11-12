using System.Windows.Media;

namespace Lomont.WPF2048
{
    class Tile :NotifiableBase
    {


        public Tile(int score, double x, double y)
        {
            Value = score;
            PositionX = x;
            PositionY = y;
        }

        public Tile() : this(2,0,0)
        {
        }

        void SetColors()
        {

            switch (Value)
            {
                case 2:
                    Color = SolidBrush("#eee4da");
                    FontColor = Brushes.DarkSlateGray;
                    FontSize = 55;
                    break;
                case 4:
                    Color = SolidBrush("#ede0c8");
                    FontColor = Brushes.DarkSlateGray;
                    FontSize = 55;
                    break;
                case 8:
                    Color = SolidBrush("#f2b179");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 55;
                    break;
                case 16:
                    Color = SolidBrush("#f59563");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 55;
                    break;
                case 32:
                    Color = SolidBrush("#f67c5f");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 55;
                    break;
                case 64:
                    Color = SolidBrush("#f65e3b");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 55;
                    break;
                case 128:
                    Color = SolidBrush("#edcf72");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 45;
                    break;
                case 256:
                    Color = SolidBrush("#edcc61");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 45;
                    break;
                case 512:
                    Color = SolidBrush("#edc850");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 45;
                    break;
                case 1024:
                    Color = SolidBrush("#edc53f");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 35;
                    break;
                case 2048:
                    Color = SolidBrush("#edc22e");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 35;
                    break;
                case 4096:
                    Color = SolidBrush("#3c3a32");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 35;
                    break;
                default:
                    Color = SolidBrush("#3c3a32");
                    FontColor = SolidBrush("#f9f6f2");
                    FontSize = 30;
                    break;
            }
        }

        private Brush SolidBrush(string text)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(text));
        }


        #region Properties

        #region PositionX Property
        private double positionX = 0.0;
        /// <summary>
        /// Gets or sets tile x position.
        /// </summary>
        public double PositionX
        {
            get { return positionX; }
            set
            {
                // return true if there was a change.
                SetField(ref positionX, value);
            }
        }
        #endregion

        #region PositionY Property
        private double positionY = 0.0;
        /// <summary>
        /// Gets or sets tile y position.
        /// </summary>
        public double PositionY
        {
            get { return positionY; }
            set
            {
                // return true if there was a change.
                SetField(ref positionY, value);
            }
        }
        #endregion

        #region Color Property
        private Brush color = Brushes.AntiqueWhite;
        /// <summary>
        /// Gets or sets property description...
        /// </summary>
        public Brush Color
        {
            get { return color; }
            set
            {
                // return true if there was a change.
                SetField(ref color, value);
            }
        }
        #endregion

        #region FontColor Property
        private Brush fontColor = Brushes.DarkRed;
        /// <summary>
        /// Gets or sets property description...
        /// </summary>
        public Brush FontColor
        {
            get { return fontColor; }
            set
            {
                // return true if there was a change.
                SetField(ref fontColor, value);
            }
        }
        #endregion

        #region Value Property
        private int score = 0;
        /// <summary>
        /// Gets or sets the tile value.
        /// </summary>
        public int Value
        {
            get { return score; }
            set
            {
                // return true if there was a change.
                if (SetField(ref score, value))
                {
                    SetColors();
                }
            }
        }

        #endregion

        #region FontSize Property
        private double fontSize = 1.0;
        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public double FontSize
        {
            get { return fontSize; }
            set
            {
                // return true if there was a change.
                value /= 100; // todo - make in XAML?
                SetField(ref fontSize, value);
            }
        }
        #endregion

        #endregion


    }
}
