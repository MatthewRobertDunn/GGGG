using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GGGG;
using GGGG.Algorithms;
using GGGG.Interface;


public class MonteCarloEngine : IGoEngine, IAdvancedGoEngine
{
    private IFastBoard board = null;
    private UCTSearch search = new UCTSearch();
    private Action<string> debugLog;
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");
    private AlphaBetaSearch alphaBetaSearch = new AlphaBetaSearch();
    private FastMoveGenerator moveGenerator = new FastMoveGenerator();
    private int TimeLeftWhite = 0;
    private int TimeLeftBlack = 0;
    private int MaxGameTime = 0;
    private List<IFastBoard> PlayedBoards = new List<IFastBoard>();

    public bool ThinkInOpponent = false;
    public bool PassEarly = true;
    public bool DontResign = false;
    public TimeSpan DefaultTimePerMove = GGGGGoText.Properties.UctSearch.Default.TimeToThink;

    public MonteCarloEngine()
    {
        //NewPatternLibrary.LoadPatterns();
    }

    public void Start(uint size, float komi, uint level)
    {
        Log.InfoFormat("Starting with komi of {0} Think in opponent {1} pass early {2} ", komi, ThinkInOpponent, PassEarly);
        search.StopThinking();
        search = new UCTSearch(komi, PassEarly, DontResign);
        board = BoardFactory.GetBoard((int)size);
        Log.Info(board.ToString());
        TimeLeftBlack = MaxGameTime;
        TimeLeftWhite = MaxGameTime;
        PlayedBoards.Clear();
        PlayedBoards.Add(board);
    }

    public void Setup(GoColor color, uint x, uint y)
    {
        throw new NotImplementedException();
    }

    public GTextMove Reply(GoColor color)
    {
        BoardSquares player;
        int timeLeft = 0;
        if (color == GoColor.White)
        {
            player = BoardSquares.White;
            timeLeft = TimeLeftWhite * 1000;
        }
        else
        {
            player = BoardSquares.Black;
            timeLeft = TimeLeftBlack * 1000;
        }
        TimeSpan timeToThink = DefaultTimePerMove;

        if (timeLeft != 0)
        {
            var movesLeft = (double)moveGenerator.GetValidMoves(board, player).Count();

            if (movesLeft > 30)
                movesLeft = movesLeft * 0.5;
            else
                movesLeft = movesLeft + 3;

            timeToThink = TimeSpan.FromMilliseconds(timeLeft / ((double)movesLeft));


            Log.InfoFormat("Got time control, time to think is {0}", timeToThink);
        }


        var result = MonteCarloReply(color, timeToThink);

        if (!result.Pass && !result.Resign && !board.MoveIsValid(result.ChosenBoard, player))
        {
            Log.Info("Bot tried to return an invalid move!!");
        }
        
        board = result.ChosenBoard;
        PlayedBoards.Add(board);
        Log.Info(board.ToString());
        
        if (result.ChosenBoard == null)
        {
            Log.Info("Engine did not return a board, passing");
            return new GoMovePass();
        }

        if (result.Pass)
        {
            Log.Info("Engine chose to pass");
            search.StopThinking();
            return new GoMovePass();
        }

        if (result.Resign)
        {
            Log.Info("Engine chose to resign");
            search.StopThinking();
            return new GoMoveResign();
        }

        return new GTextMove(color, (uint)board.LastMoveXY().Item1 - 1, (uint)board.LastMoveXY().Item2 - 1);
    }

    private ChosenMoveInfo MonteCarloReply(GoColor color, TimeSpan length)
    {
        BoardSquares player;
        if (color == GoColor.Black)
            player = BoardSquares.Black;
        else
            player = BoardSquares.White;

        ////Before thinking check if there is a move good enough to play immediately
        //var testMove = search.GetBestMove(board, player);
        //if (testMove.WinPercentage > 0.98 && testMove.SampleCount > 1000)
        //{
        //    Log.Info("Move good enough to play without thinking!");
        //    return new Tuple<FastBoard, double>(testMove.ChosenBoard, 0.0);
        //}

        search.StartThinking(board, player);

        System.Threading.Thread.Sleep(length);

        if (!this.ThinkInOpponent)
            search.StopThinking();

        return search.GetBestMove(board, player);

    }

    public void Play(GTextMove move)
    {
        if (move is GoMoveResign)
        {
            search.StopThinking();
        }


        BoardSquares player;

        if (move.Color == GoColor.White)
            player = BoardSquares.White;
        else
            player = BoardSquares.Black;


        if (move is GoMovePass)
        {
            board = board.PlayMove(0, player);
        }
        else
        {
            board = board.PlayMove((int)move.X + 1, (int)move.Y + 1, player);
        }
        PlayedBoards.Add(board);
        Log.Info(board.ToString());
    }



    public void Undo()
    {
        if (PlayedBoards.Count > 1)
        {
            this.PlayedBoards.Remove(this.PlayedBoards.Last());
            board = this.PlayedBoards.Last();
        }

        Log.InfoFormat("Undo called new board is");
        Log.InfoFormat(board.ToString());
    }

    public void Quit()
    {
        Environment.Exit(0);
    }

    public string Name
    {
        get { return "Matty Go"; }
    }

    public string Version
    {
        get { return "V0.1"; }
    }


    public Action<string> DebugMessageHandler
    {
        set { debugLog = value; }
    }

    public void SetTimeSettings(int seconds)
    {
        this.MaxGameTime = seconds;
    }

    public void SetTimeLeft(GoColor color, int p)
    {
        Log.InfoFormat("Time left called {0} {1}", color, p);
        if (color == GoColor.White)
            TimeLeftWhite = p;
        else
            TimeLeftBlack = p;

    }

    public GoScore Score()
    {
        var score = search.EstimateScore(board, BoardSquares.White);

        if (score > 0)
            return new GoScore(GoColor.White, (float)Math.Abs(score));
        else
            return new GoScore(GoColor.Black, (float)Math.Abs(score));
    }

    public GoPoint[] Dead()
    {
        var deadStones = search.GetDeadStones(board, BoardSquares.White);

        List<GoPoint> results = new List<GoPoint>();

        foreach (var stone in deadStones)
        {
            var xy = board.PosToXY(stone);
            var point = new GoPoint((uint)xy.Item1 - 1, (uint)xy.Item2 - 1);
            results.Add(point);
        }

        return results.ToArray();
    }

    public GoPoint[] Territory(GoColor color)
    {
        throw new NotImplementedException();
    }

    public GoPoint[] Ideas(GoColor color)
    {
        throw new NotImplementedException();
    }

    public bool Ping()
    {
        throw new NotImplementedException();
    }
}
