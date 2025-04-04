﻿using System.Text;
using HToolEx.ProEx.Type;
using JetBrains.Annotations;

namespace HToolEx.ProEx.FormatJob;

/// <summary>
///     Step message data class
/// </summary>
[PublicAPI]
public sealed class FormatStepMessage : FormatStep {
    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="revision">revision</param>
    public FormatStepMessage(int revision = 0) {
        // set type
        Type = JobStepTypes.Message;
        // check message length
        for (var i = 0; i < 3; i++)
            // reset message
            Message[i] = string.Empty;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="values">values</param>
    /// <param name="revision">revision</param>
    public FormatStepMessage(byte[] values, int revision = 0) : this(revision) {
        // set values
        Set(values, revision);
    }

    /// <summary>
    ///     Messages items
    /// </summary>
    public string[] Message { get; } = new string[3];


    /// <summary>
    ///     Image path
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    ///     Message type
    /// </summary>
    public MessageTypes MessageType { get; set; } = MessageTypes.Validation;

    /// <summary>
    ///     Delay time
    /// </summary>
    public int DelayTime { get; set; }

    /// <summary>
    ///     Update values
    /// </summary>
    /// <param name="revision">revision</param>
    public override void Update(int revision = 0) {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryWriter(stream);

        // update step values
        bin.Write((int)Type);
        bin.Write(Name.ToCharArray());
        bin.Write(new byte[128 - Name.Length]);

        // update step data values
        foreach (var msg in Message) {
            bin.Write(msg.ToCharArray());
            bin.Write(new byte[128 - msg.Length]);
        }

        bin.Write(ImagePath.ToCharArray());
        bin.Write(new byte[256 - ImagePath.Length]);

        // update message type
        bin.Write((int)MessageType);
        // check message type
        switch (MessageType) {
            case MessageTypes.DelayTime:
                // update delay time
                bin.Write(DelayTime);
                break;
            case MessageTypes.Validation:
            case MessageTypes.NextStep:
            default:
                break;
        }
    }

    /// <summary>
    ///     Refresh values
    /// </summary>
    /// <param name="revision">revision</param>
    public override void Refresh(int revision = 0) {
        // memory stream
        using var stream = new MemoryStream(Values);
        // binary reader
        using var bin = new BinaryReader(stream);

        // get type value
        var type = bin.ReadInt32();
        // check defined
        if (Enum.IsDefined(typeof(JobStepTypes), type))
            // set type
            Type = (JobStepTypes)type;
        // set step name
        Name = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');

        // check message length
        for (var i = 0; i < Message.Length; i++)
            // set message
            Message[i] = Encoding.ASCII.GetString(bin.ReadBytes(128)).TrimEnd('\0');
        // set image file path
        ImagePath = Encoding.ASCII.GetString(bin.ReadBytes(256)).TrimEnd('\0');

        // get message type
        var message = bin.ReadInt32();
        // check defined
        if (Enum.IsDefined(typeof(MessageTypes), message))
            // set message type
            MessageType = (MessageTypes)message;
        // check message type
        switch (MessageType) {
            case MessageTypes.DelayTime:
                // set time value
                DelayTime = bin.ReadInt32();
                break;
            case MessageTypes.Validation:
            case MessageTypes.NextStep:
            default:
                break;
        }
    }
}