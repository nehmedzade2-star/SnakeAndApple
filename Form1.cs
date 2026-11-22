using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace SnakeAndApple;

public class Form1 : Form
{
    private const int Cell = 20;
    private const int Columns = 34;
    private const int Rows = 24;

    private readonly List<Point> snake = new();
    private readonly Timer gameTimer = new() { Interval = 120 };
    private readonly Timer itemTimer = new() { Interval = 5000 };
    private readonly Random random = new();

    private Point item;
    private bool itemIsBomb;
    private int itemTime;
    private int score;
    private int direction = 1;
    private int nextDirection = 1;
    private bool gameOver;

    public Form1()
    {
        Text = "Snake and apple";
        ClientSize = new Size(Columns * Cell, Rows * Cell + 44);
        BackColor = Color.White;
        DoubleBuffered = true;
        KeyPreview = true;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        gameTimer.Tick += GameTimer_Tick;
        itemTimer.Tick += (s, e) => SpawnItem();
        KeyDown += Form1_KeyDown;
        StartGame();
    }

    private void StartGame()
    {
        snake.Clear();
        snake.Add(new Point(10, 12));
        snake.Add(new Point(9, 12));
        snake.Add(new Point(8, 12));
        direction = 1;
        nextDirection = 1;
        score = 0;
        gameOver = false;
        SpawnItem();
        gameTimer.Start();
        itemTimer.Start();
        Invalidate();
    }

    private void SpawnItem()
    {
        do
        {
            item = new Point(random.Next(Columns), random.Next(Rows));
        } while (snake.Contains(item));

        itemIsBomb = random.Next(100) < 30;
        itemTimer.Stop();
        itemTimer.Start();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (gameOver) return;

        direction = nextDirection;
        Point head = snake[0];
        Point newHead = direction switch
        {
            0 => new Point(head.X, head.Y - 1),
            1 => new Point(head.X + 1, head.Y),
            2 => new Point(head.X, head.Y + 1),
            _ => new Point(head.X - 1, head.Y)
        };

        if (newHead.X < 0 || newHead.X >= Columns || newHead.Y < 0 || newHead.Y >= Rows || snake.Contains(newHead))
        {
            EndGame();
            return;
        }

        snake.Insert(0, newHead);
        if (newHead == item)
        {
            if (itemIsBomb)
            {
                EndGame();
                return;
            }

            score++;
            gameTimer.Interval = Math.Max(55, 120 - score * 3);
            SpawnItem();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }

        Invalidate();
    }


    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (gameOver)
        {
            if (e.KeyCode == Keys.Enter) StartGame();
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.W:
                if (direction != 2) nextDirection = 0;
                break;
            case Keys.Right:
            case Keys.D:
                if (direction != 3) nextDirection = 1;
                break;
            case Keys.Down:
            case Keys.S:
                if (direction != 0) nextDirection = 2;
                break;
            case Keys.Left:
            case Keys.A:
                if (direction != 1) nextDirection = 3;
                break;
        }
    }

    private void EndGame()
    {
        gameOver = true;
        gameTimer.Stop();
        itemTimer.Stop();
        Invalidate();
        MessageBox.Show($"Oyun bitdi!\nTopladığınız xal: {score}\n\nYeni oyun üçün Enter düyməsini basın.",
            "Snake and apple", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (Font font = new("Segoe UI", 12, FontStyle.Bold))
        using (Brush textBrush = new SolidBrush(Color.FromArgb(45, 45, 45)))
            g.DrawString($"Xal: {score}      İdarəetmə: ox düymələri / WASD", font, textBrush, 12, Rows * Cell + 10);

        using (Pen gridPen = new(Color.FromArgb(245, 245, 245)))
        {
            for (int x = 0; x <= Columns; x++) g.DrawLine(gridPen, x * Cell, 0, x * Cell, Rows * Cell);
            for (int y = 0; y <= Rows; y++) g.DrawLine(gridPen, 0, y * Cell, Columns * Cell, y * Cell);
        }

        if (itemIsBomb) DrawBomb(g, item);
        else DrawApple(g, item);

        for (int i = snake.Count - 1; i >= 0; i--)
        {
            Rectangle rect = new(snake[i].X * Cell + 2, snake[i].Y * Cell + 2, Cell - 4, Cell - 4);
            using Brush bodyBrush = new SolidBrush(i == 0 ? Color.FromArgb(91, 190, 45) : Color.FromArgb(142, 218, 74));
            g.FillEllipse(bodyBrush, rect);
            g.DrawEllipse(Pens.DarkGreen, rect);
        }

        if (snake.Count > 0)
        {
            Point head = snake[0];
            int eyeX = head.X * Cell + (direction == 1 ? 13 : direction == 3 ? 5 : 10);
            int eyeY = head.Y * Cell + (direction == 2 ? 13 : direction == 0 ? 5 : 10);
            g.FillEllipse(Brushes.Black, eyeX, eyeY, 3, 3);
        }
    }

    private void DrawApple(Graphics g, Point p)
    {
        int x = p.X * Cell + 2, y = p.Y * Cell + 3;
        using Brush appleBrush = new SolidBrush(Color.FromArgb(240, 52, 63));
        g.FillEllipse(appleBrush, x, y + 3, 16, 15);
        g.FillEllipse(appleBrush, x + 1, y, 8, 13);
        g.FillEllipse(appleBrush, x + 7, y, 8, 13);
        g.DrawArc(Pens.DarkRed, x, y + 3, 16, 15, 0, 180);
        g.DrawLine(new Pen(Color.SaddleBrown, 2), x + 8, y + 1, x + 9, y - 3);
        g.FillEllipse(Brushes.ForestGreen, x + 9, y - 2, 8, 5);
        g.FillEllipse(Brushes.White, x + 11, y + 4, 3, 4);
    }

    private void DrawBomb(Graphics g, Point p)
    {
        int x = p.X * Cell + 2, y = p.Y * Cell + 2;
        g.FillEllipse(Brushes.DimGray, x + 1, y + 3, 16, 16);
        g.DrawEllipse(Pens.Black, x + 1, y + 3, 16, 16);
        g.DrawLine(new Pen(Color.Black, 3), x + 10, y + 2, x + 14, y - 2);
        g.DrawLine(new Pen(Color.OrangeRed, 2), x + 14, y - 2, x + 17, y - 5);
        g.FillEllipse(Brushes.Gold, x + 15, y - 7, 4, 4);
        g.FillEllipse(Brushes.LightGray, x + 4, y + 6, 4, 4);
    }
}
