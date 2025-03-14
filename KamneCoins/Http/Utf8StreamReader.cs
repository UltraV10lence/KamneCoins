namespace KamneCoins.Http;

public class Utf8StreamReader : TextReader {
    public readonly Stream BaseStream;
    private int peekedByte = -1;

    public Utf8StreamReader(Stream baseStream) {
        BaseStream = baseStream;
    }

    public override int Read() {
        if (peekedByte == -1) return ReadUtf8Char();

        var temp = peekedByte;
        peekedByte = -1;
        return temp;
    }

    public override int Peek() {
        if (peekedByte == -1)
            peekedByte = ReadUtf8Char();

        return peekedByte;
    }

    private int ReadUtf8Char() {
        var firstByte = BaseStream.ReadByte();
        switch (firstByte) {
            case -1:
                return -1;
            case < 0x80:
                return firstByte;
        }

        var additionalBytes = firstByte switch {
            >= 0xC2 and <= 0xDF => 1,
            >= 0xE0 and <= 0xEF => 2,
            >= 0xF0 and <= 0xF4 => 3,
            _ => throw new IOException("Invalid UTF-8 sequence.")
        };

        var codepoint = firstByte & (1 << 7 - additionalBytes) - 1;
        for (var i = 0; i < additionalBytes; i++) {
            var nextByte = BaseStream.ReadByte();
            if (nextByte is < 0x80 or > 0xBF) throw new IOException("Invalid UTF-8 sequence.");
            codepoint = codepoint << 6 | nextByte & 0x3F;
        }

        return codepoint;
    }
}