using System.Text;

namespace HToolEx.Util;

/// <summary>
///     Convert to array static class
/// </summary>
public static class ConvertValues {
    /// <summary>
    ///     Convert to int16
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="reverse">reverse</param>
    /// <returns>values</returns>
    public static short[] ToInt16(byte[] items, bool reverse = true) {
        var list = new List<short>();
        // check items count
        var count = items.Length / 2;
        // check count
        for (var i = 0; i < count; i++) {
            // get data values
            var values = items.Skip(i * 2).Take(2);
            // add value
            list.Add(!reverse
                ? BitConverter.ToInt16(values.ToArray())
                : BitConverter.ToInt16(values.Reverse().ToArray()));
        }

        // check length
        if (items.Length % 2 > 0)
            // add last item
            list.Add(!reverse
                ? BitConverter.ToInt16(new byte[] { 0x00, items[^1] })
                : BitConverter.ToInt16(new byte[] { items[^1], 0x00 }));

        return list.ToArray();
    }

    /// <summary>
    ///     Convert to unsigned int16
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="reverse">reverse</param>
    /// <returns>values</returns>
    public static ushort[] ToUInt16(byte[] items, bool reverse = true) {
        var list = new List<ushort>();
        // check items count
        var count = items.Length / 2;
        // check count
        for (var i = 0; i < count; i++) {
            // get data values
            var values = items.Skip(i * 2).Take(2);
            // add value
            list.Add(!reverse
                ? BitConverter.ToUInt16(values.ToArray())
                : BitConverter.ToUInt16(values.Reverse().ToArray()));
        }

        // check length
        if (items.Length % 2 > 0)
            // add last item
            list.Add(!reverse
                ? BitConverter.ToUInt16(new byte[] { 0x00, items[^1] })
                : BitConverter.ToUInt16(new byte[] { items[^1], 0x00 }));

        return list.ToArray();
    }

    /// <summary>
    ///     Convert to int32
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="reverse">reverse</param>
    /// <returns>values</returns>
    public static int[] ToInt32(byte[] items, bool reverse = true) {
        var list = new List<int>();
        // check items count
        var count = items.Length / 4;
        // check count
        for (var i = 0; i < count; i++) {
            // get data values
            var values = items.Skip(i * 4).Take(4);
            // add value
            list.Add(!reverse
                ? BitConverter.ToInt32(values.ToArray())
                : BitConverter.ToInt32(values.Reverse().ToArray()));
        }

        // check length
        if (items.Length % 4 > 0) {
            // 0x01 0x02 0x03
            var values = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            // get remain data count
            count = items.Length % 4;
            // check count
            for (var i = 0; i < count; i++)
                // check reverse
                if (!reverse)
                    // set value
                    values[^(i + 1)] = items[^(i + 1)];
                else
                    // set value
                    values[i] = items[^(i + 1)];
            // add value
            list.Add(BitConverter.ToInt32(values));

            return list.ToArray();
        }

        return list.ToArray();
    }

    /// <summary>
    ///     Convert to unsigned int32
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="reverse">reverse</param>
    /// <returns>values</returns>
    public static uint[] ToUInt32(byte[] items, bool reverse = true) {
        var list = new List<uint>();
        // check items count
        var count = items.Length / 4;
        // check count
        for (var i = 0; i < count; i++) {
            // get data values
            var values = items.Skip(i * 4).Take(4);
            // add value
            list.Add(!reverse
                ? BitConverter.ToUInt32(values.ToArray())
                : BitConverter.ToUInt32(values.Reverse().ToArray()));
        }

        // check length
        if (items.Length % 4 > 0) {
            // 0x01 0x02 0x03
            var values = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            // get remain data count
            count = items.Length % 4;
            // check count
            for (var i = 0; i < count; i++)
                // check reverse
                if (!reverse)
                    // set value
                    values[^(i + 1)] = items[^(i + 1)];
                else
                    // set value
                    values[i] = items[^(i + 1)];
            // add value
            list.Add(BitConverter.ToUInt32(values));
        }

        return list.ToArray();
    }

    /// <summary>
    ///     Convert to single
    /// </summary>
    /// <param name="items">items</param>
    /// <param name="reverse">reverse</param>
    /// <returns>values</returns>
    public static float[] ToSingle(byte[] items, bool reverse = true) {
        var list = new List<float>();
        // check items count
        var count = items.Length / 4;
        // check count
        for (var i = 0; i < count; i++) {
            // get data values
            var values = items.Skip(i * 4).Take(4);
            // add value
            list.Add(!reverse
                ? BitConverter.ToSingle(values.ToArray())
                : BitConverter.ToSingle(values.Reverse().ToArray()));
        }

        // check length
        if (items.Length % 4 > 0) {
            // 0x01 0x02 0x03
            var values = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            // get remain data count
            count = items.Length % 4;
            // check count
            for (var i = 0; i < count; i++)
                // check reverse
                if (!reverse)
                    // set value
                    values[^(i + 1)] = items[^(i + 1)];
                else
                    // set value
                    values[i] = items[^(i + 1)];
            // add value
            list.Add(BitConverter.ToSingle(values));
        }

        return list.ToArray();
    }

    /// <summary>
    ///     Convert to string
    /// </summary>
    /// <param name="items">items</param>
    /// <returns>value</returns>
    public static string ToString(byte[] items) {
        return Encoding.ASCII.GetString(items).Replace("\0", string.Empty);
    }
}