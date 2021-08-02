using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using GGGG;
using GGGG.GnuTextProtocol;

namespace GGGGGoText
{
    public class GtpEngineAdapter
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("GtpEngineAdapter");
        public GtpEngineAdapter(IGoEngine engine, TextReader reader, TextWriter writer, TextWriter log)
        {
            this.engine = engine;
            this.reader = new GtpCommandReader(reader, log);
            this.writer = new GtpResponseWriter(writer, log);
            Log.Info("Starting");
        }

        public bool KeepThinking = false;

        public GtpEngineAdapter(IGoEngine engine, TextReader reader, TextWriter writer)
        {
            this.engine = engine;
            this.reader = new GtpCommandReader(reader);
            this.writer = new GtpResponseWriter(writer);
            Log.Info("Starting");
        }

        public void Start()
        {
            //if (messageThread != null)
            //	Abort();
            //messageThread = new Thread(new ThreadStart(MessageLoop));
            //messageThread.Start();
            try
            {
                MessageLoop();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }


        }

        /* Thread.Suspend()/Resume()/Abort() are not available in Compact Framework!
        public void Suspend()
        {
            messageThread.Suspend();
        }

        public void Resume()
        {
            messageThread.Resume();
        }

        public void Abort()
        {
            messageThread.Abort();
            messageThread = null;
        }
        */

