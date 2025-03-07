﻿using System.ComponentModel;
using System.Text;
using HToolEx.Util;
using JetBrains.Annotations;

namespace HToolEx.ProEx.Format;

/// <summary>
///     Format log setting class
/// </summary>
public class FormatSetLog {
    /// <summary>
    ///     Data field count
    /// </summary>
    [PublicAPI] public static readonly int[] DataFieldCount = [22, 35];

    /// <summary>
    ///     Operation setting size each version
    /// </summary>
    [PublicAPI] public static readonly int[] Size = [27, 56];

    /// <summary>
    ///     Data field
    /// </summary>
    [PublicAPI] public int[] DataField;

    /// <summary>
    ///     Constructor
    /// </summary>
    public FormatSetLog() {
        // create data field
        DataField = [];
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatSetLog(byte[] values, int revision = 0) : this() {
        // check revision
        if (revision >= Size.Length)
            // reset revision
            revision = 0;
        // check size
        if (values.Length < Size[revision])
            return;

        // memory stream
        using var stream = new MemoryStream(values);
        // binary reader
        using var bin = new BinaryReaderBigEndian(stream);

        // set check sum
        CheckSum = values.Sum(x => x);

        // get revision.0 information
        Storage = Convert.ToInt32(bin.ReadByte());
        // create data field
        DataField = new int[DataFieldCount[revision]];
        // check data field count
        for (var i = 0; i < DataFieldCount[0]; i++)
            // set data field
            DataField[i] = Convert.ToInt32(bin.ReadByte());
        Graph = Convert.ToInt32(bin.ReadByte());
        TypeOfChannel1 = bin.ReadByte();
        TypeOfChannel2 = bin.ReadByte();
        SampleTime = bin.ReadByte();
        // check revision.1 information
        if (revision < 1)
            return;
        // check data field count
        for (var i = DataFieldCount[0]; i < DataFieldCount[1]; i++)
            // set data field
            DataField[i] = Convert.ToInt32(bin.ReadByte());
        UsbLabel = Encoding.ASCII.GetString(bin.ReadBytes(16)).TrimEnd('\0');
    }

    /// <summary>
    ///     Check sum
    /// </summary>
    [Browsable(false)]
    [PublicAPI]
    public int CheckSum { get; set; }

    #region REV.1

    /// <summary>
    ///     USB label
    /// </summary>
    [PublicAPI]
    public string UsbLabel { get; set; } = string.Empty;

    #endregion

    /// <summary>
    ///     Get values
    /// </summary>
    /// <param name="revision">revision</param>
    /// <returns>values</returns>
    [PublicAPI]
    public byte[] GetValues(int revision = 0) {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var values = new List<byte>();
        // get revision.0 values
        values.Add(Convert.ToByte(Storage));
        // check data field count
        for (var i = 0; i < DataFieldCount[0]; i++)
            // get data field
            values.Add(Convert.ToByte(DataField[i]));
        values.Add(Convert.ToByte(Graph));
        values.Add(Convert.ToByte(TypeOfChannel1));
        values.Add(Convert.ToByte(TypeOfChannel2));
        values.Add(Convert.ToByte(SampleTime));
        // check revision.1
        if (revision < 1)
            return values.ToArray();
        // get string values
        var label = Encoding.ASCII.GetBytes(UsbLabel).ToList();
        // string length offset
        label.AddRange(new byte[16 - UsbLabel.Length]);
        // get revision.1 values
        for (var i = DataFieldCount[0]; i < DataFieldCount[1]; i++)
            // get data field
            values.Add(Convert.ToByte(DataField[i]));
        values.AddRange(label);
        // values
        return values.ToArray();
    }

    #region REV.0

    /// <summary>
    ///     Storage
    /// </summary>
    [PublicAPI]
    public int Storage { get; set; }

    /// <summary>
    ///     Graph
    /// </summary>
    [PublicAPI]
    public int Graph { get; set; }

    /// <summary>
    ///     Type of channel 1
    /// </summary>
    [PublicAPI]
    public int TypeOfChannel1 { get; set; }

    /// <summary>
    ///     Type of channel 2
    /// </summary>
    [PublicAPI]
    public int TypeOfChannel2 { get; set; }

    /// <summary>
    ///     Sampling time
    /// </summary>
    [PublicAPI]
    public int SampleTime { get; set; }

    #endregion
}