using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace GGGG.GnuTextProtocol
{
    public enum GtpColor
    {
        Black, White
    }

    public enum GtpPassResign
    {
        Pass, Resign
    }

    public abstract class GtpWriter
    {
        public GtpWriter(TextWriter writer) : this(writer, null) { }

        public GtpWriter(TextWriter writer, TextWriter log)
        {
            this.writer = writer;
            this.log = log;
        }

        public void Write(uint x, uint y, uint size)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            Debug.Assert(x < 25 && y < 25 && size <= 25);
            Debug.Assert(x < size && y < size);
            WriteRaw(' ');
            WriteRaw((char)('A' + x + (x < ('I' - 'A') ? 0 : 1)));
            WriteRaw(size - y);
            state = State.Value;
        }

        public void Write(GtpPassResign value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            Debug.Assert(value == GtpPassResign.Pass || value == GtpPassResign.Resign);
            WriteRaw(' ');
            WriteRaw(value == GtpPassResign.Pass ? "PASS" : "resign");
            state = State.Value;
        }

        public void Write(GtpColor value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            Debug.Assert(value == GtpColor.Black || value == GtpColor.White);
            WriteRaw(' ');
            WriteRaw(value == GtpColor.Black ? "B" : "W");
            state = State.Value;
        }

        public void Write(GtpColor color, uint x, uint y, uint size)
        {
            Write(color);
            Write(x, y, size);
        }

        public void Write(bool value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            WriteRaw(' ');
            WriteRaw(value ? "true" : "false");
            state = State.Value;
        }

        public void Write(uint value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            WriteRaw(' ');
            WriteRaw(value);
            state = State.Value;
        }

        public void Write(float value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            WriteRaw(' ');
            WriteRaw(value);
            state = State.Value;
        }

        public void Write(string value)
        {
            Debug.Assert(state == State.Value || state == State.Newline);
            Debug.Assert(ValidString(value, false, true));
            WriteRaw(' ');
            WriteRaw(value);
            state = State.Value;
        }

        public void WriteComment(string comment)
        {
            Debug.Assert(ValidString(comment, false, true));
            WriteRaw('#');
            WriteRaw(comment);
            if (state == State.Start)
            {
                WriteRawNewline();
                state = State.Start;
            }
            else
            {
                state = State.Comment;
            }
        }

        protected bool ValidString(string value, bool allowNewline, bool allowSpace)
        {
            foreach (char c in value)
            {
                if (c > 32 && c < 127)
                    continue;
                if (!allowNewline && c == '\n')
                    return false;
                if (!allowSpace && c == ' ')
                    return false;
            }
            return true;
        }

        protected State state = State.Start;

        protected enum State
        {
            Start, Value, Comment, Newline
        }

        protected void WriteRaw(char c)
        {
            writer.Write(c);
            //writer.Flush();
            Log(c);
        }

        protected void WriteRaw(string s)
        {
            writer.Write(s);
            //writer.Flush();
            Log(s);
        }

        protected void WriteRaw(uint i)
        {
            writer.Write(i);
            //writer.Flush();
            Log(i);
        }

        protected void WriteRaw(float f)
        {
            writer.Write(f);
            //writer.Flush();
            Log(f);
        }

        protected void WriteRawNewline()
        {
            writer.WriteLine();
            //writer.Flush();
            if (log == null)
                return;
            log.WriteLine();
            log.Flush();
        }

        private void Log(char c)
        {
            if (log == null)
                return;
            log.Write(c);
            log.Flush();
        }

        protected void Log(string s)
        {
            if (log == null)
                return;
            log.Write(s);
            log.Flush();
        }

        private void Log(uint i)
        {
            if (log == null)
                return;
            log.Write(i);
            log.Flush();
        }

        private void Log(float f)
        {
            if (log == null)
                return;
            log.Write(f);
            log.Flush();
        }

        protected void LogNewline()
        {
            if (log == null)
                return;
            log.WriteLine();
        }

        protected TextWriter writer;
        protected TextWriter log;
    }

    public class GtpCommandWriter : GtpWriter
    {
        public GtpCommandWriter(TextWriter writer, TextWriter log) : base(writer, log) { }

        public GtpCommandWriter(TextWriter writer) : base(writer) { }

        public void WriteCommand(int id, string name)
        {
            WriteCommandStart(id, name);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, uint x, uint y, uint size)
        {
            WriteCommandStart(id, name);
            Write(x, y, size);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, GtpPassResign argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, GtpColor argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, GtpColor color, uint x, uint y, uint size)
        {
            WriteCommandStart(id, name);
            Write(color, x, y, size);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, bool argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, uint argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, float argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(int id, string name, string argument)
        {
            WriteCommandStart(id, name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name)
        {
            WriteCommandStart(name);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, uint x, uint y, uint size)
        {
            WriteCommandStart(name);
            Write(x, y, size);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, GtpPassResign argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, GtpColor argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, GtpColor color, uint x, uint y, uint size)
        {
            WriteCommandStart(name);
            Write(color, x, y, size);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, bool argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, uint argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, float argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommand(string name, string argument)
        {
            WriteCommandStart(name);
            Write(argument);
            WriteCommandEnd();
        }

        public void WriteCommandStart(int id, string name)
        {
            WriteCommandStart(id, true, name);
        }

        public void WriteCommandStart(string name)
        {
            WriteCommandStart(0, false, name);
        }

        public void WriteCommandEnd()
        {
            Debug.Assert(state == State.Value || state == State.Comment);
            WriteRawNewline();
            writer.Write("                                                                "); // FLUSHTEST
            writer.Flush();
            state = State.Start;
        }

        private void WriteCommandStart(int id, bool useId, string name)
        {
            Debug.Assert(state == State.Start);
            Debug.Assert(id >= 0);
            Debug.Assert(ValidString(name, false, false));
            Debug.Assert(name.Length == 0 || !char.IsNumber(name[0]));
            LogNewline();
            Log("WRITE COMMAND: ");
            if (useId)
            {
                WriteRaw(id);
                WriteRaw(' ');
            }
            WriteRaw(name);
            state = State.Value;
        }
    }

    public class GtpResponseWriter : GtpWriter
    {
        public GtpResponseWriter(TextWriter writer, TextWriter log) : base(writer, log) { }

        public GtpResponseWriter(TextWriter writer) : base(writer) { }

        public void WriteResponse(int id)
        {
            WriteResponseStart(id);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, uint x, uint y, uint size)
        {
            WriteResponseStart(id);
            Write(x, y, size);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, GtpPassResign argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, GtpColor argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, GtpColor color, uint x, uint y, uint size)
        {
            WriteResponseStart(id);
            Write(color, x, y, size);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, bool argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, uint argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, float argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(int id, string argument)
        {
            WriteResponseStart(id);
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse()
        {
            WriteResponseStart();
            WriteResponseEnd();
        }

        public void WriteResponse(uint x, uint y, uint size)
        {
            WriteResponseStart();
            Write(x, y, size);
            WriteResponseEnd();
        }

        public void WriteResponse(GtpPassResign argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(GtpColor argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(GtpColor color, uint x, uint y, uint size)
        {
            WriteResponseStart();
            Write(color, x, y, size);
            WriteResponseEnd();
        }

        public void WriteResponse(bool argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(uint argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(float argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteResponse(string argument)
        {
            WriteResponseStart();
            Write(argument);
            WriteResponseEnd();
        }

        public void WriteNewline()
        {
            Debug.Assert(state == State.Value || state == State.Comment);
            Debug.Assert(state != State.Newline); // this means accidental termination of response
            WriteRawNewline();
            state = State.Newline;
        }

        public void WriteResponseStart(int id)
        {
            WriteResponseStart(id, (id >= 0));
        }

        public void WriteResponseStart()
        {
            WriteResponseStart(0, false);
        }

        public void WriteResponseEnd()
        {
            Debug.Assert(state != State.Start);
            WriteRawNewline();
            WriteRawNewline();
            //writer.Flush();
            state = State.Start;
        }

        public void WriteFailure(int id, string message)
        {
            WriteFailure(id, (id >= 0), message);
        }

        public void WriteFailure(string message)
        {
            WriteFailure(0, false, message);
        }

        private void WriteResponseStart(int id, bool useId)
        {
            Debug.Assert(state == State.Start);
            Debug.Assert(!useId || id >= 0);
            LogNewline();
            Log("WRITE RESPONSE: ");
            WriteRaw('=');
            if (useId)
                WriteRaw(id);
            state = State.Value;
        }

        private void WriteFailure(int id, bool useId, string message)
        {
            Debug.Assert(state == State.Start);
            Debug.Assert(!useId || id >= 0);
            Debug.Assert(ValidString(message, true, true));
            LogNewline();
            Log("WRITE FAILURE: ");
            WriteRaw('?');
            if (useId)
                WriteRaw(id);
            WriteRaw(' ');
            WriteRaw(message);
            WriteRawNewline();
            WriteRawNewline();
            writer.Write("                                                                "); // FLUSHTEST
            writer.Flush();
            state = State.Start;
        }
    }

    public abstract class GtpReader
    {
        public GtpReader(TextReader reader) : this(reader, null) { }

        public GtpReader(TextReader reader, TextWriter log)
        {
            this.reader = reader;
            this.log = log;
        }

        public bool ReadInt()
        {
            SkipSingleSpace();
            if (!char.IsNumber(Peek()))
                return false;
            valInt = ReadIntInternal();
            return true;
        }

        public void ReadId()
        {
            id = ReadIntInternal();
        }

        public bool ReadString()
        {
            SkipSingleSpace();
            sb.Remove(0, sb.Length);
            while (Peek() != ' ' && Peek() != '\n')
                sb.Append(Read());
            valString = sb.ToString();
            return valString.Length > 0;
        }

        public bool ReadFloat()
        {
            string temp = valString;
            if (!ReadString())
                return false;
            valFloat = float.Parse(valString, CultureInfo.InvariantCulture);
            valString = temp;
            return true;
        }

        public bool ReadColor()
        {
            SkipSingleSpace();
            switch (Peek())
            {
                case 'b':
                case 'B':
                    valColor = GtpColor.Black;
                    Read();
                    if (Peek() == 'l' || Peek() == 'L')
                    {
                        Read();
                        if (Peek() != 'a' || Peek() != 'A') Read(); else throw new GtpException("ReadColor: malformed 'black'");
                        if (Peek() != 'c' || Peek() != 'C') Read(); else throw new GtpException("ReadColor: malformed 'black'");
                        if (Peek() != 'k' || Peek() != 'K') Read(); else throw new GtpException("ReadColor: malformed 'black'");
                    }
                    return true;
                case 'w':
                case 'W':
                    valColor = GtpColor.White;
                    Read();
                    if (Peek() == 'h' || Peek() == 'H')
                    {
                        Read();
                        if (Peek() != 'i' || Peek() != 'I') Read(); else throw new GtpException("ReadColor: malformed 'white'");
                        if (Peek() != 't' || Peek() != 'T') Read(); else throw new GtpException("ReadColor: malformed 'white'");
                        if (Peek() != 'e' || Peek() != 'E') Read(); else throw new GtpException("ReadColor: malformed 'white'");
                    }
                    return true;
                default: return false;
            }
        }

        public bool ReadBool()
        {
            SkipSingleSpace();
            switch (Peek())
            {
                case 't':
                case 'T':
                    valBool = true;
                    Read();
                    if (Peek() == 'r' || Peek() == 'R')
                    {
                        Read();
                        if (Peek() != 'u' || Peek() != 'U') Read(); else throw new GtpException("ReadBool: malformed 'true'");
                        if (Peek() != 'e' || Peek() != 'E') Read(); else throw new GtpException("ReadBool: malformed 'true'");
                    }
                    return true;
                case 'f':
                case 'F':
                    valBool = false;
                    Read();
                    if (Peek() == 'a' || Peek() == 'A')
                    {
                        Read();
                        if (Peek() != 'l' || Peek() != 'L') Read(); else throw new GtpException("ReadBool: malformed 'false'");
                        if (Peek() != 's' || Peek() != 'S') Read(); else throw new GtpException("ReadBool: malformed 'false'");
                        if (Peek() != 'e' || Peek() != 'E') Read(); else throw new GtpException("ReadBool: malformed 'false'");
                    }
                    return true;
                default: return false;
            }
        }

        public bool ReadVertex(uint size)
        {
            if (size == 0)
                throw new GtpException("ReadVertex: invalid size.");
            SkipSingleSpace();
            if (Peek() == '\n')
                return false;
            char c = Read();
            if (char.IsLower(c))
                c = char.ToUpper(c);
            if (c < 'A' || c > 'Z' || c == 'I')
                throw new GtpException("ReadVertex: Off-board horizontal coordinate.");
            if (c == 'P') // count be "PASS"
            {
                if (Peek() == 'a' || Peek() == 'A')
                {
                    Read();
                    if (Peek() != 's' || Peek() != 'S') Read(); else throw new GtpException("ReadVertex: malformed 'pass'.");
                    if (Peek() != 's' || Peek() != 'S') Read(); else throw new GtpException("ReadVertex: malformed 'pass'.");
                    valX = 0;
                    valY = 0;
                    valPass = true;
                    return true;
                }
            }
            else if (c == 'R') // count be "resign"
            {
                if (Peek() == 'e' || Peek() == 'E')
                {
                    Read();
                    if (Peek() != 's' || Peek() != 'S') Read(); else throw new GtpException("ReadVertex: malformed 'resign'.");
                    if (Peek() != 'i' || Peek() != 'I') Read(); else throw new GtpException("ReadVertex: malformed 'resign'.");
                    if (Peek() != 'g' || Peek() != 'G') Read(); else throw new GtpException("ReadVertex: malformed 'resign'.");
                    if (Peek() != 'n' || Peek() != 'N') Read(); else throw new GtpException("ReadVertex: malformed 'resign'.");
                    valX = 0;
                    valY = 0;
                    valPass = true;
                    return true;
                }
            }
            valX = (uint)(c - 'A' - (c > 'I' ? 1 : 0));
            int i = ReadIntInternal();
            if (i <= 0)
                throw new GtpException("ReadVertex: off-board vertical coordinate.");
            valY = size - (uint)i;
            return true;
        }

        public bool ReadMove(uint size)
        {
            if (size == 0)
                throw new GtpException("ReadMove: invalid size.");
            if (!ReadColor())
                return false;
            return ReadVertex(size);
        }

        public void ReadEnd()
        {
            while (Peek() != '\n')
                Read();
        }

        public int ID
        {
            get { return id; }
        }

        public int Int
        {
            get { return valInt; }
        }

        public float Float
        {
            get { return valFloat; }
        }

        public bool Bool
        {
            get { return valBool; }
        }

        public string String
        {
            get { return valString; }
        }

        public GtpColor Color
        {
            get { return valColor; }
        }

        public uint X
        {
            get { return valX; }
        }

        public uint Y
        {
            get { return valY; }
        }

        public bool Pass
        {
            get { return valPass; }
        }

        public bool Resign
        {
            get { return valResign; }
        }

        protected void SkipInsignificantSpace()
        {
            while (Peek() == ' ' || Peek() == '\n')
                Read();
        }

        protected virtual void Reset()
        {
            valInt = 0;
            id = -1;
            valX = valY = 0;
            valBool = valPass = valResign = false;
            valString = String.Empty;
            valFloat = 0.0F;
            valColor = GtpColor.Black;
        }

        protected char Peek()
        {
            char c = PeekRaw();
            while (c != '\t' && c != '\n' && (c < 32 || c == 127))
            {
                ReadRaw();
                c = PeekRaw(); // skip control chars (except HT, LF)
            }
            if (c == '#') // comment?
            {
                while (PeekRaw() != '\n')
                    c = ReadRaw(); // skip comments
                ReadRaw();
                c = Peek();
            }
            if (c == '\t')
                return ' ';
            return c;
        }

        protected char Read()
        {
            char c = PeekRaw();
            if (c == '\t')
                c = ' ';
            Debug.Assert(c == Peek()); // should never call Read() without first calling Peek()
            ReadRaw();
            return c;
        }

        private void SkipSingleSpace()
        {
            if (Peek() == ' ')
                Read();
        }

        private int ReadIntInternal()
        {
            if (!char.IsNumber(Peek()))
                return -1;
            int i = 0;
            while (char.IsNumber(Peek()))
            {
                i *= 10;
                i += Read() - '0';
            }
            return i;
        }

        public void AbortRead() // this will cause a GtpAbortException if waiting during read
        {
            abortRead = true;
        }

        protected char PeekRaw()
        {
            if (alreadyPeeked)
                return (char)lastPeek;
            alreadyPeeked = true;
            // assume interactive communications
            // the proper thing to do in a gtp file is send a quit command at the end
            try
            {
                do
                {
                    lastPeek = reader.Read();
                    if (lastPeek != -1)
                        return (char)lastPeek;
                    if (abortRead)
                    {
                        abortRead = false;
                        throw new GtpAbortException("PeekRaw: Read aborted.");
                    }
                    Thread.Sleep(100); // TODO: Waiting event
                } while (reader.Peek() == -1);
            }
            catch (IOException ex)
            {
                throw new GtpAbortException(ex.Message);
            }
            return (char)lastPeek;
        }

        protected char ReadRaw()
        {
            Debug.Assert(alreadyPeeked);
            alreadyPeeked = false;
            Log((char)lastPeek);
            return (char)lastPeek;
        }

        private void Log(char c)
        {
            if (log == null)
                return;
            log.Write(c);
            log.Flush();
        }

        protected void Log(string s)
        {
            if (log == null)
                return;
            log.Write(s);
            log.Flush();
        }

        protected void LogNewline()
        {
            if (log == null)
                return;
            log.WriteLine();
        }

        private int id;
        private int valInt;
        private float valFloat;
        private bool valBool;
        protected string valString;
        private GtpColor valColor;
        private uint valX;
        private uint valY;
        private bool valPass;
        private bool valResign;

        private bool alreadyPeeked = false;
        private bool abortRead = false;
        int lastPeek = -1;

        protected TextReader reader;
        private TextWriter log;

        protected readonly StringBuilder sb = new StringBuilder();
    }

    public class GtpCommandReader : GtpReader
    {
        public GtpCommandReader(TextReader reader, TextWriter log) : base(reader, log) { }

        public GtpCommandReader(TextReader reader) : base(reader) { }

        public void ReadCommand()
        {
            LogNewline();
            Log("READ COMMAND: ");
            Reset();
            SkipInsignificantSpace();
            ReadId();
            ReadName();
        }

        public void ReadName()
        {
            if (!ReadString())
                throw new GtpException("ReadName: Did not include a string.");
            name = valString;
            if (name.Length == 0)
                throw new GtpException("ReadName: Length zero.");
            valString = String.Empty;
        }

        public string Name
        {
            get { return name; }
        }

        protected override void Reset()
        {
            base.Reset();
            name = String.Empty;
        }

        private string name;
    }

    public class GtpResponseReader : GtpReader
    {
        public GtpResponseReader(TextReader reader, TextWriter log) : base(reader, log) { }

        public GtpResponseReader(TextReader reader) : base(reader) { }

        public void ReadResponse()
        {
            LogNewline();
            Log("READ RESPONSE: ");
            Reset();
            SkipInsignificantSpace();
            if (Peek() == '?') // error?
                isError = true;
            else if (Peek() == '=') // success?
                isError = false;
            else
                throw new GtpException("ReadResponse: Invalid token.");
            Read();
            ReadId();
            if (isError)
                ReadError();
        }

        public bool IsError
        {
            get { return isError; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        public bool ReadNewline()
        {
            if (Peek() != '\n')
                throw new GtpException("ReadNewline: Next char is not actually a newline.");
            Read();
            return Peek() != '\n';
        }

        protected override void Reset()
        {
            base.Reset();
            isError = false;
            errorMessage = String.Empty;
        }

        private void ReadError()
        {
            sb.Remove(0, sb.Length);
            while (Peek() != '\n')
                sb.Append(Read());
            errorMessage = sb.ToString();
        }

        private bool isError = false;
        private string errorMessage = String.Empty;
    }

    // TODO: GtpReader state

    public class GtpException : Exception
    {
        public GtpException(string message) : base(message) { }

        public readonly string Why;
    }

    public class GtpAbortException : GtpException
    {
        public GtpAbortException(string message) : base(message) { }
    }
}

// Note: I don't like that you have to know the board size in order to convert normal cartesian coordinates
// Note: Writers assert while Readers throw because I own code that writes but readers are accepting input from who-knows-where