        public void ProcessMessage()
        {
            int timeRemaining = 0;
            reader.ReadCommand();
            switch (reader.Name.ToLower())
            {
                case "time_settings":
                    reader.ReadInt();
                    timeRemaining = reader.Int;
                    reader.ReadInt();
                    reader.ReadInt();
                    engine.SetTimeSettings(timeRemaining);
                    writer.WriteResponse(reader.ID);
                    break;
                case "time_left":
                    if (!reader.ReadColor())
                        throw new GtpException("Command: 'genmove' did not include a color.");

                    if (!reader.ReadInt())
                        throw new GtpException("Command: 'Time_left' did not include an int");
                    timeRemaining = reader.Int;
                    reader.ReadInt();
                    engine.SetTimeLeft(reader.Color == GtpColor.Black ? GoColor.Black : GoColor.White, timeRemaining);
                    writer.WriteResponse(reader.ID);
                    break;
                case "protocol_version":
                    writer.WriteResponse(reader.ID, 2);
                    break;
                case "name":
                    writer.WriteResponse(reader.ID, engine.Name);
                    break;
                case "version":
                    writer.WriteResponse(reader.ID, engine.Version);
                    break;
                case "known_command":
                    if (!reader.ReadString())
                        throw new GtpException("Command: 'known_command' did not include a string.");
                    writer.WriteResponse(reader.ID, KnownCommand(reader.String));
                    break;
                case "list_commands":
                    writer.WriteResponseStart(reader.ID);
                    ListCommands();
                    writer.WriteResponseEnd();
                    break;
                case "quit":
                    engine.Quit();
                    writer.WriteResponse(reader.ID);
                    throw new GtpAbortException("quit");
                case "boardsize":
                    start = true;
                    if (!reader.ReadInt())
                        throw new GtpException("Command: 'boardsize' did not include an int.");
                    size = (uint)reader.Int;
                    writer.WriteResponse(reader.ID);
                    break;
                case "clear_board":
                    start = true;
                    writer.WriteResponse(reader.ID);
                    break;
                case "komi":
                    start = true;
                    if (!reader.ReadFloat())
                        throw new GtpException("Command: 'komi' did not include a float.");
                    komi = reader.Float;
                    writer.WriteResponse(reader.ID);
                    break;
                case "fixed_handicap":
                    if (start)
                    {
                        engine.Start(size, komi, level);
                        start = false;
                    }
                    if (!reader.ReadInt())
                        throw new GtpException("Command: 'fixed_handicap' did not include an int.");
                    writer.WriteResponseStart(reader.ID);
                    FixedHandicap(reader.Int);
                    writer.WriteResponseEnd();
                    break;
                case "place_free_handicap":
                    if (start)
                    {
                        engine.Start(size, komi, level);
                        start = false;
                    }
                    if (!reader.ReadInt())
                        throw new GtpException("Command: 'place_free_handicap' did not include an int.");
                    writer.WriteResponseStart(reader.ID);
                    for (int i = 0; i < reader.Int; i++)
                    {
                        GTextMove handicap = engine.Reply(GoColor.Black);
                        if (handicap is GoMovePass || handicap is GoMoveResign)
                            continue;
                        writer.Write(handicap.X, handicap.Y, size);
                    }
                    writer.WriteResponseEnd();
                    break;
                case "set_free_handicap":
                    start = true;
                    while (reader.ReadVertex(size))
                        engine.Setup(GoColor.Black, reader.X, reader.Y);
                    writer.WriteResponse(reader.ID);
                    break;
                case "play":
                    if (start)
                    {
                        engine.Start(size, komi, level);
                        start = false;
                    }
                    if (!reader.ReadMove(size))
                        throw new GtpException("Command: 'play' did not include a move.");
                    writer.WriteResponse(reader.ID);
                    GTextMove move = reader.Pass ? new GoMovePass() : reader.Resign ? new GoMoveResign() : new GTextMove(reader.Color == GtpColor.Black ? GoColor.Black : GoColor.White, reader.X, reader.Y);
                    engine.Play(move);
                    break;
                case "genmove":
                    if (start)
                    {
                        engine.Start(size, komi, level);
                        start = false;
                    }
                    if (!reader.ReadColor())
                        throw new GtpException("Command: 'genmove' did not include a color.");
                    GTextMove reply = engine.Reply(reader.Color == GtpColor.Black ? GoColor.Black : GoColor.White);
                    if (reply is GoMovePass)
                        writer.WriteResponse(reader.ID, GtpPassResign.Pass);
                    else if (reply is GoMoveResign)
                        writer.WriteResponse(reader.ID, GtpPassResign.Resign);
                    else
                        writer.WriteResponse(reader.ID, reply.X, reply.Y, size);
                    break;
                case "undo":
                    engine.Undo();
                    writer.WriteResponse(reader.ID);
                    break;
                case "final_status_list":
                    if (!(engine is IAdvancedGoEngine))
                    {
                        writer.WriteFailure(reader.ID, "unknown command");
                        break;
                    }
                    if (!reader.ReadString())
                        throw new GtpException("Command: 'final_status_list' did not include a string.");
                    GoPoint[] points = null;
                    switch (reader.String)
                    {
                        case "dead":
                            points = ((IAdvancedGoEngine)engine).Dead();
                            break;
                        case "black_territory":
                            points = ((IAdvancedGoEngine)engine).Territory(GoColor.Black);
                            break;
                        case "white_territory":
                            points = ((IAdvancedGoEngine)engine).Territory(GoColor.White);
                            break;
                    }
                    if (points == null)
                    {
                        writer.WriteFailure(reader.ID, "syntax error");
                        break;
                    }
                    writer.WriteResponseStart(reader.ID);
                    foreach (GoPoint p in points)
                        writer.Write(p.X, p.Y, size);
                    writer.WriteResponseEnd();
                    break;
                case "estimate_score":
                    if (!(engine is IAdvancedGoEngine))
                    {
                        writer.WriteFailure(reader.ID, "unknown command");
                        break;
                    }
                    GoScore score = ((IAdvancedGoEngine)engine).Score();
                    writer.WriteResponse(score.ToString());
                    break;
                case "level":
                    if (!(engine is IAdvancedGoEngine))
                    {
                        writer.WriteFailure(reader.ID, "unknown command");
                        break;
                    }
                    if (!reader.ReadInt())
                        throw new GtpException("Command: 'level' did not include an int.");
                    level = (uint)reader.Int;
                    writer.WriteResponse(reader.ID);
                    break;
                default:
                    writer.WriteFailure(reader.ID, "unknown command");
                    break;
            }
            reader.ReadEnd();
        }

