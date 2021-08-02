using System.Diagnostics;
using System.Globalization;
using System;
using GGGG.GnuTextProtocol;

namespace GGGG
{
    public interface IGoEngine
    {
        void Start(uint size, float komi, uint level);
        void Setup(GoColor color, uint x, uint y); // used to setup handicap or other initial board position
        GTextMove Reply(GoColor color); // throw GoEngineException exception if illegal (switched colors, ko, etc.)
        void Play(GTextMove move); // throw GoEngineException exception if illegal (switched colors, ko, etc.)
        void Undo(); // throw GoEngineException if nothing to undo
        void Quit();
        string Name { get; }
        string Version { get; }
        Action<string> DebugMessageHandler { set; }

        void SetTimeSettings(int seconds);
        void SetTimeLeft(GoColor color, int milliseconds);
    }

    public class GoEngineException : Exception { }

    public class GoMovePass : GTextMove { }

    public class GoMoveResign : GTextMove { }

    public class GTextMove
    {
        protected GTextMove() { }

        public GTextMove(GoColor color, uint x, uint y)
        {
            Color = color;
            X = x;
            Y = y;
        }

        public readonly GoColor Color;
        public readonly uint X;
        public readonly uint Y;
    }

    public enum GoColor
    {
        Black, White
    }

    public interface IAdvancedGoEngine : IGoEngine
    {
        GoScore Score(); // estimate score
        GoPoint[] Dead(); // enumerate dead stones
        GoPoint[] Territory(GoColor color); // enumerate territory points
        GoPoint[] Ideas(GoColor color); // enumerate top move hints
        bool Ping(); // are you there?
    }

    public struct GoScore
    {
        public GoScore(GoColor winner, float score)
        {
            Debug.Assert(score >= 0.0F);
            Winner = winner;
            Score = score;
        }

        public override string ToString()
        {
            return (Winner == GoColor.Black ? "B+" : "W+") + Score.ToString(CultureInfo.InvariantCulture);
        }

        public float ToFloat(GoColor color)
        {
            if (Winner != color)
                return -Score;
            return Score;
        }

        public readonly GoColor Winner;
        public readonly float Score;
    }

    public struct GoPoint
    {
        public GoPoint(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public readonly uint X;
        public readonly uint Y;
    }
}
