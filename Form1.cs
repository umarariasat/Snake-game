using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace finalgame
{
    public partial class Form1 : Form
    {

        enum Direction { Up, Down, Left, Right }

        const int cols = 50;
        const int rows = 40;
        const int cellsize = 20;
        private int gameSpeed = 200; // default 200ms interval
      
     


        private List<Point> snake = null!; // suppress "must contain non-null value" warning
        private Point food;
        private Direction dir = Direction.Right;
        private Direction nextdir = Direction.Right;
        private int score = 0;
        private bool isrunning = false;
        private bool ispaused = false;

        private Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            buttonstart.TabStop = false;
            buttonrestart.TabStop = false;
            buttonpause.TabStop = false;
            panel1.TabStop = false;
            this.ActiveControl = null; // remove focus from any control



            panel1.Width = cols * cellsize;
            panel1.Height = rows * cellsize;
           

            buttonstart.Click += buttonstart_Click;
            buttonrestart.Click += buttonrestart_Click;
            buttonpause.Click += buttonpause_Click;

            timer1.Tick += timer1_Tick;
            panel1.Paint += panel1_Paint;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;


            InitializeGame();
        }

        private void InitializeGame()
        {
            snake = new List<Point>
            {
                new Point(cols/2 - 1, rows/2),
                new Point(cols/2, rows/2),
                new Point(cols/2 + 1, rows/2)
            };

            dir = Direction.Right;
            nextdir = Direction.Right;
            score = 0;
            isrunning = false;
            ispaused = false;

            PlaceFood();
            UpdateLabels();
            panel1.Invalidate();
            buttonrestart.Visible = false; // Hide restart at first
            buttonstart.Visible = true;
            comboBoxlevel.Enabled = true;

        }

        private void SetLevel()
        {
            switch (comboBoxlevel.SelectedItem?.ToString())
            {
                case "Easy":
                    gameSpeed = 200; // 200 ms per move
                    break;
                case "Medium":
                    gameSpeed = 120; // 120 ms per move
                    break;
                case "Hard":
                    gameSpeed = 60; 
                    break;
                default:
                    gameSpeed = 200;
                    break;
            }

            timer1.Interval = gameSpeed;
        }

        private void StartGame()
        {
            if (isrunning) return;
            MessageBox.Show(
       "Instructions:\n\n" +
       "- Use  W/A/S/Z to move the snake\n" +
       "- Press Space to Pause/Resume\n" +
       "- Eat the red food to score points\n" +
       "- Avoid hitting the walls or yourself\n\n" +
       "Good Luck!",
       "Snake Game Instructions",
       MessageBoxButtons.OK,
       MessageBoxIcon.Information
   );
            InitializeGame();
            SetLevel();
            isrunning = true;
            ispaused = false;
            timer1.Start();
            comboBoxlevel.Enabled = !isrunning;
            this.ActiveControl = null;
            panel1.Focus();
            buttonstart.Visible = false;   
            buttonrestart.Visible = true;
            comboBoxlevel.Enabled = false;


        }

        private void RestartGame()
        {
            timer1.Stop();
            InitializeGame();
            SetLevel();
            isrunning = true;
            timer1.Start();
            comboBoxlevel.Enabled = !isrunning;
            this.ActiveControl = null;
            panel1.Focus();
        }

        private void TogglePause()
        {
            if (!isrunning) return;


            ispaused = !ispaused;
       
            if (ispaused) timer1.Stop(); else timer1.Start();
            buttonpause.Text = ispaused ? "Resume" : "Pause";
        }

        private void PlaceFood()
        {
            var emptyCells = new List<Point>();
            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                {
                    var p = new Point(x, y);
                    if (!snake.Contains(p)) emptyCells.Add(p);
                }

            if (emptyCells.Count == 0)
            {
                GameOver();
                return;
            }

            food = emptyCells[rnd.Next(emptyCells.Count)];
        }

        private void UpdateLabels()
        {
            Scorelabel.Text = "Score: " + score;
        }

        private void GameOver()
        {
            timer1.Stop();
            isrunning = false;
            MessageBox.Show("Game Over! Score: " + score);
        }

        private bool IsOpposite(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down) ||
                   (a == Direction.Down && b == Direction.Up) ||
                   (a == Direction.Left && b == Direction.Right) ||
                   (a == Direction.Right && b == Direction.Left);
        }

        // ✅ Event handlers with nullable sender
        private void timer1_Tick(object? sender, EventArgs e)
        {
            if (!IsOpposite(nextdir, dir))
                dir = nextdir;

            Point head = snake.Last();
            Point newHead = head;

            switch (dir)
            {
                case Direction.Up: newHead = new Point(head.X, head.Y - 1); break;
                case Direction.Down: newHead = new Point(head.X, head.Y + 1); break;
                case Direction.Left: newHead = new Point(head.X - 1, head.Y); break;
                case Direction.Right: newHead = new Point(head.X + 1, head.Y); break;
            }

            if (newHead.X < 0 || newHead.X >= cols || newHead.Y < 0 || newHead.Y >= rows)
            {
                GameOver();
                return;
            }

            bool eats = newHead == food;

            if (snake.Contains(newHead) && !(eats && newHead == snake.First()))
            {
                GameOver();
                return;
            }

            snake.Add(newHead);

            if (!eats)
                snake.RemoveAt(0);
            else
            {
                score += 10;
                UpdateLabels();
                PlaceFood();
            }

            panel1.Invalidate();
        }

        private void panel1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);
     
            g.Clear(Color.FromArgb(30, 30, 30)); 

            // Optional: draw grid lines
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 50))) // subtle grid
            {
                for (int x = 0; x <= cols; x++)
                    g.DrawLine(gridPen, x * cellsize, 0, x * cellsize, rows * cellsize);
                for (int y = 0; y <= rows; y++)
                    g.DrawLine(gridPen, 0, y * cellsize, cols * cellsize, y * cellsize);
            }


            Rectangle fr = new Rectangle(food.X * cellsize + 1,
                                         food.Y * cellsize + 1,
                                         cellsize - 2,
                                         cellsize - 2);
            g.FillEllipse(Brushes.Red, fr);

            for (int i = 0; i < snake.Count; i++)
            {
                var p = snake[i];
                Rectangle r = new Rectangle(
                    p.X * cellsize + 1,
                    p.Y * cellsize + 1,
                    cellsize - 2,
                    cellsize - 2
                );
                g.FillRectangle(i == snake.Count - 1 ? Brushes.Lime : Brushes.Green, r);
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // WASD / Z/A/S for Mac
                case Keys.W:
                case Keys.Up:     
                    nextdir = Direction.Up;
                    break;

                case Keys.Z:
                case Keys.Down:   
                    nextdir = Direction.Down;
                    break;

                case Keys.A:
                case Keys.Left:  
                    nextdir = Direction.Left;
                    break;

                case Keys.S:
                case Keys.Right:   
                    nextdir = Direction.Right;
                    break;

                case Keys.Space:  
                    TogglePause();
                    break;
            }
        }


        private void buttonstart_Click(object? sender, EventArgs e) => StartGame();
        private void buttonrestart_Click(object? sender, EventArgs e) => RestartGame();
        private void buttonpause_Click(object? sender, EventArgs e) => TogglePause();
    }
}