        private bool KnownCommand(string command)
        {
            switch (command)
            {
                case "protocol_version":
                case "name":
                case "version":
                case "known_command":
                case "list_commands":
                case "quit":
                case "boardsize":
                case "clear_board":
                case "komi":
                case "fixed_handicap":
                case "place_free_handicap":
                case "play":
                case "genmove":
                case "undo":
                    return true;
                case "final_status_list":
                case "estimate_score":
                case "level":
                    return (engine is IAdvancedGoEngine);
                default:
                    return false;
            }
        }

        private void FixedHandicap(int h)
        {
            if (h <= 1)
                return;
            if (size < 7)
                return;
            int max = size % 2 == 0 ? 4 : 9;
            Debug.Assert(h <= max);
            if (h > max)
                h = max;
            uint edge = size > 12 ? 4U : 3U;
            uint center = (size - 1) / 2;
            uint near = edge - 1;
            uint far = size - edge;
            AddFixedHandicapStone(near, far); // SW
            AddFixedHandicapStone(far, near); // NE
            if (h > 2)
                AddFixedHandicapStone(near, near); // NW
            if (h > 3)
                AddFixedHandicapStone(far, far); // SE
            if (h == 5)
                AddFixedHandicapStone(center, center);
            else if (h > 4)
                AddFixedHandicapStone(near, center); // W
            if (h > 5)
                AddFixedHandicapStone(far, center); // E
            if (h == 7)
                AddFixedHandicapStone(center, center);
            else if (h > 6)
                AddFixedHandicapStone(center, far); // S
            if (h > 7)
                AddFixedHandicapStone(center, near); // N
            if (h == 9)
                AddFixedHandicapStone(center, center);
        }

        private void AddFixedHandicapStone(uint x, uint y)
        {
            writer.Write(x, y, size);
            engine.Setup(GoColor.Black, x, y);
        }

        private void ListCommands()
        {
            if (engine is IAdvancedGoEngine)
            {
                writer.Write("final_status_list"); writer.WriteNewline();
                writer.Write("estimate_score"); writer.WriteNewline();
                writer.Write("level"); writer.WriteNewline();
            }
            writer.Write("protocol_version"); writer.WriteNewline();
            writer.Write("name"); writer.WriteNewline();
            writer.Write("version"); writer.WriteNewline();
            writer.Write("known_command"); writer.WriteNewline();
            writer.Write("list_commands"); writer.WriteNewline();
            writer.Write("quit"); writer.WriteNewline();
            writer.Write("boardsize"); writer.WriteNewline();
            writer.Write("clear_board"); writer.WriteNewline();
            writer.Write("komi"); writer.WriteNewline();
            writer.Write("fixed_handicap"); writer.WriteNewline();
            writer.Write("place_free_handicap"); writer.WriteNewline();
            writer.Write("play"); writer.WriteNewline();
            writer.Write("genmove"); writer.WriteNewline();
            writer.Write("time_settings"); writer.WriteNewline();
            writer.Write("time_left"); writer.WriteNewline();
            writer.Write("undo");
        }

        private void MessageLoop()
        {
            while (true)
            {
                try
                {
                    ProcessMessage();
                }
                catch (GtpAbortException)
                {
                    break;
                }
                catch (GtpException ex)
                {
                    writer.WriteFailure(reader.ID, "exception thrown: " + ex.Message);
                }
            }
        }

        private IGoEngine engine;
        private GtpCommandReader reader;
        private GtpResponseWriter writer;
        //private Thread messageThread = null;
        private uint size = 19;
        private float komi = 0.5F;
        private uint level = 1;
        private bool start = false;
    }
}

// TODO: Reconcile GoColor vs. GtpColor?