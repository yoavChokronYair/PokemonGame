// PokemonGame.Services/Network/PacketHelper.cs
// Low-level TCP framing: every message is prefixed with a 4-byte (int32 big-endian) length,
// followed by UTF-8 JSON.  Both client and server use this helper.


using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace PokemonGame.Services.Network
{
    public static class PacketHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        // ── Write ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Serialize <paramref name="packet"/> to JSON, prefix with a 4-byte length
        /// (big-endian) and write to <paramref name="stream"/>.
        /// </summary>
        public static async Task WritePacketAsync(NetworkStream stream, object packet)
        {
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(packet, _jsonOptions);
            byte[] length = BitConverter.GetBytes(body.Length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(length); // always write big-endian

            await stream.WriteAsync(length, 0, 4).ConfigureAwait(false);
            await stream.WriteAsync(body, 0, body.Length).ConfigureAwait(false);
        }

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Read the next framed packet from <paramref name="stream"/> and deserialize
        /// it into <typeparamref name="T"/>.
        /// Returns <c>null</c> if the connection was closed cleanly.
        /// </summary>
        public static async Task<T?> ReadPacketAsync<T>(NetworkStream stream) where T : class
        {
            byte[]? body = await ReadBodyAsync(stream).ConfigureAwait(false);
            if (body == null) return null;

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        /// <summary>
        /// Read the next framed packet and return it as a raw JSON string so the
        /// caller can inspect the "type" discriminator before full deserialization.
        /// Returns <c>null</c> if the connection was closed cleanly.
        /// </summary>
        public static async Task<string?> ReadRawPacketAsync(NetworkStream stream)
        {
            byte[]? body = await ReadBodyAsync(stream).ConfigureAwait(false);
            return body == null ? null : Encoding.UTF8.GetString(body);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static async Task<byte[]?> ReadBodyAsync(NetworkStream stream)
        {
            // Read 4-byte length prefix
            byte[] lenBuf = new byte[4];
            int read = await ReadExactAsync(stream, lenBuf, 4).ConfigureAwait(false);
            if (read == 0) return null; // connection closed

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBuf);

            int bodyLength = BitConverter.ToInt32(lenBuf, 0);
            if (bodyLength <= 0 || bodyLength > 1_048_576) // sanity: max 1 MB per packet
                throw new InvalidDataException($"Invalid packet length: {bodyLength}");

            byte[] body = new byte[bodyLength];
            await ReadExactAsync(stream, body, bodyLength).ConfigureAwait(false);
            return body;
        }

        /// <summary>Read exactly <paramref name="count"/> bytes; returns bytes read (0 = closed).</summary>
        private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int n = await stream.ReadAsync(buffer, offset, count - offset).ConfigureAwait(false);
                if (n == 0) return offset; // remote closed
                offset += n;
            }
            return offset;
        }
    }
}